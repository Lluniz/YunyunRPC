using DiscordRPC;
using DiscordRPC.Logging;
using UnityEngine;

namespace YunyunRPC
{
    public class DiscordController : MonoBehaviour
    {
        private DiscordRpcClient client;
        private const string APPLICATION_ID = "1547571293543596082";
        private RichPresence currentPresence;
        private bool isInitialized = false;
        private float loadingStartTime = 0f;
        private bool isLoading = false;
        private bool firstTimeLoaded = false;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (APPLICATION_ID == "1234567890123456789")
            {
                YunyunRPCPlugin.Log.LogWarning("Place your Application ID on line 14!");
                return;
            }
            InitializeDiscord();
        }

        private void InitializeDiscord()
        {
            try
            {
                client = new DiscordRpcClient(APPLICATION_ID, autoEvents: false);
                client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
                client.OnReady += (sender, e) =>
                {
                    YunyunRPCPlugin.Log.LogInfo($"Discord RPC connected: {e.User.Username}");
                    isInitialized = true;
                    SetLoadingPresence();
                };
                client.OnError += (sender, e) => { YunyunRPCPlugin.Log.LogError($"Discord RPC Error: {e.Message}"); };
                client.Initialize();
            }
            catch (System.Exception ex) { YunyunRPCPlugin.Log.LogError($"Error initializing Discord RPC: {ex.Message}"); }
        }

        void Update()
        {
            client?.Invoke();

            if (isLoading && !firstTimeLoaded && Time.time - loadingStartTime > 20f)
            {
                isLoading = false;
                firstTimeLoaded = true;
                UpdatePresence(new RichPresence
                {
                    Details = "Loading dream...",
                    State = "",
                    Assets = new Assets
                    {
                        LargeImageKey = "game_logo",
                        LargeImageText = "YunyunRPC v0.0.1"
                    },
                    Timestamps = new Timestamps(System.DateTime.UtcNow)
                });
            }
        }

        public void SetLoadingPresence()
        {
            if (!firstTimeLoaded)
            {
                loadingStartTime = Time.time;
                isLoading = true;

                UpdatePresence(new RichPresence
                {
                    Details = "Loading dream...",
                    State = "Starting the game",
                    Assets = new Assets
                    {
                        LargeImageKey = "game_logo",
                        LargeImageText = "YunyunRPC v0.0.1"
                    },
                    Timestamps = new Timestamps(System.DateTime.UtcNow)
                });
            }
            else
            {
                UpdatePresence(new RichPresence
                {
                    Details = "Loading dream...",
                    State = "",
                    Assets = new Assets
                    {
                        LargeImageKey = "game_logo",
                        LargeImageText = "YunyunRPC v0.0.1"
                    },
                    Timestamps = new Timestamps(System.DateTime.UtcNow)
                });
            }
        }

        public void SetGameplayPresence(string songName, int score, int combo, int maxCombo, string difficulty, string accuracy)
        {
            isLoading = false;
            string stateText = $"Score: {score:N0} | Combo: {combo}x | Accuracy: {accuracy}";

            UpdatePresence(new RichPresence
            {
                Details = $"Playing: {songName} [{difficulty}]",
                State = stateText,
                Assets = new Assets
                {
                    LargeImageKey = "game_logo",
                    LargeImageText = "YunyunRPC v0.0.1",
                    SmallImageKey = "status_icon",
                    SmallImageText = ""
                }
            });
        }

        public void SetPausedPresence(string songName)
        {
            isLoading = false;
            UpdatePresence(new RichPresence
            {
                Details = $"Paused: {songName}",
                State = "Game paused",
                Assets = new Assets
                {
                    LargeImageKey = "game_logo",
                    LargeImageText = "YunyunRPC v0.0.1",
                    SmallImageKey = "status_icon",
                    SmallImageText = ""
                }
            });
        }

        public void SetResultPresence(string songName, int finalScore, string rank, bool isFullCombo, string accuracy, string difficulty)
        {
            isLoading = false;
            string rankEmoji = rank switch { "S" => "🏆", "A" => "⭐", "B" => "👍", "C" => "📋", _ => "📊" };

            UpdatePresence(new RichPresence
            {
                Details = $"{rankEmoji} {songName} [{difficulty}] - Rank {rank}",
                State = $"Score: {finalScore:N0} | Accuracy: {accuracy}" + (isFullCombo ? " | ✨ FULL COMBO!" : ""),
                Assets = new Assets
                {
                    LargeImageKey = "game_logo",
                    LargeImageText = "YunyunRPC v0.0.1",
                    SmallImageKey = "status_icon",
                    SmallImageText = ""
                }
            });
        }

        private void UpdatePresence(RichPresence presence)
        {
            if (!isInitialized || client == null) return;
            currentPresence = presence;
            client.SetPresence(presence);
        }

        void OnDestroy()
        {
            client?.Dispose();
            YunyunRPCPlugin.Log.LogInfo("Discord RPC disconnected.");
        }
    }
}