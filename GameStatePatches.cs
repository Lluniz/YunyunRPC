using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using App.InGame;
using UnityEngine.SceneManagement;

namespace YunyunRPC
{
    public static class GameStateTracker
    {
        public static bool IsPlaying = false;
        public static bool IsInResults = false;
        public static string CurrentSong = "Unknown";
        public static string CurrentSongDisplayName = "Unknown";
        public static int CurrentScore = 0;
        public static int CurrentCombo = 0;
        public static int MaxCombo = 0;
        public static string Difficulty = "Normal";
        public static bool IsPaused = false;

        public static int PerfectCount = 0;
        public static int GreatCount = 0;
        public static int GoodCount = 0;
        public static int MissCount = 0;

        private static FieldInfo gameControllerPointCalculatorField = null;
        private static FieldInfo gameControllerDataField = null;
        private static FieldInfo gameControllerViewModelField = null;
        private static FieldInfo scoreDataSetMusicNameField = null;
        private static PropertyInfo gamePointCalculatorPointProperty = null;
        private static PropertyInfo gamePointCalculatorMaxComboProperty = null;
        private static bool cacheInitialized = false;

        private static readonly Dictionary<string, string> SongNames = new Dictionary<string, string>
        {
            ["0"] = "Denpa-tic Imaginary Girl \"Q\"",
            ["10"] = "Peak Ecstasy!! Rim de Lacent☆",
            ["20"] = "YUNYUN HARDCORE",
            ["30"] = "Antenna Light",
            ["40"] = "DONIDEMONARE (Who cares anymore)",
            ["50"] = "Marisa stole the precious thing",
            ["60"] = "Kanbu de Tomatte sugu Tokeru Kyoki no Udongein",
            ["70"] = "Cirno's Perfect Math Class",
            ["80"] = "Nee,...shiyou yo!",
            ["90"] = "Mighty Heart ~Aru Hi no Kenka, Itsumo no Koigokoro~",
            ["100"] = "sakuranbokissu~bakuhatsudamo~n~",
            ["110"] = "Kyururun Kiss de Janbo ♪ ♪",
            ["120"] = "Raspberry",
            ["130"] = "Achichi na Natsu no Monogatari",
            ["140"] = "Princess Bride!",
            ["150"] = "Princess Brave!",
            ["160"] = "Change my Style~Anata gonomi no Watashi ni~",
            ["170"] = "true my heart",
            ["180"] = "Love Cheat!",
            ["190"] = "Gacha Gacha Cute Figu@mate",
            ["200"] = "Senno Sakushu Tora no Maki",
            ["210"] = "INTERNET OVERDOSE",
            ["220"] = "INTERNET YAMERO",
            ["230"] = "Miko Miko Nurse - Ai no Theme",
            ["240"] = "Dakko Shite Gyu!",
            ["250"] = "PETTAN PETTAN TSURUPETTAN",
            ["260"] = "summer is machine gun",
            ["270"] = "winter is machine gun",
            ["280"] = "Shukusei!! Loli Kami Requiem",
            ["300"] = "CosmicKING☆sens@tion!!!!!!!!",
            ["310"] = "MuseDashを作っているPeroPeroGamesさんが倒産しちゃったよ～",
            ["1000"] = "Help me, ERINNNNNN!! (Band ver.)",
            ["1010"] = "Usatei",
            ["1020"] = "Scarlet! Police!! on Ghetto Patrol",
            ["1030"] = "Oyome ni Shinasai!",
            ["1040"] = "Wakasagihime's 100 Sushi-Topping Game",
            ["1050"] = "kero9destiny",
            ["1120"] = "We are Pollen Fairies \"Pollino-Sis\"!",
            ["1130"] = "Versus!",
            ["1140"] = "Can I Friend you on Bassbook? lol",
            ["1150"] = "Strongest Dental Caries Constructionist Starts Excavation",
            ["1160"] = "I don't care about Christmas though",
            ["1170"] = "FULLFLAVOR ONDO",
            ["1180"] = "Newbies take 3 years, geeks 8 years, and internets are forever",
            ["1190"] = "Energy-Dringirl Ffeine-chan!",
            ["1200"] = "Please! Concon Inari-sama",
            ["1210"] = "Denpa-tic Imaginary Girl \"Q\" (Cover)",
            ["1220"] = "INTERNET OVERDOSE (Cover)",
            ["1230"] = "INTERNET YAMERO (Cover)",
            ["1240"] = "Bamboo",
            ["1250"] = "Punai Punai Taiso",
            ["1260"] = "Punai Punai Fantasy",
            ["1270"] = "Punai Punai War",
            ["1280"] = "Tanaka",
            ["1290"] = "Waaa",
            ["1300"] = "Kakikuke Caution",
            ["1310"] = "Nitrogen",
            ["1320"] = "As the angel says",
            ["1330"] = "Pa Pi Pu Pi Pu Pi Pa",
            ["1340"] = "B.B.K.K.B.K.K. (Rish&Choko Remix)",
            ["1350"] = "MaiMai Memomo Memomo MaiMai",
            ["1500"] = "Let's Go Lovely Henshin Time!",
            ["1510"] = "LITTLE MY STAR",
            ["1520"] = "Kurukuru lovely day!!!",
            ["1530"] = "Gingakei no Uchuu no Hate made",
            ["1540"] = "the first the last",
            ["1550"] = "Shinseiji Kono Koi To Kimi To Atashi",
            ["1560"] = "Sakura Saku",
            ["1570"] = "Omoi Wa Ko Kon To Zai",
            ["1580"] = "Help! heaven!",
            ["1590"] = "BITE",
            ["1600"] = "Go★Home!",
            ["1610"] = "Koi no Recipe",
            ["9910"] = "Observing Glitch \"YUN-YUN\"",
            ["9920"] = "Desktop Society",
            ["9930"] = "Glittering Trap",
            ["9940"] = "KAKUSEI☆SYNDROME",
            ["9950"] = "Thanks for playing",
            ["9960"] = "EXC3PT!ON_H4NDL1NG"
        };

