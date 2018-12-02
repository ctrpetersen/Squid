using System.IO;
using Newtonsoft.Json;

namespace Squid.Entity
{
    internal class Config
    {
        [JsonProperty("token")] internal string Token = "Token";
        [JsonProperty("prefix")] internal string Prefix = "--"; // Default prefix. Guild prefix will take priority if set.
        [JsonProperty("twitch_client_id")] internal string TwitchClientId = "Twitch client id";

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