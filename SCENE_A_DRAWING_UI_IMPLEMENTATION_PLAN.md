# Scene A 드로잉 UI 구현 계획

> 작성일: 2026-05-11
> 상태: 구현 전 상세 계획
> 기준 자료: `Play Ref/UI 초안`

---

## 1. 목표

SceneA의 우측 작업대 영역을 `Play Ref/UI 초안/전체/ui 초안 최종.png`와 같은 배치로 구현한다.

중요 원칙:

- `ui 초안 최종.png`는 단순 참고 이미지가 아니라 최종 배치 기준이다.
- 각 UI 요소는 가능한 한 `Play Ref/UI 초안` 안의 PNG를 그대로 활용한다.
- 버튼/슬라이더/팔레트/RGB 피커의 위치, 크기, 간격은 `ui 초안 최종.png`와 맞춘다.
- 임의로 새 레이아웃을 만들거나 기능 때문에 위치를 바꾸지 않는다.
- 기능 구현은 이미지 위에 클릭 영역과 드래그 영역을 얹는 방식으로 한다.

핵심은 새 드로잉 기능을 처음부터 다시 만드는 것이 아니라, 이미 존재하는 `DrawingCanvas.cs`를 `ui 고정.png`의 테이프로 붙은 종이 영역 위에 실제 드로잉 레이어로 얹는 것이다.

최종 목표는 다음 상태다.

- `TalkScene`: 풀화면 배경 + 손님 + 대사창
- `SceneA`: 좌측 손님 + 우측 작업대
- 우측 작업대는 `ui 고정.png`를 기본판으로 사용
- 종이 영역에는 기존 `DrawingCanvas` 기능을 배치
- 좌측 도구 PNG로 Pen / Brush / Eraser / Picker 선택
- 우측 버튼 PNG로 Undo / Redo / Reset / Submit 실행
- `THICKNESS` 영역에는 `유동 초안/슬라이더 바.png`를 별도 손잡이로 얹고 위아래 드래그
- `COLOR` 영역에는 RGB 피커를 넣어 `ui 초안 최종.png`처럼 동작

---

## 2. 기준 이미지 해석

### 2.1 전체 기준

경로:

```text
Play Ref/UI 초안/전체/ui 고정.png
Play Ref/UI 초안/전체/ui final.png
Play Ref/UI 초안/전체/ui 초안 최종.png
```

역할:

- `ui 고정.png`
  - 가장 기본이 되는 고정 배경판
  - 나무 작업대, 테이프로 붙은 종이, 팔레트 틀, COLOR 박스, THICKNESS 판이 포함됨
  - 유동 요소가 없는 기본 상태
- `ui final.png`
  - 도구/팔레트/우측 버튼/슬라이더 손잡이가 배치된 상태
  - RGB 컬러피커는 닫혀 있음
- `ui 초안 최종.png`
  - 최종 목표 상태
  - `COLOR` 박스 안에 RGB 피커와 스포이드가 열린 모습

현재 확인된 이미지 크기:

```text
ui 고정.png:      1952 x 2180
ui final.png:     1187 x 1325
ui 초안 최종.png: 1185 x 1327
```

구현에서는 `ui 초안 최종.png`의 픽셀 배치를 기준으로 각 요소의 RectTransform 좌표를 산출한 뒤, Unity의 SceneA 우측 패널 기준 좌표로 변환한다.

SceneA UI 기준:

```text
Canvas 기준 해상도: 1280 x 720
우측 작업대 기준 영역: 640 x 720
RightPanel 중심 위치: (320, 0)
```

즉 `RightPanel` 내부 좌표는 대략 다음처럼 생각한다.

```text
RightPanel 왼쪽: -320
RightPanel 오른쪽: 320
RightPanel 위: 360
RightPanel 아래: -360
```

배치 방식:

