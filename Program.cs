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

        private string openAiApiKey;

        public Program()
        {
            // Initialize the token and client in the constructor

            var lines1 = File.ReadAllLines(".env");
            foreach (var line1 in lines1)
            {
                if (line1.StartsWith("DISCORD_TOKEN="))
                {
                    token = line1.Substring("DISCORD_TOKEN=".Length).Trim();
                    break;
                }
            }

            var lines2 = File.ReadAllLines(".env");
            foreach (var line2 in lines2)
            {
                if (line2.StartsWith("OPENAI_API_KEY="))
                {
                    openAiApiKey = line2.Substring("OPENAI_API_KEY=".Length).Trim();
                    break;
                }
            }



            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
            };
            _client = new DiscordSocketClient(config);
        }

        private async Task<string> GenerateStoryAsync(string prompt)
        {
            var apiUrl = "https://api.openai.com/v1/chat/completions";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiApiKey}");

                var requestBody = new
                {
                    model = "gpt-3.5-turbo", // Switch to GPT-3.5
                    messages = new[]
                    {
                new { role = "system", content = "You are a creative RPG storyteller." },
                new { role = "user", content = prompt }
            },
                    max_tokens = 300, // Reduce token usage
                    temperature = 0.8
                };

                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"OpenAI API error: {response.StatusCode} {responseContent}");
                    throw new Exception("Failed to generate story from GPT.");
                }

                var jsonResponse = System.Text.Json.JsonDocument.Parse(responseContent);
                var story = jsonResponse.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

                return story.Trim();
            }
        }


        private async Task HandleStoryCommand(SocketMessage message)
        {
            if (message.Author.IsBot) return;

            // Define the initial story prompt
            var prompt = "Begin a fantasy RPG story. Provide a scene description and three choices for the player to continue.";

            try
            {
                // Generate the story
                var story = await GenerateStoryAsync(prompt);

                // Send the story to the channel
                await message.Channel.SendMessageAsync(story);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating story: {ex.Message}");
                await message.Channel.SendMessageAsync("Sorry, I couldn't generate the story. Please try again later.");
            }
        }



        public async Task RunBotAsync()
        {
            // Log client activities
            _client.Log += Log;

            // Add event handlers
            _client.MessageReceived += HandleMessageReceived;
            _client.JoinedGuild += HandleJoinedGuild;

            // Log in and start the bot
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

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
            // Console.WriteLine($"Received message from {message.Author.Username}: {message.Content}");
            // if (message.Author.IsBot) return;

            if (message.Content == "$help")
            {
                string welcomeMsg = @$"List of commands beginning with $ -- more to be added.\n
                $story - this begins a session
                ";
                await message.Channel.SendMessageAsync(welcomeMsg);
            }
            else if (message.Content == "$story")
            {
                await HandleStoryCommand(message);
            }
        }

        private async Task HandleJoinedGuild(SocketGuild guild)
        {
            Console.WriteLine($"Joined a new guild: {guild.Name}");

            // Find the first non-voice-linked text channel where the bot has permission to send messages
            var channel = guild.TextChannels
                .Where(c => c.Category == null || !c.Category.Name.ToLower().Contains("voice")) // Exclude channels in a "voice" category
                .Where(c => c.IsNsfw == false) // Exclude NSFW channels
                .FirstOrDefault(c => c.GetUser(guild.CurrentUser.Id).GetPermissions(c).SendMessages);

            if (channel != null)
            {
                Console.WriteLine($"Found channel: {channel.Name}");
                await channel.SendMessageAsync($"Hello {guild.Name}!\nI'm your new RPG bot. I am here to provide you with a new exciting adventure!\n\nType `$help` to see what I can do.");
                await channel.SendMessageAsync($"Will this print?");
            }
            else
            {
                Console.WriteLine("No suitable channel found to send a welcome message.");
            }
        }

    }
}
