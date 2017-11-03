using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

using Renci.SshNet;
namespace Shared
{
    public static class HelperSSH
    {

        public static string ExecuteCommand(string value)
        {
            var ip = "180.76.170.157";
            var usrename = "root";
            var password = "P8DNKCDT8zP1miNOC%";
            using (var ssh = new SshClient(ip, usrename, password))
            {
                ssh.Connect();
                return ssh.CreateCommand(value).Execute();

            }
        }
        public static void UploadFiles(IEnumerable<string> files, string dstPath)
        {
            var ip = "180.76.170.157";
            var usrename = "root";
            var password = "P8DNKCDT8zP1miNOC%";
            using (var ssh = new  SftpClient(ip, usrename, password))
            {
                ssh.Connect();

                foreach (var item in files)
                {
                    var stream = item.Open(System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    var dst = dstPath+"/"+item.GetFileName();
                    ssh.UploadFile(stream, dst);
                    stream.Dispose();
                }
            }
        }
    }
    public class HelperWebServer
    {
        private readonly HttpClient _client;
         // private string _baseURL = "http://localhost:18081";
        private string _baseURL = "https://www.everstore.cn/";

        public HelperWebServer()
        {
            _client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                UseProxy = false,
            });
            _client.BaseAddress = new Uri(_baseURL);
            _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13B143 Safari/601.1");

        }


        public string ImportAll(string filePath)
        {
            var res = _client.PostAsync("psycho/importAll", new StringContent(filePath.ReadAllText(), new UTF8Encoding(false), "application/json")).GetAwaiter().GetResult();

            return res.Content.ReadAsStringAsync().Result;
        }
        public string GetToken(string password)
        {
            Dictionary<string, string> obj = new Dictionary<string, string>();
            obj.Add("password", password);

            var value = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            var res = _client.PostAsync("psycho/token", new StringContent(value, new UTF8Encoding(false), "application/json")).GetAwaiter().GetResult();

            return string.Join(";", res.Headers.GetValues("Authorization"));

        }

        public string Insert(Dictionary<string, dynamic> obj)
        {
            var res = _client.PostAsync("psycho/insert", new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(obj), new UTF8Encoding(false), "application/json")).GetAwaiter().GetResult();

            return res.Content.ReadAsStringAsync().Result;
        }

        public string Insert(string token, Dictionary<string, dynamic> obj)
        {
            if (_client.DefaultRequestHeaders.Contains("Authorization"))
            {
                _client.DefaultRequestHeaders.Remove("Authorization");
            }

            _client.DefaultRequestHeaders.Add("Authorization", token);
            var res = _client.PostAsync("psycho/insert", new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(obj), new UTF8Encoding(false), "application/json")).GetAwaiter().GetResult();

            return res.Content.ReadAsStringAsync().Result;
        }
        public string Delete(string token, Dictionary<string, dynamic> obj)
        {
            if (_client.DefaultRequestHeaders.Contains("Authorization"))
            {
                _client.DefaultRequestHeaders.Remove("Authorization");
            }

            _client.DefaultRequestHeaders.Add("Authorization", token);
            var res = _client.PostAsync("psycho/delete", new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(obj), new UTF8Encoding(false), "application/json")).GetAwaiter().GetResult();

            return res.Content.ReadAsStringAsync().Result;
        }
        public string Update(string token, Dictionary<string, dynamic> obj)
        {
            if (_client.DefaultRequestHeaders.Contains("Authorization"))
            {
                _client.DefaultRequestHeaders.Remove("Authorization");
            }

            _client.DefaultRequestHeaders.Add("Authorization", token);
            var res = _client.PostAsync("psycho/update", new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(obj), new UTF8Encoding(false), "application/json")).GetAwaiter().GetResult();

            return res.Content.ReadAsStringAsync().Result;
        }
        public string Update(Dictionary<string, dynamic> obj)
        {
           
            var res = _client.PostAsync("psycho/update", new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(obj), new UTF8Encoding(false), "application/json")).GetAwaiter().GetResult();

            return res.Content.ReadAsStringAsync().Result;
        }
        private static HelperWebServer sHelperWebServer;
        public static HelperWebServer GetInstance()
        {
            //if (baseAddress.IsReadable() && sHelperWebServer == null)
            //{
            //    sHelperWebServer = new HelperWebServer(baseAddress);
            //}
            //else if (baseAddress.IsReadable() && sHelperWebServer != null)
            //{
            //    sHelperWebServer._client.BaseAddress = new Uri(baseAddress);
            //}
            if (sHelperWebServer == null)
            {
                sHelperWebServer = new HelperWebServer();
            }
            return sHelperWebServer;
        }
    }
}
