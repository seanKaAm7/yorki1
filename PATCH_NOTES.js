/*
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
