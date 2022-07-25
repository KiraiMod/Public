using UnityEngine;
using VRC.SDKBase;

namespace KiraiMod.Modules.Pickups
{
    [Module]
    public static class Orbit
    {
        private static bool _enabled;
        [Configure<bool>("Pickups.Orbit.Enabled", false)]
        public static bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled = value)
                {
                    player = Core.Types.UserSelectionManager.SelectedPlayer ?? Networking.LocalPlayer;
                    pickups = Object.FindObjectsOfType<VRC_Pickup>();
                    Events.Player.Left += CheckTargetMissing;
                    Events.Update += Update;
                }
                else
                {
                    Events.Player.Left -= CheckTargetMissing;
                    Events.Update -= Update;
                }
            }
        }

        [Configure<float>("Pickups.Orbit.Speed", 1)]
        public static float Speed;

        [Configure<float>("Pickups.Orbit.Distance", 1)]
        public static float Distance;

        [Configure<float>("Pickups.Orbit.Offset", 0)]
        public static float Offset;

        [Configure<bool>("Pickups.Orbit.Head", false)]
        public static bool Head;

        private static VRC_Pickup[] pickups;
        private static VRCPlayerApi player;

        private static void Update()
        {
            if (player == null)
            {
                Enabled = false;
                return;
            }

            float degrees = 360 / pickups.Length;

            Vector3 target = Head
                ? player.GetBonePosition(HumanBodyBones.Head)
                : player.gameObject.transform.position;

            for (int i = 0; i < pickups.Length; i++)
            {
                VRC_Pickup pickup = pickups[i];

                if (pickup is null)
                    continue;

                if (Networking.GetOwner(pickup.gameObject) != Networking.LocalPlayer)
                    Networking.SetOwner(Networking.LocalPlayer, pickup.gameObject);

                pickup.transform.position = target + new Vector3(
                    Mathf.Sin(Time.time * Speed + degrees * i) * Distance, Offset,
                    Mathf.Cos(Time.time * Speed + degrees * i) * Distance
                );
            }
        }

        private static void CheckTargetMissing(Core.Types.Player leftPlayer)
        {
            if (leftPlayer.VRCPlayerApi.playerId == player.playerId)
                Enabled = false;
        }
    }
}
