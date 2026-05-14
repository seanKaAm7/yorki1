# Scene A / Drawing Canvas 기술 문서

> 마지막 업데이트: 2026-05-14
> 현행 기준: `TalkScene.unity` 대화 씬 + `SceneA.unity` 드로잉 씬 분리 구조

## 1. 전체 흐름

Scene A는 현재 하나의 씬 안에서 대화와 드로잉을 모두 처리하지 않는다.

```text
TalkScene
  -> TalkSceneController: PreDraw 대사 출력
  -> 마지막 대사 종료
  -> SceneTransition.TalkSceneToSceneA()

SceneA
  -> 오른쪽 반칸 Drawing UI 표시
  -> 플레이어가 DrawingCanvas에 그림
  -> SubmitButton
  -> SceneADrawingUIController.OnSubmitClicked()
  -> GameManager.OnSubmit()
  -> ScoreCalculator.Calculate()
  -> ReactionSystem.Evaluate()
  -> SceneTransition.SceneAToTalkScene(ResultGood/ResultBad)

TalkScene
  -> TalkSceneController: 결과 대사 출력
```

삭제 완료된 구형 스크립트:

- `SceneADialogue.cs`
- `SceneADialogue.cs.meta`
- `SceneATestTransitionInput.cs`
- `SceneATestTransitionInput.cs.meta`

현재 역할 분리:

| 영역 | 담당 |
|------|------|
| 대사 출력 | `TalkSceneController` |
| 손님 컷 표시 | `CustomerDisplay` |
| 씬 전환 | `SceneTransition` |
| 드로잉 입력 | `DrawingCanvas` |
| 드로잉 UI 버튼/색/굵기 | `SceneADrawingUIController` |
| 제출/채점/수입 | `GameManager`, `ScoreCalculator`, `ReactionSystem` |

## 2. TalkScene.unity

TalkScene은 손님과 대사에 집중하는 풀화면 대화 씬이다.

주요 계층:

```text
SceneCanvas
├── DialogueBox
│   ├── DialogueText
│   └── ContinueArrow
└── TalkSceneController

PersistentCanvas
└── CustomerStage
    ├── Background
    └── Customer
```

주요 컴포넌트:

| 컴포넌트 | 역할 |
|---------|------|
| `PersistentBootstrap` | `PersistentCanvas`, `CustomerStage`, `Background`, `Customer`를 씬 전환 중 유지 |
| `CustomerDisplay` | 손님 표정/입 컷 전환, Shake |
| `TalkSceneController` | PreDraw / ResultGood / ResultBad 대사 출력 |
| `SceneTransition` | TalkScene과 SceneA 사이 전환 |

현재 대화창 스타일:

| 항목 | 값 |
|------|----|
| 방식 | `Image` + `Outline`, 별도 대화창 sprite 사용 안 함 |
| 크기 | `820 x 160` |
| 위치 | `anchoredPosition (0, -250)` |
| 배경색 | 어두운 반투명, alpha `0.78` |
| 테두리 | 얇은 `Outline`, alpha `0.70` |
| 폰트 | `Assets/Fonts/Moneygraphy-Pixel.ttf` |
| 본문색 | 밝은 크림 계열 |
| 화살표 | 우하단 `▼`, 밝은 크림 계열 |

유지보수 주의:

- 사용자가 조정한 `Customer`, `DialogueBox`, `DialogueText` 위치/크기는 기본값으로 취급한다.
- `Yorki/Build Talk Scene` 실행 전 `TalkSceneBuilder.cs`에 현재 수동 조정값이 반영되어 있는지 확인한다.
- TalkScene 대화창은 더 이상 `DialogueBox.png` 9-slice 기반이 아니다.

## 3. TalkSceneController

대사 페이즈:

```csharp
public enum TalkScenePhase
{
    PreDraw,
    ResultGood,
    ResultBad
}
```

페이즈별 동작:

| 페이즈 | 진입 조건 | 종료 동작 |
|--------|-----------|----------|
| `PreDraw` | 처음 TalkScene 시작 | 손님을 neutral 기본 컷으로 돌린 뒤 SceneA로 전환 |
| `ResultGood` | Submit 결과가 Satisfied 이상 | 현재는 결과 대사 종료 로그만 출력 |
| `ResultBad` | Submit 결과가 Neutral 이하 | 현재는 결과 대사 종료 로그만 출력 |

입력:

- `Enter` / `Space`
  - 타이핑 중이면 현재 문장을 즉시 전체 출력
  - 문장 출력이 끝난 상태면 다음 줄로 이동

대사 출력 방식:

- `typeSpeed = 0.04`
- 포즈 그룹이 바뀔 때 `poseDelay = 0.35`
- 대사 출력 완료 후 `ContinueArrow`가 `arrowBlinkRate = 0.5` 간격으로 깜빡임
- 글자가 출력될 때 `CustomerDisplay.AdvanceTalkFrame()` 호출
- 공백/마침표 등 입을 닫아야 하는 문자에서는 `CustomerDisplay.CloseMouth()` 호출

## 4. CustomerDisplay

손님 컷은 감정 문자열로 제어한다.

| 감정 | 기본 컷 | 말하기 처리 |
|------|---------|-------------|
| `neutral` | `neutralIdle` | `neutralTalk` 중심 |
| `happy` | `happyIdle` | 별도 talk 컷 없이 idle 유지 |
| `surprised` | `surprised` | 단발 컷 |
| `gesture` | `gestureIdle` | 제한적으로 `gestureTalk` 토글 |
| `thinking` | 별도 컷 없음 | neutral 폴백 |

현재 픽셀 튐 완화 설정:

| 필드 | 값 |
|------|----|
| `mouthFrameInterval` | `3` |
| `useExtraNeutralTalkFrames` | `false` |

`useExtraNeutralTalkFrames = false` 상태에서는 neutral 말하기가 `talk1/talk2`를 빠르게 순환하지 않고 `neutralTalk` 단일 컷 중심으로 동작한다. AI 생성 컷 사이의 크롭/픽셀 불일치 때문에 캐릭터가 흔들려 보이는 문제를 줄이기 위한 설정이다.

SceneA 진입 보정:

- `TalkSceneController`는 PreDraw 종료 직전 손님을 `neutral`로 되돌린다.
- `SceneTransition`도 `TalkSceneToSceneA()`와 `PlaceStageForSceneA()`에서 `ResetCustomerForDrawing()`을 호출한다.
- 목적: 반반 캔버스 화면에서 손님 입이 벌어진 컷으로 남지 않게 하는 것.

스프라이트 제작 원칙:

- 같은 손님의 표정/입 컷은 같은 캔버스 크기와 같은 피벗을 사용한다.
- 몸, 머리, 의자, 어깨선은 고정하고 입/눈/손처럼 필요한 부분만 바뀌게 만든다.
- AI로 새 컷을 뽑았을 때는 바로 쓰지 말고 기준 컷 위에 겹쳐 크롭, 베이스라인, 실루엣을 맞춘다.
- 컷 일관성이 낮으면 말하기 프레임을 줄이고 한 대사 한 컷에 가깝게 운용한다.

## 5. SceneTransition

`SceneTransition`은 `DontDestroyOnLoad` 싱글턴이다.

상수:

| 이름 | 값 | 의미 |
|------|----|------|
| `TalkStagePosition` | `(0, 0)` | TalkScene에서 손님/배경 위치 |
| `DrawingStagePosition` | `(-320, 0)` | SceneA에서 손님/배경 위치 |

전환 설정:

| 필드 | 값 |
|------|----|
| `duration` | `0.7` |
| `loadAt` | `0.5` |
| `fadeDuration` | `0.3` |
| easing | `EaseInOutQuad` |

전환 중 처리:

