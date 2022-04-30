using BepInEx.Configuration;
using System;
using UnityEngine;
using VRC.SDKBase;

namespace KiraiMod.Modules.Movement
{
    [Module]
    public static class ClickTeleport
    {
        public static ConfigEntry<bool> enabled = Plugin.cfg.Bind("Movement.ClickTeleport", "Enabled", false, "Should you be able to teleport using left ctrl and left mouse click");
        public static ConfigEntry<int> range = Plugin.cfg.Bind("Movement.ClickTeleport", "Range", 10_000, "How far should you be able to teleport");
        public static Camera camera;

        static ClickTeleport()
        {
            Events.UIManagerLoaded += () => camera = Camera.main;

            GUI.Groups.Loaded += () => GUI.Groups.Movement.AddElement("ClickTeleport", enabled.Value).Bound.Bind(enabled);

            enabled.SettingChanged += ((EventHandler)((sender, args) =>
            {
                if (enabled.Value)
                    Events.Update += OnUpdate;
                else Events.Update -= OnUpdate;
            })).Invoke();
        }

        private static void OnUpdate()
        {
            if (Input.GetKey(KeyCode.LeftControl) 
                && Input.GetKeyUp(KeyCode.Mouse0) 
                && Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, range.Value))
                Networking.LocalPlayer.gameObject.transform.position = hit.point;
        }
    }
}