using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Squid.Entity;

namespace Squid
{
    public class Squid
    {
        internal DiscordSocketClient _client;
        internal Config _config;
        internal List<Guild> _guilds = new List<Guild>();
        internal readonly string _twitchApi = "https://api.twitch.tv/helix/";

        public Squid()
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            WriteCenter("                                  88          88  ", 4);
            WriteCenter("                                  \"\"          88  ");
            WriteCenter("                                              88  ");
            WriteCenter(",adPPYba,  ,adPPYb,d8 88       88 88  ,adPPYb,88  ");
            WriteCenter("I8[    \"\" a8\"    `Y88 88       88 88 a8\"    `Y88  ");
            WriteCenter(" `\"Y8ba,  8b       88 88       88 88 8b       88  ");
            WriteCenter("aa    ]8I \"8a    ,d88 \"8a,   ,a88 88 \"8a,   ,d88  ");
            WriteCenter("`\"YbbdP\"\'  `\"YbbdP\'88  `\"YbbdP\'Y8 88  `\"8bbdP\"Y8  ");
            WriteCenter("                   88                             ");
            WriteCenter("                   88                             ");
            Console.WriteLine("\n\n\n");
            Console.ResetColor();


            //Config
            if (!File.Exists("config.json"))
            {
                new Config().SaveConfig("config.json");
                WriteCenter("Error - No config found. Please fill out the 'config.json' that has been generated.", 2);
                System.Threading.Thread.Sleep(3000);
                Environment.Exit(0);
            }
            _config = Config.LoadConfig("config.json");
        }


