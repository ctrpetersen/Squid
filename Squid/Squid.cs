using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft;
using Squid.Entity;


namespace Squid
{
    public class Squid
    {
        private DiscordSocketClient _client;
        private Config _config;

        public Squid()
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            WriteCenter("                                  88          88  ");
            WriteCenter("                                  \"\"          88  ");
            WriteCenter("                                              88  ");
            WriteCenter(",adPPYba,  ,adPPYb,d8 88       88 88  ,adPPYb,88  ");
            WriteCenter("I8[    \"\" a8\"    `Y88 88       88 88 a8\"    `Y88  ");
            WriteCenter(" `\"Y8ba,  8b       88 88       88 88 8b       88  ");
            WriteCenter("aa    ]8I \"8a    ,d88 \"8a,   ,a88 88 \"8a,   ,d88  ");
            WriteCenter("`\"YbbdP\"\'  `\"YbbdP\'88  `\"YbbdP\'Y8 88  `\"8bbdP\"Y8  ");
            WriteCenter("                   88                             ");
            WriteCenter("                   88                             ");
            Console.ResetColor();
            if (!File.Exists("config.json"))
            {
                new Config().SaveConfig("config.json");
                WriteCenter("Error - No config found. Please fill out the 'config.json' that has been generated.", 2);
                System.Threading.Thread.Sleep(4000);
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

            await Task.Delay(-1);
        }


        
        internal void WriteCenter(string value, int skipline = 0)
        {
            //Taken from https://github.com/DSharpPlus/DSharpPlus-Example/blob/master/DiscordBot/DiscordBot/Bot.cs
            for (int i = 0; i < skipline; i++)
                Console.WriteLine();

            Console.SetCursorPosition((Console.WindowWidth - value.Length) / 2, Console.CursorTop);
            Console.WriteLine(value);
        }
    }
}