1. `ui 초안 최종.png`를 기준 레퍼런스 오버레이로 둔다.
2. `ui 고정.png`를 `DeskBase`로 배치한다.
3. 좌측 도구, 우측 버튼, 슬라이더 손잡이, RGB 피커 요소를 각 초안 PNG로 얹는다.
4. 각 오브젝트의 위치는 기준 이미지와 최대한 픽셀 단위로 맞춘다.
5. 마지막에 `ui 초안 최종.png`와 SceneA 스크린샷을 비교해 어긋난 부분을 조정한다.

허용되는 차이:

- Unity 캔버스 기준 해상도 변환으로 생기는 1~2px 수준의 미세한 차이
- 실제 드로잉 입력을 위해 종이 안쪽에 투명 클릭 영역을 얹는 것

허용하지 않는 차이:

- 도구/버튼/팔레트/RGB 피커의 위치를 임의 변경
- PNG 대신 새로 그린 UI로 대체
- `ui 초안 최종.png`와 다른 레이아웃으로 재해석

---

## 3. 목표 Hierarchy

SceneA의 최종 UI 계층은 다음 구조를 목표로 한다.

```text
SceneCanvas
└─ RightPanel
   ├─ DeskBase
   ├─ DrawingPaper
   │  ├─ DrawingSurface
   │  └─ PaperInputGuard
   ├─ LeftTools
   │  ├─ PenButton
   │  ├─ BrushButton
   │  ├─ EraserButton
   │  └─ PickerButton
   ├─ PalettePanel
   │  ├─ PaletteSlot_0
   │  ├─ PaletteSlot_1
   │  ├─ PaletteSlot_2
   │  ├─ PaletteSlot_3
   │  ├─ PaletteSlot_4
   │  ├─ PaletteSlot_5
   │  ├─ PaletteSlot_6
   │  └─ PaletteSlot_7
   ├─ ThicknessControl
   │  ├─ SliderTrack
   │  ├─ SliderHandle
   │  ├─ ThicknessText
   │  └─ PreviewDot
   ├─ ColorPanel
   │  ├─ ColorPreview
   │  └─ RGBPickerPopup
   │     ├─ SaturationValueArea
   │     ├─ HueBar
   │     ├─ RValueText
   │     ├─ GValueText
   │     ├─ BValueText
   │     └─ EyedropperButton
   ├─ RightActions
   │  ├─ UndoButton
   │  ├─ RedoButton
   │  ├─ ResetButton
   │  └─ SubmitButton
   └─ SceneADrawingUIController
```

주의:

- `DeskBase`는 기본 배경 이미지다.
- 실제 클릭/드래그 가능한 요소는 별도 `Button`, `Image`, `EventTrigger` 또는 커스텀 핸들러로 구성한다.
- 버튼 이미지는 텍스트 UI 버튼이 아니라 PNG 이미지 자체를 클릭 대상으로 쓴다.

---

## 4. 에셋 배치 계획

현재 참조 이미지는 `Play Ref` 폴더에 있다. 구현 시에는 Unity가 안정적으로 import할 수 있도록 `Assets` 내부로 복사한다.

목표 경로:

```text
yorki/Assets/Sprites/UI/SceneA/Desk/ui_fixed.png
yorki/Assets/Sprites/UI/SceneA/Desk/ui_final_reference.png
yorki/Assets/Sprites/UI/SceneA/Desk/ui_final_rgb_reference.png

yorki/Assets/Sprites/UI/SceneA/Tools/Pen/pen_base.png
yorki/Assets/Sprites/UI/SceneA/Tools/Pen/pen_selecting.png
yorki/Assets/Sprites/UI/SceneA/Tools/Pen/pen_selected.png

yorki/Assets/Sprites/UI/SceneA/Tools/Brush/brush_base.png
yorki/Assets/Sprites/UI/SceneA/Tools/Brush/brush_selecting.png
yorki/Assets/Sprites/UI/SceneA/Tools/Brush/brush_selected.png

yorki/Assets/Sprites/UI/SceneA/Tools/Eraser/eraser_base.png
yorki/Assets/Sprites/UI/SceneA/Tools/Eraser/eraser_selecting.png
yorki/Assets/Sprites/UI/SceneA/Tools/Eraser/eraser_selected.png

yorki/Assets/Sprites/UI/SceneA/Tools/Picker/picker_base.png
yorki/Assets/Sprites/UI/SceneA/Tools/Picker/picker_selecting.png
yorki/Assets/Sprites/UI/SceneA/Tools/Picker/picker_selected.png

yorki/Assets/Sprites/UI/SceneA/Actions/undo.png
yorki/Assets/Sprites/UI/SceneA/Actions/unundo.png
yorki/Assets/Sprites/UI/SceneA/Actions/redo.png
yorki/Assets/Sprites/UI/SceneA/Actions/unredo.png
yorki/Assets/Sprites/UI/SceneA/Actions/reset.png
yorki/Assets/Sprites/UI/SceneA/Actions/submit.png

yorki/Assets/Sprites/UI/SceneA/Controls/slider_handle.png
```