        public async Task StartAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Debug
            });

            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.StartAsync();

            _client.GuildMemberUpdated += GuildMemberUpdated;
            _client.JoinedGuild += JoinedGuild;

            _client.Ready += () =>
            {
                LoadGuilds();
                CheckGuildMismatch();

                Log(new LogMessage(LogSeverity.Info, "Squid", $"Logged in as {_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator}." +
                                                              $"\nServing {_client.Guilds.Count} guilds with a total of {_client.Guilds.Sum(guild => guild.Users.Count)} online users."));
                

                




                return Task.CompletedTask;
            };

            await Task.Delay(-1);
        }

        private void LoadGuilds()
        {
            if (!File.Exists("guilds.json"))
            {
                using (var sw = new StreamWriter("guilds.json"))
                {
                    sw.Write("{}");
                }
                WriteCenter("No guilds found. Saving empty one and populating with client guilds if applicable.\n", 2);
            }
            else
            {
                try
                {
                    _guilds = JsonConvert.DeserializeObject<List<Guild>>(File.ReadAllText("guilds.json"));
                    Log(new LogMessage(LogSeverity.Info, "squid",
                        $"Successfully loaded {_guilds.Count} guilds from storage."));
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    WriteCenter($"Malformed or empty guilds.json. Adding from client.\n", 2);
                }
                
            }

            if (_client.Guilds.Count > 0)
            {
                foreach (var guild in _client.Guilds)
                {
                    Guild g = new Guild
                    {
                        Id = guild.Id,
                        Prefix = "--",
                        LiveroleId = 0,
                        TrackedGames = new List<string> {"Factorio"}
                    };
                    _guilds.Add(g);
                }
            }

            using (var sw = new StreamWriter("guilds.json"))
            {
                sw.Write(JsonConvert.SerializeObject(_guilds));
            }
        }

        private async Task JoinedGuild(SocketGuild guild)
        {
            for (int i = 0; i < _guilds.Count; i++)
            {
                if (_guilds[i].Id == guild.Id)
                {
                    var newGuild = new Guild
                    {
                        Id = guild.Id,
                        LiveroleId = 0,
                        Prefix = "--",
                        TrackedGames = new List<string> { "Factorio" }
                    };
                    _guilds.RemoveAt(i);
                    _guilds.Add(newGuild);

                    using (var sw = new StreamWriter("guilds.json"))
                    {
                        sw.Write(JsonConvert.SerializeObject(_guilds));
                    }

                    await Log(new LogMessage(LogSeverity.Info, "squid",
                        $"Joined new guild {guild.Name} that was already stored. Updating with default values."));
                    return;
                }
            }
            await Log(new LogMessage(LogSeverity.Info, "squid",
                $"Joined new guild {guild.Name}. Saving with default values."));

            var newGuildNotStored = new Guild
            {
                Id = guild.Id,
                LiveroleId = 0,
                Prefix = "--",
                TrackedGames = new List<string> { "Factorio" }
            };

            _guilds.Add(newGuildNotStored);

            using (var sw = new StreamWriter("guilds.json"))
            {
                sw.Write(JsonConvert.SerializeObject(_guilds));
            }
        }

        private void CheckGuildMismatch()
        {
            Log(new LogMessage(LogSeverity.Info, "squid", "Checking for mismatch between guilds."));

            //missing guilds from storage
            foreach (var cGuild in _client.Guilds)
            {
                bool missing = true;
                foreach (var x in _guilds)
                {
                    if (x.Id == cGuild.Id)
                    {
                        missing = false;
                        break;
                    }
                }

                if (missing)
                {
                    var newGuild = new Guild
                    {
                        Id = cGuild.Id,
                        LiveroleId = 0,
                        Prefix = "--",
                        TrackedGames = new List<string> { "Factorio" }
                    };

                    _guilds.Add(newGuild);

                    using (var sw = new StreamWriter("guilds.json"))
                    {
                        sw.Write(JsonConvert.SerializeObject(_guilds));
                    }

                    Log(new LogMessage(LogSeverity.Info, "squid", $"Found missing guild {cGuild.Name}. Adding to storage."));
                }
            }

            //extra guilds in storage
            for (int i = 0; i < _guilds.Count; i++)
            {
                bool extra = true;
                foreach (var cGuild in _client.Guilds)
                {
                    if (_guilds[i].Id == cGuild.Id)
                    {
                        extra = false;
                    }
                }

                if (extra)
                {
                    _guilds.RemoveAt(i);
                    using (var sw = new StreamWriter("guilds.json"))
                    {
                        sw.Write(JsonConvert.SerializeObject(_guilds));
                    }
                    Log(new LogMessage(LogSeverity.Info, "squid", $"Found missing guild {_guilds[i].Id}. Deleting from storage."));
                }
            }
        }

        private async Task GuildMemberUpdated(SocketUser oldSocketUser, SocketUser newSocketUser)
        {
            if (newSocketUser.Activity != null && (newSocketUser.Activity.Type == ActivityType.Streaming))
            {
                var streaming = newSocketUser.Activity as StreamingGame;
                var twitchName = streaming?.Url.Substring(streaming.Url.LastIndexOf('/') + 1);

                try
                {
                    var streamReq = (HttpWebRequest)WebRequest.Create(_twitchApi + "streams/" + "?user_login=" + twitchName);
                    streamReq.Headers["client-id"] = _config.TwitchClientId;

                    using (HttpWebResponse streamResp = (HttpWebResponse) streamReq.GetResponse())
                    using (Stream streamStream = streamResp.GetResponseStream())
                    using (StreamReader streamSR = new StreamReader(streamStream ?? throw new InvalidOperationException()))
                    {
                        var streamRespJson = JsonConvert.DeserializeObject<dynamic>(streamSR.ReadToEnd());

                        //check if game being streamed is on the list

                        int gameId = streamRespJson.data[0].game_id;

                        var gameReq = (HttpWebRequest) WebRequest.Create(_twitchApi + "games/" + "?id=" + gameId);
                        gameReq.Headers["client-id"] = _config.TwitchClientId;

                        using (HttpWebResponse gameResp = (HttpWebResponse) gameReq.GetResponse())
                        using (Stream gameStream = gameResp.GetResponseStream())
                        using (StreamReader gameSR = new StreamReader(gameStream ?? throw new InvalidOperationException()))
                        {
                            var gameRespJson = JsonConvert.DeserializeObject<dynamic>(gameSR.ReadToEnd());

                            string gameName = gameRespJson.data[0].name.ToString();

                            var socketGuildUser = (SocketGuildUser) newSocketUser;
                            var guild = _guilds.FirstOrDefault(x => x.Id == socketGuildUser.Id);

                            if (guild != null)
                            {
                                if (guild.TrackedGames.Any(x => x == gameName))
                                {
                                    var role = socketGuildUser.Guild.GetRole(guild.LiveroleId);

                                    if (role != null)
                                    {
                                        await socketGuildUser.AddRoleAsync(role);
                                    }
                                    else
                                    {
                                        await Log(new LogMessage(LogSeverity.Critical, "squid",
                                            $"Role id {guild.LiveroleId} does not exist in guild {guild.Id}"));
                                    }
                                }
                            }
                            else
                            {
                                await Log(new LogMessage(LogSeverity.Critical, "squid",
                                    $"Guild was null!"));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    await Log(new LogMessage(LogSeverity.Critical, "squid", e.ToString()));
                    return;
                }
            }
        }

        private static bool IsModerator(SocketGuildUser user)
        {
            return user.Roles.Any(role => role.Name.ToLower().Contains("mod") || role.Name.ToLower().Contains("moderator") ||
                                          role.Name.ToLower().Contains("admin") || role.Name.ToLower().Contains("owner") ||
                                          role.Name.ToLower().Contains("bot commander") || role.Name.ToLower().Contains("squid")) || 
                                          user.Roles.Any(role => role.Permissions.Administrator || role.Permissions.BanMembers || 
                                          role.Permissions.ManageChannels ||role.Permissions.ManageGuild || role.Permissions.ManageRoles || role.Permissions.ManageMessages);
        }

        private static bool ClientCanSeeChannel(SocketGuildChannel channel, SocketGuild guild)
        {
            return guild.CurrentUser.GetPermissions(channel).SendMessages;
        }

        private static void WriteCenter(string value, int skipline = 0)
        {
            //Taken from https://github.com/DSharpPlus/DSharpPlus-Example/blob/master/DiscordBot/DiscordBot/Bot.cs
            for (int i = 0; i < skipline; i++)
                Console.WriteLine();

            Console.SetCursorPosition((Console.WindowWidth - value.Length) / 2, Console.CursorTop);
            Console.WriteLine(value);
        }

        private static Task Log(LogMessage logmsg)
        {
            switch (logmsg.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now} [{logmsg.Severity,8}] {logmsg.Source}: {logmsg.Message} {logmsg.Exception}");
            Console.ResetColor();
            return Task.CompletedTask;
        }
    }
}