- TalkScene -> SceneA
  - `DialogueBox` CanvasGroup alpha를 1 -> 0으로 페이드아웃
  - `CustomerStage`를 `(0,0)` -> `(-320,0)`로 이동
  - `loadAt` 시점에 `SceneA` 로드
  - `RightPanel` CanvasGroup alpha를 0 -> 1로 페이드인
- SceneA -> TalkScene
  - `RightPanel` alpha를 1 -> 0으로 페이드아웃
  - `CustomerStage`를 `(-320,0)` -> `(0,0)`로 이동
  - `TalkScene` 로드 후 `DialogueBox`를 페이드인

## 6. SceneA.unity

SceneA는 오른쪽 반칸 드로잉 UI 씬이다. 손님/배경은 PersistentCanvas 쪽에서 유지되고, SceneA 자체는 오른쪽 작업대 UI를 담당한다.

주요 계층:

```text
SceneCanvas
└── RightPanel
    ├── DeskBase
    ├── ReferenceOverlay_FinalRGB (비활성)
    ├── DrawingPaper
    │   └── DrawingSurface
    ├── PenButton
    ├── BrushButton
    ├── EraserButton
    ├── PickerButton
    ├── ThicknessTrack
    │   └── SliderHandle
    ├── ThicknessText
    ├── PreviewDot
    ├── UndoButton
    ├── RedoButton
    ├── ResetButton
    ├── SubmitButton
    ├── PalettePanel
    │   └── PaletteSlot_0 ~ PaletteSlot_7
    ├── ColorPanel
    │   ├── SaturationValueArea
    │   ├── HueBar
    │   ├── RLabel / GLabel / BLabel
    │   ├── RValueInput / GValueInput / BValueInput
    │   └── ColorPreview
    └── SceneADrawingUIController
```

핵심 RectTransform 기준값:

| 오브젝트 | 위치 | 크기 | 비고 |
|----------|------|------|------|
| `RightPanel` | `(320, 0)` | `640 x 720` | 오른쪽 반칸 |
| `DeskBase` | `(0, 0)` | `640 x 720` | `ui_fixed.png` |
| `DrawingPaper` | `(-18.18435, 118)` | `330.915 x 406.5569` | 사용자가 맞춘 종이 영역 |
| `DrawingSurface` | stretch | 종이 전체 | 실제 `RawImage + DrawingCanvas` |
| `ThicknessTrack` | `(180, 79.85)` | `40 x 220` | 투명 입력 영역 |
| `SliderHandle` | `(14.822153, 4.98325)` | `34.9842 x 28.5703` | 사용자가 맞춘 기본값 |

유지보수 주의:

- `DrawingPaper`, `DrawingSurface`, `SliderHandle` 위치/크기는 사용자가 맞춘 현재 값이 기본값이다.
- `Yorki/Build Scene A`는 씬을 재생성하므로, 실행 전 `SceneABuilder.cs`에 수동 조정값이 반영되어 있는지 확인한다.
- COLOR 박스 안의 별도 스포이드 아이콘은 구현하지 않았다.
- 색 추출은 좌측 `PickerButton` 도구가 담당한다.

## 7. DrawingCanvas

`DrawingCanvas`는 실제 그림이 저장되는 텍스처와 포인터 입력을 담당한다.

기본 구조:

| 항목 | 값 / 설명 |
|------|-----------|
| 컴포넌트 위치 | `DrawingSurface` |
| UI 표시 | `RawImage.texture = drawTexture` |
| 텍스처 | `Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false)` |
| 기본 크기 | `512 x 512` |
| 필터 | `FilterMode.Point` |
| SceneA 배경 | `transparentBackground = true` |
| 제출 배경 | `GetFlattenedTextureForScoring()`에서 흰 배경 합성 |

입력 인터페이스:

```csharp
IPointerDownHandler
IDragHandler
IPointerUpHandler
```

좌표 변환:

```text
screen position
  -> RectTransformUtility.ScreenPointToLocalPointInRectangle()
  -> local x/y를 RectTransform 크기 기준 0~1로 정규화
  -> canvasWidth/canvasHeight 기준 픽셀 좌표로 변환
```

드로잉 처리:

- `OnPointerDown`
  - Picker 모드면 현재 픽셀 색을 찍고 `ColorPicked` 이벤트 발생
  - 그리기 도구면 `SaveSnapshot()` 후 첫 점을 찍음
- `OnDrag`
  - 이전 좌표와 현재 좌표 사이를 보간해 선을 그림
- `OnPointerUp`
  - `isDrawing = false`

선 보간:

```text
dist = Distance(from, to)
steps = round(dist * 2)
0..steps 사이를 Lerp해서 PaintDot 반복
```

점 그리기:

- `PaintDot(cx, cy, radius, color)`
- 원형 브러시 방식
- `x*x + y*y <= radius*radius`인 픽셀만 칠함
- 텍스처 범위 밖 픽셀은 무시

도구:

```csharp
public enum DrawingTool
{
    Pen,
    Brush,
    Eraser,
    Picker
}
```

도구별 굵기:

| 도구 | min | default | max |
|------|-----|---------|-----|
| Pen | `1` | `6` | `11` |
| Brush | `2` | `12` | `22` |
| Eraser | `3` | `18` | `33` |

도구별 색/알파:

| 도구 | 처리 |
|------|------|
| Pen | `brushColor` 그대로 사용 |
| Brush | `brushColor.a *= brushAlpha`, 현재 `brushAlpha = 0.7` |
| Eraser | SceneA에서는 투명 픽셀 `(0,0,0,0)`로 지움 |
| Picker | 텍스처 픽셀 색 추출 후 그리기는 하지 않음 |

Undo / Redo:

| 항목 | 설명 |
|------|------|
| 저장 방식 | `Color32[]` 스냅샷 |
| 최대 단계 | `MAX_UNDO = 20` |
| Undo | 현재 상태를 redoStack에 넣고 undoStack 마지막 상태 복원 |
| Redo | 현재 상태를 undoStack에 넣고 redoStack 마지막 상태 복원 |
| 새 그리기 시작 | `SaveSnapshot()` 호출 후 redoStack clear |
| 단축키 | `Ctrl/Cmd + Z` Undo, `Ctrl/Cmd + Shift + Z` Redo |

제출용 합성:

```csharp
Texture2D GetFlattenedTextureForScoring()
```

역할:

- SceneA의 실제 드로잉 레이어는 투명 배경이다.
- `ScoreCalculator`는 흰 배경과의 차이로 잉크를 판단한다.
- 따라서 제출 시 `backgroundColor` 흰색과 투명 드로잉 레이어를 합성해 alpha 1인 텍스처를 만든다.

합성 방식:

```text
source = drawTexture pixel
mixed = Lerp(backgroundColor, source, source.a)
mixed.a = 1
```

## 8. SceneADrawingUIController

`SceneADrawingUIController`는 SceneA 오른쪽 UI의 연결자다. 직접 그림을 그리지는 않고, `DrawingCanvas`에 명령을 전달한다.

담당:

- Pen / Brush / Eraser / Picker 버튼 연결
- 도구 버튼 sprite 상태 변경
- Thickness 슬라이더 값 적용
- Undo / Redo / Reset / Submit 연결
- 팔레트 8칸 선택/색 변경
- COLOR RGB 박스 조작
- Picker로 찍은 색을 현재 팔레트 슬롯에 반영

도구 버튼 상태:

| 상태 | 의미 |
|------|------|
| `Base` | 기본 |
| `Selecting` | PointerDown 중 |
| `Selected` | 현재 선택된 도구 |

버튼 연결 방식:

- `EventTrigger`를 런타임에 붙인다.
- PointerDown: selecting sprite
- PointerUp / PointerExit: 현재 선택 상태에 맞게 복원
- PointerClick: `SelectTool(tool)`

도구 선택 시:

```text
currentTool = tool
DrawingCanvas.SetTool(tool)
도구 버튼 sprite 갱신
Picker가 아니면 thicknessSlider.SetNormalizedValue(0.5)
해당 도구 default thickness 표시
```

액션 버튼:

| 버튼 | 동작 |
|------|------|
| Undo | `DrawingCanvas.Undo()` |
| Redo | `DrawingCanvas.Redo()` |
| Reset | `DrawingCanvas.ClearCanvas()` |
| Submit | `GameManager.OnSubmit()` 또는 fallback submit |

Undo/Redo 시각 상태:

- `DrawingCanvas.CanUndo`가 true면 `undoActive`, false면 `undoInactive`
- `DrawingCanvas.CanRedo`가 true면 `redoActive`, false면 `redoInactive`
- `DrawingCanvas.UndoRedoChanged` 이벤트를 받아 갱신

## 9. ThicknessSliderHandle

`ThicknessSliderHandle`은 손잡이 자체에 붙는 드래그 입력 스크립트다.

필드:

| 필드 | 현재 값 | 설명 |
|------|---------|------|
| `track` | `ThicknessTrack` RectTransform | 기준 좌표계 |
| `handle` | `SliderHandle` RectTransform | 움직이는 손잡이 |
| `centerY` | `4.98325` | 사용자가 맞춘 중앙 기준 |
| `minY` | `-70` | 이동 하한 |
| `maxY` | `70` | 이동 상한 |
| `NormalizedValue` | 기본 `0.5` | 0~1 정규화 값 |

입력:

- `OnPointerDown`
- `OnDrag`

처리:

```text
포인터 위치를 track 로컬 좌표로 변환
localPos.y - centerY를 minY~maxY로 clamp
handle.anchoredPosition.y = centerY + clampedY
t = InverseLerp(minY, maxY, clampedY)
ValueChanged(t)
```

`SceneADrawingUIController.OnThicknessChanged(t)`는 이 `t`를 현재 도구의 min/max 굵기 범위에 매핑한다.

```text
thickness = round(lerp(toolMin, toolMax, t))
DrawingCanvas.SetBrushSize(thickness)
ThicknessText = "{thickness} px"
PreviewDot 크기 갱신
```

## 10. 팔레트와 COLOR RGB 박스

팔레트는 8칸이다. 기본색은 `SceneABuilder.cs`의 `paletteDefaultColors`에 있다.

기본 팔레트:

| 슬롯 | RGB | 용도 |
|------|-----|------|
| 0 | `226,157,104` | 살색 |
| 1 | `183,115,64` | 밝은 갈색 |
| 2 | `127,69,38` | 중간 갈색 |
| 3 | `66,37,23` | 어두운 갈색 |
| 4 | `106,92,79` | 회갈색 |
| 5 | `185,54,39` | 빨강 |
| 6 | `230,194,148` | 크림색 |
| 7 | `33,45,69` | 남색 |

팔레트 선택:

- 슬롯 클릭 시 `selectedPaletteIndex` 변경
- 슬롯 색을 `DrawingCanvas.SetBrushColor()`에 전달
- 현재 도구가 Picker 또는 Eraser면 자동으로 Pen으로 전환
- 선택 슬롯에는 `Outline`을 켠다.

COLOR 박스 구성:

| 오브젝트 | 역할 |
|----------|------|
| `SaturationValueArea` | 현재 hue 기준 saturation/value 선택 |
| `HueBar` | hue 선택 |
| `RValueInput` | R 값 입력 |
| `GValueInput` | G 값 입력 |
| `BValueInput` | B 값 입력 |
| `ColorPreview` | 현재 색 미리보기 |

텍스처:

- `SaturationValueArea`: 128x128 `Texture2D`
- `HueBar`: 16x128 `Texture2D`
- 둘 다 `FilterMode.Point`
- hue/value/saturation 변경 시 런타임 생성/갱신

RGB 입력:

- `InputField.ContentType.IntegerNumber`
- `characterLimit = 3`
- `onEndEdit`에서 0~255로 clamp
- 입력값은 RGB -> HSV로 변환 후 내부 hue/saturation/value와 동기화

Picker:

- 좌측 `PickerButton` 도구를 선택한다.
- 종이 위 픽셀을 클릭하면 `DrawingCanvas.ColorPicked` 이벤트가 발생한다.
- `SceneADrawingUIController.OnColorPicked()`가 현재 팔레트 슬롯 색을 찍은 색으로 변경한다.
- 이후 자동으로 Pen으로 돌아간다.

## 11. Submit / 채점 / 반응

기본 제출 경로:

```text
SubmitButton
  -> SceneADrawingUIController.OnSubmitClicked()
  -> GameManager.Instance가 있으면 GameManager.OnSubmit()
  -> 없으면 SampleCustomer fallback 채점
```

`GameManager.OnSubmit()`:

```text
currentCustomer 확인
없으면 Resources/Customers/SampleCustomer fallback
DrawingCanvas.GetFlattenedTextureForScoring()
ScoreCalculator.Calculate(playerTex, customer)
ReactionSystem.Evaluate(score, customer)
todayEarnings += payment
customersServed++
mentalDamage면 mentalHealth--
Good/Bad 결과 페이즈 결정
SceneTransition.SceneAToTalkScene(nextPhase)
```

Good/Bad 분기:

| ReactionLevel | TalkScenePhase |
|---------------|----------------|
| `Satisfied` | `ResultGood` |
| `VerySatisfied` | `ResultGood` |
| `Neutral` 이하 | `ResultBad` |

## 12. ScoreCalculator

현재 채점은 정밀 얼굴 인식이 아니라 프로토타입용 8x8 잉크 분포 비교다.

상수:

| 이름 | 값 |
|------|----|
| `GRID_SIZE` | `8` |
| `SLIDE_RANGE` | `2` |
| `INK_THRESHOLD` | `0.1` |

처리 순서:

```text
playerTex -> BuildGrid()
customer.referenceImage -> BuildGrid()
dx, dy를 -2~2 범위로 이동시키며 CompareGrids()
가장 높은 점수 반환
```

`BuildGrid()`:

- 입력 텍스처를 `RenderTexture`를 거쳐 읽기 가능한 512x512 `Texture2D`로 복사한다.
- 512x512를 8x8 그리드로 나눈다.
- 한 칸은 64x64 픽셀이다.
- 픽셀의 RGB가 흰색 `(1,1,1)`과 얼마나 다른지 계산한다.
- diff가 `INK_THRESHOLD`보다 크면 잉크로 본다.
- 한 칸의 5% 이상이 잉크면 그 칸은 `1`, 아니면 `0`.

`CompareGrids()`:

- reference에 잉크가 있고 player에도 잉크가 있으면 가산
- reference가 비어 있는데 player가 칠한 곳은 penalty
- `CustomerData.zoneWeights`가 64개 있으면 칸별 가중치 사용
- 잘못 그린 잉크 penalty는 `wrongPenalty * 0.5`, 최종 penalty는 최대 30점 규모

주의:

- 이 방식은 “얼굴이 닮았는가”보다 “기준 이미지와 잉크 분포가 비슷한가”에 가깝다.
- 손님별 기준 이미지를 잘 만들지 않으면 점수 품질도 낮아진다.
- 현재 다음 작업 후보 중 하나가 손님별 기준 이미지/채점 데이터 연결이다.

## 13. CustomerData / ReactionSystem

`CustomerData` 주요 필드:

| 필드 | 설명 |
|------|------|
| `customerName` | 손님 이름 |
| `type` | Normal / Picky / Relaxed / Jerk |
| `referenceImage` | 채점 기준 이미지 |
| `patienceTime` | 인내심 시간, 현재 루프에는 본격 미연동 |
| `basePay` | 기본 보상 |
| `zoneWeights` | 8x8 = 64칸 가중치 |

