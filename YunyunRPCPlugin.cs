using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace YunyunRPC
{
    [BepInPlugin("com.seunome.yunyunrpc", "YunyunRPC", "0.5.0")]
    [BepInDependency("com.bepinex.bepinex", BepInDependency.DependencyFlags.SoftDependency)]
    public class YunyunRPCPlugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        public static YunyunRPCPlugin Instance;

        private DiscordController discordController;
        private Harmony harmony;

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            Log.LogMessage("╔════════════════════════════════════╗");
            Log.LogMessage("║     YunyunRPC Mod Running!         ║");
            Log.LogMessage("╚════════════════════════════════════╝");

            discordController = gameObject.AddComponent<DiscordController>();
            harmony = new Harmony("com.seunome.yunyunrpc");
            harmony.PatchAll();

            Log.LogInfo("Harmony patches sucess applied!");
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
            if (discordController != null)
            {
                Destroy(discordController);
            }
            Log.LogInfo("YunyunRPC turned off.");
        }
    }
}
