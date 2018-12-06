using System.Collections.Generic;
using Newtonsoft.Json;

namespace Squid.Entity
{
    public class Guild
    {
        [JsonProperty("id")] internal ulong Id { get; set; }
        [JsonProperty("prefix")] internal string Prefix { get; set; }
        [JsonProperty("tracked_games")] internal List<string> TrackedGames { get; set; }
        [JsonProperty("liverole_id")] internal ulong LiveroleId { get; set; }
    }
}