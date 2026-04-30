# Scene A — 컷·모드 시스템 계획

> 작성일: 2026-04-30 (개정 1차)
> 상태: 계획 확정, 단계별 구현 진행

---

## 1. 개요

씬을 두 개로 분리한다.

- **TalkScene.unity** (신규): 풀화면 배경 + 손님 + 대사창
- **SceneA.unity** (기존, 수정): 좌측 손님 + 우측 작업대, 대사창 없음

손님과 손님 뒤 배경은 두 씬 사이를 **그대로 이동**한다 (씬 전환 시 사라지지 않음).
손님 캐릭터의 입 애니메이션은 **역전재판 방식**(2~3프레임 cycle, 몸 위치 고정, 입만 교체)을 적용한다.

손님 크기는 두 씬에서 **동일** (320×320). 슬라이드(좌우 이동)만 발생, 축소 애니메이션 없음.

---

## 2. 영속(Persistent) 구조

손님 뒤 건물·골목길 픽셀이 어긋나지 않도록 손님과 배경을 한 덩어리로 묶고, 두 씬 사이를 그대로 옮긴다.

```
[부트스트랩] 게임 시작 시 1회 — 영속 객체 생성

PersistentCanvas (Canvas, ScreenSpaceOverlay, DontDestroyOnLoad)
  └─ CustomerStage (빈 RectTransform 부모, 손님+배경 묶음)
       ├─ Background (Image, BG_Street.png, 1330×720, preserveAspect)
       └─ Customer   (Image, 320×320, CustomerDisplay, FadeIn)
```

이 구조 덕에:
- 씬 전환 시 손님·배경이 사라지지 않음
- CustomerStage 한 객체만 슬라이드 → 손님 뒤 건물 위치 어긋남 없음
- 두 씬에 손님을 따로 만들 필요 없음

---

## 3. 두 씬 구성

### 3.1 TalkScene.unity (신규)

화면 풀스크린, 대화 모드.

| 오브젝트 | anchoredPosition | sizeDelta | 비고 |
|----------|-----------------|-----------|------|
| CustomerStage | (0, 0) | — | TalkScene 진입 시 이 위치로 이동 |
| Background (CustomerStage 자식) | (0, 0) | (1330, 720) | preserveAspect, 영속 |
| Customer (CustomerStage 자식) | (0, -50) | (320, 320) | preserveAspect, 영속 |
| DialogueBox | (0, -260) | (760, 200) | 씬 전용 (영속 X) |
| DialogueText | stretch | — | DialogueBox 자식 |
| ContinueArrow | 우하단 | (20, 20) | DialogueBox 자식 |

화면 가시 영역(1280×720) 안에서 배경은 좌우로 약간 잘리지만 손님 주변은 모두 보임.

### 3.2 SceneA.unity (수정)

기존 캔버스 모드. 대사창 제거, 영속 손님 받음.

| 오브젝트 | anchoredPosition | sizeDelta | 비고 |
|----------|-----------------|-----------|------|
| CustomerStage | (-320, 0) | — | SceneA 진입 시 이 위치로 이동 |
| Background (CustomerStage 자식) | (0, 0) | (1330, 720) | 영속, 위치는 부모 따라 |
| Customer (CustomerStage 자식) | (0, -50) | (320, 320) | 영속, 위치는 부모 따라 |
| RightPanel | (320, 0) | (640, 720) | 작업대 영역 (드로잉 UI 들어갈 자리) |
| (DialogueBox) | — | — | **만들지 않음** |

손님 절대 좌표 = (-320 + 0, 0 + -50) = (-320, -50). 기존 SceneA와 동일.

---

## 4. 흐름

```
[게임 시작 / 부트스트랩]
   PersistentCanvas + CustomerStage 생성 (DontDestroyOnLoad)
   │
   ▼
[TalkScene 로드]
   CustomerStage를 (0, 0)으로 배치
   대사 1~12 출력
   │  (마지막 대사 종료)
   ▼
[전환 — TalkScene → SceneA]
   CustomerStage Tween (0, 0) → (-320, 0)  [0.7초 EaseInOut]
   대사창 페이드아웃 + SceneManager.LoadScene("SceneA")
   │
   ▼
[SceneA]
   손님 좌측 고정, 작업대 사용
   그리기 → SUBMIT
   │
   ▼
[전환 — SceneA → TalkScene]
   CustomerStage Tween (-320, 0) → (0, 0)
   SceneManager.LoadScene("TalkScene")  → 결과 페이즈 플래그 true
   │
   ▼
[TalkScene 결과 페이즈]
   잘/못 분기에 따라 대사 4줄 출력
   │
   ▼
[다음 손님] (이후 작업)
```

