using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Xml;
using SQLite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Shared
{




    public class Dict
    {
        public string word { get; set; }
        public string autoSugg { get; set; }

    }
    public class HelperTranslatorImporter
    {



        public static async Task<string> QueryOnBing(string value, HttpClient client, HtmlAgilityPack.HtmlDocument htmlDocument)

        {

            const string baseURL = "https://cn.bing.com/dict/search?q=";
            var response = await client.GetAsync(baseURL + value).ConfigureAwait(false);
            var stringValue = await response.Content.ReadAsStringAsync().ConfigureAwait(false);


            htmlDocument.LoadHtml(stringValue);

            // /html/body/div[1]/div/div/div[1]/div[1]

            // var baseNode = htmlDocument.DocumentNode.SelectSingleNode("//*[@class=\"qdef\"]");

            // var defNodes = baseNode?.SelectNodes("//*[@class='def']");

            var defNodes = htmlDocument.DocumentNode.SelectNodes("//*[@class=\"qdef\"]//*[@class=\"def\"]");


            if (defNodes == null) return string.Empty;
            var result = string.Empty;



            foreach (var item in defNodes)
            {
                result += item.InnerText.Trim() + Environment.NewLine;
            }

            return result;
        }

        private readonly object sLock = new object();


        public IEnumerable<string> ListAllKey()
        {
            return _connection.Query<dic>("select key from dic").Select(i => i.key);
        }
        private List<dic> _collection = new List<dic>();
        private List<string> _collectionDisable = new List<string>();

        private readonly HttpClient _client;
        private SQLiteConnection _connection;
        public int Count { get; private set; }

        private HtmlAgilityPack.HtmlDocument _document;

        public async Task Query(IEnumerable<string> keys,Action<int> act)
        {




            var allTasks = new List<Task>();
            var throttler = new SemaphoreSlim(initialCount: 3);
            foreach (var q in keys)
            {
                // do an async wait until we can schedule again
                await throttler.WaitAsync();

                // using Task.Run(...) to run the lambda in its own parallel
                // flow on the threadpool
                allTasks.Add(
                    Task.Run(async () =>
                    {
                        try
                        {
                            await QueryKey(q);
                            act(Count);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
            }

            // won't get here until all urls have been put into tasks
            await Task.WhenAll(allTasks);
            if (_collection.Count > 0)
            {
                _connection.InsertAll(_collection);
                _collection.Clear();

                "disable.txt".GetCommandPath().AppendAllText(Environment.NewLine + string.Join(Environment.NewLine, _collectionDisable));
                _collectionDisable.Clear();
            }
            // var actions = new List<Action>();

            // foreach (var link in links)
            // {
            //     actions.Add(async () =>
            //     {



            //     });
            // }

            // Parallel.Invoke(new ParallelOptions()
            // {
            //     MaxDegreeOfParallelism = 2
            // }, actions.ToArray());


            Console.WriteLine("All Finished.");
        }
        private async Task QueryKey(string q)
        {
            Count = Count + 1;
            var resultStr = string.Empty;

            try
            {
                resultStr = await QueryOnBing(q, _client, new HtmlAgilityPack.HtmlDocument()).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _collectionDisable.Add(e.Message);
            }
            // var resultStr = QueryFromGoogle(q);
            if (resultStr.IsVacuum())
            {
                _collectionDisable.Add(q);

                return;
            }
            AddToCollection(new dic { key = q, word = resultStr });
        }
        private void AddToCollection(dic dic)
        {
            lock (sLock)
            {
                if (dic.word.IsVacuum())
                {
                    _collectionDisable.Add(dic.key);
                    return;
                }
                _collection.Add(dic);
                if (_collection.Count > 100)
                {
                    _connection.InsertAll(_collection);
                    _collection.Clear();

                    "disable.txt".GetCommandPath().AppendAllText(Environment.NewLine + string.Join(Environment.NewLine, _collectionDisable));
                    _collectionDisable.Clear();
                }
            }
        }

        public HelperTranslatorImporter()
        {
            _client = new HttpClient(new HttpClientHandler
            {
                UseProxy = false,
                // UseProxy = true,
                //  Proxy = new WebProxy("127.0.0.1", 51426)
            });
            _client.Timeout = TimeSpan.FromSeconds(5);
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13B143 Safari/601.1");

            _connection = new SQLiteConnection("datas\\psycho.db".GetCommandPath());
            _connection.CreateTable<dic>();
            _document = new HtmlAgilityPack.HtmlDocument();

        }
        public void ImportPsychoDatabase()
        {
            var con = new SQLiteConnection("psycho.db".GetCommandPath());
            var collection = con.Query<dic>("select key,word from dic");


            var keys = ListAllKey();

            foreach (var item in collection)
            {
                if (keys.Contains(item.key)) continue;
                _collection.Add(item);
                if (_collection.Count() > 100)
                {

                    _connection.InsertAll(_collection);
                    _collection.Clear();

                }
            }
            if (_collection.Count > 0)
            {
                _connection.InsertAll(_collection);

            }

        }
        public void ImportBingDatabase()
        {
            var con = new SQLiteConnection("defaultdict.db".GetCommandPath());
            var collection = con.Query<Dict>("select word,autoSugg from Dict").Skip(1);


            var keys = ListAllKey();

            foreach (var item in collection)
            {
                if (keys.Contains(item.word)) continue;
                _collection.Add(new dic
                {
                    key = item.word,
                    word = item.autoSugg,
                });
                if (_collection.Count() > 100)
                {

                    _connection.InsertAll(_collection);
                    _collection.Clear();

                }
            }
            if (_collection.Count > 0)
            {
                _connection.InsertAll(_collection);

            }

        }
        private string FormatExplain(string htm)
        {
            _document.LoadHtml(htm);
            if (_document.DocumentNode.SelectSingleNode("//*[@class=\"error-wrapper\"]") != null)
            {
                return string.Empty;
            }

            var nodes = _document.DocumentNode.SelectNodes("//*[@id=\"phrsListTab\"]/div[@class='trans-container']//li");
            if (nodes != null)
            {

                var str = "";
                foreach (var node in nodes)
                {
                    str += node.InnerText.Trim() + "\r\n";
                }

                return str;
            }
            return string.Empty;

        }


        public void ExportPsychodatKeyList()
        {
            var fileName = "datas\\psycho.dat".GetCommandPath();
            if (fileName.FileExists())
            {
                using (var con = new SQLiteConnection(fileName))
                {
                    (fileName + ".txt").WriteAllText(con.Query<dic>("select key from dic").Select(i => i.key).ToLine());
                }
            }
        }

        public async Task ImportEnglish(string q)
        {
            var resultStr = string.Empty;

            try
            {
                resultStr = await QueryOnBing(q, _client, _document).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _collectionDisable.Add(e.Message);
            }
            // var resultStr = QueryFromGoogle(q);
            if (resultStr.IsVacuum())
            {
                _collectionDisable.Add(q);

                return;
            }
            //if (resultStr.Length > 1)
            //{
            _collection.Add(new dic
            {
                key = q,
                word = resultStr,
            });
            if (_collection.Count() > 100)
            {

                _connection.InsertAll(_collection);
                _collection.Clear();

                "disable.txt".GetCommandPath().AppendAllText(Environment.NewLine + string.Join(Environment.NewLine, _collectionDisable));
                _collectionDisable.Clear();

            }

            //  }

        }
        private string QueryFromGoogle(string q)
        {
            var url = "https://translate.google.cn/translate_a/single?client=gtx&sl=auto&tl="
                + "zh" + "&dt=t&dt=bd&ie=UTF-8&oe=UTF-8&dj=1&source=icon&q=" + WebUtility.UrlEncode(q);


            var res = _client.GetAsync(url).GetAwaiter().GetResult();

            var resStr = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var obj = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(resStr);
            JObject array = obj["sentences"][0];

            var word = "";
            JToken trans;
            if (array.TryGetValue("trans", out trans))
            {
                word += trans.Value<string>() + Environment.NewLine;
            }

            if (obj.ContainsKey("dict"))
            {
                JObject dic = obj["dict"][0];
                word += string.Join(",", dic.GetValue("terms"));

            }
            word = word.Trim();
            if (word.ToLower() == q || Regex.IsMatch(word, "^的[a-zA-Z]+$|^[a-zA-Z]+的$"))
            {
                return string.Empty;
            }
            return word;
        }
        public void Close()
        {
            if (_collection.Count() > 0)
            {
                _connection.InsertAll(_collection);
                _collection.Clear();
            }


            "disable.txt".GetCommandPath().AppendAllText(Environment.NewLine + string.Join(Environment.NewLine, _collectionDisable));
            _collectionDisable.Clear();
        }

    }


}
