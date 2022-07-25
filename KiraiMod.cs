global using KiraiMod.Core;
global using KiraiMod.Core.ModuleAPI;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using KiraiMod.Managers;

namespace KiraiMod
{
    [BepInPlugin("me.kiraihooks.KiraiMod", "KiraiMod", "2.0.0.0")]
    [BepInDependency("me.kiraihooks.KiraiMod.Core")]
    [BepInDependency("me.kiraihooks.KiraiMod.Core.UI")]
    public class Plugin : BasePlugin
    {
        internal static ManualLogSource Logger;
        internal static ConfigFile Configuration;
        internal static Harmony HarmonyInstance;

        public override void Load()
        {
            Logger = Log;
            Configuration = Config;
            HarmonyInstance = new("me.kiraihooks.KiraiMod");

            typeof(ModuleManager).Initialize();
            typeof(GUIManager).Initialize();

            Core.MessageAPI.Components.ModList.Mods.Add("KiraiMod Redux");
        }
    }
}
