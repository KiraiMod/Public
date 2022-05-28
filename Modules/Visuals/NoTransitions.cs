using BepInEx.Configuration;
using System;
using UnityEngine;

namespace KiraiMod.Modules.Visuals
{
    // the idea for this comes from that person that was friends with keafy
    // their mod had an issue, i think it was causing the game to crash
    [Module]
    public static class NoTransitions
    {
        public static ConfigEntry<bool> Enabled = Plugin.Configuration.Bind("Visuals", "No Transitions", true, "Should the black fade between worlds be removed?");

        public static GameObject Fade;

        static NoTransitions()
        {
            Events.UIManagerLoaded += () =>
            {
                Fade = GameObject.Find("UserInterface/PlayerDisplay/BlackFade/inverted_sphere");

                Enabled.SettingChanged += ((EventHandler)((sender, args) => Fade.active = !Enabled.Value)).Invoke();
            };
        }
    }
}
