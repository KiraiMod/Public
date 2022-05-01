using BepInEx.Configuration;
using System;
using System.Collections;
using UnityEngine;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    [Module]
    public static class Logging
    {
        public static ConfigEntry<bool> PlayerJoin = Plugin.cfg.Bind("Logging", "PlayerJoin", true, "Should you be notified when someone joins?");
        public static ConfigEntry<bool> PlayerLeave = Plugin.cfg.Bind("Logging", "PlayerLeave", true, "Should you be notified when someone leaves?");
        public static ConfigEntry<bool> VotesReady = Plugin.cfg.Bind("Logging", "VotesReady", true, "Should you be notified when you can partake in votes?");
        public static ConfigEntry<bool> PortalDrop = Plugin.cfg.Bind("Logging", "PortalDrop", true, "Should you be notified when a portal is dropped?");

        static Logging()
        {
            PlayerJoin.SettingChanged += ((EventHandler)((sender, args) =>
            {
                if (PlayerJoin.Value) Events.Player.Joined += LogPlayerJoined;
                else Events.Player.Joined -= LogPlayerJoined;
            })).Invoke();

            PlayerLeave.SettingChanged += ((EventHandler)((sender, args) =>
            {
                if (PlayerLeave.Value) Events.Player.Left += LogPlayerLeft;
                else Events.Player.Left -= LogPlayerLeft;
            })).Invoke();

            VotesReady.SettingChanged += ((EventHandler)((sender, args) =>
            {
                if (VotesReady.Value) Events.World.InstanceLoaded += StartVoteTimer;
                else Events.World.InstanceLoaded -= StartVoteTimer;
            })).Invoke();

            PortalDrop.SettingChanged += ((EventHandler)((sender, args) =>
            {
                if (PortalDrop.Value) Events.VRCEvent.Recieved += LogPortalDropped;
                else Events.VRCEvent.Recieved -= LogPortalDropped;
            })).Invoke();
        }

        private static void LogPlayerJoined(Core.Types.Player player) => Plugin.log.LogMessage($"{player.APIUser.displayName} joined");
        private static void LogPlayerLeft(Core.Types.Player player) => Plugin.log.LogMessage($"{player.APIUser.displayName} left");
        private static void StartVoteTimer(VRC.Core.ApiWorldInstance _)
        {
            if (votesCoroutine != null)
            {
                votesCoroutine.Stop();
                votesCoroutine = null;
            }

            votesCoroutine = WaitForVotes().Start();
        }

        private static void LogPortalDropped(Core.Types.Player sender, ref VRC_EventHandler.VrcEvent ev, ref VRC_EventHandler.VrcBroadcastType broadcast)
        {
            if (ev.EventType != VRC_EventHandler.VrcEventType.SendRPC
                || ev.ParameterString != "ConfigurePortal")
                return;

            var data = Networking.DecodeParameters(ev.ParameterBytes);
            string wlrd = data[0].ToString();
            string id = data[1].ToString();

            Plugin.log.LogMessage(sender.APIUser.displayName + " dropped a portal");
            Plugin.log.LogInfo(wlrd + ":" + id);
        }

        private static Coroutine votesCoroutine;
        private static readonly WaitForSeconds wait = new(300);
        private static IEnumerator WaitForVotes()
        {
            yield return wait;

            Plugin.log.LogMessage("You can now partake in votes");
        }
    }
}
