# Scene A — 기획 문서

## 씬 개요

- Coffee Talk 방식 — 고정 배경, 손님이 찾아오는 구조
- **두 씬으로 분리** (상세: `SCENE_A_CUT_PLAN.md`)
  - **TalkScene.unity**: 풀화면 배경 + 손님 + 대사창 (인사·잡담·결과 반응)
  - **SceneA.unity**: 좌측 손님 + 우측 작업대, 대사창 없음 (그리기)
- 손님 + 배경은 **영속 객체** (PersistentCanvas + CustomerStage), 두 씬 사이를 그대로 이동
- 손님 크기 두 씬 동일 (320×320), 슬라이드만 발생
- 흐름: TalkScene(인사·잡담) → 슬라이드 전환 → SceneA(침묵, 그리기) → SUBMIT → TalkScene(결과 반응)
- 배경: 거리 노천 카페 / 스튜디오 분위기

---

## 레이아웃 구조

### 대화 모드 (풀화면)
```
┌─────────────────────────────────┐
│        배경 풀화면                │
│                                 │
│         [손님 중앙]               │
│                                 │
│      [대사창 하단 중앙]            │
└─────────────────────────────────┘
```

### 캔버스 모드 (분할)
```
[왼쪽 절반]          [오른쪽 절반]
  배경 + 손님           드로잉 작업대
  (대사창 없음)         (캔버스 + 팔레트 + 툴바)
```

> 모드 전환은 0.7초 EaseInOutQuad 보간. 손님 크기는 320×320으로 고정하고, CustomerStage 좌측 슬라이드와 작업대 페이드인/아웃만 적용한다.

---

## 손님 캐릭터

### 스프라이트 세트 (8종)

| 상태 | 설명 | 입 단계 |
|------|------|---------|
| neutral_idle | 기본 대기 | 닫힘 |
| neutral_talk | 기본 말하기 | 반 |
| talk1 | 입 크게 벌림 | 활짝 |
| talk2 | 입 살짝 벌림 | 살짝 |
| happy_idle | 기쁜 표정 (talk 없음, idle만 사용) | — |
| surprised | 놀란 표정 (단독) | — |
| gesture_idle | 손 제스처 대기 | — |
| gesture_talk | 손 제스처 말하기 | — |

※ happy_talk, thinking_idle 제거됨 — happy는 idle만, thinking 감정은 neutralIdle로 폴백.

### 컷 토글 메커니즘 (역재 방식)

몸·옷·머리·의자 위치는 모든 컷에서 동일하게 유지하고, **입(과 향후 눈)만 교체**한다.

- **Neutral 감정 말하기**: 3프레임 ping-pong 사이클, 0.1초 간격
  - `Talk2 (살짝) → Neutral_Talk (반) → Talk1 (활짝) → Neutral_Talk (반)` 반복
  - 대사 종료 시 Neutral_Idle (닫힘)로 복귀
- **Gesture 감정 말하기**: 2프레임 토글 `gestureIdle ↔ gestureTalk`, 0.12초
- **Happy / Surprised**: 토글 없음, idle 유지
- 상세: `SCENE_A_CUT_PLAN.md` 6번 섹션

### 포즈 그룹 규칙

- **resting** (neutral/happy): 자유롭게 혼합 가능
- **gesture**: 팔 든 상태 — 연속 대사에 사용, 중간에 끊기면 어색
- **thinking**: 단독 사용 권장
- **reaction** (surprised 등): 단발성 반응

그룹 전환 시 0.35초 pause 자동 삽입.

---

## 대사 구성 (21줄)

손님 첫 방문 기준. 대화 모드(그리기 전) + 침묵(캔버스 모드) + 대화 모드(결과 반응) 구성.

### 대화 모드 — 등장 (2줄) — resting
```
"안녕하세요."
"잘 부탁드려요."
```

### 대화 모드 — 일반 대화 (11줄)

> 기존 "그리는 중 일반 대화"에서 변경. 모두 그리기 전(대화 모드)에서 출력.

```
[resting 블록]
"오늘 날씨 좋죠?"
"긴장되네요, 처음이라서."
"잘 그려주실 거죠?"
"저 나중에 사진 찍어도 될까요?"
"천천히 해도 괜찮아요."

[gesture 블록]
"여기서 자주 하세요?"
"오래 걸려요?"
"어떤 스타일로 그려주세요."

[thinking 단독]
"저 어떤 표정 짓고 있으면 될까요?"

[resting — 마지막 대사]
"조용히 있을게요."   ← 이 줄 종료 시 자동으로 캔버스 모드 전환
```

### 캔버스 모드 — 침묵
- 손님은 대사 출력하지 않음 (idle 고정)
- 그리는 동안 추가 대사 없음

### 대화 모드 — 결과 반응 (잘 그렸을 때, 4줄)
```
"와...!"           (surprised, shake)
"진짜 저예요?"      (happy)
"너무 잘 그려주셨어요."  (happy)
"친구들한테 꼭 보여줘야겠다."  (gesture)
```

### 대화 모드 — 결과 반응 (못 그렸을 때, 4줄)
```
"...음."
"뭐... 나쁘지 않네요."
"저 이렇게 생겼나요?"
"다시 해줄 수 있어요?"
```

> SUBMIT 후 대화 모드로 자동 복귀 → 잘/못 분기 (ScoreCalculator + ReactionSystem) → 해당 4줄 출력.

---

## 대화창 디자인

- PNG 9-slice 방식 (DialogueBox.png, 1523×513)
- 나무 테두리 + 베이지 배경 + 상단 삼각형 뿔
- 우하단 ▼ 깜빡임 인디케이터

---

## 미구현 / 예정 작업

### 두 씬 분리 + 컷 시스템 (SCENE_A_CUT_PLAN.md 기반)
- [x] PersistentBootstrap 신규 — 게임 시작 시 PersistentCanvas + CustomerStage 영속 객체 생성
- [x] TalkSceneBuilder 신규 — TalkScene.unity 자동 빌더
- [x] SceneABuilder 수정 — DialogueBox / 손님 / 배경 제거 (영속 객체 사용)
- [x] TalkSceneController 신규 — Phase별 대사 (PreDraw / ResultGood / ResultBad)
- [x] SceneTransition 신규 — TalkScene ↔ SceneA 전환 코루틴 (CustomerStage 슬라이드 0.7초)
- [x] 12번째 대사 종료 시 자동 SceneA 전환
- [x] SUBMIT 후 결과 Phase로 TalkScene 복귀
- [x] 역재식 컷 토글 메커니즘 활성화 (CustomerDisplay TalkLoop 부활, 3프레임 ping-pong 0.1초)

### 드로잉 UI (UI 계획.md 기반)
- [ ] 오른쪽 드로잉 패널 연결 (캔버스 + 팔레트 + 툴바)
- [ ] 디제틱 도구 배치 (펜·붓·지우개 / 팔레트 8칸 / 굵기 슬라이더 / Undo·Redo / 시계)
- [ ] RGB 컬러피커 팝업 (나무 톤, 스포이드 내장)

### 기타
- [ ] 결과 반응 분기 처리 (점수 기반 잘/못 그렸을 때 전환)
- [ ] 손님 등장 FadeIn 연출 확인
- [ ] 한글 폰트 적용
- [ ] NameTag (화자 이름 탭) — 대화창 좌상단 위 작은 탭, 미구현
- [ ] 눈 깜빡임 컷 추가 및 메커니즘 활성화 (후순위)