손님 타입별 점수 보정:

| 타입 | `ScoreOffset()` | 의미 |
|------|-----------------|------|
| Normal | `0` | 보정 없음 |
| Picky | `15` | 더 까다로움 |
| Relaxed | `-15` | 더 관대함 |
| Jerk | `20` | 더 까다로움 |

반응 기준:

```text
adjusted = score - ScoreOffset()
87 이상: VerySatisfied
70 이상: Satisfied
50 이상: Neutral
25 이상: Unsatisfied
25 미만: VeryUnsatisfied
```

보상:

| 반응 | 보상 |
|------|------|
| VerySatisfied | `basePay * 2.0` |
| Satisfied | `basePay * 1.5` |
| Neutral | `basePay` |
| Unsatisfied | `basePay * 0.5` |
| VeryUnsatisfied | `0` |

멘탈 피해:

- `VeryUnsatisfied`면 true
- 손님 타입이 `Jerk`이고 `Unsatisfied`여도 true

## 14. 빌더와 에셋 관리

사용 메뉴:

| 메뉴 | 스크립트 | 설명 |
|------|----------|------|
| `Yorki/Build Talk Scene` | `TalkSceneBuilder.cs` | TalkScene 재생성 |
| `Yorki/Build Scene A` | `SceneABuilder.cs` | SceneA 재생성 |

공통 Editor 유틸:

| 스크립트 | 역할 |
|----------|------|
| `YorkiEditorAssets.cs` | Moneygraphy Pixel 폰트 로딩, UI 스프라이트 import 설정 |

중요:

- 빌더는 씬을 재생성한다.
- Unity에서 수동으로 맞춘 UI 좌표는 반드시 빌더 코드에도 반영해야 한다.
- 특히 다음 값은 임의 변경 금지:
  - TalkScene `Customer`
  - TalkScene `DialogueBox`
  - TalkScene `DialogueText`
  - SceneA `DrawingPaper`
  - SceneA `DrawingSurface`
  - SceneA `SliderHandle`

## 15. 현재 남은 기술 과제

우선순위:

1. 실제 Play 모드에서 `TalkScene -> SceneA -> Submit -> TalkScene 결과 대사` 전체 수동 검증
2. 결과 대사 종료 후 다음 손님/하루 루프로 넘어가는 구조 설계
3. 손님별 `CustomerData.referenceImage`와 `zoneWeights` 제작/연동
4. RGB 컬러 박스 조작감 세부 조정
5. 구형 SampleScene 계열 스크립트 유지/삭제 결정

현재 가장 중요한 미구현:

- 결과 대사 종료 후 다음 손님을 부르거나 하루를 끝내는 로직
- 여러 손님 데이터
- 하루 수입/손님 수를 보여주는 정산 화면

## 16. 파일별 책임 맵

Scene A 관련 파일은 현재 기능이 꽤 분산되어 있으므로, 작업 전에 어느 파일을 봐야 하는지 먼저 정리한다.

### 런타임 스크립트

| 파일 | 핵심 책임 | 현재 중요도 |
|------|-----------|-------------|
| `Assets/Scripts/TalkSceneController.cs` | TalkScene 대사 타이핑, 입력 처리, 결과 페이즈 대사, PreDraw 종료 후 SceneA 이동 | 높음 |
| `Assets/Scripts/SceneTransition.cs` | TalkScene/SceneA 로드, CustomerStage 슬라이드, DialogueBox/RightPanel 페이드, SceneA 진입 시 손님 기본 컷 보정 | 높음 |
| `Assets/Scripts/PersistentBootstrap.cs` | TalkScene에서 생성된 PersistentCanvas/CustomerStage/Customer 참조 보관 및 DontDestroyOnLoad 처리 | 높음 |
| `Assets/Scripts/CustomerDisplay.cs` | 손님 컷 교체, 말하기 프레임 진행, 입 닫기, Shake | 높음 |
| `Assets/Scripts/DrawingCanvas.cs` | 실제 512x512 드로잉 텍스처, 마우스 입력, 브러시/지우개/피커, Undo/Redo, 제출용 합성 | 높음 |
| `Assets/Scripts/SceneADrawingUIController.cs` | SceneA 오른쪽 UI 전체 연결, 도구 버튼, 액션 버튼, 팔레트, RGB 박스, Submit | 높음 |
| `Assets/Scripts/ThicknessSliderHandle.cs` | THICKNESS 손잡이 드래그 입력 및 0~1 정규화 값 전달 | 중간 |
| `Assets/Scripts/GameManager.cs` | Submit 후 채점, 수입/손님 수/멘탈 수치 갱신, 결과 페이즈 결정 | 중간 |
| `Assets/Scripts/ScoreCalculator.cs` | 드로잉 결과와 기준 이미지를 8x8 잉크 분포로 비교해 0~100 점수 산출 | 중간 |
| `Assets/Scripts/ReactionSystem.cs` | 점수와 손님 데이터 기반 반응 등급/대사/보상 계산 | 중간 |
| `Assets/Scripts/CustomerData.cs` | 손님별 이름, 타입, 기준 이미지, 보상, 8x8 zoneWeights | 중간 |
| `Assets/Scripts/ReactionUI.cs` | 구형 SampleScene 반응 UI. 현재 TalkScene 결과 페이즈 흐름에서는 주 흐름 아님 | 낮음 |
| `Assets/Scripts/DialogueUI.cs` | 구형 SampleScene 하단 대사창. 현재 TalkScene 대사에는 사용하지 않음 | 낮음 |
| `Assets/Scripts/NarratorController.cs` | 구형 내레이터 흐름. 현재 TalkScene/SceneA 메인 흐름에서는 후순위 | 낮음 |

### Editor 빌더

| 파일 | 핵심 책임 |
|------|-----------|
| `Assets/Editor/TalkSceneBuilder.cs` | TalkScene 재생성. PersistentBootstrap, PersistentCanvas, CustomerStage, Background, Customer, DialogueBox, TalkSceneController, SceneTransition 생성 |
| `Assets/Editor/SceneABuilder.cs` | SceneA 재생성. RightPanel, DeskBase, DrawingPaper, DrawingSurface, DrawingCanvas, 도구/팔레트/RGB/Submit UI 생성 |
| `Assets/Editor/YorkiEditorAssets.cs` | 공통 UI 폰트 로딩, Sprite import 설정 |

### 씬 파일

| 파일 | 설명 |
|------|------|
| `Assets/Scenes/TalkScene.unity` | 대화 씬. 현재 buildIndex 0 |
| `Assets/Scenes/SceneA.unity` | 드로잉 씬. 현재 buildIndex 1 |

### 주요 에셋 경로

| 종류 | 경로 |
|------|------|
| TalkScene 배경 | `Assets/Sprites/SceneA/MapBackgroundDraft2.png` |
| 손님 컷 | `Assets/Sprites/SceneA/Customer_*.png` |
| SceneA 작업대 베이스 | `Assets/Sprites/UI/SceneA/Desk/ui_fixed.png` |
| SceneA RGB 레퍼런스 | `Assets/Sprites/UI/SceneA/Desk/ui_final_rgb_reference.png` |
| SceneA 도구 버튼 | `Assets/Sprites/UI/SceneA/Tools/...` |
| SceneA 액션 버튼 | `Assets/Sprites/UI/SceneA/Actions/...` |
| THICKNESS 손잡이 | `Assets/Sprites/UI/SceneA/Controls/slider_handle.png` |
| UI 폰트 | `Assets/Fonts/Moneygraphy-Pixel.ttf` |

## 17. 씬 생명주기 상세

### 17.1 게임을 TalkScene에서 시작하는 경우

