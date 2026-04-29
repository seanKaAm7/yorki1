# Scene A — 기술 문서

## 1. 씬 오브젝트 구조

```
Canvas (ScreenSpaceOverlay, 1280×720, ScaleWithScreenSize)
├── Background        — 배경 이미지
├── RightPanel        — 오른쪽 드로잉 영역 (빈 패널)
├── Customer          — 손님 캐릭터 (CustomerDisplay, SceneADialogue, FadeIn)
└── DialogueBox       — 대화창 (9-slice PNG)
    ├── DialogueText  — 대사 본문 텍스트
    └── ContinueArrow — ▼ 진행 인디케이터
```

---

## 2. RectTransform 수치값

| 오브젝트 | anchoredPosition | sizeDelta | 비고 |
|---------|-----------------|-----------|------|
| Background | (-320, 0) | (1330, 720) | preserveAspect=true, BG 비율 2780/1504 역산 |
| RightPanel | (320, 0) | (640, 720) | 색상 (0.15, 0.12, 0.10) |
| Customer | (-320, -50) | (320, 320) | preserveAspect=true |
| DialogueBox | (-320, -268) 코드 기본값 | (600, 200) 코드 기본값 | 9-slice, Image.Type.Sliced / 수동 조정 가능 |

※ DialogueBox PNG 없을 때 폴백: `color (0.96, 0.93, 0.84, 0.97)` 단색으로 표시

### DialogueText (DialogueBox 자식, stretch)
```
anchorMin: (0, 0) / anchorMax: (1, 1)
offsetMin: (22, 38)    // Left, Bottom
offsetMax: (-22, -108) // -Right, -Top
fontSize: 22 / color: new Color(0.20, 0.12, 0.06) / alignment: MiddleLeft
```

### ContinueArrow (DialogueBox 자식)
```
anchorMin/Max: (1, 0) — 우하단 고정
pivot: (1, 0)
anchoredPosition: (-10, 10)
sizeDelta: (20, 20)
text: "▼" / fontSize: 14 / color: #6B4D2A
```

---

## 3. DialogueBox.png — 9-slice 설정

- 파일: `Assets/Sprites/SceneA/DialogueBox.png`
- 크기: 1523×513
- 디자인: 나무 테두리 + 베이지 배경 + 상단 삼각형 뿔
- Sprite Mode: Single / Filter: Bilinear / Compression: Uncompressed
- **spriteBorder**: L=80, B=30, R=80, T=100 (Vector4 순서: x=L, y=B, z=R, w=T)
- Sprite Editor에서 rect 상단을 삼각형 포함하도록 수동 조정

---

## 4. FadeIn 컴포넌트

Customer GameObject에 `FadeIn` 컴포넌트 자동 부착. 등장 시 알파값 0→1 페이드인 연출.

---

## 5. 스프라이트 파일 매핑

| 슬롯 | 파일명 |
|------|--------|
| neutralIdle | Customer_Neutral_Idle.png |
| neutralTalk | Customer_Neutral_Talk.png |
| happyIdle | Customer_Happy_Idle.png |
| surprised | Customer_Surprised.png |
| gestureIdle | Customer_Gesture_Idle.png |
| gestureTalk | Customer_Gesture_Talk.png |

※ happyTalk, thinkingIdle 슬롯 제거됨. happy 감정은 idle만 사용(토글 시 idle 유지), thinking 감정은 neutralIdle로 폴백.

모든 캐릭터 스프라이트: Point filter / Uncompressed / alphaIsTransparency

---

## 6. CustomerDisplay.cs

손님 스프라이트 교체 및 애니메이션 담당.

### 메서드
| 메서드 | 동작 |
|--------|------|
| `SetEmotion(emotion)` | 즉시 idle 스프라이트로 교체 |
| `StartTalking(emotion)` | idle ↔ talk 0.12초 토글 루프 시작 |
| `StopTalking()` | 루프 중단, idle 복귀 |
| `Shake()` | 0.25초간 랜덤 흔들림 (shakeMagnitude=6) |

### 감정별 스프라이트
- `happy`, `surprised` — talk 스프라이트 없음, idle만 사용
- `thinking` — 전용 스프라이트 없음, neutralIdle로 폴백
- talk 스프라이트 없는 감정은 `GetTalkSprite()`에서 idle로 폴백

---

## 7. SceneADialogue.cs

대사 진행 컨트롤러. Customer GameObject에 부착.

### 대사 구조체
```csharp
struct SceneALine {
    string text;
    string emotion;  // neutral / happy / surprised / gesture / thinking
    bool   shake;
}
```

### 대사 진행 흐름
```
ShowLineRoutine(i)
  → 포즈 그룹 전환 감지 → 0.35초 pause (poseDelay)
  → SetEmotion() + StartTalking()
  → shake=true 이면 Shake()
  → TypeLine() — 한 글자씩 출력 (typeSpeed=0.04초)
  → 완료 → StopTalking() + ▼ 깜빡임 시작

엔터/스페이스 입력
  → 타이핑 중: 즉시 전체 출력 + ▼ 깜빡임
  → 대기 중: 다음 줄로 이동
```

### 포즈 그룹 시스템

감정이 다른 포즈 그룹으로 바뀔 때만 0.35초 pause 삽입.
같은 그룹 내 연속 대사는 pause 없이 자연스럽게 이어짐.

| 그룹 | 감정 | 의미 |
|------|------|------|
| resting | neutral, happy | 팔 내린 자연스러운 상태 |
| gesture | gesture | 손 제스처 상태 — 연속 사용 권장 |
| thinking | thinking | 생각하는 포즈 — 단독 사용 |
| reaction | surprised 등 | 반응 표정 |

### 대사 순서 (20줄)
```
[등장 — resting]
neutral  "안녕하세요."
neutral  "잘 부탁드려요."

[일반 대화 — resting 블록]
neutral  "오늘 날씨 좋죠?"
neutral  "긴장되네요, 처음이라서."
happy    "잘 그려주실 거죠?"
happy    "저 나중에 사진 찍어도 될까요?"
neutral  "천천히 해도 괜찮아요."

[gesture 블록]
gesture  "여기서 자주 하세요?"
gesture  "오래 걸려요?"
gesture  "어떤 스타일로 그려주세요."

[thinking 단독]
thinking "저 어떤 표정 짓고 있으면 될까요?"

[resting]
neutral  "조용히 있을게요."

[결과 — 잘 그렸을 때]
surprised "와...!"  (shake=true)
happy     "진짜 저예요?"
happy     "너무 잘 그려주셨어요."
gesture   "친구들한테 꼭 보여줘야겠다."

[결과 — 못 그렸을 때]
neutral  "...음."응
neutral  "뭐... 나쁘지 않네요."
neutral  "저 이렇게 생겼나요?"
neutral  "다시 해줄 수 있어요?"
```

---

## 8. SceneABuilder.cs — 빌드 메뉴

`Yorki/Build Scene A` 실행 시:
1. `SetupSprites()` — 스프라이트 임포트 설정 적용 후 AssetDatabase.Refresh()
2. `BuildScene()` — 씬 오브젝트 생성 및 컴포넌트 연결
3. `Assets/Scenes/SceneA.unity` 저장

**주의**: 빌드 시 씬 전체 재생성 — 수동 변경사항 덮어씌워짐. 반드시 저장 후 실행.

