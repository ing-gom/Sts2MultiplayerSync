# Sts2MultiplayerSync

호스트와 mod 셋이 정확히 맞지 않아도 멀티플레이를 가능하게 해주는 Slay the Spire 2 mod. `ModMismatch` 연결 거부를 우회하면서 어떤 mod 가 차이나는지 in-game UI 로 보여줘서, host 와 client 가 무작정 재연결 시도가 아니라 의식적으로 "이건 깔고 이건 비활성화하자" 결정할 수 있게 도와줘요.

🇺🇸 [English README](README.md)

## 동작 개요

STS2 멀티플레이는 client 의 `affects_gameplay=true` mod 목록이 host 와 한 글자라도 다르면 join 자체를 거부해요. 검증은 client 쪽 `JoinFlow.HandleInitialGameInfoMessage` → `ConnectAsync` 에서 일어나고 `ClientConnectionFailedException("ModMismatch")` 가 발생하면서 연결 종료.

이 mod 는 세 가지를 제공해요:

- **Client 측 mismatch 우회 (`HandleInitialGameInfoMessagePatch`).** `JoinFlow.HandleInitialGameInfoMessage` 에 Harmony Prefix. host 의 mod list 가 도착하면 (1) UI 표시용으로 실제 diff 를 기록 (2) 메시지의 `mods` 필드를 우리 local mod list 로 *덮어쓰기* — 위쪽의 except 비교가 빈 list 두 개를 받아 연결 진행. `ModSyncState.BypassEnabled` 로 토글 가능 (기본 `true`).
- **Host 측 경고 (`HostModWarningOverlay`).** 부팅 시 `ModManager.Mods` 를 스캔해서 `affects_gameplay=true` mod 들을 찾고 15초 카운트다운 모달로 표시. 그 mod 들은 `settings.save` 에 비활성화 예약 — 확인 누르면 자동 재시작하면서 적용, 취소하면 그대로 유지. 같은 모달은 2시간 동안 다시 안 떠요 (반복 방지).
- **Client 측 mismatch 모달 (`ClientMismatchOverlay`).** 우회가 발동되면 기록해 둔 diff 를 client 에게 두 섹션으로 보여줘요 — *"host 가 깔아 둔 mod 라 설치하세요"* 와 *"host 에 없으니 비활성화하세요"*. 후자는 자동으로 `settings.save` 에 비활성화 예약 → 재시작.

## 한 문단으로 보는 원리

`JoinFlow.HandleInitialGameInfoMessage` 는 host 가 첫 hello 메시지를 보냈을 때 client 에서 호출돼요. struct 가 값 전달이지만 Harmony Prefix 에서 `ref` 로 받으면 수정이 가능해서, 우리가 `message.mods` 를 client 자신의 gameplay-relevant mod list 로 덮어쓰면 그 다음 `JoinFlow.Begin` 의 `await` 가 받는 것도 수정된 메시지. 결과적으로 `list.Except(list2)` / `list2.Except(list)` 둘 다 빈 list → `ConnectAsync` 가 throw 없이 join 흐름 계속. **호스트 측은 어떤 patch 도 필요 없음** — STS2 의 host 측 `OnPeerConnected` 는 client mod 검증을 안 하기 때문.

## 주의사항

- **Desync 위험은 실제 있음.** 우회로 join 은 됐지만 그 mod 가 진짜 gameplay (카드 효과, RNG, 새 캐릭터 등) 에 영향을 미치면, 게임 중 STS2 의 `ChecksumTracker` 가 매 action 마다 비교해서 mismatch 발견 시 `NetError.StateDivergence` 로 즉시 kick. 이 mod 는 in-game checksum 은 일부러 안 건드려요. 우회는 *join 게이트* 만 통과시키고, 실제 동기 상태는 사용자가 신중히 mod 선택해야 함.
- **Host 측 경고는 휴리스틱.** `affects_gameplay=true` 는 mod 작성자가 설정한 값이라 잘못 표시한 mod 도 있을 수 있어요. "확실히 문제 일으킴" 이 아니라 "mismatch 일으킬 가능성 있음" 으로 해석해주세요.
- **Steam lobby preview 없음.** STS2 가 Steam lobby metadata 에 mod 정보를 publish 하지 않아서, mismatch 는 connection handshake *후* 에만 감지 가능. mod 의 client 측 overlay 는 reactive (시도 후 표시) — proactive (시도 전 차단) 아님.

## 설치

1. 최신 release zip 다운로드.
2. `Sts2MultiplayerSync` 폴더를 `<Slay the Spire 2 설치 경로>/mods/` 에 압축 해제.
3. STS2 실행. host 측이고 gameplay 영향 mod 가 있으면 메인 메뉴 도달 후 ~5초 내에 경고 모달 표시.

## 호환성

- **mod 로드 순서 무관.** Harmony Prefix 는 `MainFile.Initialize` 에서 등록되어 세션 내내 활성 — 다른 mod 보다 먼저 로드될 필요 없음.
- **Sts2SkinManager 등 다른 Sts2\* 자매 mod 와 충돌 없이 공존.**

## 라이선스

MIT. `LICENSE` 참조.
