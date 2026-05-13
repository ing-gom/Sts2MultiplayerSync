using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Modding;

namespace Sts2MultiplayerSync.Discovery;

public record GameplayMod(string ModId, string DisplayName, string? Version, string? Author);

public static class AffectsGameplayScanner
{
    private static readonly HashSet<string> SelfExcluded = new(StringComparer.OrdinalIgnoreCase)
    {
        "Sts2MultiplayerSync",
    };

    public static List<GameplayMod> Scan()
    {
        var result = new List<GameplayMod>();
        foreach (var mod in ModManager.Mods)
        {
            var m = mod.manifest;
            if (m == null) continue;
            if (m.id == null) continue;
            if (SelfExcluded.Contains(m.id)) continue;
            if (mod.state != ModLoadState.Loaded) continue;
            if (!m.affectsGameplay) continue;

            result.Add(new GameplayMod(
                ModId: m.id,
                DisplayName: m.name ?? m.id,
                Version: m.version,
                Author: m.author));
        }
        return result.OrderBy(m => m.ModId, StringComparer.OrdinalIgnoreCase).ToList();
    }
}
