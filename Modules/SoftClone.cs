using ExitGames.Client.Photon;
using Il2CppSystem.Collections;
using System;
using System.Linq;
using System.Reflection;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    // todo: this module
    //[Module]
    public static class SoftClone
    {
        private static bool _enabled = false;
        [Configure<bool>("Visuals.Soft Clone.Enabled", false, Saved: false)]
        public static bool Enabled
        {
            get => _enabled;
            set
            {
                Plugin.Logger.LogInfo("ABDC");
                Plugin.Logger.LogInfo(m_ReloadAvatar);
                Plugin.Logger.LogInfo(m_ReloadAvatar.DeclaringType);
                
                if (_enabled == value) return;
                _enabled = value;

                Plugin.Logger.LogMessage("A");
                if (value)
                {
                    if (Modules.Players.Target?.Inner == null)
                    {
                        Plugin.Logger.LogMessage("No player selected");
                        return;
                    }

                    object inner = Modules.Players.Target.VRCPlayer;
                    if (inner is null)
                    {
                        Plugin.Logger.LogError("Failed to get inner VRCPlayer");
                        return;
                    }

                    object hashtable = Core.Types.VRCPlayer.m_GetHashtable.GetValue(inner);
                    if (hashtable is null)
                    {
                        Plugin.Logger.LogError("Failed to find hashtable on player");
                        return;
                    }

                    avatarDict = ((Hashtable)hashtable)["avatarDict"];
                }

                ReloadAvatar();
            }
        }

        private static readonly MethodInfo m_GetPlayer = typeof(MonoBehaviour)
            .GetMethod(nameof(MonoBehaviour.GetComponent), Array.Empty<Type>())
            ?.MakeGenericMethod(Core.Types.Player.Type);

        private static readonly MethodInfo m_ReloadAvatar = Core.Types.VRCPlayer.Type.GetMethods()
            .FirstOrDefault(x =>
            {
                if (x.ReturnType != typeof(void))
                    return false;
        
                ParameterInfo[] parms = x.GetParameters();
                return parms.Length > 0
                    && parms[0].ParameterType == typeof(bool)
                    && parms.Any(parm => parm.IsOptional)
                    && XrefScanner.UsedBy(x)
                        .Any(xref => xref.Type == XrefType.Method && xref.TryResolve()?.Name == "ReloadAvatarNetworkedRPC");
            });
        
        private static Il2CppSystem.Object avatarDict;

        // todo: LogAs
        static SoftClone() =>
            Plugin.HarmonyInstance.Patch(
                Core.Types.VRCNetworkingClient.m_OnEvent,
                typeof(SoftClone).GetMethod(nameof(HookOnEvent), BindingFlags.NonPublic | BindingFlags.Static).ToHM()
            );

        [Interact("Visuals.Soft Clone.Reload Avatar")]
        public static void ReloadAvatar() => m_ReloadAvatar.Invoke(m_GetPlayer.Invoke(Networking.LocalPlayer, Array.Empty<object>()), new object[1] { true });

        private static void HookOnEvent(ref EventData __0)
        {
            if (_enabled
                && __0.Code == 42
                && avatarDict != null
                && __0.Sender == Networking.LocalPlayer.playerId
            ) __0.CustomData.Cast<Hashtable>()["avatarDict"] = avatarDict;
        }
    }
}
