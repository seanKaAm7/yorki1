# Yorki 유지보수 계획

> 마지막 업데이트: 2026-05-13

## 목표

- 프로토타입이 동작하는 상태를 보존하면서, 빌더 재실행/씬 전환/폰트 교체 같은 반복 작업이 덜 위험하도록 정리한다.
- 기능 구현 완료 상태와 문서 상태가 어긋나지 않게 유지한다.
- 수동으로 맞춘 UI 좌표는 임의 수정하지 않고, 빌더 기본값에만 명확히 반영한다.
- 문서는 루트 기준 핵심 문서만 남기고, 완료된 구현 계획서는 현재 문서에 흡수한 뒤 삭제한다.

## 현재 기준 상태

- `TalkScene`은 대사 전용 씬이다.
- `SceneA`는 오른쪽 반칸 드로잉 UI 씬이다.
- `TalkSceneController`가 PreDraw / ResultGood / ResultBad 대사를 담당한다.
- `SceneADialogue`와 `SceneATestTransitionInput`은 제거 완료 상태다.
- Scene A 드로잉 UI 1차 기능은 구현 완료 상태다.
- 남은 핵심 검증은 `TalkScene -> SceneA -> Submit -> TalkScene 결과 대사` 전체 플레이 플로우다.

## 유지보수 원칙

- `Yorki/Build Scene A`, `Yorki/Build Talk Scene`은 씬을 재생성하므로 실행 전 현재 씬 수동 변경값이 빌더에 반영됐는지 확인한다.
- `SceneA.unity`의 SliderHandle 위치/크기는 사용자가 맞춘 값을 기본값으로 취급하고 임의 변경하지 않는다.
- `TalkScene.unity`의 Customer / DialogueBox / DialogueText 수동 조정값도 기본값으로 취급한다.
- 폰트, 공통 에셋 경로, import 설정은 Editor 공통 유틸에서 관리한다.
- 구형 프로토타입 스크립트는 실제 씬 참조가 없고 대체 구현이 안정화된 뒤 삭제한다.
- 보존 레퍼런스 문서: `레퍼런스_SuperPaperMario_interludes.md`, `역재 컷 활용 정보.md`

## 우선순위

### 1. 코드 정리

- [x] Submit 연결 후 불필요한 `SceneATestTransitionInput` 제거
- [x] `TalkSceneController`로 대체된 `SceneADialogue` 제거
- [x] 공통 폰트/스프라이트 로딩 유틸 생성
- [ ] `GameSceneBuilder` / `DrawingSceneBuilder` 구형 프로토타입 빌더 정리 여부 결정
- [ ] `ReactionUI` / `DialogueUI` 구형 SampleScene 흐름 유지 여부 결정

### 2. 문서 정리

- [x] `PROGRESS.md` 최신 구현 상태 반영
- [x] `CLAUDE.md` 현재 구현 상태 갱신
- [x] `SCENE_A_TECH.md`의 구 단일 SceneA 설명을 TalkScene/SceneA 분리 구조로 정리
- [x] 삭제 전 `SCENE_A_DRAWING_UI_IMPLEMENTATION_PLAN.md`의 완료/잔여 검증 내용을 현행 문서에 반영
- [x] 완료된 옛 구현 계획서 삭제: `SCENE_A_PLAN.md`, `SCENE_A_CUT_PLAN.md`, `SCENE_A_DRAWING_UI_IMPLEMENTATION_PLAN.md`, `UI 계획.md`, `pivot.md`
- [x] 삭제 허용된 RTF 채팅 기록 삭제

### 3. 검증

- [x] 런타임/에디터 csproj 빌드
- [x] TalkScene / SceneA missing script 검증
- [ ] 실제 Play 모드 전체 플로우 수동 검증
- [ ] Submit 후 결과 대사 종료 이후 다음 손님/하루 루프 연결 방식 결정