        public static void InitializeCache()
        {
            if (cacheInitialized) return;
            try
            {
                var gameControllerType = typeof(GameController);
                gameControllerPointCalculatorField = gameControllerType.GetField("m_PointCalculator", BindingFlags.NonPublic | BindingFlags.Instance);
                gameControllerDataField = gameControllerType.GetField("m_Data", BindingFlags.NonPublic | BindingFlags.Instance);
                gameControllerViewModelField = gameControllerType.GetField("m_ViewModel", BindingFlags.NonPublic | BindingFlags.Instance);
                if (gameControllerPointCalculatorField != null)
                {
                    var pointCalculatorType = gameControllerPointCalculatorField.FieldType;
                    gamePointCalculatorPointProperty = pointCalculatorType.GetProperty("Point");
                    gamePointCalculatorMaxComboProperty = pointCalculatorType.GetProperty("MaxCombo");
                }
                if (gameControllerDataField != null)
                {
                    var scoreDataSetType = gameControllerDataField.FieldType;
                    scoreDataSetMusicNameField = scoreDataSetType.GetField("MusicName", BindingFlags.Public | BindingFlags.Instance);
                }
                cacheInitialized = true;
                YunyunRPCPlugin.Log.LogInfo("Cache initialized!");
            }
            catch (System.Exception ex) { YunyunRPCPlugin.Log.LogError($"Error initializing cache: {ex.Message}"); }
        }

        public static string GetSongDisplayName(string musicId)
        {
            if (string.IsNullOrEmpty(musicId)) return "Unknown Song";
            var match = System.Text.RegularExpressions.Regex.Match(musicId, @"(\d+)");
            if (match.Success)
            {
                string songKey = match.Groups[1].Value.TrimStart('0');
                if (string.IsNullOrEmpty(songKey)) songKey = "0";
                if (SongNames.TryGetValue(songKey, out string songName)) return songName;
            }
            return $"Song {match.Value}";
        }

