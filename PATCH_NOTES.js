/*
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
