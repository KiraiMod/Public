using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using VRC.SDKBase;

namespace KiraiMod.Modules.Movement
{
    [Module]
    public static class Speed
    {
        [Configure<float>("Movement.Speed.Double Tap Duration", 0.3f)]
        public static float DoubleTapDuration;

        static Speed()
        {
            // desktop only for now
            Core.Managers.KeybindManager.binds.Add("movement.speed.forward", new() { keys = new[] { Key.W }, OnClick = OnForward });

            typeof(Hooks).Initialize();
        }

        private static float LastForward;

        private static void OnForward()
        {
            float CurrentTime = Time.time;

            if (CurrentTime - LastForward <= DoubleTapDuration)
            {
                Hooks.Multiplier = 2;
                Events.Update += CheckKeyStatus;
            }

            LastForward = CurrentTime;
        }

        // this can be removed when the bind manager is created
        private static void CheckKeyStatus()
        {
            if (!Input.GetKey(KeyCode.W))
            {
                Events.Update -= CheckKeyStatus;
                Hooks.Multiplier = 1;
                return;
            }
        }

        private static class Hooks
        {
            private static float _multiplier = 1;
            public static float Multiplier
            {
                get => _multiplier;
                set
                {
                    if (_multiplier == value) return;

                    if (Networking.LocalPlayer != null)
                    {
                        float run = Networking.LocalPlayer.GetRunSpeed();
                        float walk = Networking.LocalPlayer.GetWalkSpeed();
                        float strafe = Networking.LocalPlayer.GetStrafeSpeed();

                        _multiplier = value;

                        Networking.LocalPlayer.SetRunSpeed(run);
                        Networking.LocalPlayer.SetWalkSpeed(walk);
                        Networking.LocalPlayer.SetStrafeSpeed(strafe);
                    }
                    else _multiplier = value;
                }
            }

            static Hooks()
            {
                HarmonyMethod getter = typeof(Hooks).GetMethod(nameof(Hooks.Hook_Getter), BindingFlags.NonPublic | BindingFlags.Static).ToHM();
                HarmonyMethod setter = typeof(Hooks).GetMethod(nameof(Hooks.Hook_Setter), BindingFlags.NonPublic | BindingFlags.Static).ToHM();

                Type type = typeof(VRCPlayerApi);
                Plugin.HarmonyInstance.Patch(type.GetMethod(nameof(VRCPlayerApi.GetRunSpeed)), null, getter);
                Plugin.HarmonyInstance.Patch(type.GetMethod(nameof(VRCPlayerApi.GetWalkSpeed)), null, getter);
                Plugin.HarmonyInstance.Patch(type.GetMethod(nameof(VRCPlayerApi.GetStrafeSpeed)), null, getter);
                Plugin.HarmonyInstance.Patch(type.GetMethod(nameof(VRCPlayerApi.SetRunSpeed)), setter);
                Plugin.HarmonyInstance.Patch(type.GetMethod(nameof(VRCPlayerApi.SetWalkSpeed)), setter);
                Plugin.HarmonyInstance.Patch(type.GetMethod(nameof(VRCPlayerApi.SetStrafeSpeed)), setter);

                Harmony.CreateAndPatchAll(typeof(Hooks));
            }

            private static void Hook_Getter(ref float __result) => __result /= _multiplier;
            private static void Hook_Setter(ref float __0) => __0 *= _multiplier;
        }
    }
}