1. Unity가 `TalkScene.unity`를 로드한다.
2. `PersistentBootstrap.Awake()`가 실행된다.
3. 이미 살아있는 `PersistentBootstrap.Instance`가 없으면 자기 자신을 `Instance`로 등록하고 `DontDestroyOnLoad(gameObject)`를 호출한다.
4. `SceneTransition.Awake()`가 실행된다.
5. 이미 살아있는 `SceneTransition.Instance`가 없으면 자기 자신을 `Instance`로 등록하고 `DontDestroyOnLoad(gameObject)`를 호출한다.
6. `TalkSceneController.Start()`가 실행된다.
7. `TalkSceneController.BindReferences()`가 필요한 참조를 찾는다.
8. `useGameManagerPhase = true`이면 `GameManager.currentTalkPhase`를 읽어 현재 페이즈로 사용한다.
9. `GameManager.currentTalkPhase`의 초기값은 `PreDraw`다.
10. `PreDraw` 대사가 첫 줄부터 출력된다.

### 17.2 PreDraw 대사 종료 시

1. 플레이어가 마지막 대사까지 넘긴다.
2. `TalkSceneController.OnDialogueEnd()`가 호출된다.
3. `phase == TalkScenePhase.PreDraw`이므로 결과 로그를 찍지 않고 전환 흐름으로 간다.
4. 전환 직전 `customerDisplay.SetEmotion("neutral")`과 `customerDisplay.StopTalking()`을 호출한다.
5. 목적은 손님 입이 열린 컷으로 SceneA에 남는 문제를 막는 것이다.
6. `sceneTransition.TalkSceneToSceneA()`가 호출된다.
7. `SceneTransition.TalkSceneToSceneA()` 내부에서도 `ResetCustomerForDrawing()`을 한 번 더 호출한다.
8. `TransitionRoutine("SceneA", DrawingStagePosition, true, TalkScenePhase.PreDraw)` 코루틴이 시작된다.

### 17.3 TalkScene -> SceneA 전환 중

1. `IsTransitioning = true`가 된다.
2. 현재 `CustomerStage` 위치를 시작점으로 저장한다.
3. `DialogueBox`의 CanvasGroup을 찾아 outgoingGroup으로 둔다.
4. 코루틴 동안 `CustomerStage.anchoredPosition`을 `TalkStagePosition`에서 `DrawingStagePosition`으로 보간한다.
5. `DialogueBox.alpha`는 `fadeDuration = 0.3` 기준으로 1에서 0으로 감소한다.
6. `elapsed >= loadAt` 시점에 `SceneManager.LoadScene("SceneA")`가 호출된다.
7. 새 SceneA에서 `RightPanel` CanvasGroup을 찾아 incomingGroup으로 둔다.
8. `RightPanel.alpha`를 0으로 놓고 `FadeCanvasGroup()`으로 1까지 올린다.
9. 전환 종료 시 `CustomerStage.anchoredPosition`을 정확히 `(-320, 0)`으로 고정한다.
10. `IsTransitioning = false`가 된다.

### 17.4 SceneA에서 Submit

1. 플레이어가 `SubmitButton`을 클릭한다.
2. `SceneADrawingUIController.OnSubmitClicked()`가 실행된다.
3. `drawingCanvas` 참조가 없으면 warning을 출력하고 중단한다.
4. `GameManager.Instance`가 있으면 `GameManager.Instance.drawingCanvas = drawingCanvas`로 현재 캔버스를 넣는다.
5. 이어서 `GameManager.Instance.OnSubmit()`을 호출한다.
6. `GameManager.Instance`가 없으면 `Resources/Customers/SampleCustomer`를 fallback으로 로드한다.
7. fallback 손님도 없으면 `ResultGood`으로 TalkScene 복귀한다.

### 17.5 Submit -> 결과 TalkScene

1. `GameManager.OnSubmit()` 또는 fallback submit이 채점한다.
2. 결과 등급이 `Satisfied` 또는 `VerySatisfied`면 `TalkScenePhase.ResultGood`이다.
3. 그 외는 `TalkScenePhase.ResultBad`다.
4. `SceneTransition.SceneAToTalkScene(nextPhase)`가 호출된다.
5. `GameManager.currentTalkPhase`에 다음 페이즈가 저장된다.
6. SceneA -> TalkScene 전환 코루틴이 실행된다.
7. `RightPanel`이 페이드아웃되고 `CustomerStage`가 중앙으로 이동한다.
8. `TalkScene` 로드 후 `TalkSceneController.Start()`가 `GameManager.currentTalkPhase`를 읽는다.
9. ResultGood 또는 ResultBad 대사를 출력한다.

### 17.6 결과 대사 종료 후 현재 상태

현재 `TalkSceneController.OnDialogueEnd()`는 `PreDraw`가 아닌 페이즈에서는 다음 루프를 시작하지 않는다.

```text
ResultGood/ResultBad 대사 종료
  -> dialogueText 비움
  -> 화살표 숨김
  -> Debug.Log("[TalkSceneController] 결과 대사 종료")
  -> 끝
```

다음 개발 단계에서는 이 지점에 다음 손님 호출, 하루 손님 수 체크, 하루 종료 화면 이동 중 하나를 연결해야 한다.

## 18. Canvas / UI 렌더링 계층

현재 Scene A 흐름에는 Unity Canvas가 두 종류 있다.

| Canvas | 생성 위치 | sortingOrder | 역할 |
|--------|-----------|--------------|------|
| `PersistentCanvas` | TalkScene의 `PersistentBootstrap` 하위 | `0` | 손님과 배경을 씬 전환 중 유지 |
| `SceneCanvas` | TalkScene 또는 SceneA 루트 | `10` | 현재 씬 전용 UI. TalkScene은 대화창, SceneA는 RightPanel |

이 구조 때문에 다음 규칙이 중요하다.

- 손님과 배경은 SceneA 씬에 직접 있지 않다.
- SceneA의 `RightPanel`만 새 씬에 존재한다.
- TalkScene의 대화창은 `PersistentCanvas`가 아니라 `SceneCanvas`에 있다.
- 전환 중에는 `PersistentCanvas`가 살아 있고, 씬 전용 `SceneCanvas`가 로드/언로드된다.

### TalkScene 렌더링 순서

```text
PersistentCanvas sortingOrder 0
  Background
  Customer

SceneCanvas sortingOrder 10
  DialogueBox
  DialogueText
  ContinueArrow
```

### SceneA 렌더링 순서

```text
PersistentCanvas sortingOrder 0
  Background
  Customer

SceneCanvas sortingOrder 10
  RightPanel
    DeskBase
    DrawingPaper / DrawingSurface
    tool/action/palette/color controls
```

## 19. TalkScene 오브젝트 필드 상세

### PersistentCanvas

| 항목 | 값 |
|------|----|
| renderMode | `ScreenSpaceOverlay` |
| sortingOrder | `0` |
| CanvasScaler | `ScaleWithScreenSize` |
| referenceResolution | `1280 x 720` |
| screenMatchMode | `MatchWidthOrHeight` |
| matchWidthOrHeight | `0.5` |
| GraphicRaycaster | 있음 |

### CustomerStage

| 항목 | 값 |
|------|----|
| anchorMin / anchorMax | `(0.5, 0.5)` |
| pivot | `(0.5, 0.5)` |
| sizeDelta | `(0, 0)` |
| TalkScene 위치 | `(0, 0)` |
| SceneA 위치 | `(-320, 0)` |

### Background

| 항목 | 값 |
|------|----|
| Image sprite | `MapBackgroundDraft2.png` |
| preserveAspect | `true` |
| anchoredPosition | `(0, 0)` |
| sizeDelta | `1330 x 720` |

### Customer