        public static string GetDifficultyName(int level)
        {
            return level switch { 1 => "NORMAL", 3 => "PULSING", 4 => "BURSTING", 5 => "DEGENERATE", _ => $"Lv.{level}" };
        }

        public static string GetAccuracyText()
        {
            try
            {
                var gameController = Object.FindFirstObjectByType<GameController>();
                if (gameController != null)
                {
                    var pointCalcField = gameController.GetType().GetField("m_PointCalculator", BindingFlags.NonPublic | BindingFlags.Instance);
                    var pointCalc = pointCalcField?.GetValue(gameController);
                    if (pointCalc != null)
                    {
                        var rateProp = pointCalc.GetType().GetProperty("Rate");
                        if (rateProp != null)
                        {
                            var rateValue = rateProp.GetValue(pointCalc);
                            if (rateValue != null) return $"{(double)rateValue * 100:F2}%";
                        }
                    }
                }
            }
            catch { }
            return $"{GetAccuracy():F2}%";
        }

        public static float GetAccuracy()
        {
            int totalHits = PerfectCount + GreatCount + GoodCount + MissCount;
            if (totalHits == 0) return 100f;
            float weightedScore = (PerfectCount * 1.0f) + (GreatCount * 0.70f) + (GoodCount * 0.30f);
            return Mathf.Clamp((weightedScore / totalHits) * 100f, 0f, 100f);
        }

        public static string GetRank()
        {
            try
            {
                var gameController = Object.FindFirstObjectByType<GameController>();
                if (gameController != null)
                {
                    var pointCalcField = gameController.GetType().GetField("m_PointCalculator", BindingFlags.NonPublic | BindingFlags.Instance);
                    var pointCalc = pointCalcField?.GetValue(gameController);
                    if (pointCalc != null)
                    {
                        var calcRankMethod = pointCalc.GetType().GetMethod("CalcRank");
                        if (calcRankMethod != null)
                        {
                            var rank = calcRankMethod.Invoke(pointCalc, null);
                            if (rank != null) return rank.ToString();
                        }
                        var rateProp = pointCalc.GetType().GetProperty("Rate");
                        if (rateProp != null)
                        {
                            var rateValue = rateProp.GetValue(pointCalc);
                            if (rateValue != null)
                            {
                                double rate = (double)rateValue;
                                if (rate >= 0.95) return "S";
                                if (rate >= 0.90) return "A";
                                if (rate >= 0.80) return "B";
                                if (rate >= 0.70) return "C";
                                return "D";
                            }
                        }
                    }
                }
            }
            catch { }
            float acc = GetAccuracy();
            if (acc >= 99.0f) return "S";
            if (acc >= 95.0f) return "A";
            if (acc >= 90.0f) return "B";
            if (acc >= 80.0f) return "C";
            return "D";
        }

