using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization;

namespace Sts2MultiplayerSync.Localization;

public static class Strings
{
    public static string Get(string key)
    {
        var lang = GetCurrentLanguage();
        if (_tables.TryGetValue(lang, out var table) && table.TryGetValue(key, out var value)) return value;
        if (_tables.TryGetValue("ENG", out var eng) && eng.TryGetValue(key, out var engValue)) return engValue;
        return key;
    }

    public static string Get(string key, params object[] args)
    {
        var raw = Get(key);
        try { return args.Length == 0 ? raw : string.Format(raw, args); }
        catch { return raw; }
    }

    private static string GetCurrentLanguage()
    {
        try { return (LocManager.Instance?.Language ?? "ENG").ToUpperInvariant(); }
        catch { return "ENG"; }
    }

    private static readonly Dictionary<string, Dictionary<string, string>> _tables = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ENG"] = new()
        {
            ["modal_title"] = "Sts2 Multiplayer Sync",
            ["btn_restart_now"] = "Restart now",
            ["btn_cancel"] = "Cancel",

            ["host_warning_title"] = "Multiplayer Sync — Gameplay-affecting mods detected",
            // args: {0} = count, {1} = list (multi-line), {2} = remaining seconds
            ["host_warning_body"] = "{0} mod(s) declare affects_gameplay=true. These will block multiplayer matchmaking unless joined by a client with the same set:\n\n{1}\n\nThese mods are staged for disable in settings.save. Restart STS2 once to apply, or Cancel to keep them.\n\nAuto-restart in {2}s.",

            ["client_mismatch_title"] = "Multiplayer Sync — Mod mismatch with host",
            ["client_missing_install_header"] = "Install these mods to match the host:",
            ["client_missing_disable_header"] = "Disable these mods (host doesn't have them):",
            // args: {0} = detail block, {1} = remaining seconds
            ["client_mismatch_body_disable"] = "{0}\n\nThe mods above were staged for disable in settings.save. Restart STS2 once to apply, or Cancel to keep them and join later.\n\nAuto-restart in {1}s.",
            // args: {0} = detail block, {1} = remaining seconds
            ["client_mismatch_body_install_only"] = "{0}\n\nInstall the listed mods, then re-launch STS2 and rejoin. Auto-restart in {1}s (Cancel if you'll install first).",
        },
        ["KOR"] = new()
        {
            ["modal_title"] = "Sts2 멀티플레이 싱크",
            ["btn_restart_now"] = "지금 재시작",
            ["btn_cancel"] = "취소",

            ["host_warning_title"] = "멀티플레이 싱크 — 게임플레이 영향 mod 감지",
            ["host_warning_body"] = "{0}개 mod 가 affects_gameplay=true 로 표시되어 있어요. 같은 mod 셋을 가진 client 가 아니면 멀티플레이 매칭이 막혀요:\n\n{1}\n\n이 mod 들을 settings.save 에 비활성화 예약했어요. STS2 를 한 번 재시작하면 적용되고, 취소하면 그대로 유지돼요.\n\n{2}초 뒤 자동 재시작.",

            ["client_mismatch_title"] = "멀티플레이 싱크 — host 와 mod 가 안 맞아요",
            ["client_missing_install_header"] = "이 mod 들을 설치하세요 (host 가 깔아 둠):",
            ["client_missing_disable_header"] = "이 mod 들을 비활성화하세요 (host 에 없어요):",
            ["client_mismatch_body_disable"] = "{0}\n\n위 mod 들을 settings.save 에 비활성화 예약했어요. STS2 를 한 번 재시작하면 적용되고, 취소하면 그대로 두고 나중에 join 할 수 있어요.\n\n{1}초 뒤 자동 재시작.",
            ["client_mismatch_body_install_only"] = "{0}\n\n위 mod 들을 설치한 뒤 STS2 를 다시 띄우고 재시도하세요. {1}초 뒤 자동 재시작 (직접 설치할 거면 취소).",
        },
    };
}
