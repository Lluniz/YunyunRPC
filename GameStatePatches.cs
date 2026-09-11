using App.InGame;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.Localization.Settings;
using YunyunRPC;

namespace YunyunRPC
{
    public static class GameStateTracker
    {
        public static bool IsPlaying = false;
        public static bool IsInResults = false;
        public static string CurrentSongDisplayName = "Unknown";
        public static string CurrentArtist = "Unknown Artist";
        public static int CurrentScore = 0;
        public static int CurrentCombo = 0;
        public static int MaxCombo = 0;
        public static string Difficulty = "NORMAL";
        public static int DifficultyLevel = 0;
        public static bool IsPaused = false;

        public static int PerfectCount = 0;
        public static int GreatCount = 0;
        public static int GoodCount = 0;
        public static int MissCount = 0;
        public static double CurrentRate = 1.0;

        private static FieldInfo gameControllerPointCalculatorField = null;
        private static FieldInfo gameControllerDataField = null;
        private static FieldInfo gameControllerViewModelField = null;
        private static PropertyInfo gamePointCalculatorPointProperty = null;
        private static PropertyInfo gamePointCalculatorMaxComboProperty = null;
        private static PropertyInfo gamePointCalculatorRateProperty = null;
        private static MethodInfo gamePointCalculatorCalcRankMethod = null;
        private static bool cacheInitialized = false;
        public static bool HasOfficialRate = false;

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
                    gamePointCalculatorRateProperty = pointCalculatorType.GetProperty("Rate")
                                                   ?? pointCalculatorType.GetProperty("Accuracy")
                                                   ?? pointCalculatorType.GetProperty("HitRate");
                    gamePointCalculatorCalcRankMethod = pointCalculatorType.GetMethod("CalcRank", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                }

