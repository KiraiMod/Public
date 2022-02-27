﻿global using KiraiMod.Core;

using BepInEx;
using BepInEx.IL2CPP;
using KiraiMod.Managers;

namespace KiraiMod
{
    [BepInPlugin("me.kiraihooks.KiraiMod", "KiraiMod", "2.0.0.0")]
    [BepInDependency("me.kiraihooks.KiraiMod.Core", BepInDependency.DependencyFlags.HardDependency)]
    public class KiraiMod : BasePlugin
    {
        public override void Load()
        {
            Shared.Logger = Log;
            Shared.Config = Config;
            Shared.Harmony = new("me.kiraihooks.KiraiMod");

            typeof(KeybindManager).Initialize();
            typeof(ModuleManager).Initialize();
            typeof(GUIManager).Initialize(); 
        }
    }
}
