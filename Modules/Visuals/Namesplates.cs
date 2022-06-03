using KiraiMod.Core.TagAPI;
using UnityEngine;

namespace KiraiMod.Modules.Visuals
{
    [Module]
    public static class Namesplates
    {
        static Namesplates()
        {
            MasterTag = new(player => new()
            {
                Visible = _enabled && player.VRCPlayerApi.isMaster,
                Text = "Master",
                TextColor = Color.white,
                BackgroundColor = Color.black
            }, 1000);

            // alternatively, find the master and only recalculate their tag instance
            Events.Player.Left += _ => MasterTag.CalculateAll();
        }

        private static bool _enabled;
        [Configure<bool>("Visuals.Nameplates", true)]
        public static bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value) return;
                _enabled = value;

                MasterTag.CalculateAll();
            }
        }

        public static Tag MasterTag;
    }
}
