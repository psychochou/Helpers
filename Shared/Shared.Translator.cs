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

namespace Shared
{
    public class HelperTranslator
    {
        private readonly HttpClient _client;

        public HelperTranslator()
        {
            _client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                UseProxy = false,
            });
            _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13B143 Safari/601.1");

        }

        public string QueryEnglish(string q)
        {
            var url = "https://translate.google.cn/translate_a/single?client=gtx&sl=auto&tl="
                + "zh" + "&dt=t&dt=bd&ie=UTF-8&oe=UTF-8&dj=1&source=icon&q=" + WebUtility.UrlEncode(q);

            var res = _client.GetAsync(url).GetAwaiter().GetResult();

            return res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }
        public string QueryChinese(string q)
        {
            var url = "https://translate.google.cn/translate_a/single?client=gtx&sl=auto&tl="
                + "en" + "&dt=t&dt=bd&ie=UTF-8&oe=UTF-8&dj=1&source=icon&q=" + WebUtility.UrlEncode(q);

            var res = _client.GetAsync(url).GetAwaiter().GetResult();

            return res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }
        public static HelperTranslator s;

        public static HelperTranslator GetInstance() => s ?? (s = new HelperTranslator());
    }
    public class dic
    {
        [Indexed(Name = "idx_key", Unique = true)]
        public string key { get; set; }
        public string word { get; set; }

    }
    public class HelperTranslatorMerriam
    {

        private List<dic> _collection = new List<dic>();
        private readonly HttpClient _client;
        private SQLiteConnection _connection;
        public HelperTranslatorMerriam()
        {
            _client = new HttpClient(new HttpClientHandler
            {
                // UseProxy = false,
                UseProxy = true,
                Proxy = new WebProxy("127.0.0.1", 51426)
            });
            _client.Timeout = TimeSpan.FromSeconds(5);
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13B143 Safari/601.1");

            _connection = new SQLiteConnection("datas\\psycho_en.db".GetCommandPath());
            _connection.CreateTable<dic>();

        }

        public IEnumerable<string> GetShortList()
        {
            return _connection.Query<dic>("select key from dic where length(word)<6").Select(i => i.key);
        }
        public IEnumerable<string> ListAllKey()
        {
            return _connection.Query<dic>("select key from dic").Select(i => i.key);
        }
        public void QueryEmpty(System.Threading.SynchronizationContext context, Action<string> action)
        {
            var document = new HtmlAgilityPack.HtmlDocument();

            var dicList = _connection.Query<dic>("select key,word from dic where length(word)=0 order by key");
            var count = 0;

            foreach (var item in dicList)
            {
                //if (item.key == item.word.Trim().ToLower() || item.word.StartsWith("<!--def"))
                //{
                //    _connection.Execute("update dic set word = ? where key = ?", new string[] { "", item.key });


                //}
                //else 

                if (item.word.IsVacuum())
                {
                    var url = "http://youdao.com/w/" + item.key;

                    context.Post((i) =>
                    {
                        action((++count) + " " + item.key);

                    }, null);

                    try
                    {
                        var res = _client.GetAsync(url).GetAwaiter().GetResult();

                        var resStr = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        document.LoadHtml(resStr);
                        if (document.DocumentNode.SelectSingleNode("//*[@class=\"error-wrapper\"]") != null)
                        {
                            continue;
                        }

                        var nodes = document.DocumentNode.SelectNodes("//*[@id=\"phrsListTab\"]/div[@class='trans-container']//li");
                        if (nodes != null)
                        {

                            var str = "";
                            foreach (var node in nodes)
                            {
                                str += node.InnerText.Trim() + "\r\n";
                            }

                            _connection.Execute("update dic set word = ? where key = ?", new object[] { str.Trim(), item.key });
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
            }







        }
        //public void QueryEmpty()
        //{
        //    var diclist = _connection.Query<dic>("select key from dic where word = ?", new object[] { "" }).Select(i => i.key).ToArray();
        //    foreach (var item in diclist)
        //    {

        //        var url = "https://translate.google.cn/translate_a/single?client=gtx&sl=auto&tl="
        //            + "zh" + "&dt=t&dt=bd&ie=UTF-8&oe=UTF-8&dj=1&source=icon&q=" + WebUtility.UrlEncode(item);


        //        var res = _client.GetAsync(url).GetAwaiter().GetResult();

        //        var resStr = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        //        var obj = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(resStr);
        //        JObject array = obj["sentences"][0];

        //        var word = "";
        //        JToken trans;
        //        if (array.TryGetValue("trans", out trans))
        //        {
        //            word += trans.Value<string>() + Environment.NewLine;
        //        }

        //        if (obj.ContainsKey("dict"))
        //        {
        //            JObject dic = obj["dict"][0];
        //            word += string.Join(",", dic.GetValue("terms"));

        //        }
        //        _connection.Execute("update dic set word = ? where key = ?", new object[] { word, item });
        //    }


        //}


        private XmlDocument _xml = new XmlDocument();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="q">Lowercase and trimed</param>
        /// <returns></returns>
        public string QueryChinese(string q)
        {
            var resultStr = string.Empty;

            //var diclist = _connection.Query<dic>("select word from dic where key = ?", new object[] { q });
            //if (diclist.Any())
            //{
            //    resultStr = diclist.First().word;
            //    return resultStr;
            //}
            var url = "http://www.dictionaryapi.com/api/v1/references/learners/xml/" + WebUtility.UrlEncode(q) + "?key=eec2a287-c274-4066-b4c2-3f6cc014d77f";

            var res = _client.GetAsync(url).GetAwaiter().GetResult();

            var resStr = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();



            try
            {
                _xml.LoadXml(resStr);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            var dts = _xml.SelectNodes("//dt");
            foreach (XmlNode item in dts)
            {
                if (item.FirstChild.Value != null)
                    resultStr += item.FirstChild.Value.Trim().Trim(':') + Environment.NewLine;
            }
            resultStr = resultStr.Trim();
            if (resultStr.Length > 1)
            {
                _collection.Add(new dic
            {
                key = q,
                word = resultStr,
            });
            if (_collection.Count() > 100)
            {
                _connection.InsertAll(_collection);
                _collection.Clear();
            }
            }
            return resultStr;
        }

        public void Close()
        {
            if (_collection.Count() > 0)
            {
                _connection.InsertAll(_collection);
                _collection.Clear();
            }
        }
        public static HelperTranslatorMerriam s;

        public static HelperTranslatorMerriam GetInstance() => s ?? (s = new HelperTranslatorMerriam());
    }


    public class HelperTranslatorYoudao
    {
        public IEnumerable<string> ListAllKey()
        {
            return _connection.Query<dic>("select key from dic").Select(i => i.key);
        }
        private List<dic> _collection = new List<dic>();
        private List<string> _collectionDisable = new List<string>();

        private readonly HttpClient _client;
        private SQLiteConnection _connection;

        private HtmlAgilityPack.HtmlDocument _document;

        public HelperTranslatorYoudao()
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="q">Lowercase and trimed</param>
        /// <returns></returns>
        public void ImportEnglish(string q)
        {
            var resultStr = string.Empty;

            //var diclist = _connection.Query<dic>("select word from dic where key = ?", new object[] { q });
            //if (diclist.Any())
            //{
            //    resultStr = diclist.First().word;
            //    return resultStr;
            //}

            var url = "http://youdao.com/w/" + q;



            var res = _client.GetAsync(url).GetAwaiter().GetResult();

            var resStr = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();


            resultStr = FormatExplain(resStr);
            //if (resultStr.IsVacuum())
            //{
            //    resultStr = QueryFromGoogle(q);


            //}
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
        }
        public static HelperTranslatorYoudao s;

        public static HelperTranslatorYoudao GetInstance() => s ?? (s = new HelperTranslatorYoudao());
    }

    public class HelperTranslatorToKindle
    {

        private SQLiteConnection _connection;


        public HelperTranslatorToKindle()
        {


        }


        public void ExportEnglish(string title = "Merriam Advanced Learner Dictionary", string databaseName = "psycho_en.db")
        {
            _connection = new SQLiteConnection(("datas\\" + databaseName).GetCommandPath());
            List<dic> dicList = _connection.Query<dic>("select * from dic");

            var headArray = new string[]{
"<html>",
"<head>",
"<meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\" />",
"<title>",
"",
                "</title>",
"</head>",
"<body topmargin=\"0\" bottommargin=\"0\" leftmargin=\"5\" rightmargin=\"5\">",
"<center>",
"	<hr />",
"	<font size=\"+4\">",
                "",
                "</font>",
"	<hr />",
"</center>",
}
;
            var itemArray = new string[]{
"<mbp:pagebreak />",
"<idx:entry>",
"	<h1>",
"		<idx:orth>",
"",
"</idx:orth>",
"	</h1>",
"",
"</idx:entry>",
};

            var sb = new StringBuilder();

            headArray[4] = title;
            headArray[11] = title;
            sb.Append(string.Join("", headArray));


            foreach (var item in dicList)
            {
                if (item.key != item.word.Trim())
                {
                    itemArray[4] = item.key;
                    var ls = item.word.Trim().Split('\n');
                    itemArray[7] = string.Join("", ls.Where(i => i.IsReadable()).Select((i, index) =>
                     {
                         if (index + 1 < ls.Length)
                             return "<span>" + i.Trim() + " ● " + "</span>";
                         return "<span>" + i.Trim() + "</span>";

                     }));
                }
                sb.Append(string.Join("", itemArray));
            }
            sb.Append("</body></html>");

            "english.htm".GetCommandPath().WriteAllText(sb.ToString());
        }
        public void Close()
        {

        }
    }

}
