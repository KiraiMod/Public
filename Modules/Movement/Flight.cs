using BepInEx.Configuration;
using HarmonyLib;
using KiraiMod.Core.UI;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using VRC.SDKBase;

namespace KiraiMod.Modules.Movement
{
    [Module]
    public static class Flight
    {
        private static bool _enabled;
        [Configure<bool>("Movement.Flight", "Enabled", false, false)]
        [Keybind<bool>("Movement.Flight", "Keybind", true, false, Key.F)]
        public static bool Enabled
        {
            get => _enabled;
            set {
                if (_enabled == value)
                    return;
                _enabled = value;

                if (value) Enable();
                else Disable();
            }
        }

        private static bool _noclip;
        [Configure<bool>("Movement.Flight", "No Clip", true)]
        public static bool NoClip
        {
            get => _noclip;
            set
            {
                if (_noclip == value)
                    return;
                _noclip = value;

                Collisions.Set(value);
            }
        }

        private static bool _directional;
        [Configure<bool>("Movement.Flight", "Directional", false)]
        public static bool Directional
        {
            get => _directional;
            set
            {
                if (_directional == value)
                    return;
                _directional = value;

                Target.Fetch();
            }
        }

        [Configure<float>("Movement.Flight", "Speed", 8.0f)]
        public static float speed;

        static Flight()
        {
            Events.World.Unloaded += scene => Enabled = false;

            typeof(Hooks).Initialize();
        }

        public static Vector3 oGrav = new(0, -9.8f, 0);

        public static void Enable()
        {
            Physics.gravity = Vector3.zero;

            if (XRDevice.isPresent)
                Events.Update += UpdateVR;
            else Events.Update += UpdateDesktop;
            Events.World.Loaded += WorldLoaded;
            if (_noclip)
                Collisions.Set(true);
            Target.Fetch();
        }

        public static void Disable()
        {
            if (XRDevice.isPresent)
                Events.Update -= UpdateVR;
            else Events.Update -= UpdateDesktop;
            Events.World.Loaded -= WorldLoaded;
            Collisions.Set(false);

            Physics.gravity = oGrav;
        }

        private static void WorldLoaded(UnityEngine.SceneManagement.Scene scene) => Events.Update += UpdateCheck;

        private static void UpdateCheck()
        {
            if (Networking.LocalPlayer == null)
                return;

            Events.Update -= UpdateCheck;

            if (_noclip)
                Collisions.Set(true);
            Target.Fetch();
        }

        private static void UpdateDesktop()
        {
            if (Networking.LocalPlayer == null) return;

            unsafe
            {
                byte shift = ToByte(Input.GetKey(KeyCode.LeftShift));
                float _speed = speed;

                Networking.LocalPlayer.gameObject.transform.position +=
                    Target.value.forward * _speed * Time.deltaTime *
                        (ToByte(Input.GetKey(KeyCode.W)) + ~ToByte(Input.GetKey(KeyCode.S)) + 1) * (shift * 8 + 1)
                    + Target.value.right * _speed * Time.deltaTime *
                        (ToByte(Input.GetKey(KeyCode.D)) + ~ToByte(Input.GetKey(KeyCode.A)) + 1) * (shift * 8 + 1)
                    + Target.value.up * _speed * Time.deltaTime *
                        (ToByte(Input.GetKey(KeyCode.E)) + ~ToByte(Input.GetKey(KeyCode.Q)) + 1) * (shift * 8 + 1);
            }

            Networking.LocalPlayer.SetVelocity(new Vector3(0f, 0f, 0f));
        }

        private static void UpdateVR()
        {
            if (Networking.LocalPlayer == null) return;
            float _speed = speed;

            Networking.LocalPlayer.gameObject.transform.position +=
                Target.value.forward * _speed * Time.deltaTime * Input.GetAxis("Vertical")
                + Target.value.right * _speed * Time.deltaTime * Input.GetAxis("Horizontal")
                + Target.value.up * _speed * Time.deltaTime * Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical");

            Networking.LocalPlayer.SetVelocity(new Vector3(0f, 0f, 0f));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe byte ToByte(bool from) => *(byte*)&from;

        private static class Collisions
        {
            private static Collider collider;

            public static void Set(bool state)
            {
                if (collider == null)
                {
                    if (Networking.LocalPlayer == null) return;
                    collider = Networking.LocalPlayer.gameObject.GetComponent<Collider>();
                }

                collider.enabled = !state;
            }
        }

        private static class Target
        {
            public static Transform value;

            public static void Fetch()
            {
                if (Networking.LocalPlayer == null)
                    return;

                value = _directional
                    ? GameObject.Find("_Application/TrackingVolume/TrackingSteam(Clone)/SteamCamera/[CameraRig]/Neck/Camera (head)").transform
                    : Networking.LocalPlayer.gameObject.transform;
            }
        }

        private static class Hooks
        {
            static Hooks() => Harmony.CreateAndPatchAll(typeof(Hooks));

            [HarmonyPrefix, HarmonyPatch(typeof(Physics), nameof(Physics.gravity), MethodType.Setter)]
            public static bool Hook_set_gravity(Vector3 __0)
            {
                if (__0.magnitude == 0)
                    return true;

                oGrav = __0;
                return !Enabled;
            }
        }
    }
}