Import 설정:

- Texture Type: `Sprite`
- Sprite Mode: `Single`
- Filter Mode: `Point`
- Compression: `None`
- Pixels Per Unit은 UI Image에서는 큰 의미가 적지만, 일관성을 위해 기본값 유지 또는 100 사용

---

## 5. DrawingCanvas 배치 계획

### 5.1 현재 상태

현재 `DrawingCanvas.cs`에는 기능이 존재한다.

- 마우스 입력
- 드래그 선 그리기
- Undo
- Clear
- 지우개
- 브러시 색상 변경
- 브러시 크기 변경
- `GetDrawTexture()`

하지만 현재 `SceneA.unity`에는 `DrawingCanvas`, `DrawingPanel`, `DrawingToolbar`가 배치되어 있지 않다.

### 5.2 배치 위치

`DrawingCanvas`는 `ui 고정.png`의 테이프로 붙은 종이 안쪽에 배치한다.

중요한 점:

- 종이 이미지는 `ui 고정.png`에 이미 그려져 있다.
- `DrawingCanvas`가 흰 배경으로 종이를 덮으면 안 된다.
- 따라서 실제 드로잉 레이어는 투명 배경 RawImage로 둔다.
- 제출/채점용 텍스처는 흰 배경과 드로잉 레이어를 합성해서 만든다.

구조:

```text
DrawingPaper
├─ PaperVisualFromDeskBase  // 실제로는 DeskBase 이미지 안에 포함됨
└─ DrawingSurface           // 투명 RawImage + DrawingCanvas
```

`DrawingSurface`는 종이의 테두리 안쪽만 클릭되게 한다. 테이프나 종이 밖 나무판에는 선이 그려지지 않아야 한다.

### 5.3 DrawingCanvas 수정 방향

`DrawingCanvas.cs`는 다음 기능을 추가/수정한다.

- 배경을 흰색으로 채우는 현재 방식과 투명 배경 방식을 선택 가능하게 한다.
- SceneA에서는 투명 배경 모드 사용.
- Pen / Brush / Eraser 모드를 분리한다.
- Brush는 Pen보다 약간 부드럽고 투명한 느낌을 준다.
- Eraser는 투명 픽셀로 지우거나, 제출 합성 기준에서는 종이 배경색으로 지우는 방식 중 하나를 선택한다.
- Redo 스택을 추가한다.
- 도구별 굵기 기억:
  - Pen thickness
  - Brush thickness
  - Eraser thickness
- Submit용 함수 추가:

```csharp
Texture2D GetFlattenedTextureForScoring()
```

이 함수는 투명 드로잉 레이어를 흰 배경 위에 합성해서 `ScoreCalculator`가 기존처럼 읽을 수 있게 한다.

---

## 6. THICKNESS 구현 계획

### 6.1 핵심 이해

`ui 고정.png`에는 THICKNESS 판과 검은 세로 홈이 이미 있다.

하지만 움직이는 슬라이더 손잡이는 없다.

움직이는 손잡이는 다음 파일을 사용한다.