| 항목 | 값 |
|------|----|
| Image sprite | `Customer_Neutral_Idle.png` |
| preserveAspect | `true` |
| anchoredPosition | `(-9.320013, -29.144989)` |
| sizeDelta | `652.1983 x 778.29` |
| components | `Image`, `FadeIn`, `CustomerDisplay` |

### DialogueBox

| 항목 | 값 |
|------|----|
| Image sprite | 없음 |
| Image type | `Simple` |
| Image color | 어두운 반투명, alpha `0.78` |
| raycastTarget | `false` |
| Outline | 있음 |
| CanvasGroup | 있음 |
| anchoredPosition | `(0, -250)` |
| sizeDelta | `820 x 160` |

### DialogueText

| 항목 | 값 |
|------|----|
| 부모 | `DialogueBox` |
| anchorMin / anchorMax | `(0,0)` / `(1,1)` |
| anchoredPosition | `(0, 0)` |
| sizeDelta | `(-72, -52)` |
| font | `Moneygraphy-Pixel.ttf` |
| fontSize | `22` |
| alignment | `MiddleLeft` |
| color | 밝은 크림 계열 |

### ContinueArrow

| 항목 | 값 |
|------|----|
| text | `▼` |
| anchorMin / anchorMax | `(1,0)` / `(1,0)` |
| pivot | `(1,0)` |
| anchoredPosition | `(-18, 16)` |
| sizeDelta | `20 x 20` |
| fontSize | `14` |
| color | 밝은 크림 계열 |

## 20. SceneA 오브젝트 필드 상세

### RightPanel

| 항목 | 값 |
|------|----|
| 부모 | `SceneCanvas` |
| anchoredPosition | `(320, 0)` |
| sizeDelta | `640 x 720` |
| Image color | 투명 |
| CanvasGroup | 있음 |
| 역할 | 오른쪽 반칸 전체 UI 컨테이너 |

### DeskBase

| 항목 | 값 |
|------|----|
| 부모 | `RightPanel` |
| sprite | `ui_fixed.png` |
| anchoredPosition | `(0, 0)` |
| sizeDelta | `640 x 720` |
| preserveAspect | `false` |
| raycastTarget | `false` |

### ReferenceOverlay_FinalRGB

| 항목 | 값 |
|------|----|
| 부모 | `RightPanel` |
| sprite | `ui_final_rgb_reference.png` |
| active | `false` |
| 용도 | 배치 비교용 레퍼런스. 런타임 UI가 아님 |

### DrawingPaper

| 항목 | 값 |
|------|----|
| 부모 | `RightPanel` |
| anchoredPosition | `(-18.18435, 118)` |
| sizeDelta | `330.915 x 406.5569` |
| 역할 | 종이 영역 기준 RectTransform |

### DrawingSurface

| 항목 | 값 |
|------|----|
| 부모 | `DrawingPaper` |
| anchorMin / anchorMax | `(0,0)` / `(1,1)` |
| offsetMin / offsetMax | `(0,0)` / `(0,0)` |
| components | `RawImage`, `DrawingCanvas` |
| RawImage color | `white` |
| raycastTarget | `true` |
| 역할 | 실제 드로잉 입력/표시 영역 |

### 도구 버튼

| 버튼 | 위치 | 크기 | 기본 sprite |
|------|------|------|-------------|
| `PenButton` | `(-257, 277.4)` | `95 x 105` | `pen_selected.png` |
| `BrushButton` | `(-257, 180.1)` | `95 x 95.2` | `brush_base.png` |
| `EraserButton` | `(-257, 84.1)` | `95 x 97.3` | `eraser_base.png` |
| `PickerButton` | `(-257.4, -14.7)` | `95 x 97.2` | `picker_base.png` |

각 버튼은 런타임에 `SceneADrawingUIController.WireToolButton()`이 `EventTrigger`를 붙여 입력을 처리한다.

### 액션 버튼

| 버튼 | 위치 | 크기 | 동작 |
|------|------|------|------|
| `UndoButton` | `(198.9, -27)` | `67.2 x 86.8` | Undo |
| `RedoButton` | `(265.6, -26.6)` | `67.2 x 86.8` | Redo |
| `ResetButton` | `(220.3, -153)` | `175.3 x 94.1` | ClearCanvas |
| `SubmitButton` | `(219.9, -264.2)` | `163.8 x 112.5` | Submit |

## 21. DrawingCanvas 내부 동작 상세

### 21.1 텍스처 생성

`DrawingCanvas.Start()`에서 다음 순서로 초기화한다.

```text
rawImage = GetComponent<RawImage>()
rectTransform = GetComponent<RectTransform>()
drawTexture = new Texture2D(512, 512, RGBA32, false)
drawTexture.filterMode = Point
ResetCanvas()
rawImage.texture = drawTexture
savedDrawColor = brushColor
```

`transparentBackground = true`이면 `ResetCanvas()`가 모든 픽셀을 `(0,0,0,0)`으로 채운다. SceneA에서는 종이 그림이 `DeskBase` 이미지에 포함되어 있으므로, 드로잉 레이어가 흰색으로 덮이면 안 된다. 그래서 투명 배경을 쓴다.

### 21.2 입력 좌표 계산

포인터 입력은 UI 좌표계에서 들어온다. `ScreenToTexture()`는 다음 계산으로 텍스처 픽셀 좌표를 만든다.

```text
localPos.x / rect.width + 0.5 -> 0~1 범위
localPos.y / rect.height + 0.5 -> 0~1 범위
x = normalizedX * canvasWidth
y = normalizedY * canvasHeight
```

주의:

- 이 계산은 `DrawingSurface` RectTransform이 실제 입력 가능한 종이 영역과 맞아야 정상이다.
- 종이 영역을 수정하면 `DrawingPaper` / `DrawingSurface` RectTransform을 같이 검토해야 한다.
- 현재는 종이 전체를 axis-aligned rectangle로 사용한다. 종이 테이프와 약간 겹치는 것도 허용한다.

### 21.3 브러시 모양

브러시는 원형이다.

```text
for x = -radius..radius
for y = -radius..radius
if x*x + y*y <= radius*radius:
    SetPixel(cx+x, cy+y, color)
```

결과적으로 `brushSize`는 지름이 아니라 반지름처럼 동작한다. 즉 `brushSize = 6`이면 중심 기준 반경 6픽셀 원을 찍는다.

### 21.4 선 보간

마우스를 빠르게 움직여도 선이 끊기지 않게 `PaintStroke()`에서 이전 점과 현재 점 사이를 보간한다.

```text
dist = Vector2.Distance(from, to)
steps = max(1, round(dist * 2))
for i = 0..steps:
    t = i / steps
    point = Lerp(from, to, t)
    PaintDot(point)
```

`dist * 2`이므로 1픽셀 간격보다 더 촘촘하게 점을 찍는 편이다. 대신 큰 브러시에서는 성능 비용이 커질 수 있다. 현재 512x512 프로토타입에서는 문제 없다.

### 21.5 Undo 스냅샷 비용

Undo는 벡터 명령 기록이 아니라 전체 텍스처 픽셀 스냅샷이다.

```text
512 * 512 = 262,144 pixels
Color32 = 4 bytes
스냅샷 1개 약 1MB
MAX_UNDO 20이면 undoStack만 약 20MB
redoStack까지 최대치면 더 커질 수 있음
```

프로토타입에서는 단순하고 안정적인 방식이다. 나중에 모바일이나 큰 캔버스를 고려하면 stroke command 기반 Undo 또는 dirty rect 기반 스냅샷으로 바꿀 수 있다.

### 21.6 투명 지우개와 제출 합성

SceneA에서는 `transparentBackground = true`다.

따라서 Eraser는 흰색을 칠하는 게 아니라 alpha 0 픽셀로 지운다.

```csharp
if (currentTool == DrawingTool.Eraser)
    return transparentBackground ? new Color(0,0,0,0) : backgroundColor;
```

