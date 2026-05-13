# Sts2MultiplayerSync

A Slay the Spire 2 mod that lets you play multiplayer when your mod loadout doesn't match the host's. Bypasses the `ModMismatch` connection rejection while surfacing the actual mod diff in-game so you (and the host) can decide what to install or disable instead of blindly reconnecting.

🇰🇷 [한국어 README](README.ko.md)

> ⚠️ **v0.1.0-alpha — host-side only verified.**
> Host-side `affects_gameplay` scanner, warning modal, staged-disable flow, and auto-restart are verified live (single-instance boot test). The **client-side `ModMismatch` bypass and mismatch overlay are NOT yet verified** — that path requires two running STS2 instances (host + client). Treat the client bypass as experimental until 0.1.0 (non-alpha) ships.

## What it does

STS2 multiplayer rejects any client whose `affects_gameplay=true` mod list doesn't exactly match the host's. The check happens client-side inside `JoinFlow.HandleInitialGameInfoMessage` → `ConnectAsync` and throws `ClientConnectionFailedException("ModMismatch")` on any diff.

This mod adds two pieces:

- **Client-side bypass (`HandleInitialGameInfoMessagePatch`).** Harmony Prefix on `JoinFlow.HandleInitialGameInfoMessage`. When the host's mod list arrives, we record the real diff for the UI, then *mask* the message's `mods` field with our own local list so the upstream comparison sees zero diff and lets the connection proceed. Toggle via `ModSyncState.BypassEnabled` (default `true`).
- **Host-side warning (`HostModWarningOverlay`).** On boot, scans `ModManager.Mods` for any with `affects_gameplay=true` and pops a 15-second modal listing them. The mods are staged for disable in `settings.save`; confirm to auto-restart and apply, cancel to revert. Suppressed for 2 hours after each show so it isn't spammy.
- **Client-side mismatch overlay (`ClientMismatchOverlay`).** When the bypass fires, the recorded diff is shown to the client with two sections — *"install these to match host"* and *"disable these (host doesn't have them)"*. The extra-mods set is staged for disable; restart applies it.

## How it works (in one paragraph)

`JoinFlow.HandleInitialGameInfoMessage` runs on the client when host's first hello message arrives. The struct is passed by value, but Harmony lets us patch it with `ref` so the modified `message.mods` is what `JoinFlow.Begin`'s `await` unwraps. With the field rewritten to mirror the client's own gameplay-relevant mod list, the downstream `list.Except(list2)` / `list2.Except(list)` checks both come back empty, and `ConnectAsync` skips the throw and continues the join flow. The host needs zero patches — host-side `OnPeerConnected` never validates client mods anyway.

## Caveats

- **Desync risk is real.** If the masked-out mod actually affects gameplay (card stats, RNG, new characters), STS2's mid-game `ChecksumTracker` will catch the divergence and kick the client with `NetError.StateDivergence`. This mod doesn't bypass the in-game checksum — that's intentional. The bypass only gets you past the *join* gate; staying in sync is up to which mods you actually run.
- **Host-side warning is a heuristic.** `affects_gameplay=true` is set by the mod author; some mods misdeclare. Treat the warning as "these are likely to cause mismatch", not "these are guaranteed to break anything".
- **No Steam lobby preview.** STS2 doesn't publish mod info in Steam lobby metadata, so the mismatch can only be detected *after* the connection handshake. The overlay is reactive (post-join-attempt), not proactive (pre-join).

## Install

1. Download the latest release zip.
2. Extract the `Sts2MultiplayerSync` folder into `<Slay the Spire 2 install>/mods/`.
3. Launch STS2. If you're the host and have gameplay-affecting mods, the warning modal appears within ~5 seconds of reaching the main menu.

## Compatibility

- **STS2 mod load order**: this mod doesn't require being first. The Harmony Prefix on `JoinFlow.HandleInitialGameInfoMessage` registers during `MainFile.Initialize` and is active for the rest of the session.
- **Coexists with Sts2SkinManager** and other Sts2* sister mods.

## License

MIT. See `LICENSE`.
