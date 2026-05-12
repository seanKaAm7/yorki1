/*
---------
[2026-05-12] (v32)
 * [DrawingCanvas] 도구별 굵기 범위 분리 — penMin/penDefault/penMax, brushMin/brushDefault/brushMax, eraserMin/eraserDefault/eraserMax 필드. 도구 전환 시 default 굵기 자동 적용. GetDefaultThicknessForTool / GetThicknessRangeForTool API 추가
 * [SceneADrawingUIController] 도구 버튼 3상태 sprite 활성화 — PointerDown → selecting, PointerClick → selected, 나머지 → base
 * [SceneADrawingUIController] Undo/Redo 비활성 sprite 활성화 — 스택 비어있을 시 unundo/unredo 자동 전환
 * [SceneADrawingUIController] 팔레트 8칸 연동 — 슬롯 클릭 시 브러시 색 변경, 스포이드 색 선택 슬롯에 반영
 * [SceneADrawingUIController] 슬라이더 도구별 중앙 초기화 — 도구 전환 시 t=0.5 강제 리셋, 단위는 도구별 상이
 * [SceneABuilder] 사용자 수동 조정값 하드코딩 — 이후 빌더 재실행 시 레이아웃 보존
 * [SceneABuilder] ThicknessTrack 위치 수정 — y 189.85 → 79.85
 * [SceneABuilder] PalettePanel + PaletteSlot_0~7 생성 — 기본 색상 8종 배치
 * [SceneA.unity] SliderHandle anchoredPosition 수정 — (16.95, -88.07) → (0, 0) 복구
---------
*/
/*
---------
[2026-05-11] (v31)
 * SCENE_A_DRAWING_UI_IMPLEMENTATION_PLAN.md 1.5차 작업 추가
 * 1차 구현 후 실사용 피드백 — 종이 위에 선이 그려지지만 드로잉 영역이 종이보다 조금 좁음
 * 5.2 배치 위치 — 종이 전체를 그대로 드로잉 영역으로 사용, 테이프와 약간 겹쳐도 허용, 종이 바깥 나무판은 제외하는 새 영역 규칙 명시
 * 14.5 위험 요소 추가 — 종이와 드로잉 영역 불일치 및 대응 방향(`DrawingSurface` RectTransform 종이 픽셀 경계와 동일하게 확장) 기록
 * 구현 상태 — 1.5차 항목 신설(영역 확장 작업), 2차 도구 연결 작업과 분리해 진행
---------
[2026-05-11] (v30)
 * Scene A 드로잉 UI 1차 구현
 * Play Ref/UI 초안 PNG들을 `Assets/Sprites/UI/SceneA` 하위 ASCII 경로로 복사하고 Unity .meta 생성 확인
 * SceneABuilder.cs — `ui 고정.png` 기반 DeskBase, 비활성 ReferenceOverlay_FinalRGB, DrawingPaper/DrawingSurface 생성
 * DrawingCanvas.cs — SceneA 종이 질감을 덮지 않도록 투명 배경 모드 추가, 채점용 흰 배경 합성 텍스처 함수 추가
 * SceneA.unity — Yorki/Build Scene A 실행으로 DeskBase와 DrawingCanvas가 붙은 DrawingSurface 반영
 * 검증 — dotnet build 성공(경고 0, 오류 0), Unity refresh/compile 완료, SceneA validate 통과(missing script 0, broken prefab 0)
---------
[2026-05-11] (v29)
 * SCENE_A_DRAWING_UI_IMPLEMENTATION_PLAN.md 팔레트 구현 설명 보강
 * 팔레트는 고정 색상 버튼이 아니라 초기 RGB값을 가진 수정 가능 슬롯 8개로 관리한다고 명시
 * 슬롯 클릭 시 현재 슬롯 색으로 그리기 색을 바꾸고, COLOR 박스 RGB 조작 시 선택/편집 슬롯의 RGB값과 표시 색, DrawingCanvas 색을 함께 갱신하는 흐름 정리
---------
[2026-05-11] (v28)
 * SCENE_A_DRAWING_UI_IMPLEMENTATION_PLAN.md 보강
 * `ui 초안 최종.png`는 단순 참고가 아니라 최종 배치 기준이며, Play Ref/UI 초안의 PNG들을 그대로 활용해야 한다는 원칙 명시
 * 구현 방식 추가 — 기준 이미지를 오버레이/스크린샷 비교 기준으로 삼아 좌측 도구, 우측 버튼, 팔레트, THICKNESS, COLOR/RGB 피커 위치를 동일하게 맞춤
---------
[2026-05-11] (v27)
 * SCENE_A_DRAWING_UI_IMPLEMENTATION_PLAN.md 신규 작성
 * Play Ref/UI 초안 기준으로 SceneA 드로잉 UI 구현 계획 상세화
 * 기준 이미지 역할 정리 — ui 고정.png는 기본 작업대, ui final.png는 RGB 닫힌 완성 참고, ui 초안 최종.png는 RGB 열린 목표 상태
 * 핵심 구현 방향 — 기존 DrawingCanvas를 테이프로 붙은 종이 영역 위 투명 드로잉 레이어로 배치하고 제출 시 흰 배경 합성 텍스처 사용
 * THICKNESS 구현 방향 — ui 고정의 세로 홈 위에 유동 초안/슬라이더 바.png를 SliderHandle로 얹어 2px~24px 범위 드래그 처리
---------
[2026-05-11] (v26)
 * CustomerDisplay.cs — 말하기 입 애니메이션을 독립 루프 방식에서 타이핑 글자 기반 프레임 전환 방식으로 변경
 * TalkSceneController.cs — 글자가 출력될 때만 입 프레임을 넘기고, 공백/마침표/쉼표/물음표/느낌표/말줄임표 등에서는 입을 닫도록 처리
 * 의도 — 0.1초 간격으로 계속 입을 벌리는 인형 같은 느낌을 줄이고, 대사 출력 리듬에 맞춰 말하는 느낌으로 조정
 * 검증 — dotnet build 성공(경고 0, 오류 0), Unity refresh/compile 완료, 최신 Editor.log 기준 error CS / warning CS / NullReference 없음, TalkScene validate 통과
---------
[2026-04-30] (v25)
 * SceneA G/B 임시 결과 전환 입력 미동작 수정
 * 원인: SceneATestTransitionInput이 SceneTransition 오브젝트에 같이 붙어 있어 TalkScene에서 넘어온 영속 SceneTransition과 중복 처리될 때 오브젝트째 Destroy되어 입력도 사라짐
 * SceneABuilder.cs — SceneATestTransitionInput을 SceneTransition과 분리된 별도 루트 오브젝트로 생성하도록 수정
 * SceneA.unity 재생성 확인 — 루트에 SceneTransition과 SceneATestTransitionInput이 각각 별도 오브젝트로 존재
 * 검증 — dotnet build 성공(경고 0, 오류 0), SceneA validate 통과(missing script 0, broken prefab 0), 활성 씬 TalkScene으로 복귀
---------
[2026-04-30] (v24)
 * SceneATestTransitionInput.cs 신규 — UI 구현 전 SceneA → TalkScene 결과 복귀 테스트용 임시 입력 추가
 * 임시 키 입력 — SceneA에서 G 키는 ResultGood, B 키는 ResultBad로 SceneTransition.SceneAToTalkScene 호출
 * SceneABuilder.cs — SceneTransition 오브젝트에 SceneATestTransitionInput 자동 부착
 * SceneA.unity 재생성 — SceneTransition에 SceneATestTransitionInput 컴포넌트 반영 확인
 * 검증 — dotnet build 성공(경고 0, 오류 0), SceneA/TalkScene validate 통과(missing script 0, broken prefab 0), 활성 씬 TalkScene으로 복귀
---------
[2026-04-30] (v23)
 * Unity 빌더 실행 — Yorki/Build Talk Scene, Yorki/Build Scene A 실행으로 TalkScene.unity 생성 및 SceneA.unity 재생성
 * TalkScene.unity — PersistentBootstrap / PersistentCanvas / CustomerStage / DialogueBox / TalkSceneController / SceneTransition 계층 생성 확인
 * SceneA.unity — Main Camera / EventSystem / SceneCanvas(RightPanel) / SceneTransition 계층 생성 확인
 * Build Settings 수정 — TalkScene.unity buildIndex 0, SceneA.unity buildIndex 1로 등록 후 File/Save Project로 EditorBuildSettings.asset 저장
 * Unity 씬 검증 — TalkScene, SceneA 모두 missing script 0 / broken prefab 0
 * Play smoke test — TalkScene에서 짧게 Play 진입 후 정지, 최신 로그 기준 NullReference/MissingReference/씬 로드 실패/CS 오류 없음
 * 주의: Play Ref 폴더는 상위 PNG 삭제 + 분류 폴더 추가 상태가 감지됨. Scene A 작업 범위 밖이라 되돌리지 않음
---------
[2026-04-30] (v22)
 * GameManager.cs — SUBMIT 후 점수 계산 결과를 Good / Bad 2분기로 변환하여 TalkScene 결과 Phase로 복귀하도록 연결
 * 결과 기준 — Satisfied / VerySatisfied는 ResultGood, Neutral 이하 등급은 ResultBad
 * 기존 ReactionUI 직접 출력 대신 SceneTransition.SceneAToTalkScene(nextPhase) 호출
 * Unity MCP force refresh/compile 완료 — Assembly-CSharp.dll 갱신 확인, 최신 Editor.log tail 기준 error CS / warning CS 없음
 * dotnet build Assembly-CSharp.csproj --no-restore 성공 — 경고 0, 오류 0
 * SCENE_A_PLAN.md / SCENE_A_CUT_PLAN.md / PROGRESS.md — Step 7 완료 상태 반영
---------
[2026-04-30] (v21)
 * TalkSceneController.cs 신규 — PreDraw / ResultGood / ResultBad Phase별 대사 출력, 타이핑, 포즈 그룹 pause, ▼ 깜빡임, 12번째 대사 후 SceneA 자동 전환
 * SceneTransition.cs 신규 — TalkScene ↔ SceneA 전환 싱글턴, CustomerStage 0.7초 EaseInOut 슬라이드, DialogueBox / RightPanel 페이드, 0.5초 시점 LoadScene
 * CustomerDisplay.cs — neutral 말하기 3프레임 ping-pong(Talk2 → Neutral_Talk → Talk1 → Neutral_Talk) 0.1초, gesture 2프레임 0.12초 토글 부활
 * TalkSceneBuilder.cs / SceneABuilder.cs — CanvasGroup, SceneTransition, TalkSceneController 자동 연결 코드 추가
 * GameManager.cs — TalkScene 결과 Phase 전달용 currentTalkPhase 추가
 * SCENE_A_PLAN.md / SCENE_A_CUT_PLAN.md / PROGRESS.md — Step 4~6 완료 상태 반영
 * 검증: 새 스크립트 포함 임시 csproj로 dotnet build 성공(경고 0, 오류 0). Unity batch refresh는 라이선스 초기화 실패로 중단되어 Editor 컴파일은 미확인
---------
[2026-04-30] (v20)
 * Scene A 컷 시스템 Step 1~3 이어받기 — PersistentBootstrap.cs 신규, TalkSceneBuilder.cs 신규, SceneABuilder.cs 수정 상태 확인
 * TalkSceneBuilder.cs — TalkScene.unity 자동 빌더 추가: PersistentCanvas + CustomerStage(배경/손님) + 중앙 DialogueBox(760x200) 생성
 * PersistentBootstrap.cs — 손님/배경 영속 객체 참조 보유 및 DontDestroyOnLoad 처리
 * SceneABuilder.cs — SceneA를 작업대 전용 씬으로 축소, 손님/배경/대사창 생성 제거, RightPanel만 생성
 * Unity refresh/compile 확인 — 최신 Editor.log 기준 error CS / warning CS 없음, 신규 스크립트 .meta 생성 확인
 * SCENE_A_PLAN.md / SCENE_A_CUT_PLAN.md — Step 1~3 완료 상태 반영
 * 빌더 메뉴 실행은 하지 않음 — SceneA.unity 덮어쓰기 방지
---------
[2026-04-30] (v19)
 * SCENE_A_CUT_PLAN.md 신규 — Scene A 두 모드 분리(대화 풀화면 / 캔버스 분할) + 역재식 컷 토글 메커니즘
 * 흐름: 대화 모드(인사+잡담 13줄) → 자동 전환 → 캔버스 모드(침묵, 그리기) → SUBMIT → 대화 모드(결과 반응 4줄)
 * 전환 0.7초 EaseInOutQuad — 손님 좌측 슬라이드+축소(420→320), 배경 좌측 슬라이드, 작업대 페이드인/아웃
 * 풀화면 대사 모드 수치: 손님 (0,-40)/420×420, 배경 (0,0)/1280×720, 대사창 (0,-260)/760×200
 * 컷 토글: neutral 감정 3프레임 ping-pong (Talk2→Neutral_Talk→Talk1→Neutral_Talk) 0.1초, idle은 입 다문 컷 고정
 * Customer Neutral 컷 4종 입 단계 매핑(닫힘/살짝/반/활짝) 정리
 * 4개 PNG idle 입 다문 버전으로 복원 커밋 (자동 커밋이 입 벌린 상태로 덮어쓴 문제 수정)
---------
[2026-04-29] (v18)
 * UI 계획.md 작성 — Scene A 드로잉 UI 디제틱 방향 확정 (초안.png 기반)
 * 도구 3종(펜/붓/지우개), 굵기 슬라이더 2~24px(도구별 따로 기억), 팔레트 8칸(검정/흰색/밝은살색/어두운살색/진한갈색/중간갈색/회색/붉은분홍)
 * RGB 컬러피커(나무 톤 팝업, HSL 제외, 스포이드 내장), Undo/Redo 좌상단, 우상단 PM 시계, 활성 표시 살짝 위로 뜸
 * 추가 도구·색상 해금, 서명/제목, 굵기 슬라이더 디자인은 후순위
---------
[2026-04-29] (v17)
 * 미사용 잉여 캐릭터 스프라이트 5종 삭제 (Customer_Gesture_Idle1/2, Customer_Surprised1/2/3)
---------
[2026-04-29] (v16)
 * 캐릭터 스프라이트 크롭 rect를 0,0,320,320으로 통일하여 컷 전환 시 위치/크기 튀는 문제 해결
 * CustomerDisplay.cs에 talk1/talk2 슬롯 추가 및 neutral 대사 시 입 모양 토글 기능 추가 (현재는 자연스러움을 위해 토글 임시 비활성화)
 * Customer_Gesture_Idle.png 캔버스 크기 320x320으로 통일
---------
[2026-04-29] (v15)
 * Customer_Base.png 삭제 (미사용 파일 정리, 코드 영향 없음)
---------
[2026-04-29] (v14)
 * 미커밋 변경 일주일치를 의미 단위 3개 커밋으로 분리(yorki_moving 정리 / Scene A 구현 / 문서·인프라)
 * GitHub origin/main으로 수동 push 완료 (4개 커밋 업로드)
 * auto_commit.sh에 git push origin main 추가 — 매일 17:00 commit 후 자동 push까지
 * push 실패 시 스크립트는 0 종료 + 로그 남김(다음 실행 시 재시도)
---------
[2026-04-29] (v13)
 * scripts/auto_commit.sh 신규 — 변경 있을 때만 자동 커밋, --dry-run 지원
 * crontab 등록 — 매일 17:00 KST 자동 백업 커밋, 로그는 ~/.yorki_auto_commit.log
 * 의미 단위 커밋은 사용자 수동 진행 (자동 커밋은 보험용)
---------
[2026-04-29] (v12)
 * 초기플랜 .md 삭제 — GAME_PLAN.md에 모든 내용이 더 상세하게 흡수되어 있어 단일 기획서로 정리
 * CLAUDE.md - "7. 세션 시작 시 자동 로드" 섹션 신설, @GAME_PLAN.md / @SCENE_A_PLAN.md / @SCENE_A_TECH.md / @PROGRESS.md 임포트 등록
 * 새 채팅 시작 시 4개 핵심 문서가 자동 컨텍스트 로드됨 (매번 Read 안 해도 됨)
---------
[2026-04-28] (v11)
 * Customer_Happy_Talk.png, Customer_Thinking_Idle.png 영구 제거 (Unity manage_asset, .meta 포함)
 * SceneABuilder.cs - happyTalkPath/thinkingIdlePath 상수 + paths[] + cd 할당 제거
 * CustomerDisplay.cs - happyTalk/thinkingIdle 필드 제거, GetIdleSprite의 thinking case 제거(neutralIdle 폴백), GetTalkSprite의 happy는 happyIdle 반환
 * SCENE_A_TECH.md, SCENE_A_PLAN.md, PROGRESS.md - 8종 → 6종 반영
 * 컴파일 에러/경고 0건
---------
[2026-04-27] (v10)
 * SceneABuilder.cs 수정 - 배경 컨테이너 (1330,720) + preserveAspect=true, 비율 역산으로 레터박스/찌그러짐 제거
 * SceneABuilder.cs 수정 - 캐릭터 컨테이너 (300,480) → (320,320), 위치 (-320,10) → (-320,-50), 공중부양 수정
 * SceneADialogue.cs 수정 - 포즈 그룹 시스템 적용 (resting/gesture/thinking/reaction), 그룹 전환 시 0.35초 pause
 * SceneADialogue.cs 수정 - ▼ ContinueArrow 깜빡임 코루틴 연동, 타이핑 완료 후 표시/진행 시 숨김
 * DialogueBox.png - 9-slice 임포트 설정 (Single 모드, Bilinear, border L80/R80/T100/B30)
 * DialogueBox.png - Sprite Editor에서 rect 상단 조정, 삼각형 뿔 포함하도록 수동 수정
 * DialogueBox.png.meta - Multiple 모드 잔존 데이터 완전 초기화로 "rect lies outside of texture" 경고 해결
---------
[2026-04-22] (v9)
 * SceneABuilder.cs 수정 - 배경 720px/preserveAspect=true, 캐릭터 위치 (-370,-60), neutral_idle = Customer_Neutral_Idle.png
 * SceneABuilder.cs 수정 - CustomerDisplay 컴포넌트 자동 추가, 스프라이트 슬롯 8종 자동 할당
 * SceneADialogue.cs 신규 - SceneALine 구조체, 대사 20줄(등장2/일반10/잘그렸을때4/못그렸을때4), 타이핑 출력, 표정/Shake 연동
 * SceneA.unity 생성 완료 - "Yorki/Build Scene A" 메뉴
---------
[2026-04-22] (v8)
 * NPCWander.cs 추가 - Idle/Walking 상태 반복, wanderRadius 내 랜덤 목적지 이동, SpriteRenderer 없으면 흰 픽셀 자동 생성
 * SceneB_Test.unity 수정 - NPC_01~03 배치, Yorki scale (3,3,1) → (1.5,1.5,1) 조정
 * 기획 방향 변경 - 캐릭터 직접 이동 방식 제거, Coffee Talk 방식(고정 배경 + 손님 찾아오는 구조)으로 전환
 * GAME_PLAN.md 업데이트 - 씬 구조 수정, 비주얼 레퍼런스 재정의(Coffee Talk/황혼의 공주/HLD), 플레이 화면 UI 레이아웃 확정
 * play ref/ 폴더 추가 - 플레이 화면 샘플 이미지 (초안.png 확정)
---------
[2026-04-17] (v7)
 * YorkiMovement.cs 추가 - WASD 8방향 이동, dirIndex/isMoving 파라미터 제어
 * YorkiSetup.cs 추가 (Editor) - 스프라이트 임포트 설정(Point/Uncompressed/68PPU), idle/walk 애니메이션 클립 각 8개 생성, AnyState 기반 AnimatorController 생성, SceneB_Test 씬 자동 생성
 * Assets/Sprites/Yorki/ 추가 - PixelLab 생성 Yorki 8방향 정지(rotations) + 8방향 걷기 애니메이션(6프레임)
 * SceneB_Test.unity 추가 - 초록 바닥 임시 맵 + Yorki 캐릭터 배치, 8방향 이동 및 방향별 모션 전환 동작 확인
---------
[2026-04-13] (v6)
 * NarratorController.cs 추가 - 엔터/스페이스로 대사 진행, 마지막 줄 후 드로잉 화면 전환
 * GameManager.cs 수정 - GameState(Narrator/Drawing) 상태 관리, narratorController 연동
 * ReactionUI.cs 수정 - 반응 종료 후 Narrator 상태로 복귀
 * DialogueBox 위치 하단에서 80px 위로 조정
---------
[2026-04-13] (v5)
 * GameManager.cs 수정 - Narrator/Drawing 상태 전환, 드로잉 UI(캔버스/손님이미지/툴바) 토글
 * GameSceneBuilder.cs 수정 - toolbar 레퍼런스 연결
---------
[2026-04-13] (v4)
 * DrawingCanvas.cs 수정 - Undo 기능 추가
   - OnPointerDown / ClearCanvas 시점에 Color32[] 스냅샷 저장
   - 최대 20단계 undo 스택 유지
   - Cmd+Z (macOS) / Ctrl+Z (Windows) 입력으로 이전 획 복원
---------
[2026-04-13] (v3)
 * DialogueUI.cs 추가 - 화면 하단 고정 대사창 (이름 + 본문, 자동 지우기 지원)
 * ReactionUI.cs 재작성 - 오버레이 UI 제거, DialogueUI에 반응 대사 위임, 3초 후 캔버스 리셋
 * GameSceneBuilder.cs 재작성 - ReactionPanel 시각 제거, DialogueBox 생성, ReactionUI를 GameManager에 부착
 * GameManager.cs 수정 - reactionUI.Show()에 customerName 전달
 * DrawingToolbar.cs 수정 - Submit → GameManager.Instance?.OnSubmit()
 * DrawingSceneBuilder.cs 수정 - Toolbar에 DrawingToolbar 컴포넌트 추가
---------
[2026-04-13] (v2)
 * GameSceneBuilder.cs 수정 - CustomerData 에셋 자동 생성 블록 추가
   - Assets/Resources/Customers/SampleCustomer.asset 자동 생성
   - 토미에 샘플.png referenceImage 자동 연결
   - zoneWeights 기본값 초기화 (중심 4x4 = 2.0, 나머지 = 0.5)
   - GameManager.currentCustomer 자동 연결
---------
[2026-04-13] (v1)
 * CustomerData.cs 추가 - 손님 데이터 ScriptableObject (5단계 반응 등급 포함)
 * ScoreCalculator.cs 추가 - 코스 그리드(8x8) + 슬라이딩 윈도우(+-2칸) 점수 계산
 * ReactionSystem.cs 추가 - 점수 -> 반응 등급 분기, 대사 풀, 수입 계산
 * GameManager.cs 추가 - 제출 흐름 총괄, 하루 데이터 관리
 * ReactionUI.cs 추가 - 반응 연출 UI (등급별 색상, 대사, 수입 표시)
---------
*/
