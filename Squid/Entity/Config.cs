using System.IO;
using Newtonsoft.Json;

namespace Squid.Entity
{
    internal class Config
    {
        [JsonProperty("token")] internal string Token = "Token";
        [JsonProperty("prefix")] internal string Prefix = "--";

        public static Config LoadConfig(string path)
        {
            using (var sr = new StreamReader(path))
            {
                return JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
            }
        }

        public void SaveConfig(string path)
        {
            using (var sw = new StreamWriter(path))
            {
                sw.Write(JsonConvert.SerializeObject(this));
            }
        }
    }
}