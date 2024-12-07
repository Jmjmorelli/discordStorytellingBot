using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.IO;

string token = File.ReadAllText(".env").Trim();


namespace discordStorytellingBot
{
    class Program
    {
        private required DiscordSocketClient _client;

        static async Task Main(string[] args)
        {
            await new Program().RunBotAsync();
        }
        public async Task RunBotAsync()
        {
            // Define the configuration for the DiscordSocketClient
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages
            };

            // Pass the configuration when creating the client
            _client = new DiscordSocketClient(config);

            // Log client activities
            _client.Log += Log;

            // Log in and start the bot
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Add event handlers
            _client.MessageReceived += HandleMessageReceived;

            // Keep the bot running
            await Task.Delay(-1);
        }


        private Task Log(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private async Task HandleMessageReceived(SocketMessage message)
        {
            if (message.Author.IsBot) return;

            if (message.Content == "!ping")
            {
                await message.Channel.SendMessageAsync("Pong!");
            }
        }
    }
}
