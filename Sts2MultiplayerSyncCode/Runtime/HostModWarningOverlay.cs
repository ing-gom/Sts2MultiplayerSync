using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Sts2MultiplayerSync.Config;
using Sts2MultiplayerSync.Discovery;

namespace Sts2MultiplayerSync.Runtime;

public static class HostModWarningOverlay
{
    private const string SessionMarkerName = "host_warning_shown.session";
    private const double SuppressionWindowHours = 2.0;

    private static List<GameplayMod>? _pendingMods;

    public static void MaybeShow(string managerDataDir, List<GameplayMod> gameplayMods)
    {
        if (gameplayMods == null || gameplayMods.Count == 0)
        {
            MainFile.Logger.Info("no affects_gameplay=true mods to warn about — skipping host overlay.");
            return;
        }

        var marker = Path.Combine(managerDataDir, SessionMarkerName);
        if (File.Exists(marker))
        {
            try
            {
                var mtime = File.GetLastWriteTimeUtc(marker);
                if ((DateTime.UtcNow - mtime).TotalHours < SuppressionWindowHours)
                {
                    MainFile.Logger.Info($"host warning suppressed — last shown {mtime:O} (window {SuppressionWindowHours}h).");
                    return;
                }
            }
            catch { }
        }

        try { File.WriteAllText(marker, DateTime.UtcNow.ToString("O")); }
        catch (Exception ex) { MainFile.Logger.Warn($"could not write session marker: {ex.Message}"); }

        _pendingMods = gameplayMods;
        var listText = string.Join("\n", gameplayMods.Select(m => $"  - {m.DisplayName} ({m.ModId})"));

        ApplyDisableToSettings(gameplayMods);

        RestartCountdownModal.ShowOrReset(
            managerDataDir,
            seconds: 15,
            titleKey: "host_warning_title",
            bodyKey: "host_warning_body",
            onCancel: RevertDisable,
            extraBodyArgs: new object[] { gameplayMods.Count, listText });
    }

    private static void ApplyDisableToSettings(List<GameplayMod> mods)
    {
        var userDataDir = OS.GetUserDataDir();
        var settings = Sts2SettingsWriter.FindAndLoad(userDataDir);
        if (settings == null)
        {
            MainFile.Logger.Warn("settings.save not found — cannot stage disable changes.");
            return;
        }

        var desired = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in mods) desired[m.ModId] = false;

        var changed = Sts2SettingsWriter.ApplyModEnabledState(settings, desired);
        if (changed)
        {
            Sts2SettingsWriter.Save(settings);
            MainFile.Logger.Info($"staged disable for {mods.Count} gameplay mod(s) in settings.save — will apply on next restart.");
        }
        else
        {
            MainFile.Logger.Info("staged disable was a no-op (already disabled or not in mod_list).");
        }
    }

    private static void RevertDisable()
    {
        if (_pendingMods == null) return;
        var userDataDir = OS.GetUserDataDir();
        var settings = Sts2SettingsWriter.FindAndLoad(userDataDir);
        if (settings == null) return;

        var desired = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in _pendingMods) desired[m.ModId] = true;
        var changed = Sts2SettingsWriter.ApplyModEnabledState(settings, desired);
        if (changed)
        {
            Sts2SettingsWriter.Save(settings);
            MainFile.Logger.Info("host chose Cancel — reverted staged disable in settings.save.");
        }
    }
}
