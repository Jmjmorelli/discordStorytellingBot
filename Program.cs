using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.IO;

namespace discordStorytellingBot
{
    class Program
    {
        private DiscordSocketClient _client;
        private string token;

        static async Task Main(string[] args)
        {
            await new Program().RunBotAsync();
        }

        public Program()
        {
            // Initialize the token and client in the constructor
            token = File.ReadAllText(".env").Trim();
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
            };
            _client = new DiscordSocketClient(config);

        }

        public async Task RunBotAsync()
        {
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
            // Log the message for debugging
            // Console.WriteLine($"Received message from {message.Author.Username}: {message.Content}");

            // Ignore bot messages
            if (message.Author.IsBot) return;

            // Respond to any message (for debugging)
            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                await message.Channel.SendMessageAsync("Hello! I received your message.");
            }

            // Respond to "!ping"
            if (message.Content == "!ping")
            {
                await message.Channel.SendMessageAsync("Pong!");
            }
        }


    }
}