```text
Play Ref/UI 초안/유동 초안/슬라이더 바.png
```

즉 구현은 다음처럼 한다.

```text
고정 배경:
  ui 고정.png 안의 THICKNESS 판 + 검은 세로 홈

유동 오브젝트:
  SliderHandle Image = 슬라이더 바.png
```

### 6.2 Hierarchy

```text
ThicknessControl
├─ SliderTrack
├─ SliderHandle
├─ ThicknessText
└─ PreviewDot
```

역할:

- `SliderTrack`
  - 검은 세로 홈 위에 얹는 투명 입력 영역
  - 사용자가 이 영역을 클릭하거나 드래그할 수 있음
- `SliderHandle`
  - `슬라이더 바.png`
  - 실제로 위아래 움직이는 손잡이
- `ThicknessText`
  - `8 px` 같은 현재 값 표시
- `PreviewDot`
  - 현재 굵기를 보여주는 검은 원

### 6.3 값 범위

범위:

```text
최소: 2 px
최대: 24 px
기본: 8 px
```

매핑:

```text
SliderTrack 아래쪽 = 2 px
SliderTrack 위쪽   = 24 px
```

공식:

```text
t = (handleY - minY) / (maxY - minY)
thickness = round(lerp(2, 24, t))
```

드래그 중에는 항상 `handleY`를 `minY ~ maxY` 안으로 clamp한다.

### 6.4 입력 방식

지원할 입력:

- 손잡이를 마우스로 잡고 위아래 드래그
- 검은 세로 홈을 클릭하면 그 위치로 손잡이가 이동
- 드래그 중 숫자와 미리보기 점이 실시간 갱신

### 6.5 도구별 굵기 기억

도구를 바꿀 때 현재 굵기를 저장한다.

예:

```text
Pen = 6 px
Brush = 12 px
Eraser = 18 px
```

Pen에서 Brush로 바꾸면 슬라이더 손잡이도 Brush의 저장된 위치로 이동한다.

Picker는 그림을 찍는 도구이므로 굵기와 무관하다. Picker 선택 중에는 THICKNESS 조작을 비활성화하거나 마지막 드로잉 도구 값을 유지한다.

---

## 7. 좌측 도구 구현 계획

경로:

```text
Play Ref/UI 초안/좌측 툴 초안/pen
Play Ref/UI 초안/좌측 툴 초안/brush
Play Ref/UI 초안/좌측 툴 초안/eraser
Play Ref/UI 초안/좌측 툴 초안/picker
```

각 도구는 3상태 이미지를 가진다.

```text
base       = 기본 상태
selecting  = 호버/누르는 중
selected   = 선택됨
```

동작:

- Pen 클릭
  - 도구를 Pen으로 변경
  - `DrawingCanvas`를 선명한 불투명 선 모드로 설정
  - Pen 버튼은 `pen_selected.png`
- Brush 클릭
  - 도구를 Brush로 변경
  - `DrawingCanvas`를 약간 부드럽고 투명한 선 모드로 설정
  - Brush 버튼은 `brush_selected.png`
- Eraser 클릭
  - 도구를 Eraser로 변경
  - 현재 굵기는 Eraser 저장값 사용
  - Eraser 버튼은 `eraser_selected.png`
- Picker 클릭
  - 스포이드 모드 진입
  - 다음 종이 클릭에서 색을 찍고 현재 팔레트 슬롯 색으로 적용

선택 시각 효과:

- 기존 계획대로 선택된 도구만 살짝 위로 뜨는 정도
- 큰 글로우나 과한 이펙트는 사용하지 않음
- PNG 자체가 selected 상태를 갖고 있으므로 Image sprite 교체가 우선

---

## 8. 팔레트 8칸 구현 계획

기본 색상은 이전 UI 계획과 사용자의 확정 내용을 따른다.

```text
0: 밝은 살색
1: 어두운 살색
2: 중간 갈색
3: 진한 갈색
4: 회색
5: 붉은 분홍
6: 흰색/종이색
7: 남색 또는 예비 슬롯
```