---

## 5. 전환 애니메이션

기간 **0.7초**, 이징 **EaseInOutQuad**, 코루틴 + Mathf.SmoothStep.

### 5.1 TalkScene → SceneA
- CustomerStage anchoredPosition: (0, 0) → (-320, 0)
- DialogueBox 알파 1 → 0 (앞 0.3초)
- 0.5초 시점에 `SceneManager.LoadScene("SceneA")`
- SceneA 로드 후 RightPanel 알파 0 → 1 (뒤 0.3초)
- 손님 사이즈 변화 없음

### 5.2 SceneA → TalkScene
- RightPanel 알파 1 → 0 (앞 0.3초)
- CustomerStage anchoredPosition: (-320, 0) → (0, 0)
- 0.5초 시점에 `SceneManager.LoadScene("TalkScene")` (결과 페이즈 플래그 true)
- TalkScene 로드 후 DialogueBox 알파 0 → 1 (뒤 0.3초)

> 카메라/EventSystem은 두 씬에 각각 두지만, 둘 다 기본값(orthographic)이라 전환 시점에 시각적 점프 없음. 안전을 위해 PersistentEventSystem 영속도 권장.

---

## 6. 컷 메커니즘 — 역전재판 방식

### 6.1 원칙
- **몸·옷·머리·의자·캔버스 위치는 모든 컷에서 동일** (320×320 통일된 rect)
- **변하는 부분은 입(과 향후 눈)뿐**

### 6.2 보유 컷 4종 (Neutral)

| 컷 | 입 상태 | 파일 |
|------|---------|------|
| 닫힘 | 다문 상태 | Customer_Neutral_Idle.png |
| 살짝 | 살짝 벌림 | Customer_Talk2.png |
| 반 | 반 벌림 | Customer_Neutral_Talk.png |
| 활짝 | 크게 벌림 | Customer_Talk1.png |

### 6.3 말하기 사이클 (Neutral)

대사 출력 중 활성. **3프레임 ping-pong**, 0.1초 간격.

```
[Talk2 → Neutral_Talk → Talk1 → Neutral_Talk] 반복
```

대사 종료 시 Neutral_Idle로 복귀.

### 6.4 감정별 토글 정책

| 감정 | 컷 구성 | 사이클 |
|------|---------|--------|
| neutral | 4종 | 3프레임 ping-pong, 0.1초 |
| happy | happyIdle만 | 토글 없음 |
| surprised | surprised만 | 토글 없음 (단발) |
| gesture | gestureIdle / gestureTalk | 2프레임 토글, 0.12초 |
| thinking | (전용 X) | neutralIdle 폴백, 토글 없음 |

### 6.5 포즈 그룹 (기존 유지)

| 그룹 | 감정 |
|------|------|
| resting | neutral / happy |
| gesture | gesture |
| thinking | thinking |
| reaction | surprised |

그룹 전환 시 0.35초 pause.

### 6.6 눈 깜빡임 (후순위)

컷 추가 후 활성화 예정.

### 6.7 Shake (강조)

기존 그대로. 결과 surprised 줄 등에서 호출.

---

## 7. 대사 매핑

### 7.1 TalkScene PreDraw — 12줄

```
[등장 — resting]
1. neutral   "안녕하세요."
2. neutral   "잘 부탁드려요."

[일반 — resting]
3. neutral   "오늘 날씨 좋죠?"
4. neutral   "긴장되네요, 처음이라서."
5. happy     "잘 그려주실 거죠?"
6. happy     "저 나중에 사진 찍어도 될까요?"
7. neutral   "천천히 해도 괜찮아요."

[gesture]
8.  gesture  "여기서 자주 하세요?"
9.  gesture  "오래 걸려요?"
10. gesture  "어떤 스타일로 그려주세요."

[thinking]
11. thinking "저 어떤 표정 짓고 있으면 될까요?"

[resting — 마지막]
12. neutral  "조용히 있을게요."   ← 종료 시 자동 SceneA 전환
```

### 7.2 SceneA — 0줄
손님 idle 고정, 침묵.

### 7.3 TalkScene Result-Good — 4줄

```
13. surprised "와...!"            (shake)
14. happy     "진짜 저예요?"
15. happy     "너무 잘 그려주셨어요."
16. gesture   "친구들한테 꼭 보여줘야겠다."
```

### 7.4 TalkScene Result-Bad — 4줄

```
13. neutral "...음."
14. neutral "뭐... 나쁘지 않네요."
15. neutral "저 이렇게 생겼나요?"
16. neutral "다시 해줄 수 있어요?"
```

---

