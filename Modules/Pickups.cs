using BepInEx.Configuration;
using KiraiMod.Core.UI;
using KiraiMod.Core.Utils;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDKBase;
using VRC.Udon.Wrapper.Modules;

namespace KiraiMod.Modules
{
    [Module]
    public static class PickupsLegacy
    {
        private static readonly MethodInfo NoOp = typeof(PickupsLegacy).GetMethod(nameof(HkNoOp), BindingFlags.NonPublic | BindingFlags.Static);
        private static bool HkNoOp() => false;

        static PickupsLegacy()
        {
            typeof(Modifiers).Initialize();

            LegacyGUIManager.OnLoad += () =>
            {
                UIGroup ui = new(nameof(Pickups));
                ui.RegisterAsHighest();

                UIGroup modifiers = new("Modifiers", ui);
                modifiers.AddElement("Unlock", Modifiers.unlock.Value).Bound.Bind(Modifiers.unlock); //              if you think it's an anti pattern
                modifiers.AddElement("Theft", Modifiers.theft.Value).Bound.Bind(Modifiers.theft); //                 writing the name of the config entry
                modifiers.AddElement("Rotate", Modifiers.rotate.Value).Bound.Bind(Modifiers.rotate); //              twice for every single ui element
                modifiers.AddElement("Reach", Modifiers.reach.Value).Bound.Bind(Modifiers.reach); //                 then hope for bepinex to give
                modifiers.AddElement("Boost", Modifiers.boost.Value).Bound.Bind(Modifiers.boost); //                 config entry a base class we can use 
                modifiers.AddElement("Boost Speed", Modifiers.boostSpeed.Value).Bound.Bind(Modifiers.boostSpeed); // instead of bound

                ui.AddElement("Drop").Changed += Drop;
                ui.AddElement("Scramble").Changed += Scramble;
            };
        }

        public static class Modifiers
        {
            public static ConfigEntry<bool> unlock = Plugin.Configuration.Bind("Pickups", "Unlock", false, "Should all pickups always pickupable");
            public static ConfigEntry<bool> theft = Plugin.Configuration.Bind("Pickups", "Theft", false, "Should you be able to take things out of people's hands");
            public static ConfigEntry<bool> rotate = Plugin.Configuration.Bind("Pickups", "Rotate", false, "Shoud you be able to rotate all pickups in your hand");
            public static ConfigEntry<bool> reach = Plugin.Configuration.Bind("Pickups", "Reach", false, "Should you be able to pickup things from any distance");
            public static ConfigEntry<bool> boost = Plugin.Configuration.Bind("Pickups", "Boost", false, "Should you be able to throw things faster");
            public static ConfigEntry<float> boostSpeed = Plugin.Configuration.Bind("Pickups", "BoostSpeed", 5.0f, "The speed at which pickups are thrown");

            static Modifiers()
            {
                Type type = typeof(ExternVRCSDK3ComponentsVRCPickup);
                new BasePickupModifier(type.GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_pickupable__SystemBoolean)), unlock, pickup => pickup.pickupable = true);
                new BasePickupModifier(type.GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_DisallowTheft__SystemBoolean)), theft, pickup => pickup.DisallowTheft = false);
                new BasePickupModifier(type.GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_allowManipulationWhenEquipped__SystemBoolean)), rotate, pickup => pickup.allowManipulationWhenEquipped = true);
                new BasePickupModifier(type.GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_proximity__SystemSingle)), reach, pickup => pickup.proximity = float.MaxValue);
                new BasePickupModifier(type.GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_ThrowVelocityBoostScale__SystemSingle)), boost, pickup => pickup.ThrowVelocityBoostScale = boostSpeed.Value);

                boostSpeed.SettingChanged += (sender, args) =>
                {
                    if (boost.Value)
                        foreach (VRC_Pickup pickup in UnityEngine.Object.FindObjectsOfType<VRC_Pickup>())
                            pickup.ThrowVelocityBoostScale = boostSpeed.Value;
                };

                Plugin.HarmonyInstance.Patch(
                    typeof(VRC_Pickup).GetMethod(nameof(VRC_Pickup.Awake)),
                    typeof(Modifiers).GetMethod(nameof(HookAwake), BindingFlags.NonPublic | BindingFlags.Static).ToHM()
                );
            }

            private static void HookAwake(ref VRC_Pickup __instance)
            {
                if (unlock.Value) __instance.pickupable = true;
                if (theft.Value) __instance.DisallowTheft = false;
                if (rotate.Value) __instance.allowManipulationWhenEquipped = true;
                if (reach.Value) __instance.proximity = float.MaxValue;
                if (boost.Value) __instance.ThrowVelocityBoostScale = boostSpeed.Value;
            }

            public class BasePickupModifier
            {
                private readonly Action<VRC_Pickup> setup;

                public BasePickupModifier(MethodInfo orig, ConfigEntry<bool> entry, Action<VRC_Pickup> setup)
                {
                    this.setup = setup;
                    ToggleHook hook = new(orig, NoOp);

                    entry.SettingChanged += ((EventHandler)((sender, args) =>
                    {
                        if (theft.Value)
                        {
                            Events.World.Loaded += OnWorldLoaded;
                            foreach (VRC_Pickup pickup in UnityEngine.Object.FindObjectsOfType<VRC_Pickup>())
                                setup(pickup);
                            hook.Toggle(true);
                        }
                        else
                        {
                            Events.World.Loaded -= OnWorldLoaded;
                            hook.Toggle(false);
                        }
                    })).Invoke();
                }

                private void OnWorldLoaded(Scene scene)
                {
                    foreach (VRC_Pickup pickup in UnityEngine.Object.FindObjectsOfType<VRC_Pickup>())
                        setup(pickup);
                }
            }
        }

        public static void Drop() =>
            ApplyToAll(pickup =>
            {
                if (Networking.GetOwner(pickup.gameObject) != Networking.LocalPlayer)
                    Networking.SetOwner(Networking.LocalPlayer, pickup.gameObject);
            });

        public static void Scramble() => 
            ApplyToAll(pickup =>
            {
                if (Networking.GetOwner(pickup.gameObject) != Networking.LocalPlayer)
                    Networking.SetOwner(VRCPlayerApi.AllPlayers[UnityEngine.Random.Range(0, VRCPlayerApi.AllPlayers.Count)], pickup.gameObject);
            });

        private static void ApplyToAll(Action<VRC_Pickup> func) => 
            UnityEngine.Object.FindObjectsOfTypeAll(UnhollowerRuntimeLib.Il2CppType.Of<VRC_Pickup>())
                .Cast<UnhollowerBaseLib.Il2CppReferenceArray<VRC_Pickup>>()
                .ForEach(func);
    }
}
