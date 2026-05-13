# Changelog

All notable changes to Sts2MultiplayerSync are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-05-13

### Added
- **Client-side ModMismatch bypass.** Harmony Prefix on `JoinFlow.HandleInitialGameInfoMessage` masks the host's mod list with the client's local list (via `ref` parameter rewrite), so the downstream `ConnectAsync` comparison sees zero diff and the join proceeds. Toggle via `ModSyncState.BypassEnabled`.
- **Host-side affects_gameplay scanner + warning modal.** Scans `ModManager.Mods` at boot for any with `affects_gameplay=true`, stages those for disable in `settings.save`, and surfaces a 15-second restart-countdown modal. Cancel reverts the staged disable. Suppressed for 2 hours after each show.
- **Client-side mismatch overlay.** When the bypass fires and a real diff is detected, a modal lists exactly which mods to install (host has them, client doesn't) and which to disable (client has them, host doesn't). The disable set is auto-staged in `settings.save`.
- **English + Korean locales.** Other 14 languages will fall back to English for now.
- **`Sts2SettingsWriter`** ported from Sts2SkinManager for safe `settings.save` mutation with a `.mpsync.bak` backup on first write.
- **`RestartCountdownModal`** ported from Sts2SkinManager; extended with `params object[] extraBodyArgs` so different overlays can supply different body templates.