주의:

- 예전 `UI 계획.md`에는 남색을 기본 팔레트에서 빼자는 내용이 있었다.
- 하지만 현재 `ui final.png`와 `ui 초안 최종.png`에는 8번째 슬롯에 남색이 들어가 있다.
- 구현 전 최종 색 구성은 한 번 더 확정하는 것이 좋다.

기능:

- 좌클릭: 해당 슬롯 색 선택
- 우클릭: 해당 슬롯을 편집 대상으로 설정하고 RGB 피커 열기
- 길게 누르기: 터치 대응용, 우클릭과 동일하게 RGB 피커 열기

선택 표시:

- 선택된 슬롯은 테두리 강조 또는 살짝 위로 올라간 느낌
- 이미지 기반 팔레트 틀과 충돌하지 않게 과하지 않게 처리

---

## 9. COLOR / RGB 피커 구현 계획

### 9.1 목표

`ui 초안 최종.png`의 하단 `COLOR` 박스처럼 RGB 기능을 넣는다.

구성:

```text
ColorPanel
├─ SaturationValueArea
├─ HueBar
├─ RValueText
├─ GValueText
├─ BValueText
├─ EyedropperButton
└─ ColorPreview
```

### 9.2 동작

RGB 피커는 기본적으로 현재 선택 색을 보여준다.

팔레트 슬롯 우클릭/길게 누르기:

1. 해당 슬롯을 편집 대상으로 지정
2. RGB 피커 열기
3. 피커 값은 슬롯 색으로 초기화

RGB 값 변경:

1. `R`, `G`, `B` 값 갱신
2. `ColorPreview` 갱신
3. 현재 팔레트 슬롯 색 갱신
4. 현재 선택 색도 갱신
5. `DrawingCanvas.SetBrushColor()` 호출

### 9.3 피커 UI 형태

`ui 초안 최종.png` 기준:

- 왼쪽: 큰 색상 사각형
- 가운데: 세로 무지개 Hue 바
- 오른쪽: R/G/B 숫자 박스
- 더 오른쪽: 스포이드 버튼

초기 구현은 정확한 HSV 컬러피커를 완벽히 구현하기보다 다음 순서로 진행한다.

1. RGB 숫자 변경 기능부터 구현
2. HueBar 클릭/드래그로 기본 hue 변경
3. SaturationValueArea 클릭/드래그로 채도/명도 변경
4. 스포이드 연결

이 순서가 안전하다. 시각 배치는 먼저 잡고, 세부 색상 알고리즘은 단계적으로 넣는다.

### 9.4 스포이드

스포이드 버튼을 누르면 Picker 모드와 유사한 색 추출 모드로 들어간다.

동작:

1. 스포이드 버튼 클릭
2. `isPickingColor = true`
3. 종이 위 클릭
4. `DrawingCanvas`의 해당 픽셀 색 읽기
5. 현재 편집 중인 팔레트 슬롯 색 변경
6. RGB 숫자 갱신
7. 스포이드 모드 종료

주의:

- 종이 바깥 클릭은 무시
- 투명 픽셀을 찍으면 종이 배경색 또는 흰색으로 처리

---

## 10. 우측 버튼 구현 계획

경로:

```text
Play Ref/UI 초안/우측 틀 초안/undo.png
Play Ref/UI 초안/우측 틀 초안/unundo.png
Play Ref/UI 초안/우측 틀 초안/redo.png
Play Ref/UI 초안/우측 틀 초안/unredo.png
Play Ref/UI 초안/우측 틀 초안/reset.png
Play Ref/UI 초안/우측 틀 초안/submit.png
```

동작:

- Undo
  - `DrawingCanvas.Undo()` 호출
  - Undo 스택이 없으면 `unundo.png` 표시
- Redo
  - `DrawingCanvas.Redo()` 호출
  - Redo 스택이 없으면 `unredo.png` 표시
