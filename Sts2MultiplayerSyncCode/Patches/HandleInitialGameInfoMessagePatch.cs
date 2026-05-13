using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
using Sts2MultiplayerSync.Runtime;

namespace Sts2MultiplayerSync.Patches;

[HarmonyPatch(typeof(JoinFlow), "HandleInitialGameInfoMessage")]
public static class HandleInitialGameInfoMessagePatch
{
    public static void Prefix(ref InitialGameInfoMessage message)
    {
        try
        {
            var hostMods = message.mods != null
                ? new List<string>(message.mods)
                : new List<string>();
            var localMods = ModManager.GetGameplayRelevantModNameList() ?? new List<string>();

            ModSyncState.RecordReceived(hostMods, localMods);

            if (ModSyncState.MissingOnLocal.Count > 0 || ModSyncState.MissingOnHost.Count > 0)
            {
                MainFile.Logger.Info(
                    $"mod mismatch detected — host has {ModSyncState.MissingOnLocal.Count} extra, " +
                    $"we have {ModSyncState.MissingOnHost.Count} extra.");

                if (ModSyncState.BypassEnabled)
                {
                    message.mods = new List<string>(localMods);
                    MainFile.Logger.Info(
                        $"BypassEnabled=true → masked host mod list to mirror local " +
                        $"({localMods.Count} entries) so ConnectAsync mismatch check passes.");
                }
                else
                {
                    MainFile.Logger.Info("BypassEnabled=false → leaving host mod list untouched; ConnectAsync will reject.");
                }
            }
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"prefix on HandleInitialGameInfoMessage threw: {ex}");
        }
    }
}
