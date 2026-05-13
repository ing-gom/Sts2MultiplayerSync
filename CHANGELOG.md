# Changelog

All notable changes to Sts2MultiplayerSync are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0-alpha] - 2026-05-13

### Verified live (single-instance boot)
- Host-side scanner detects all 4 `affects_gameplay=true` mods in the test environment (NeowAlwaysRewards, Ryoshu, Sts2SilkenTressBackport, Sts2UndoMod) — Booba-Necrobinder-Mod correctly excluded because Sts2SkinManager already disabled it for a different active skin.
- Warning modal renders, the 15-second countdown elapses, RestartHelper spawns the Steam auto-restart helper, STS2 quits cleanly.
- After auto-restart, `settings.save` shows all 4 mods at `is_enabled=false` and STS2 logs `Skipping loading mod ... it is set to disabled in settings` for each — the bypass actually took effect.
- Second boot's scanner finds 0 mods (`detected 0 mod(s) with affects_gameplay=true`); 2-hour debounce prevents the modal from re-firing.
- Cancel path reverts both file and in-memory state.

### Not yet verified
- `JoinFlow.HandleInitialGameInfoMessage` Prefix actually firing during a real multiplayer connection — needs two STS2 instances.
- `ClientMismatchOverlay` rendering after a real `ModMismatch` event.
- In-game `ChecksumTracker` continuing to gate desync after bypass (we intentionally don't patch it).

### Fixed during alpha verification
- **In-memory ModList must be mutated alongside `settings.save`.** First attempt only rewrote the JsonNode tree on disk; STS2 quits via our auto-restart and writes its own `_settings.ModList` back to disk, overwriting our changes with `is_enabled=true`. Fix: `Sts2SettingsWriter.MutateInMemoryModList(desired)` is now called alongside `ApplyModEnabledState` in both Host and Client overlays. Verified: `staged disable for 4 gameplay mod(s): file=True mem=4. Applies on next restart.` (commit `6d051cf`)

## [0.1.0] - 2026-05-13

### Added
- **Client-side ModMismatch bypass.** Harmony Prefix on `JoinFlow.HandleInitialGameInfoMessage` masks the host's mod list with the client's local list (via `ref` parameter rewrite), so the downstream `ConnectAsync` comparison sees zero diff and the join proceeds. Toggle via `ModSyncState.BypassEnabled`.
- **Host-side affects_gameplay scanner + warning modal.** Scans `ModManager.Mods` at boot for any with `affects_gameplay=true`, stages those for disable in `settings.save`, and surfaces a 15-second restart-countdown modal. Cancel reverts the staged disable. Suppressed for 2 hours after each show.
- **Client-side mismatch overlay.** When the bypass fires and a real diff is detected, a modal lists exactly which mods to install (host has them, client doesn't) and which to disable (client has them, host doesn't). The disable set is auto-staged in `settings.save`.
- **English + Korean locales.** Other 14 languages will fall back to English for now.
- **`Sts2SettingsWriter`** ported from Sts2SkinManager for safe `settings.save` mutation with a `.mpsync.bak` backup on first write.
- **`RestartCountdownModal`** ported from Sts2SkinManager; extended with `params object[] extraBodyArgs` so different overlays can supply different body templates.
