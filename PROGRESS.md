# Yorki, the Portraitist 구현 진행 현황

> 마지막 업데이트: 2026-05-13

## 전체 진행 요약

| 영역 | 상태 |
|------|------|
| TalkScene 대화 씬 | 1차 구현 완료 |
| SceneA 드로잉 씬 | 프로토타입 1차 구현 완료 |
| TalkScene ↔ SceneA 전환 | 구현 완료, 전체 플레이 플로우 수동 검증 필요 |
| 채점 / 반응 시스템 | 프로토타입 완료 |
| 하루 루프 | 미착수 |
| 자리 선택 | 미착수 |
| 저녁 정산 화면 | 미착수 |

## 씬 현황

### TalkScene.unity

완료:
- [x] 풀화면 배경을 `MapBackgroundDraft2.png`로 교체
- [x] 손님 캐릭터 표시 (`CustomerDisplay`)
- [x] 말하기 프레임 과다 전환 완화: 기본 대화는 한 컷 중심, 제스처는 제한 전환
- [x] `FadeIn` 등장 연출
- [x] 어두운 반투명 대화창
- [x] `TalkSceneController` 기반 대사 타이핑 출력
- [x] PreDraw / ResultGood / ResultBad 대사 페이즈 분리
- [x] 엔터/스페이스 입력: 타이핑 스킵 또는 다음 대사 진행
- [x] 대사 종료 후 `SceneTransition`으로 SceneA 이동
- [x] Moneygraphy Pixel 폰트 적용
- [x] 사용자가 조정한 Customer / DialogueBox / DialogueText 위치와 크기를 현재 기본값으로 취급

남음:
- [ ] 결과 대사 종료 후 다음 손님/하루 루프로 넘기는 방식 결정
- [ ] NameTag 필요 여부 결정
- [ ] 실제 플레이 모드에서 전체 흐름 최종 확인

### SceneA.unity

완료:
- [x] 오른쪽 반칸 작업대 구조
- [x] `ui_fixed.png` 기반 DeskBase
- [x] 종이 영역 위 투명 `DrawingCanvas` 레이어
- [x] Pen / Brush / Eraser / Picker 버튼
- [x] THICKNESS 세로 슬라이더, 수치 표시, 미리보기 점
- [x] 팔레트 8칸
- [x] COLOR 영역 RGB 컬러 박스
- [x] COLOR 박스 안의 별도 스포이드 아이콘 제외
- [x] Undo / Redo / Reset / Submit 버튼
- [x] Submit 후 채점 결과에 따라 TalkScene ResultGood / ResultBad 복귀
- [x] Moneygraphy Pixel 폰트 적용
- [x] 사용자가 조정한 SliderHandle 위치/크기를 현재 기본값으로 취급

남음:
- [ ] RGB 컬러 박스 조작감 세부 조정
- [ ] SceneA에서 Submit 후 TalkScene 결과 대사까지 수동 검증

## 시스템 현황

### DrawingCanvas.cs

완료:
- [x] 마우스 드로잉
- [x] 선 보간 처리
- [x] 투명 배경 모드
- [x] Pen / Brush / Eraser / Picker 모드
- [x] 브러시 크기 변경
- [x] 색상 변경
- [x] Undo / Redo
- [x] 캔버스 초기화
- [x] 제출/채점용 흰 배경 합성 텍스처

### ScoreCalculator.cs

완료:
- [x] 8x8 코스 그리드 기반 픽셀 유사도 계산
- [x] 슬라이딩 윈도우 ±2칸 보정

남음:
- [ ] 손님별 기준 이미지 연동

### ReactionSystem.cs / GameManager.cs

완료:
- [x] 5단계 반응 결과
- [x] 수입 계산
- [x] Submit 결과를 Good / Bad 대사 페이즈로 분기

남음:
- [ ] 하루 수입/손님 수를 실제 루프에 연결
- [ ] 저녁 정산 화면

### Editor 빌더

완료:
- [x] `TalkSceneBuilder.cs`
- [x] `SceneABuilder.cs`
- [x] 공통 폰트/스프라이트 유틸 `YorkiEditorAssets.cs`
- [x] `DrawingSceneBuilder.cs` 구형 드로잉 테스트 씬 빌더 유지
- [x] `GameSceneBuilder.cs` 구형 SampleScene 빌더 유지

주의:
- `Yorki/Build Talk Scene`, `Yorki/Build Scene A`는 씬을 재생성한다.
- 실행 전 사용자가 수동으로 맞춘 좌표가 빌더 기본값에 반영됐는지 확인해야 한다.

## 제거 완료

- [x] `SceneADialogue.cs`
- [x] `SceneADialogue.cs.meta`
- [x] `SceneATestTransitionInput.cs`
- [x] `SceneATestTransitionInput.cs.meta`

## 알려진 문제

- AI 생성 손님 컷은 픽셀 단위 일관성이 낮아 많은 프레임을 빠르게 바꾸면 캐릭터가 흔들려 보인다.
- 현재는 한 대사 한 컷에 가깝게 프레임 변화를 줄여 완화했다.
- 완전 해결은 같은 베이스 컷에서 입/손만 바꾼 스프라이트를 다시 제작해야 한다.

## 다음 작업 우선순위

1. TalkScene -> SceneA -> Submit -> TalkScene 결과 대사 전체 플로우 수동 검증
2. 결과 대사 이후 다음 손님/하루 루프 설계
3. 손님별 기준 이미지와 채점 데이터 연결
4. 오래된 SampleScene 계열 스크립트 유지/삭제 결정
5. 실제 손님 에셋과 대사 데이터 확장
