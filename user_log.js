/*
-----------------------------------------
[Log #1] [2026-04-13 13:00:00]
 * 사용자: 응 그리ㅡ고 [씬 세팅 자동화 요청 + 프로젝트 시스템 지침 전달]
 * 작업: 프로젝트 시스템 지침(언어 규칙, 로그 기록, 패치 노트 관리) 수신 및 숙지.
         CustomerData.cs, ScoreCalculator.cs, ReactionSystem.cs, GameManager.cs, ReactionUI.cs 스크립트 생성.
         user_log.js 및 PATCH_NOTES.js 초기 생성.
         씬 자동화 스크립트 작업 예정.
-----------------------------------------
[Log #2] [2026-04-13 14:00:00]
 * 사용자: 너가 해 [CustomerData 에셋 자동 생성 + GameManager 연결 요청]
 * 작업: GameSceneBuilder.cs 수정 - SetupScene() 끝부분에 CustomerData 자동 생성 블록 추가.
         Assets/Resources/Customers/SampleCustomer.asset 생성 완료.
         토미에 샘플.png referenceImage 연결 완료.
         zoneWeights 기본값 설정 (중심 4x4 = 2.0, 나머지 = 0.5).
         GameManager.currentCustomer 자동 연결 완료.
         "Yorki/Setup Game Scene" 메뉴 실행 후 전체 셋업 정상 확인.
-----------------------------------------
[Log #3] [2026-04-13 14:30:00]
 * 사용자: 제출 눌렀는데 아무것도 안 됨 / 전체 코드 점검 요청
 * 버그 수정:
   - DrawingToolbar.cs: Submit 버튼이 Debug.Log("제출!")만 호출 → GameManager.Instance?.OnSubmit()으로 변경
   - ReactionUI.cs: 비활성 오브젝트에서 StartCoroutine 시도로 에러 → SetActive(true)를 Show() 진입 시점으로 이동
   - GameSceneBuilder.cs: Submit 버튼 Non-Persistent 연결 코드 제거 (DrawingToolbar에서 처리)
   - DrawingSceneBuilder.cs: Toolbar에 DrawingToolbar 컴포넌트 추가
 * 구조 변경:
   - DialogueUI.cs 신규 추가 - 화면 하단 항상 표시되는 대사창
   - ReactionUI.cs 재작성 - 오버레이 제거, DialogueUI에 대사 위임
   - GameSceneBuilder.cs 재작성 - DialogueBox UI 생성, ReactionUI를 GameManager에 부착
   - GameManager.cs: reactionUI.Show()에 customerName 파라미터 추가
-----------------------------------------
*/