하지만 `ScoreCalculator`는 흰 배경 기준으로 잉크를 판정한다. 그래서 제출 때만 흰색 배경과 합친다.

```text
투명 드로잉 레이어
  -> GetFlattenedTextureForScoring()
  -> 흰 배경 + 드로잉 색 합성
  -> alpha 1 텍스처
  -> ScoreCalculator
```

이 구조 덕분에 화면에서는 종이 질감 위에 투명 잉크처럼 보이고, 채점에서는 기존 흰 배경 기준 알고리즘을 재사용할 수 있다.

## 22. SceneADrawingUIController 내부 동작 상세

### 22.1 Start 초기화 순서

`SceneADrawingUIController.Start()`는 다음 순서로 연결한다.

```text
WireToolButton(Pen/Brush/Eraser/Picker)
WireActionButton(Undo/Redo/Reset/Submit)
WirePaletteSlots()
WireColorBox()
thicknessSlider.ValueChanged += OnThicknessChanged
thicknessSlider.SetNormalizedValue(0.5, false)
drawingCanvas.UndoRedoChanged += RefreshActionVisuals
drawingCanvas.ColorPicked += OnColorPicked
첫 팔레트 슬롯 색 적용
SelectTool(Pen)
RefreshPaletteSelectionVisuals()
RefreshActionVisuals()
```

결과적으로 씬 시작 상태는 다음과 같다.

- 현재 도구: Pen
- 현재 색: 팔레트 0번 색
- Thickness 슬라이더 정규화 값: 0.5
- Pen 굵기: 기본 6px
- Undo/Redo 버튼은 스택 상태에 따라 비활성 sprite로 갱신

### 22.2 도구 전환과 굵기 리셋

도구를 바꿀 때마다 슬라이더는 중앙으로 돌아간다.

```text
SelectTool(Brush)
  -> DrawingCanvas.SetTool(Brush)
  -> brushSize = brushDefault
  -> thicknessSlider.SetNormalizedValue(0.5, false)
  -> ThicknessText = "12 px"
```

현재 구현은 “도구별 이전 굵기 기억”이 아니라 “도구 전환 시 해당 도구 기본 굵기로 복귀”에 가깝다. 예전 계획서에는 도구별 굵기 기억이 있었지만, 현행 코드 기준은 중앙 초기화다.

### 22.3 팔레트와 현재 색 동기화

색 상태는 세 곳에 동시에 존재한다.

| 위치 | 설명 |
|------|------|
| `paletteSlots[i].color` | UI 팔레트 슬롯 표시색 |
| `currentHue/currentSaturation/currentValue` | COLOR 박스 내부 HSV 상태 |
| `DrawingCanvas.brushColor` / `savedDrawColor` | 실제 그리기 색 |

색을 바꾸는 모든 경로는 최종적으로 `ApplyColorToSelection()`을 거친다.

```text
ApplyColorToSelection(color, syncHsv)
  -> alpha = 1
  -> 필요하면 RGBToHSV로 내부 HSV 갱신
  -> 현재 팔레트 슬롯 색 변경
  -> DrawingCanvas.SetBrushColor(color)
  -> RefreshColorBoxVisuals()
```

### 22.4 RGB 필드 무한 루프 방지

RGB InputField는 `onEndEdit`에서 `OnRgbInputEdited()`를 호출한다. 반대로 코드에서 RGB 텍스트를 갱신할 때도 InputField 값이 바뀐다. 이때 다시 이벤트가 발생해 루프가 생기지 않도록 `isUpdatingRgbFields` 플래그를 쓴다.

```text
RefreshColorBoxVisuals()
  -> isUpdatingRgbFields = true
  -> SetTextWithoutNotify()
  -> isUpdatingRgbFields = false
```

이 패턴을 유지하지 않으면 RGB 입력 중 색 갱신이 여러 번 꼬일 수 있다.

### 22.5 Hue/SV 텍스처 생성

`HueBar`는 16x128 텍스처다.

- y가 위쪽일수록 hue가 낮거나 높게 매핑된다.
- 현재 구현은 `hue = 1 - y/(size-1)`이다.
- 클릭/드래그하면 `currentHue = 1 - normalizedY`로 갱신된다.

`SaturationValueArea`는 128x128 텍스처다.

- x축: saturation
- y축: value
- 현재 hue 기준으로 `Color.HSVToRGB(currentHue, saturation, value)`를 채운다.
- hue가 바뀌면 SV 텍스처를 다시 생성한다.

## 23. 채점 알고리즘 상세 예시

현재 채점은 그림의 세부 형태가 아니라 8x8 잉크 분포를 비교한다. 예를 들어 기준 이미지의 중앙 위쪽에 잉크가 많고, 플레이어 그림도 비슷한 위치에 잉크가 있으면 점수가 오른다.

### 23.1 BuildGrid 예시

512x512 텍스처를 8x8로 나누면 한 칸은 64x64다.

```text
512 / 8 = 64
64 * 64 = 4096 pixels per cell
```

한 칸에서 5% 이상이 잉크면 그 칸은 1이다.

```text
4096 * 0.05 = 204.8
약 205픽셀 이상이 흰색과 다르면 그 칸은 ink = 1
```

### 23.2 흰색과의 차이

픽셀별 잉크 판단:

```text
diff = (abs(r - 1) + abs(g - 1) + abs(b - 1)) / 3
if diff > 0.1: ink
```

검정 선은 diff가 거의 1이라 잉크다. 매우 연한 색은 diff가 낮아 잉크로 잡히지 않을 수 있다.

### 23.3 슬라이딩 윈도우

플레이어 그림이 기준보다 조금 밀려도 완전히 실패하지 않게 `dx/dy = -2..2` 범위로 비교한다.

```text
총 비교 횟수 = 5 * 5 = 25
25개 중 가장 높은 점수 사용
```

이건 “조금 옆으로 그린 그림”을 보정하기 위한 임시 장치다.

### 23.4 가중치

`CustomerData.zoneWeights`가 64개라면 각 칸에 가중치를 줄 수 있다.

예:

- 얼굴 중심부: 2.0
- 머리 윤곽: 1.5
- 배경이나 덜 중요한 칸: 0.5

현재는 기준 이미지와 zoneWeights를 손님별로 제대로 만들지 않으면 채점 품질이 떨어진다.

## 24. 빌더 재실행 절차

빌더는 편하지만 위험하다. 씬을 새로 만들기 때문이다.

### 24.1 TalkScene 빌더 실행 전 확인

확인할 값:

- `TalkSceneBuilder`의 `Customer` 위치/크기가 현재 씬과 맞는지
- `DialogueBox` 위치/크기/색/alpha가 현재 씬과 맞는지
- `DialogueText` padding이 현재 씬과 맞는지
- `ContinueArrow` 위치가 현재 씬과 맞는지
- `MapBackgroundDraft2.png` 경로가 유지되는지
- `Moneygraphy-Pixel.ttf`가 존재하는지

### 24.2 SceneA 빌더 실행 전 확인

확인할 값:

- `DrawingPaper` 위치/크기
- `DrawingSurface` stretch 상태
- `SliderHandle` 위치/크기
- `ThicknessSliderHandle.centerY/minY/maxY`
- 좌측 도구 버튼 위치/크기
- 팔레트 슬롯 위치/크기
- RGB 박스 위치/크기
- Undo/Redo/Reset/Submit 위치/크기

### 24.3 빌더 실행 후 확인

빌더를 실행한 뒤에는 최소한 다음을 확인한다.

```text
TalkScene validate
SceneA validate
dotnet build Assembly-CSharp.csproj
dotnet build Assembly-CSharp-Editor.csproj
Play 모드 TalkScene 진입
PreDraw 끝까지 진행
SceneA 전환
종이에 선 그리기
Submit
결과 TalkScene 복귀
```

