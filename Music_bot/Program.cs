using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;
using Newtonsoft.Json;
using System.Security.AccessControl;
using System.Reflection;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Discord.Net.Providers.WS4Net;
using Music_bot.Services;

namespace Music_bot
{
    class Program
    {
        struct MP3
        {
            public string command;
            public string name;
        }

        public struct Collected
        {
            public string name { get; set; }
            public int spellId { get; set; }
            public int creatureId { get; set; }
            public int itemId { get; set; }
            public int qualityId { get; set; }
            public string icon { get; set; }
            public bool isGround { get; set; }
            public bool isFlying { get; set; }
            public bool isAquatic { get; set; }
            public bool isJumping { get; set; }
        }

        public struct Mounts
        {
            public int numCollected { get; set; }
            public int numNotCollected { get; set; }
            public List<Collected> collected { get; set; }
        }

        public struct CharacterInfo
        {
            public long lastModified { get; set; }
            public string name { get; set; }
            public string realm { get; set; }
            public string battlegroup { get; set; }
            public int @class { get; set; }
            public int race { get; set; }
            public int gender { get; set; }
            public int level { get; set; }
            public int achievementPoints { get; set; }
            public string thumbnail { get; set; }
            public string calcClass { get; set; }
            public int faction { get; set; }
            public Mounts mounts { get; set; }
            public int totalHonorableKills { get; set; }
        }

        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        private DiscordSocketClient client;
        private CommandService commands;
        private IServiceProvider service;
        private WebClient webClient = new WebClient();
        private ModuleInfo customModule;
        private Dictionary<string, string> songList = new Dictionary<string, string>();
        private string line;
        
        private List<SocketGuild> guilds;


        public async Task RunBotAsync()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig
            { WebSocketProvider = WS4NetProvider.Instance });
            
            client.Ready += ReadyCheck;
            client.Log += Log;
            client.GuildMemberUpdated += HandleUpdate;
            commands = new CommandService();

            service = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .AddSingleton<ConfigHandler>()
                .BuildServiceProvider();

            string token = "NzA1NzQ5MTkzNjUzNzQ3Nzk0.Xqwk0Q.M2rGSiUpFdeFTFTu2G-PYNul3Bo";

            await service.GetService<ConfigHandler>().PopulateConfig();

            //await GenerateCommands();

            //await AddCommands();

            await RegisterCommandsAsync();

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            await Task.Delay(-1);
        }
        private Task Log(LogMessage arg)
        {

            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), service);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message == null) return;
            int argPos = 0;
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))) return;
            var context = new SocketCommandContext(client, message);
            var result = await commands.ExecuteAsync(context, argPos, service);
            if (!result.IsSuccess)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        //private async Task AddCommands()
        //{
        //    try
        //    {
        //        customModule = await commands.CreateModuleAsync("", module =>
        //        {
        //            foreach (string entry in songList.Keys)
        //            {
        //                module.AddCommand(entry, SendAuto, cmd =>
        //                {
        //                    cmd.RunMode = RunMode.Async;
        //                });
        //            }

        //            module.AddCommand("add", AddSong, cmd =>
        //            {
        //                cmd.AddParameter<string>("command", para =>
        //                {
        //                    para.IsOptional = false;
        //                });
        //            });
        //            module.Name = "Custom Module";
        //        });
        //    }
        //    catch
        //    {
        //        Console.WriteLine("DONE LOADING CUSTOM COMMANDS");
        //    }
        //}

        //private async Task SendAuto(ICommandContext context, object[] parameters, IServiceProvider service, CommandInfo info)
        //{
        //    SocketCommandContext Context = context as SocketCommandContext;
        //    var audio = await service.GetService<Services.MusicService>().ConnectAudio(Context);
        //    await service.GetService<Services.MusicService>().SendAsync(audio, songList[Context.Message.ToString().Substring(1)]);
        //}

        private async Task ReadyCheck()
        {
            guilds = client.Guilds.ToList();
            foreach (SocketGuild serv in client.Guilds)
            {
                //await serv.DefaultChannel.SendMessageAsync("Found " + songList.Count + " custom songs!");
            }
        }

        private async Task HandleUpdate(SocketGuildUser before, SocketGuildUser after)
        {
            Console.WriteLine("USER: " + after.Username + " ACTIVITY: " + after.Activity.Name);
          
               
        }

        //public async Task SendRandom(SocketCommandContext context)
        //{
        //    SocketCommandContext Context = context as SocketCommandContext;
        //    var audio = await service.GetService<MusicService>().ConnectAudio(Context);
        //    Random rand = new Random();
        //    await service.GetService<MusicService>().SendAsync(audio, songList.ElementAt(rand.Next(0, songList.Count)).Value);
        //}

        //public CommandService GetCommands()
        //{
        //    return commands;
        //}

        //public CharacterInfo GetInfoObject(string rawResponse)
        //{
        //    return JsonConvert.DeserializeObject<CharacterInfo>(rawResponse);
        //}

        //private async Task AddSong(ICommandContext context, object[] parameters, IServiceProvider services, CommandInfo info)
        //{
        //    if (context.Message.Attachments.Count != 1)
        //    {
        //        await context.Channel.SendMessageAsync("Number of files incorrect");
        //        return;
        //    }

        //    IAttachment att = context.Message.Attachments.First();
        //    if (att.Filename.EndsWith(".mp3"))
        //    {
        //        string filePath = Path.Combine(services.GetService<ConfigHandler>().GetSongDir(), att.Filename).Replace(@"\", @"\\");
        //        webClient.DownloadFile(att.Url, filePath);
        //        MP3 mp3 = new MP3
        //        {
        //            command = (string)parameters[0],
        //            name = filePath
        //        };
        //        try
        //        {
        //            songList.Add(mp3.command, mp3.name);
        //            using (StreamWriter sw = File.AppendText(services.GetService<ConfigHandler>().GetSongConf()))
        //            {
        //                sw.WriteLine(JsonConvert.SerializeObject(mp3));
        //            }
        //            await commands.RemoveModuleAsync(customModule);
        //            await AddCommands();
        //        }
        //        catch (ArgumentException ex)
        //        {
        //            await context.Channel.SendMessageAsync(ex.Message);
        //            return;
        //        }
        //    }
        //}

        //private async Task GenerateCommands()
        //{
        //    try
        //    {
        //        MP3 song;

        //        if (!File.Exists(service.GetService<ConfigHandler>().GetSongConf()))
        //        {
        //            using (var f = File.Create(service.GetService<ConfigHandler>().GetSongConf()))
        //            {
        //                DirectoryInfo dInfo = new DirectoryInfo(service.GetService<ConfigHandler>().GetSongConf());
        //                DirectorySecurity dSecurity = dInfo.GetAccessControl();
        //                dSecurity.AddAccessRule(new FileSystemAccessRule("everyone", FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
        //                dInfo.SetAccessControl(dSecurity);
        //            }
        //        }

        //        using (StreamReader reader = new StreamReader(service.GetService<ConfigHandler>().GetSongConf()))
        //        {
        //            while ((line = reader.ReadLine()) != null)
        //            {
        //                song = JsonConvert.DeserializeObject<MP3>(line);
        //                songList.Add(song.command, song.name);
        //            }
        //        }
        //        await Task.CompletedTask;
        //    }
        //    catch
        //    {
        //        Console.WriteLine("bugs");
        //    }
        //}
    }
}