        public static void UpdateFromGameController(GameController controller)
        {
            if (!cacheInitialized) InitializeCache();
            if (controller == null) return;
            try
            {
                var pointCalculator = gameControllerPointCalculatorField?.GetValue(controller);
                if (pointCalculator != null)
                {
                    var pointValue = gamePointCalculatorPointProperty?.GetValue(pointCalculator);
                    if (pointValue != null) CurrentScore = (int)pointValue;
                    var maxComboValue = gamePointCalculatorMaxComboProperty?.GetValue(pointCalculator);
                    if (maxComboValue != null) MaxCombo = (int)maxComboValue;
                }
                var scoreDataSet = gameControllerDataField?.GetValue(controller);
                if (scoreDataSet != null)
                {
                    var musicName = scoreDataSetMusicNameField?.GetValue(scoreDataSet)?.ToString();
                    if (!string.IsNullOrEmpty(musicName) && musicName != CurrentSong)
                    {
                        CurrentSong = musicName;
                        CurrentSongDisplayName = GetSongDisplayName(musicName);
                    }
                }
                var viewModel = gameControllerViewModelField?.GetValue(controller);
                if (viewModel != null)
                {
                    var viewModelType = viewModel.GetType();
                    var comboProp = viewModelType.GetProperty("Combo");
                    if (comboProp != null) { var comboValue = comboProp.GetValue(viewModel); if (comboValue != null) CurrentCombo = (int)comboValue; }
                    var perfectProp = viewModelType.GetProperty("PerfectCount");
                    if (perfectProp != null) { var val = perfectProp.GetValue(viewModel); if (val != null) PerfectCount = (int)val; }
                    var greatProp = viewModelType.GetProperty("GreatCount");
                    if (greatProp != null) { var val = greatProp.GetValue(viewModel); if (val != null) GreatCount = (int)val; }
                    var goodProp = viewModelType.GetProperty("GoodCount");
                    if (goodProp != null) { var val = goodProp.GetValue(viewModel); if (val != null) GoodCount = (int)val; }
                    var missProp = viewModelType.GetProperty("MissCount");
                    if (missProp != null) { var val = missProp.GetValue(viewModel); if (val != null) MissCount = (int)val; }
                }
            }
            catch (System.Exception ex) { YunyunRPCPlugin.Log.LogDebug($"Error updating: {ex.Message}"); }
        }

        public static void ShowResults()
        {
            if (IsInResults) return;
            IsInResults = true;
            IsPlaying = false;
            bool isFullCombo = MissCount == 0 && MaxCombo > 10;
            string rank = GetRank();
            var discord = Object.FindFirstObjectByType<DiscordController>();
            discord?.SetResultPresence(CurrentSongDisplayName, CurrentScore, rank, isFullCombo, GetAccuracyText(), Difficulty);
            YunyunRPCPlugin.Log.LogInfo($"Rank {rank}: {CurrentSongDisplayName} - Score {CurrentScore}, Accuracy {GetAccuracyText()}");
        }
    }

    [HarmonyPatch(typeof(GameController), "Play")]
    class GameControllerPlayPatch
    {
        static void Postfix(GameController __instance)
        {
            YunyunRPCPlugin.Log.LogInfo("Gameplay started!");
            GameStateTracker.IsPlaying = true;
            GameStateTracker.IsInResults = false;
            GameStateTracker.IsPaused = false;
            GameStateTracker.CurrentScore = 0;
            GameStateTracker.CurrentCombo = 0;
            GameStateTracker.MaxCombo = 0;
            GameStateTracker.PerfectCount = 0;
            GameStateTracker.GreatCount = 0;
            GameStateTracker.GoodCount = 0;
            GameStateTracker.MissCount = 0;
            GameStateTracker.CurrentSong = "";
            GameStateTracker.CurrentSongDisplayName = "Loading...";

            GameStateTracker.UpdateFromGameController(__instance);

            try
            {
                var dataField = __instance.GetType().GetField("m_Data", BindingFlags.NonPublic | BindingFlags.Instance);
                var data = dataField?.GetValue(__instance);
                if (data != null)
                {
                    var scoreDataField = data.GetType().GetField("ScoreData");
                    var scoreData = scoreDataField?.GetValue(data);
                    if (scoreData != null)
                    {
                        var levelField = scoreData.GetType().GetField("Level");
                        var level = levelField?.GetValue(scoreData);
                        if (level != null) GameStateTracker.Difficulty = GameStateTracker.GetDifficultyName((int)level);
                    }
                }
            }
            catch { }

            var discord = Object.FindFirstObjectByType<DiscordController>();
            if (discord != null)
            {
                discord.SetGameplayPresence(GameStateTracker.CurrentSongDisplayName, GameStateTracker.CurrentScore, GameStateTracker.CurrentCombo, GameStateTracker.MaxCombo, GameStateTracker.Difficulty, GameStateTracker.GetAccuracyText());
            }
        }
    }

    [HarmonyPatch(typeof(GameController), "Update")]
    class GameControllerUpdatePatch
    {
        private static float lastUpdate = 0f;
        private static readonly float UPDATE_INTERVAL = 0.5f;
        private static bool wasPaused = false;
        private static bool endInputWasTrue = false;
        private static float lastDiscordUpdate = 0f;
        private static readonly float DISCORD_UPDATE_INTERVAL = 5f;