- Reset
  - 확인 팝업은 이번 단계에서는 생략 가능
  - 클릭 즉시 `DrawingCanvas.ClearCanvas()`
- Submit
  - `GameManager.OnSubmit()`
  - 또는 SceneA 전용 제출 컨트롤러를 거쳐 `GetFlattenedTextureForScoring()` 사용

임시 입력:

- 현재 `SceneATestTransitionInput`의 G/B 테스트 입력은 UI Submit 연결 완료 후 제거 후보
- 단, 디버그용으로 잠시 남길 수는 있음

---

## 11. 신규/수정 스크립트 계획

### 11.1 신규: SceneADrawingUIController.cs

책임:

- 전체 드로잉 UI 상태 관리
- 도구 선택
- 팔레트 선택/편집
- RGB 피커 열기/닫기
- Thickness 슬라이더 값 적용
- Undo/Redo/Reset/Submit 버튼 연결
- 버튼 sprite 상태 갱신

주요 필드:

```csharp
public DrawingCanvas drawingCanvas;
public Image penButton;
public Image brushButton;
public Image eraserButton;
public Image pickerButton;
public Image[] paletteSlots;
public RectTransform thicknessTrack;
public RectTransform thicknessHandle;
public Text thicknessText;
public RectTransform previewDot;
public GameObject rgbPickerPopup;
```

### 11.2 신규: ThicknessSliderHandle.cs

책임:

- 세로 슬라이더 드래그 입력 처리
- y 위치를 2~24px 값으로 변환
- 컨트롤러에 값 전달

구현 인터페이스:

```csharp
IPointerDownHandler
IDragHandler
```

### 11.3 신규: PaletteSlotButton.cs

책임:

- 좌클릭/우클릭/길게 누르기 구분
- 좌클릭은 색 선택
- 우클릭/길게 누르기는 RGB 피커 열기

구현 인터페이스:

```csharp
IPointerClickHandler
IPointerDownHandler
IPointerUpHandler
```

### 11.4 수정: DrawingCanvas.cs

추가할 기능:

- Pen / Brush / Eraser 도구 타입
- 투명 배경 모드
- Redo
- 도구별 굵기 기억과 연동 가능한 setter
- 픽셀 색 추출 함수
- 제출용 flatten 함수

필요 함수 예:

```csharp
public void SetTool(DrawingTool tool);
public void SetBrushColor(Color color);
public void SetBrushSize(int size);
public bool CanUndo { get; }
public bool CanRedo { get; }
public void Undo();
public void Redo();
public Color GetColorAtScreenPosition(Vector2 screenPosition, Camera eventCamera);
public Texture2D GetFlattenedTextureForScoring();
```

### 11.5 수정: GameManager.cs

현재:

```csharp
Texture2D playerTex = drawingCanvas.GetDrawTexture();
```

수정 후보:

```csharp
Texture2D playerTex = drawingCanvas.GetFlattenedTextureForScoring();
```

이유:

- SceneA에서는 DrawingCanvas 배경이 투명할 수 있음
- ScoreCalculator는 흰 배경과의 차이로 잉크를 판단함
- 따라서 제출 시 흰 배경 합성본을 넘기는 것이 안전함

### 11.6 수정: SceneABuilder.cs

현재:

- `RightPanel`만 생성

수정:

- `DeskBase` 생성
- `DrawingSurface` 생성
- 좌측 도구 생성
- 팔레트 슬롯 생성
- Thickness 입력 오브젝트 생성
- ColorPanel/RGBPickerPopup 생성
- 우측 버튼 생성
- `SceneADrawingUIController` 자동 연결

주의:

- `Yorki/Build Scene A`를 실행하면 `SceneA.unity`를 덮어쓴다.
- 빌더 실행 전 사용자 확인을 받아야 한다.

---

## 12. 구현 순서

### Step 1. 에셋 복사 및 Import 설정

- `Play Ref/UI 초안`의 필요한 PNG를 `Assets/Sprites/UI/SceneA`로 복사
- `.meta` 생성 확인
- TextureImporter 설정 적용

