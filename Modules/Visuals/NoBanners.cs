using BepInEx.Configuration;
using System;
using UnityEngine;

namespace KiraiMod.Modules.Visuals
{
    [Module]
    public static class NoBanners
    {
        public static ConfigEntry<bool> RemoveBanners = Plugin.cfg.Bind("Visuals", "No Banners", true, "Should the banners on the quick menu be removed?");

        private static Transform Banners;

        // for some reason the UserInterface is not ready by UIManagerInit
        static NoBanners() => Events.EmptyLoaded += Initialize;

        private static void Initialize()
        {
            Banners = GameObject.Find("UserInterface")
                ?.transform
                ?.Find("Canvas_QuickMenu(Clone)/Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Carousel_Banners");

            RemoveBanners.SettingChanged += ((EventHandler)((sender, args) => Banners.gameObject.active = !RemoveBanners.Value)).Invoke();

            Events.EmptyLoaded -= Initialize;
        }
    }
}