        static void Postfix(GameController __instance)
        {
            if (!GameStateTracker.IsPlaying && !GameStateTracker.IsInResults) return;

            try
            {
                var viewModelField = __instance.GetType().GetField("m_ViewModel", BindingFlags.NonPublic | BindingFlags.Instance);
                var viewModel = viewModelField?.GetValue(__instance);
                if (viewModel != null)
                {
                    var endInputProp = viewModel.GetType().GetProperty("EndInput");
                    if (endInputProp != null)
                    {
                        var endInputValue = endInputProp.GetValue(viewModel);
                        if (endInputValue != null)
                        {
                            bool endInput = (bool)endInputValue;
                            if (endInput && !endInputWasTrue && GameStateTracker.IsPlaying)
                            {
                                YunyunRPCPlugin.Log.LogInfo("Song ended!");
                                GameStateTracker.UpdateFromGameController(__instance);
                                GameStateTracker.ShowResults();
                            }
                            endInputWasTrue = endInput;
                        }
                    }
                }
            }
            catch { }

            if (GameStateTracker.IsInResults) return;

            bool currentlyPaused = false;
            try
            {
                var pauseField = __instance.GetType().GetField("m_Pause", BindingFlags.NonPublic | BindingFlags.Instance);
                if (pauseField != null) { var pauseValue = pauseField.GetValue(__instance); if (pauseValue != null) currentlyPaused = (bool)pauseValue; }
            }
            catch { }

            if (currentlyPaused != wasPaused)
            {
                wasPaused = currentlyPaused;
                GameStateTracker.IsPaused = currentlyPaused;
                var discord = Object.FindFirstObjectByType<DiscordController>();
                if (discord != null)
                {
                    if (currentlyPaused)
                    {
                        discord.SetPausedPresence(GameStateTracker.CurrentSongDisplayName);
                        YunyunRPCPlugin.Log.LogInfo("Paused");
                    }
                    else
                    {
                        discord.SetGameplayPresence(GameStateTracker.CurrentSongDisplayName, GameStateTracker.CurrentScore, GameStateTracker.CurrentCombo, GameStateTracker.MaxCombo, GameStateTracker.Difficulty, GameStateTracker.GetAccuracyText());
                        YunyunRPCPlugin.Log.LogInfo("Resumed");
                    }
                }
                lastDiscordUpdate = Time.time;
                return;
            }

            if (currentlyPaused) return;

            float currentTime = Time.time;
            if (currentTime - lastUpdate < UPDATE_INTERVAL) return;
            lastUpdate = currentTime;

            GameStateTracker.UpdateFromGameController(__instance);

            if (currentTime - lastDiscordUpdate < DISCORD_UPDATE_INTERVAL) return;
            lastDiscordUpdate = currentTime;

            var discord2 = Object.FindFirstObjectByType<DiscordController>();
            if (discord2 != null)
            {
                discord2.SetGameplayPresence(GameStateTracker.CurrentSongDisplayName, GameStateTracker.CurrentScore, GameStateTracker.CurrentCombo, GameStateTracker.MaxCombo, GameStateTracker.Difficulty, GameStateTracker.GetAccuracyText());
            }
        }
    }

    [HarmonyPatch(typeof(GameController), "OnDestroy")]
    class GameControllerDestroyPatch
    {
        static void Prefix()
        {
            if (!GameStateTracker.IsPlaying && !GameStateTracker.IsInResults) return;
            YunyunRPCPlugin.Log.LogInfo("GameController destroyed");
            if (!GameStateTracker.IsInResults) GameStateTracker.ShowResults();

            GameStateTracker.IsPlaying = false;
            GameStateTracker.IsInResults = false;
            var discord = Object.FindFirstObjectByType<DiscordController>();
            discord?.SetLoadingPresence();
        }
    }
}