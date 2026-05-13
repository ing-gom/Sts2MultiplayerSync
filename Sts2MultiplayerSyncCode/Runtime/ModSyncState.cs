using System;
using System.Collections.Generic;

namespace Sts2MultiplayerSync.Runtime;

public static class ModSyncState
{
    public static bool BypassEnabled { get; set; } = true;

    public static IReadOnlyList<string> LastReceivedHostMods { get; private set; } = Array.Empty<string>();

    public static IReadOnlyList<string> LastLocalMods { get; private set; } = Array.Empty<string>();

    public static IReadOnlyList<string> MissingOnLocal { get; private set; } = Array.Empty<string>();

    public static IReadOnlyList<string> MissingOnHost { get; private set; } = Array.Empty<string>();

    public static DateTime? LastReceivedAt { get; private set; }

    public static event Action? OnMismatchDetected;

    public static void RecordReceived(IReadOnlyList<string> hostMods, IReadOnlyList<string> localMods)
    {
        LastReceivedHostMods = hostMods;
        LastLocalMods = localMods;

        var hostSet = new HashSet<string>(hostMods, StringComparer.OrdinalIgnoreCase);
        var localSet = new HashSet<string>(localMods, StringComparer.OrdinalIgnoreCase);
        var missingOnLocal = new List<string>();
        foreach (var id in hostMods) if (!localSet.Contains(id)) missingOnLocal.Add(id);
        var missingOnHost = new List<string>();
        foreach (var id in localMods) if (!hostSet.Contains(id)) missingOnHost.Add(id);

        MissingOnLocal = missingOnLocal;
        MissingOnHost = missingOnHost;
        LastReceivedAt = DateTime.UtcNow;

        if (missingOnLocal.Count > 0 || missingOnHost.Count > 0)
        {
            try { OnMismatchDetected?.Invoke(); }
            catch (Exception ex) { MainFile.Logger.Warn($"OnMismatchDetected handler error: {ex.Message}"); }
        }
    }
}
