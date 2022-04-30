using BepInEx.Configuration;
using KiraiMod.Core.UI;
using KiraiMod.Core.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using VRC.SDKBase;

namespace KiraiMod.Modules.Movement
{
    [Module]
    public static class Movement
    {
        static Movement()
        {
            GUI.Groups.Loaded += () =>
            {
                UIGroup position = new("Position", GUI.Groups.Movement);
                position.AddElement("Save Position").Changed += SavePosition;
                position.AddElement("Load Position").Changed += LoadPosition;

                GUI.Groups.Movement.AddElement("Use Legacy Locomotion").Changed += UseLegacyLocomotion;
                GUI.Groups.Movement.AddElement("Panic", Panic.enabled);
            };
        }

        // in the future this may have more complicated logic
        public static void UseLegacyLocomotion() => Networking.LocalPlayer.UseLegacyLocomotion();

        private static Vector3 prevPos;
        public static void SavePosition() => prevPos = Networking.LocalPlayer.gameObject.transform.position;
        public static void LoadPosition() => Networking.LocalPlayer.gameObject.transform.position = prevPos;

        public static class Panic // this needs to be rewritten to save the original position better
        {
            public static ConfigEntry<Key[]> bind = Plugin.cfg.Bind("Movement", "Panic", new Key[1] { Key.NumpadMultiply });
            public static Bound<bool> enabled = new();
            public static Vector3 offset = new(0, 1_000_000_000f, 0);

            static Panic()
            {
                enabled.ValueChanged += SetState;
                bind.Register(() => enabled.Value = !enabled._value);
                Events.World.Unloaded += _ => enabled.Value = false;
            }

            private static void SetState(bool state)
            {
                if (state)
                    Networking.LocalPlayer.gameObject.transform.position += offset;
                else Networking.LocalPlayer.gameObject.transform.position -= offset;
            }
        }
    }
}
