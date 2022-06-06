using KiraiMod.Core.Utils;
using UnityEngine;

namespace KiraiMod.Modules.Visuals
{
    [Module]
    public static class NearClip
    {
        private static bool _enabled;
        [Configure<bool>("Visuals.Near Clip.Enabled", true)]
        public static bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                if (loaded && !menu.active)
                    camera.nearClipPlane = value ? _target : _regular;
            }
        }

        private static float _target;
        [Configure<float>("Visuals.Near Clip.Target", 0.01f)]
        public static float Target
        {
            get => _target;
            set
            {
                _target = value;

                if (loaded && !menu.active)
                    camera.nearClipPlane = value;
            }
        }

        private static float _regular;
        [Configure<float>("Visuals.Near Clip.Regular", 0.05f)]
        public static float Regular
        {
            get => _regular;
            set
            {
                _regular = value;

                if (loaded && menu.active)
                    camera.nearClipPlane = value;
            }
        }

        private static bool loaded = false;

        private static GameObject menu;
        private static Camera camera;

        static NearClip() => Events.EmptyLoaded += OnLoaded;

        private static void OnLoaded()
        {
            Events.EmptyLoaded -= OnLoaded;

            menu = GameObject.Find("UserInterface").transform.Find("Canvas_QuickMenu(Clone)").gameObject;
            camera = Camera.main;

            ActivationListener listener = menu.AddComponent<ActivationListener>();
            listener.Enabled += ToggleMenu;
            listener.Disabled += ToggleMenu;

            loaded = true;
        }

        private static void ToggleMenu()
        {
            if (!_enabled) return;

            camera.nearClipPlane = menu.active ? _regular : _target;
        }
    }
}
