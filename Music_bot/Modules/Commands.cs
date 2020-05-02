using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Audio;
using Music_bot.Services;

namespace Music_bot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        public MusicService music { get; set; } = new MusicService(new ConfigHandler());
        public ConfigHandler config { get; set; } = new ConfigHandler();

        [Command("Настя")]
        public async Task Nastya([Remainder]string text)
        {
            await ReplyAsync(text);
        }
        [Command("Play", true)]
        public async Task Play([Remainder]string url)
        {
            SocketVoiceChannel channel = GetChannel(this.Context.Message.Author);
            await channel.ConnectAsync(external: true);
            var audioClient = await music.ConnectAudio(Context);
            if (audioClient == null)
                return;
            await music.Stream(audioClient, url.Split('&')[0]);
            //Discord.Audio.IAudioClient client = channel.ConnectAsync(external: true).GetAwaiter().GetResult();
            //Process _currentProcess = CreateStream(@"E:\Music\Kalimba.mp3", 1);
            //var output = _currentProcess.StandardOutput.BaseStream;
            //AudioOutStream discord= client.CreatePCMStream(Discord.Audio.AudioApplication.Music,96*1024);
            //await output.CopyToAsync(discord);
            //await discord.FlushAsync();
            //_currentProcess.WaitForExit();
            //await ReplyAsync("Hello");
            ////  if(this.Context.Client.is_connected())
        }
        /* _currentProcess = CreateStream(path, speedModifier);
            var output = _currentProcess.StandardOutput.BaseStream;
            var discord = client.CreatePCMStream(AudioApplication.Music, 96 * 1024);
            await output.CopyToAsync(discord);
            await discord.FlushAsync();
            _currentProcess.WaitForExit();
            Log.Information($"ffmpeg exited with code {_currentProcess.ExitCode}");


             */

        private Process CreateStream(string path, int speedModifier)
        {
            var ffmpeg = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{path}\" -ac 2 -f s16le -filter:a \"volume=0.02\" -ar {speedModifier}000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            return Process.Start(ffmpeg);
        }

        private SocketVoiceChannel GetChannel(SocketUser user)
        {
            foreach (SocketVoiceChannel channel in this.Context.Guild.VoiceChannels)
            {
                if (channel.Users.Contains(user))
                {
                    return channel;
                }
            }
            return null;
        }
        [Command("Leave")]
        public async Task Leave()
        {
            SocketVoiceChannel channel = GetChannel(this.Context.User);
            await channel.DisconnectAsync();     //ConnectAsync(external: true);
        }
        private string Good()
        {
            return "Hello";
        }
    }

}
