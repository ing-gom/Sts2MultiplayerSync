using System;
using System.IO;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using Sts2MultiplayerSync.Discovery;
using Sts2MultiplayerSync.Runtime;

namespace Sts2MultiplayerSync;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "Sts2MultiplayerSync";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; }
        = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static string ManagerDataDir { get; private set; } = "";

    public static void Initialize()
    {
        try
        {
            ApplyHarmonyPatches();
            Run();
        }
        catch (Exception ex)
        {
            Logger.Warn($"init failed: {ex}");
        }
    }

    private static void ApplyHarmonyPatches()
    {
        var harmony = new Harmony(ModId);
        harmony.PatchAll(typeof(MainFile).Assembly);
        Logger.Info("Harmony patches applied.");
    }

    private static void Run()
    {
        var userDataDir = OS.GetUserDataDir();
        ManagerDataDir = Path.Combine(userDataDir, ModId);
        Directory.CreateDirectory(ManagerDataDir);

        var gameplayMods = AffectsGameplayScanner.Scan();
        Logger.Info($"detected {gameplayMods.Count} mod(s) with affects_gameplay=true:");
        foreach (var m in gameplayMods)
        {
            Logger.Info($"  [gameplay] {m.ModId} v{m.Version ?? "?"} (author: {m.Author ?? "?"})");
        }

        Logger.Info($"BypassEnabled = {ModSyncState.BypassEnabled} (default) — " +
                    "client-side mod-mismatch on join is masked by default. " +
                    "Set Sts2MultiplayerSync.Runtime.ModSyncState.BypassEnabled = false to disable.");

        ModSyncState.OnMismatchDetected += OnClientMismatchDetected;

        HostModWarningOverlay.MaybeShow(ManagerDataDir, gameplayMods);
    }

    private static void OnClientMismatchDetected()
    {
        ClientMismatchOverlay.Show(ManagerDataDir);
    }
}