## 25. Play 모드 수동 검증 체크리스트

### TalkScene

- [ ] 첫 진입 시 배경이 `MapBackgroundDraft2`로 보인다.
- [ ] 손님이 중앙에 보인다.
- [ ] 대화창이 하단에 820x160 정도로 보이며 너무 크지 않다.
- [ ] 대화창이 배경을 약간 비추는 반투명이다.
- [ ] 텍스트가 어두운 대화창 위에서 잘 읽힌다.
- [ ] `Enter` / `Space`로 타이핑 스킵이 된다.
- [ ] 타이핑이 끝난 뒤 `▼`가 깜빡인다.
- [ ] 손님 입 컷이 과하게 흔들리지 않는다.

### TalkScene -> SceneA

- [ ] 마지막 PreDraw 대사 후 대화창이 페이드아웃된다.
- [ ] 손님/배경이 왼쪽으로 자연스럽게 이동한다.
- [ ] SceneA가 로드될 때 손님 입이 닫힌 neutral 기본 컷이다.
- [ ] RightPanel이 페이드인된다.
- [ ] 손님이 오른쪽 작업대와 겹치지 않는다.

### SceneA 드로잉

- [ ] 종이 영역 위에서만 그려진다.
- [ ] Pen 선택 시 검정 선이 정상적으로 그려진다.
- [ ] Brush 선택 시 알파가 적용된 선이 그려진다.
- [ ] Eraser 선택 시 그린 선이 투명하게 지워진다.
- [ ] Picker 선택 후 종이 색을 찍으면 현재 팔레트 슬롯 색이 바뀐다.
- [ ] Thickness 손잡이가 검은 홈 밖으로 지나치게 벗어나지 않는다.
- [ ] Thickness 숫자와 PreviewDot이 바뀐다.
- [ ] Undo/Redo 버튼이 스택 상태에 맞게 활성/비활성 이미지로 바뀐다.
- [ ] Reset이 캔버스를 초기화한다.

### COLOR / RGB

- [ ] 팔레트 슬롯 클릭 시 현재 색이 바뀐다.
- [ ] 선택된 슬롯 Outline이 보인다.
- [ ] RGB 숫자를 입력하면 슬롯 색과 브러시 색이 바뀐다.
- [ ] HueBar 드래그 시 색상 계열이 바뀐다.
- [ ] SaturationValueArea 드래그 시 채도/명도가 바뀐다.
- [ ] COLOR 박스 내부에 별도 스포이드 아이콘은 없다.

### Submit

- [ ] Submit 클릭 시 오류 없이 채점된다.
- [ ] Console에 점수/반응 로그가 출력된다.
- [ ] Good/Bad 결과에 따라 TalkScene으로 복귀한다.
- [ ] ResultGood 또는 ResultBad 대사가 나온다.
- [ ] 결과 대사 종료 후 현재는 다음 루프가 없어 멈춘다.

## 26. 흔한 문제와 원인

| 증상 | 가능한 원인 | 확인할 곳 |
|------|-------------|----------|
| SceneA에서 손님 입이 벌어진 채 멈춤 | SceneA 전환 전 `StopTalking()`/neutral reset 누락 | `TalkSceneController.OnDialogueEnd`, `SceneTransition.ResetCustomerForDrawing` |
| 대화창 위치가 다시 커짐 | TalkScene 씬만 수정하고 `TalkSceneBuilder`에 반영하지 않음 | `TalkSceneBuilder.cs` |
| 빌더 실행 후 SliderHandle 위치가 바뀜 | Unity 수동 조정값이 `SceneABuilder`에 반영되지 않음 | `SceneABuilder.cs` |
| 종이 밖에 선이 그려짐 | `DrawingSurface` RectTransform이 종이보다 큼 | `SceneA.unity`, `SceneABuilder` |
| 종이 가장자리에서 선이 끊김 | `DrawingSurface`가 종이보다 작음 | `DrawingPaper`, `DrawingSurface` |
| Submit 점수가 항상 0 | `CustomerData.referenceImage` 없음 또는 읽기/기준 이미지 문제 | `CustomerData`, `Resources/Customers/SampleCustomer` |
| 아주 연한 색이 채점에 안 잡힘 | 흰색과의 diff가 `INK_THRESHOLD` 이하 | `ScoreCalculator.INK_THRESHOLD` |
| RGB 입력 시 값이 튐 | `isUpdatingRgbFields` 없이 InputField 이벤트가 중첩됨 | `SceneADrawingUIController` |
| Undo 후 Redo가 사라짐 | 새 그리기 시작 시 redoStack clear되는 정상 동작 | `DrawingCanvas.SaveSnapshot` |
| TalkScene 결과 대사 후 아무 일 없음 | 다음 손님/하루 루프 미구현 | `TalkSceneController.OnDialogueEnd` |

## 27. 다음 구조 확장 방향

### 27.1 다음 손님 루프

추가해야 할 최소 구조:

```text
CustomerQueue 또는 DayManager
  -> 현재 손님 index 관리
  -> TalkScene 시작 전 GameManager.currentCustomer 세팅
  -> 결과 대사 종료 후 다음 손님이 있으면 PreDraw로 다시 시작
  -> 없으면 DayResultScene 또는 정산 UI로 이동
```

`TalkSceneController.OnDialogueEnd()`의 ResultGood/ResultBad 분기 끝에 연결할 가능성이 높다.

### 27.2 손님별 대사 데이터

현재 `TalkSceneController` 대사는 코드 내부 배열에 고정되어 있다. 손님별 대사를 만들려면 다음 중 하나로 바꿔야 한다.

- `CustomerData`에 PreDraw/ResultGood/ResultBad 대사 배열 추가
- 별도 `TalkSceneDialogueData` ScriptableObject 생성
- `GameManager.currentCustomer`가 현재 대사 데이터를 제공

이때 `TalkSceneLine` 구조는 이미 `text`, `emotion`, `shake`를 담고 있으므로 재사용 가능하다.

### 27.3 손님별 기준 이미지

현재 채점은 `CustomerData.referenceImage`에 크게 의존한다. 다음 손님 시스템을 만들려면 각 손님마다 최소한 다음이 필요하다.

- 실제 표시용 손님 스프라이트
- 채점용 referenceImage
- zoneWeights 64칸
- basePay
- type
- 결과 대사

표시용 손님 스프라이트와 채점용 referenceImage는 같은 이미지일 필요가 없다. 오히려 채점용 이미지는 8x8 잉크 분포가 명확한 흑백/단순 형태가 더 안정적일 수 있다.

### 27.4 하루 루프

현재 `GameManager`에는 다음 필드가 이미 있다.

```csharp
public int todayEarnings;
public int customersServed;
public int mentalHealth;
```

하지만 아직 UI와 하루 종료 흐름에는 본격적으로 연결되어 있지 않다. 최소 구현은 다음 정도면 된다.

```text
하루 시작
  -> customersServed = 0
  -> todayEarnings = 0
  -> 손님 1 시작
  -> 결과 종료
  -> customersServed++
  -> 손님 수가 목표보다 작으면 다음 손님
  -> 목표 수 도달 시 정산 화면
```

## 28. 검증 명령

터미널에서 자주 쓰는 검증:

```bash
dotnet build yorki/Assembly-CSharp.csproj --no-restore
dotnet build yorki/Assembly-CSharp-Editor.csproj --no-restore
git diff --check -- SCENE_A_TECH.md
```

Unity MCP 또는 Unity Editor에서 확인할 것:

```text
TalkScene validate: missingScripts 0, brokenPrefabs 0
SceneA validate: missingScripts 0, brokenPrefabs 0
Build Settings:
  TalkScene buildIndex 0
  SceneA buildIndex 1
```