검증:

- Unity refresh/compile
- Sprite import 상태 확인

### Step 2. SceneABuilder에 정적 작업대 배치

- `DeskBase`를 `RightPanel` 전체에 배치
- `ui 초안 최종.png`를 임시 레퍼런스 오버레이로 켜고 끌 수 있게 하거나, 씬 캡처와 이미지 비교가 가능하게 기준 좌표를 기록
- 우선 버튼 기능 없이 이미지 배치만 확인

검증:

- SceneA 열었을 때 `ui 고정.png`가 오른쪽 작업대에 맞게 보이는지
- `ui 초안 최종.png` 기준과 종이/THICKNESS/PALETTE/COLOR 위치가 같은지
- 좌측 손님/영속 배경과 충돌하지 않는지

### Step 3. DrawingCanvas를 종이 위에 배치

- `DrawingSurface` 생성
- `RawImage + DrawingCanvas` 부착
- 종이 안쪽에만 입력되도록 RectTransform 조정

검증:

- Play 모드에서 종이 영역에 선이 그려지는지
- 종이 밖 클릭은 무시되는지
- TalkScene에서 SceneA로 넘어온 뒤에도 작동하는지

### Step 4. 좌측 도구 연결

- Pen / Brush / Eraser / Picker 버튼 생성
- PNG 상태 전환 연결
- DrawingCanvas 도구 모드 연결

검증:

- 도구 선택 시 selected 이미지가 바뀌는지
- Pen/Brush/Eraser가 실제로 다른 동작을 하는지

### Step 5. THICKNESS 세로 슬라이더 구현

- `SliderTrack` 투명 입력 영역 생성
- `SliderHandle`에 `슬라이더 바.png` 적용
- 마우스 드래그로 위아래 이동
- `2~24px` 값 매핑
- `ThicknessText`, `PreviewDot` 갱신
- 도구별 굵기 기억 연결

검증:

- 손잡이를 끌면 위아래로만 움직이는지
- 아래 2px, 위 24px, 기본 8px인지
- 값 변경이 DrawingCanvas 선 굵기에 바로 반영되는지

### Step 6. 팔레트 8칸 연결

- 8개 슬롯 생성
- 좌클릭 색 선택
- 선택 상태 표시
- 기본 색 적용

검증:

- 각 슬롯 클릭 시 브러시 색이 바뀌는지
- 선택 표시가 어색하지 않은지

### Step 7. RGB 피커 구현

- COLOR 박스 안에 RGB 피커 배치
- 우클릭/길게 누르기로 열기
- RGB 값 변경
- 현재 슬롯 색 변경
- 스포이드 연결

검증:

- `ui 초안 최종.png`처럼 COLOR 박스 내부에 맞게 보이는지
- RGB 값이 슬롯/브러시 색과 동기화되는지
- 스포이드로 종이 색을 찍을 수 있는지

### Step 8. 우측 버튼 연결

- Undo / Redo / Reset / Submit 연결
- Undo/Redo 가능 여부에 따라 비활성 이미지 적용

검증:

- Undo/Redo가 스택 상태에 맞게 작동하는지
- Reset이 캔버스를 초기화하는지
- Submit이 `GameManager.OnSubmit()`로 이어지는지

### Step 9. Submit 결과 흐름 연결

- `GetFlattenedTextureForScoring()`로 제출 텍스처 전달
- 점수 계산
- Good/Bad 2분기
- TalkScene ResultGood/ResultBad 복귀

검증:

- 제출 후 TalkScene으로 돌아가는지
- 결과 대사가 정상 출력되는지
- 기존 G/B 임시 입력과 충돌하지 않는지

### Step 10. 문서/임시 코드 정리

- `SCENE_A_PLAN.md` 또는 `PROGRESS.md`에 구현 완료 상태 반영
- `PATCH_NOTES.js` 최신 항목 추가
- `SceneATestTransitionInput` 제거 여부 결정

---

## 13. 우선순위

