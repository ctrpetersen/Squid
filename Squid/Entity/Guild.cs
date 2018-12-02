using System.Collections.Generic;
using Newtonsoft.Json;

namespace Squid.Entity
{
    internal class Guild
    {
        [JsonProperty("id")] internal ulong Id = 0;
        [JsonProperty("prefix")] internal string Prefix = "--";
        [JsonProperty("tracked_games")] internal List<string> TrackedGames = new List<string>();
        [JsonProperty("liverole_id")] internal ulong LiveroleId = 0;
    }
}