## 8. 코드 영향 범위

### 8.1 신규 / 수정 파일 목록

| 파일 | 상태 | 역할 |
|------|------|------|
| Editor/PersistentBootstrapBuilder.cs | 신규 | 영속 객체 빌더 (필요 시) |
| Editor/TalkSceneBuilder.cs | 신규 | TalkScene.unity 자동 빌더 |
| Editor/SceneABuilder.cs | 수정 | DialogueBox / Customer / Background 제거(영속화) |
| Scripts/PersistentBootstrap.cs | 신규 | 게임 시작 시 PersistentCanvas + CustomerStage 생성, DontDestroyOnLoad |
| Scripts/SceneTransition.cs | 신규 | TalkScene ↔ SceneA 전환 코루틴 |
| Scripts/TalkSceneController.cs | 신규 | 대사 진행, Phase별 라인 관리 (PreDraw / ResultGood / ResultBad) |
| Scripts/SceneADialogue.cs | 폐기 또는 통합 | 기능을 TalkSceneController로 이전 |
| Scripts/CustomerDisplay.cs | 수정 | TalkLoop 부활 (3프레임 ping-pong) |
| Scripts/GameManager.cs | 수정 | 결과 데이터(점수/레벨) 저장, Phase 플래그 |

### 8.2 참고 — 기존 SceneADialogue.cs는 TalkSceneController로 흡수
- 대사 배열을 Phase별로 3개로 분할
- Phase는 GameManager에서 받음 (PreDraw / ResultGood / ResultBad)

---

## 9. 구현 단계 (Step-by-Step)

각 단계 완료 후 **사용자 승인** 받고 다음 단계 진행.

### Step 1: 영속 부트스트랩 — 완료
- `PersistentBootstrap.cs` 작성 — 게임 시작 시 PersistentCanvas + CustomerStage 생성, DontDestroyOnLoad
- 기존 SceneABuilder의 손님/배경 생성 로직을 여기로 이전
- CustomerDisplay 슬롯 자동 할당 포함

### Step 2: SceneABuilder 수정 — 완료
- 손님·배경·DialogueBox 생성 제거
- RightPanel만 남김 (작업대 자리)
- 부트스트랩 객체가 없으면 경고 후 자동 생성하도록

### Step 3: TalkSceneBuilder 신규 — 완료
- TalkScene.unity 생성 메뉴 (`Yorki/Build Talk Scene`)
- DialogueBox + DialogueText + ContinueArrow만 생성
- 영속 CustomerStage가 (0, 0)에 들어와야 함을 가정

### Step 4: TalkSceneController 신규 — 완료
- Phase enum (PreDraw / ResultGood / ResultBad)
- 기존 SceneADialogue 대사 21줄을 Phase별로 분할
- GameManager.currentPhase 읽어서 해당 Phase 대사 출력
- 마지막 대사 종료 시 SceneTransition 호출

### Step 5: CustomerDisplay TalkLoop 부활 — 완료
- 3프레임 ping-pong 사이클 [Talk2 → Neutral_Talk → Talk1 → Neutral_Talk] 0.1초
- gesture는 2프레임 토글 0.12초
- 다른 감정은 토글 없음

### Step 6: SceneTransition 신규 — 완료
- TalkSceneToSceneA() / SceneAToTalkScene(Phase) 코루틴
- CustomerStage Tween + DialogueBox/RightPanel 알파 페이드
- 0.5초 시점에 SceneManager.LoadScene 호출

### Step 7: SceneA SUBMIT 연결 — 완료
- 기존 GameManager.OnSubmit에서 점수 계산 후 Phase 결정 (Good/Bad)
- SceneTransition.SceneAToTalkScene(phase) 호출

### Step 8: 통합 테스트
- 부트스트랩 → TalkScene → 대사 12줄 → SceneA → 그리기 → SUBMIT → TalkScene 결과 → 대사 4줄
- 손님 뒤 건물 위치 안 어긋나는지 확인 (가장 중요)

---

## 10. 미정 / 후순위

- 결과 페이즈 종료 후 다음 손님 등장 흐름
- 카메라 줌인 (결과 반응 surprised 줄에서 강조용)
- 눈 깜빡임 컷 추가
- 작업대(드로잉 UI) 실제 도구 배치 (UI 계획.md 별도 진행)
- 캔버스 모드에서 손님 idle 변형 (지루함)
- 손님 등장·퇴장 효과

---

## 11. 참고

- 역재 컷 활용: `역재 컷 활용 정보.md`
- 기획·기술: `SCENE_A_PLAN.md`, `SCENE_A_TECH.md`
- 드로잉 UI: `UI 계획.md`