                cacheInitialized = true;
                YunyunRPCPlugin.Log.LogInfo("Cache initialized!");
            }
            catch (System.Exception ex)
            {
                YunyunRPCPlugin.Log.LogError($"Error initializing cache: {ex.Message}");
            }
        }

        public static string GetDifficultyName(int level)
        {
            return level switch
            {
                1 => "NORMAL",
                3 => "PULSING",
                4 => "BURSTING",
                5 => "DEGENERATE",
                _ => $"Lv.{level}"
            };
        }

        public static string GetAccuracyText()
        {
            return $"{GetAccuracy():F2}%";
        }

        public static float GetAccuracy()
        {
            if (HasOfficialRate)
            {
                double percent = CurrentRate <= 1.0001 ? CurrentRate * 100.0 : CurrentRate;
                return Mathf.Clamp((float)percent, 0f, 100f);
            }

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
                    var pointCalc = gameControllerPointCalculatorField?.GetValue(gameController);
                    if (pointCalc != null)
                    {
                        if (gamePointCalculatorCalcRankMethod != null)
                        {
                            var rank = gamePointCalculatorCalcRankMethod.Invoke(pointCalc, null);
                            if (rank != null)
                            {
                                string rankText = rank.ToString();
                                if (!string.IsNullOrEmpty(rankText) && rankText != "0")
                                    return rankText;
                            }
                        }
                    }
                }
            }
            catch { }

            double rate = CurrentRate <= 1.0001 ? CurrentRate : CurrentRate / 100.0;
            if (rate >= 0.95) return "S";
            if (rate >= 0.90) return "A";
            if (rate >= 0.80) return "B";
            if (rate >= 0.70) return "C";
            return "D";
        }

        private static object GetPropertyValueOrField(object obj, string name)
        {
            if (obj == null) return null;
            var type = obj.GetType();
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null) return prop.GetValue(obj);
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null) return field.GetValue(obj);
            return null;
        }

        public static string GetLocalizedText(string tableName, string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            try
            {
                var stringTable = LocalizationSettings.StringDatabase.GetTable(tableName)
                               ?? LocalizationSettings.StringDatabase.GetTable("ScoreData_en");
                if (stringTable != null)
                {
                    var entry = stringTable.GetEntry(key);
                    if (entry != null && !string.IsNullOrEmpty(entry.LocalizedValue))
                    {
                        return entry.LocalizedValue;
                    }
                }
            }
            catch (System.Exception ex)
            {
                YunyunRPCPlugin.Log.LogDebug($"Failed to look up localization table {tableName} for key {key}: {ex.Message}");
            }
            return null;
        }

        public static void ExtractLevelData(GameController controller, object gameControllerData)
        {
            if (gameControllerData == null) return;

            try
            {
                var dataType = gameControllerData.GetType();

                FieldInfo levelDataField = dataType.GetField("LevelData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                       ?? dataType.GetField("ScoreLevelData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                object levelDataObj = levelDataField?.GetValue(gameControllerData);

                object scoreDataObj = GetPropertyValueOrField(gameControllerData, "ScoreData");

                if (levelDataObj == null && scoreDataObj != null)
                {
                    levelDataField = scoreDataObj.GetType().GetField("LevelData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                   ?? scoreDataObj.GetType().GetField("ScoreLevelData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    levelDataObj = levelDataField?.GetValue(scoreDataObj);
                }

                string title = null;
                string artist = null;
                string musicId = null;

                if (levelDataObj != null)
                {
                    musicId = GetPropertyValueOrField(levelDataObj, "MusicID")?.ToString()
                           ?? GetPropertyValueOrField(levelDataObj, "TAG")?.ToString();

                    title = GetPropertyValueOrField(levelDataObj, "Title")?.ToString()
                         ?? GetPropertyValueOrField(levelDataObj, "SongName")?.ToString()
                         ?? GetPropertyValueOrField(levelDataObj, "Name")?.ToString();

                    artist = GetPropertyValueOrField(levelDataObj, "Artist")?.ToString()
                          ?? GetPropertyValueOrField(levelDataObj, "Composer")?.ToString();

                    if (!string.IsNullOrEmpty(musicId))
                    {
                        string localizedTitle = GetLocalizedText("ScoreData", $"{musicId}_TITLE")
                                             ?? GetLocalizedText("ScoreData", musicId);

                        if (!string.IsNullOrEmpty(localizedTitle))
                        {
                            title = localizedTitle;
                        }

                        string localizedArtist = GetLocalizedText("ScoreData", $"{musicId}_ARTIST")
                                              ?? GetLocalizedText("ScoreData", $"{musicId}_COMPOSER");

                        if (!string.IsNullOrEmpty(localizedArtist))
                        {
                            artist = localizedArtist;
                        }
                    }
                }

                CurrentSongDisplayName = !string.IsNullOrWhiteSpace(title) && !title.StartsWith("SONG_") ? title : (musicId ?? "Unknown Song");
                CurrentArtist = !string.IsNullOrWhiteSpace(artist) ? artist : "Unknown Artist";

                int level = -1;
                var levelVal = GetPropertyValueOrField(scoreDataObj, "Level");
                if (levelVal != null)
                {
                    level = System.Convert.ToInt32(levelVal);
                }

                if (level < 0 && levelDataObj != null)
                {
                    levelVal = GetPropertyValueOrField(levelDataObj, "Level");
                    if (levelVal != null)
                    {
                        level = System.Convert.ToInt32(levelVal);
                    }
                }

                int chartLevel = -1;
                if (levelDataObj != null)
                {
                    var diffVal = GetPropertyValueOrField(levelDataObj, "Difficulty");
                    if (diffVal != null)
                    {
                        chartLevel = System.Convert.ToInt32(diffVal);
                    }
                }

                DifficultyLevel = chartLevel > 0 ? chartLevel : 0;

                string diffName = GetDifficultyName(level >= 0 ? level : 1);
                Difficulty = DifficultyLevel > 0 ? $"{diffName} Lv.{DifficultyLevel}" : diffName;

                YunyunRPCPlugin.Log.LogInfo($"Extracted -> Title: '{CurrentSongDisplayName}', Artist: '{CurrentArtist}', Diff: '{Difficulty}' (level={level}, chart={chartLevel})");
            }
            catch (System.Exception ex)
            {
                YunyunRPCPlugin.Log.LogError($"Error extracting LevelData: {ex.Message}");
            }
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

                    var rateValue = gamePointCalculatorRateProperty?.GetValue(pointCalculator)
                                 ?? GetPropertyValueOrField(pointCalculator, "Rate")
                                 ?? GetPropertyValueOrField(pointCalculator, "Accuracy")
                                 ?? GetPropertyValueOrField(pointCalculator, "HitRate");
                    if (rateValue != null)
                    {
                        CurrentRate = System.Convert.ToDouble(rateValue);
                        HasOfficialRate = true;
                    }
                }

                var viewModel = gameControllerViewModelField?.GetValue(controller);
                if (viewModel != null)
                {
                    var viewModelType = viewModel.GetType();

                    var comboProp = viewModelType.GetProperty("Combo");
                    if (comboProp != null)
                    {
                        var comboValue = comboProp.GetValue(viewModel);
                        if (comboValue != null) CurrentCombo = (int)comboValue;
                    }

                    var perfectProp = viewModelType.GetProperty("PerfectCount");
                    if (perfectProp != null)
                    {
                        var val = perfectProp.GetValue(viewModel);
                        if (val != null) PerfectCount = (int)val;
                    }

                    var greatProp = viewModelType.GetProperty("GreatCount");
                    if (greatProp != null)
                    {
                        var val = greatProp.GetValue(viewModel);
                        if (val != null) GreatCount = (int)val;
                    }

                    var goodProp = viewModelType.GetProperty("GoodCount");
                    if (goodProp != null)
                    {
                        var val = goodProp.GetValue(viewModel);
                        if (val != null) GoodCount = (int)val;
                    }

                    var missProp = viewModelType.GetProperty("MissCount");
                    if (missProp != null)
                    {
                        var val = missProp.GetValue(viewModel);
                        if (val != null) MissCount = (int)val;
                    }
                }
            }
            catch (System.Exception ex)
            {
                YunyunRPCPlugin.Log.LogDebug($"Error updating: {ex.Message}");
            }
        }

        public static void ShowResults()
        {
            if (IsInResults) return;
            IsInResults = true;
            IsPlaying = false;
            bool isFullCombo = MissCount == 0 && MaxCombo > 10;
            string rank = GetRank();

            var discord = Object.FindFirstObjectByType<DiscordController>();
            discord?.SetResultPresence(CurrentSongDisplayName, CurrentArtist, CurrentScore, rank, isFullCombo, GetAccuracyText(), Difficulty);

            YunyunRPCPlugin.Log.LogInfo($"Rank {rank}: {CurrentSongDisplayName} by {CurrentArtist} - Score {CurrentScore}, Accuracy {GetAccuracyText()}");
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
            GameStateTracker.CurrentRate = 1.0;
            GameStateTracker.HasOfficialRate = false;
            GameStateTracker.CurrentSongDisplayName = "Loading...";
            GameStateTracker.CurrentArtist = "Unknown Artist";
            GameStateTracker.Difficulty = "NORMAL";
            GameStateTracker.DifficultyLevel = 0;

            try
            {
                var dataField = __instance.GetType().GetField("m_Data", BindingFlags.NonPublic | BindingFlags.Instance);
                var data = dataField?.GetValue(__instance);

                if (data != null)
                {
                    GameStateTracker.ExtractLevelData(__instance, data);
                }
            }
            catch (System.Exception ex)
            {
                YunyunRPCPlugin.Log.LogError($"Error in Play Postfix: {ex.Message}");
            }

            GameStateTracker.UpdateFromGameController(__instance);

            var discord = Object.FindFirstObjectByType<DiscordController>();
            if (discord != null)
            {
                discord.SetGameplayPresence(
                    GameStateTracker.CurrentSongDisplayName,
                    GameStateTracker.CurrentArtist,
                    GameStateTracker.CurrentScore,
                    GameStateTracker.CurrentCombo,
                    GameStateTracker.MaxCombo,
                    GameStateTracker.Difficulty,
                    GameStateTracker.GetAccuracyText()
                );
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
                if (pauseField != null)
                {
                    var pauseValue = pauseField.GetValue(__instance);
                    if (pauseValue != null) currentlyPaused = (bool)pauseValue;
                }
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
                        discord.SetPausedPresence(GameStateTracker.CurrentSongDisplayName, GameStateTracker.CurrentArtist);
                    }
                    else
                    {
                        discord.SetGameplayPresence(
                            GameStateTracker.CurrentSongDisplayName,
                            GameStateTracker.CurrentArtist,
                            GameStateTracker.CurrentScore,
                            GameStateTracker.CurrentCombo,
                            GameStateTracker.MaxCombo,
                            GameStateTracker.Difficulty,
                            GameStateTracker.GetAccuracyText()
                        );
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
                discord2.SetGameplayPresence(
                    GameStateTracker.CurrentSongDisplayName,
                    GameStateTracker.CurrentArtist,
                    GameStateTracker.CurrentScore,
                    GameStateTracker.CurrentCombo,
                    GameStateTracker.MaxCombo,
                    GameStateTracker.Difficulty,
                    GameStateTracker.GetAccuracyText()
                );
            }
        }
    }

    [HarmonyPatch(typeof(GameController), "OnDestroy")]
    class GameControllerDestroyPatch
    {
        static void Prefix()
        {
            if (!GameStateTracker.IsPlaying && !GameStateTracker.IsInResults) return;

            if (!GameStateTracker.IsInResults) GameStateTracker.ShowResults();

            GameStateTracker.IsPlaying = false;
            GameStateTracker.IsInResults = false;

            var discord = Object.FindFirstObjectByType<DiscordController>();
            discord?.SetLoadingPresence();
        }
    }
}
