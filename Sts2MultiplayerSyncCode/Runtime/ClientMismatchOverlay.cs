using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Sts2MultiplayerSync.Config;

namespace Sts2MultiplayerSync.Runtime;

public static class ClientMismatchOverlay
{
    private static DateTime _lastShownUtc = DateTime.MinValue;
    private const double DebounceSeconds = 3.0;

    public static void Show(string managerDataDir)
    {
        var now = DateTime.UtcNow;
        if ((now - _lastShownUtc).TotalSeconds < DebounceSeconds) return;
        _lastShownUtc = now;

        var missingOnLocal = ModSyncState.MissingOnLocal;
        var missingOnHost = ModSyncState.MissingOnHost;

        if (missingOnLocal.Count == 0 && missingOnHost.Count == 0) return;

        var bodyParts = new List<string>();
        if (missingOnLocal.Count > 0)
        {
            bodyParts.Add(BuildSection("client_missing_install_header", missingOnLocal));
        }
        if (missingOnHost.Count > 0)
        {
            bodyParts.Add(BuildSection("client_missing_disable_header", missingOnHost));
        }
        var detail = string.Join("\n\n", bodyParts);

        if (missingOnHost.Count > 0)
        {
            ApplyDisableForExcessMods(missingOnHost);

            RestartCountdownModal.ShowOrReset(
                managerDataDir,
                seconds: 15,
                titleKey: "client_mismatch_title",
                bodyKey: "client_mismatch_body_disable",
                onCancel: () => RevertDisable(missingOnHost),
                extraBodyArgs: new object[] { detail });
        }
        else
        {
            RestartCountdownModal.ShowOrReset(
                managerDataDir,
                seconds: 15,
                titleKey: "client_mismatch_title",
                bodyKey: "client_mismatch_body_install_only",
                onCancel: null,
                extraBodyArgs: new object[] { detail });
        }
    }

    private static string BuildSection(string headerKey, IReadOnlyList<string> mods)
    {
        var lines = mods.Select(m => $"  - {m}");
        return Localization.Strings.Get(headerKey) + "\n" + string.Join("\n", lines);
    }

    private static void ApplyDisableForExcessMods(IReadOnlyList<string> modIds)
    {
        var userDataDir = OS.GetUserDataDir();
        var settings = Sts2SettingsWriter.FindAndLoad(userDataDir);
        if (settings == null)
        {
            MainFile.Logger.Warn("client mismatch: settings.save not found — cannot stage disable.");
            return;
        }

        var desired = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in modIds) desired[id] = false;
        var changed = Sts2SettingsWriter.ApplyModEnabledState(settings, desired);
        if (changed)
        {
            Sts2SettingsWriter.Save(settings);
            MainFile.Logger.Info($"client mismatch: staged disable for {modIds.Count} extra mod(s) in settings.save.");
        }
    }

    private static void RevertDisable(IReadOnlyList<string> modIds)
    {
        var userDataDir = OS.GetUserDataDir();
        var settings = Sts2SettingsWriter.FindAndLoad(userDataDir);
        if (settings == null) return;
        var desired = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in modIds) desired[id] = true;
        var changed = Sts2SettingsWriter.ApplyModEnabledState(settings, desired);
        if (changed)
        {
            Sts2SettingsWriter.Save(settings);
            MainFile.Logger.Info("client mismatch: user chose Cancel — reverted staged disable.");
        }
    }
}