가장 먼저 구현해야 하는 순서:

1. 작업대 이미지 배치
2. 종이 영역 위 DrawingCanvas 배치
3. Submit 연결
4. Pen / Brush / Eraser
5. Thickness 슬라이더
6. Undo / Redo / Reset
7. Palette
8. RGB 피커
9. Picker / 스포이드

이유:

- 먼저 실제로 그리고 제출할 수 있어야 한다.
- 그 다음에 조작감을 보강한다.
- RGB 피커는 시각/입력 요소가 많으므로 마지막에 붙이는 편이 안전하다.

---

## 14. 위험 요소와 대응

### 14.1 배경 이미지와 클릭 영역 불일치

문제:

- `ui 고정.png`의 원본 비율과 `RightPanel` 비율이 완전히 같지 않을 수 있다.
- `ui 고정.png`, `ui final.png`, `ui 초안 최종.png`의 원본 픽셀 크기도 서로 다르다.

대응:

- `ui 초안 최종.png`를 최종 배치 기준으로 삼고, 실제 Unity 배치는 해당 기준에 맞게 RectTransform 좌표를 조정
- `ui 고정.png`는 배경판으로 쓰되, 유동 PNG들의 위치는 `ui 초안 최종.png`와 같은 시각 결과가 나오도록 맞춤
- 필요하면 기준 좌표를 별도 상수로 관리

### 14.2 DrawingCanvas가 종이 질감을 덮는 문제

문제:

- 현재 DrawingCanvas는 흰 배경을 채운다.

대응:

- SceneA에서는 투명 배경 모드 사용
- 제출 시 흰 배경 합성본을 사용

### 14.3 ScoreCalculator와 투명 캔버스 충돌

문제:

- ScoreCalculator는 흰색과 다른 픽셀을 잉크로 판단한다.

대응:

- `GetFlattenedTextureForScoring()`로 흰 배경 합성 텍스처를 넘김

### 14.4 빌더 실행 시 씬 덮어쓰기

문제:

- `Yorki/Build Scene A`는 `SceneA.unity`를 재생성한다.

대응:

- 구현은 빌더를 기준으로 하되, 실행 전 사용자 확인
- 수동 씬 수정만 하는 방식은 피하고, 재현 가능한 빌더 코드에 반영

### 14.5 RGB 피커 구현 범위 과대

문제:

- HSV/SV 영역, Hue bar, RGB 숫자, 스포이드를 한 번에 완성하면 리스크가 큼

대응:

- 1차: 슬롯 색 선택 + RGB 숫자 표시/변경
- 2차: Hue/SV 드래그
- 3차: 스포이드

---

## 15. 완료 기준

다음 조건을 만족하면 Scene A 드로잉 UI 1차 구현 완료로 본다.

- SceneA 오른쪽 작업대가 `ui 고정.png` 기반으로 보임
- 종이 영역 위에서 실제 그림을 그릴 수 있음
- Pen / Brush / Eraser 선택 가능
- Thickness 손잡이를 마우스로 위아래 드래그할 수 있음
- 굵기 값과 미리보기 점이 갱신됨
- 팔레트 8칸에서 색 선택 가능
- RGB 피커로 슬롯 색 수정 가능
- Undo / Redo / Reset 가능
- Submit 후 TalkScene 결과 반응으로 돌아감
- Unity compile 오류 없음
- SceneA/TalkScene validate 통과

---

## 16. 당장 구현 전 확인할 것

구현 직전 사용자 확인이 필요한 항목:

1. 팔레트 8번째 색을 남색으로 둘지, 기존 계획대로 남색을 빼고 다른 기본색으로 둘지
2. RGB 피커는 처음부터 완전 구현할지, 1차에서는 숫자 변경 중심으로 갈지
3. `SceneATestTransitionInput`을 Submit 연결 후 바로 제거할지, 디버그용으로 잠시 남길지
4. `Yorki/Build Scene A` 실행으로 씬을 덮어써도 되는 시점
