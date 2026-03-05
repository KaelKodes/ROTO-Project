using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Discord.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Discord.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace DiscordBot
{
    class Program
    {
        private DiscordSocketClient _client;
        private string _botToken;
        private ulong _guildId;
        private string _roadmapUrl;
        private readonly HttpClient _httpClient = new HttpClient();

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            LoadConfig();
            if (string.IsNullOrEmpty(_botToken) || _botToken == "YOUR_BOT_TOKEN_HERE")
            {
                Console.WriteLine("[DiscordBot] Token not set in discord_config.json. Bot will not start.");
                return;
            }

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds
            });

            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.SlashCommandExecuted += SlashCommandHandler;

            await _client.LoginAsync(TokenType.Bot, _botToken);
            await _client.StartAsync();

            // Keep the app running
            await Task.Delay(-1);
        }

        private void LoadConfig()
        {
            try
            {
                // Primary: Local directory (for standalone deployment)
                string configPath = Path.Combine(Directory.GetCurrentDirectory(), "discord_config.json");

                if (!File.Exists(configPath))
                {
                    // Fallback: App context (for IDE/dev execution)
                    configPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../discord_config.json"));
                }

                if (!File.Exists(configPath))
                {
                    // Fallback 2: Project root context
                    configPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../discord_config.json"));
                }

                if (!File.Exists(configPath))
                {
                    Console.WriteLine($"[DiscordBot] Config not found. Looked in multiple locations including {configPath}");
                    return;
                }

                Console.WriteLine($"[DiscordBot] Loading config from {configPath}");
                string json = File.ReadAllText(configPath);
                var config = JObject.Parse(json);
                _botToken = config["bot_token"]?.ToString();

                string guildIdStr = config["guild_id"]?.ToString();
                if (ulong.TryParse(guildIdStr, out ulong gid)) _guildId = gid;

                _roadmapUrl = config["roadmap_url"]?.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[DiscordBot] Failed to load config: {e.Message}");
            }
        }

        private Task LogAsync(LogMessage message)
        {
            Console.WriteLine($"[DiscordBot] {message.ToString()}");
            return Task.CompletedTask;
        }

        private async Task ReadyAsync()
        {
            Console.WriteLine("[DiscordBot] Bot is connected and ready.");

            // Register /roadmap command
            Console.WriteLine($"[DiscordBot] Connected to {_client.Guilds.Count} guilds.");
            foreach (var g in _client.Guilds)
            {
                Console.WriteLine($" - {g.Name} (ID: {g.Id})");
            }

            var roadmapCommand = new SlashCommandBuilder()
                .WithName("roadmap")
                .WithDescription("View the current Project Ambition Kickstarter Roadmap");

            try
            {
                // 1. Try Guild Registration (Instant)
                var guild = _client.GetGuild(_guildId);
                if (guild != null)
                {
                    await guild.CreateApplicationCommandAsync(roadmapCommand.Build());
                    Console.WriteLine($"[DiscordBot] Registered /roadmap command to guild {_guildId} (Instant).");
                }
                else
                {
                    Console.WriteLine($"[DiscordBot] Guild {_guildId} not found in cache. This is normal if the bot just started.");
                }

                // 2. Try Global Registration (Backup - takes up to 1 hour to propagate)
                await _client.CreateGlobalApplicationCommandAsync(roadmapCommand.Build());
                Console.WriteLine("[DiscordBot] Registered /roadmap command GLOBALLY (propagating...).");
            }
            catch (HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine($"[DiscordBot] Command registration error: {json}");
            }
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            if (command.Data.Name == "roadmap")
            {
                await command.DeferAsync();
                try
                {
                    var embeds = await GetRoadmapEmbedsAsync();
                    await command.FollowupAsync(embeds: embeds.ToArray());
                }
                catch (Exception e)
                {
                    await command.FollowupAsync($"Error retrieving roadmap: {e.Message}");
                }
            }
        }

        private async Task<List<Embed>> GetRoadmapEmbedsAsync()
        {
            string content = "";

            // Primary: Fetch from GitHub if URL is provided (Live sync!)
            if (!string.IsNullOrEmpty(_roadmapUrl))
            {
                try
                {
                    Console.WriteLine($"[DiscordBot] Fetching latest roadmap from GitHub: {_roadmapUrl}");
                    content = await _httpClient.GetStringAsync(_roadmapUrl);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[DiscordBot] Failed to sync from GitHub: {e.Message}. Falling back to local file.");
                }
            }

            if (string.IsNullOrEmpty(content))
            {
                // Fallback: Local directory
                string roadmapPath = Path.Combine(Directory.GetCurrentDirectory(), "roadmap.md");

                // Fallback 1: Project root relative to bin
                if (!File.Exists(roadmapPath))
                {
                    roadmapPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../roadmap.md"));
                }

                // Fallback 2: Local directory
                if (!File.Exists(roadmapPath))
                {
                    roadmapPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../roadmap.md"));
                }

                if (File.Exists(roadmapPath))
                {
                    content = File.ReadAllText(roadmapPath);
                }
            }

            if (string.IsNullOrEmpty(content))
            {
                throw new FileNotFoundException($"Roadmap content could not be found via URL or local file.");
            }

            var sections = ParseRoadmap(content);
            var embeds = new List<Embed>();

            foreach (var section in sections)
            {
                var builder = new EmbedBuilder()
                    .WithTitle(section.Title)
                    .WithDescription(section.Description)
                    .WithColor(section.Color);

                if (!string.IsNullOrEmpty(section.Footer))
                {
                    builder.WithFooter(section.Footer);
                }

                embeds.Add(builder.Build());
            }

            return embeds;
        }

        private List<RoadmapSection> ParseRoadmap(string md)
        {
            var sections = new List<RoadmapSection>();
            var lines = md.Split('\n');

            RoadmapSection current = null;
            string descBuffer = "";

            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("## "))
                {
                    if (current != null)
                    {
                        current.Description = descBuffer.Trim();
                        sections.Add(current);
                    }

                    string title = trimmed.Substring(3);
                    current = new RoadmapSection { Title = title, Color = Color.Blue };
                    descBuffer = "";

                    if (title.Contains("Already Built")) current.Color = Color.Green;
                    else if (title.Contains("M1")) current.Color = Color.Blue;
                    else if (title.Contains("M2")) current.Color = Color.Orange;
                    else if (title.Contains("M3")) current.Color = new Color(0x9B, 0x59, 0xB6); // Purple
                    else if (title.Contains("M4") || title.Contains("M5")) current.Color = Color.Red;
                }
                else if (trimmed.StartsWith("### ") && current != null)
                {
                    descBuffer += $"\n**{trimmed.Substring(4)}**\n";
                }
                else if (trimmed.StartsWith("- [ ]") && current != null)
                {
                    descBuffer += $"• {trimmed.Substring(5)}\n";
                }
                else if (trimmed.StartsWith("|") && current != null && !trimmed.Contains("---"))
                {
                    var parts = trimmed.Split('|').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToArray();
                    if (parts.Length >= 2 && parts[0] != "System")
                    {
                        descBuffer += $"• **{parts[0]}**: {parts[2]}\n";
                    }
                }
                else if (line.Contains("Total: ~4 weeks") && current != null)
                {
                    current.Footer = trimmed;
                }
                else if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith(">") && !line.StartsWith("---") && current != null)
                {
                    descBuffer += line + "\n";
                }
            }

            if (current != null)
            {
                current.Description = descBuffer.Trim();
                sections.Add(current);
            }

            return sections;
        }

        private class RoadmapSection
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public Color Color { get; set; }
            public string Footer { get; set; }
        }
    }
}
