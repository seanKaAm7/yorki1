# Scene A 기술 문서

> 마지막 업데이트: 2026-05-13
> 현행 기준: `TalkScene.unity` 대화 씬 + `SceneA.unity` 드로잉 씬 분리 구조

## 1. 전체 구조

현재 Scene A 흐름은 한 씬 안에서 대화와 드로잉을 모두 처리하지 않는다.

```text
TalkScene
  -> PreDraw 대사
  -> SceneTransition.TalkSceneToSceneA()

SceneA
  -> 오른쪽 반칸 드로잉 UI
  -> Submit
  -> GameManager.OnSubmit()
  -> SceneTransition.SceneAToTalkScene(ResultGood/ResultBad)

TalkScene
  -> 결과 대사
```

`SceneADialogue`와 `SceneATestTransitionInput`은 제거 완료 상태다. 대사 진행은 `TalkSceneController`, 드로잉 UI 입력은 `SceneADrawingUIController`가 담당한다.

## 2. TalkScene.unity

주요 오브젝트:

```text
SceneCanvas
├── Background
├── DialogueBox
│   ├── DialogueText
│   └── ContinueArrow
└── TalkSceneController

PersistentCanvas
└── CustomerStage
    └── Customer
```

주요 컴포넌트:

| 컴포넌트 | 역할 |
|---------|------|
| `TalkSceneController` | PreDraw / ResultGood / ResultBad 대사 페이즈 처리 |
| `CustomerDisplay` | 손님 컷 교체, 제한된 말하기 프레임, Shake 연출 |
| `SceneTransition` | TalkScene과 SceneA 사이 전환 |
| `PersistentBootstrap` | 손님 표시 오브젝트를 씬 전환 중 유지 |

유지보수 주의:

- 사용자가 조정한 Customer / DialogueBox / DialogueText 위치와 크기는 현재 기본값이다.
- TalkScene 대화창은 나무 9-slice 이미지가 아니라 어두운 반투명 Image + 얇은 Outline 방식이다. 현재 기본값은 820x160, alpha 0.78이다.
- 빌더를 다시 실행할 때는 이 값들이 `TalkSceneBuilder.cs`에 반영됐는지 먼저 확인한다.
- 폰트는 `Assets/Fonts/Moneygraphy-Pixel.ttf`를 사용한다.

## 3. SceneA.unity

주요 오브젝트:

```text
SceneCanvas
└── RightPanel
    ├── DeskBase
    ├── DrawingPaper
    │   └── DrawingSurface
    ├── PenButton
    ├── BrushButton
    ├── EraserButton
    ├── PickerButton
    ├── ThicknessTrack
    ├── ThicknessText
    ├── PreviewDot
    ├── UndoButton
    ├── RedoButton
    ├── ResetButton
    ├── SubmitButton
    ├── PalettePanel
    ├── ColorPanel
    └── SceneADrawingUIController
```

주요 컴포넌트:

| 컴포넌트 | 역할 |
|---------|------|
| `DrawingCanvas` | 실제 그리기 텍스처, Undo/Redo, 투명 배경, 제출용 합성 |
| `SceneADrawingUIController` | 도구, 팔레트, RGB 박스, 액션 버튼 연결 |
| `ThicknessSliderHandle` | THICKNESS 세로 슬라이더 입력 |
| `GameManager` | Submit 후 채점/수입/결과 페이즈 결정 |

유지보수 주의:

- `SliderHandle` 위치/크기는 사용자가 맞춘 현재 값이 기본값이다.
- 슬라이더 이동 범위는 `ThicknessSliderHandle.minY = -70`, `maxY = 70` 기준이다.
- COLOR 박스 안의 별도 스포이드 아이콘은 구현 대상에서 제외했다.
- 좌측 `PickerButton`은 별도 도구로 유지한다.

## 4. 대사 페이즈

`TalkScenePhase`:

```csharp
public enum TalkScenePhase
{
    PreDraw,
    ResultGood,
    ResultBad
}
```

흐름:

| 페이즈 | 진입 조건 | 종료 동작 |
|--------|-----------|----------|
| `PreDraw` | 처음 TalkScene 시작 | SceneA로 전환 |
| `ResultGood` | Submit 결과가 Satisfied 이상 | 현재는 로그 출력 후 종료 |
| `ResultBad` | Submit 결과가 Neutral 이하 | 현재는 로그 출력 후 종료 |

결과 대사 종료 이후 다음 손님/하루 루프는 아직 미구현이다.

## 5. 손님 컷 전환

`CustomerDisplay`는 감정 문자열을 받아 스프라이트를 교체한다.

| 감정 | 기본 처리 |
|------|-----------|
| `neutral` | `neutralIdle`, 말할 때 `neutralTalk` |
| `happy` | `happyIdle` |
| `surprised` | `surprised` |
| `gesture` | `gestureIdle`, 말할 때 제한적으로 `gestureTalk` |
| `thinking` | 별도 스프라이트 없이 neutral 폴백 |

현재는 픽셀 튐을 줄이기 위해 `useExtraNeutralTalkFrames = false` 기준으로 사용한다. 여러 말하기 컷을 빠르게 토글하면 AI 이미지 간 크롭/픽셀 차이 때문에 캐릭터가 흔들려 보일 수 있다.

스프라이트 제작 원칙:

- 같은 손님의 표정/입 컷은 같은 캔버스 크기와 같은 피벗을 사용한다.
- 몸, 머리, 의자, 어깨선은 고정하고 입/눈/손처럼 필요한 부분만 바뀌게 만든다.
- AI로 새 컷을 뽑았을 때는 바로 쓰지 말고 기준 컷 위에 겹쳐 크롭, 베이스라인, 실루엣을 맞춘다.
- 컷 일관성이 낮으면 말하기 프레임을 줄이고 한 대사 한 컷에 가깝게 운용한다.

## 6. Submit 처리

기본 경로:

```text
SubmitButton
  -> SceneADrawingUIController.OnSubmitClicked()
  -> GameManager.OnSubmit()
  -> DrawingCanvas.GetFlattenedTextureForScoring()
  -> ScoreCalculator.Calculate()
  -> ReactionSystem.Evaluate()
  -> SceneTransition.SceneAToTalkScene(ResultGood/ResultBad)
```

`GameManager.Instance`가 없을 때는 `SceneADrawingUIController`가 `SampleCustomer`로 fallback 채점 후 TalkScene으로 복귀한다.

## 7. 빌더

사용 메뉴:

| 메뉴 | 스크립트 | 설명 |
|------|----------|------|
| `Yorki/Build Talk Scene` | `TalkSceneBuilder.cs` | TalkScene 재생성 |
| `Yorki/Build Scene A` | `SceneABuilder.cs` | SceneA 재생성 |

공통 유틸:

| 스크립트 | 역할 |
|----------|------|
| `YorkiEditorAssets.cs` | Moneygraphy Pixel 폰트 로딩, UI 스프라이트 import 설정 |

주의:

- 빌더는 씬을 재생성하므로 수동 수정값을 덮어쓸 수 있다.
- 수동 조정한 기본값을 유지하려면 먼저 빌더 코드에 같은 값을 반영한 뒤 실행한다.
