# Yorki, the Portraitist — 구현 진행 현황

> 마지막 업데이트: 2026-04-30

---

## 전체 진행 요약

| 영역 | 상태 |
|------|------|
| 드로잉 캔버스 | 기본 완료, UI 연결 미완 |
| 채점 / 반응 시스템 | 프로토타입 완료 |
| SceneA (TalkScene/SceneA 분리) | 진행 중 |
| 하루 루프 | 미착수 |
| 자리 선택 | 미착수 |
| 저녁 정산 화면 | 미착수 |

---

## 씬 현황

### Scene A — TalkScene / SceneA 분리

**완료:**
- [x] Canvas 레이아웃 (1280×720, 좌측 캐릭터 / 우측 드로잉 패널)
- [x] 배경 이미지 (BG_Street.png, 비율 유지)
- [x] 손님 캐릭터 표시 (CustomerDisplay, 8종 스프라이트 — talk1/talk2 토글 포함)
- [x] FadeIn 등장 연출
- [x] 대화창 (DialogueBox.png, 9-slice)
- [x] 대사 타이핑 출력 (SceneADialogue, 한 글자씩)
- [x] 포즈 그룹 시스템 (resting/gesture/thinking/reaction, 그룹 전환 pause)
- [x] ▼ 계속 화살표 깜빡임
- [x] 엔터/스페이스로 대사 진행
- [x] 대사 즉시 출력 (타이핑 중 입력 시)
- [x] 씬 자동 빌드 메뉴 (Yorki/Build Scene A)
- [x] PersistentBootstrap — 손님 + 배경을 PersistentCanvas / CustomerStage 영속 객체로 분리
- [x] TalkSceneController — PreDraw / ResultGood / ResultBad 대사 페이즈 분리
- [x] SceneTransition — TalkScene ↔ SceneA 슬라이드 + 페이드 전환 코루틴
- [x] CustomerDisplay — neutral 3프레임 ping-pong, gesture 2프레임 토글 활성화
- [x] SUBMIT 결과 분기 — Satisfied 이상은 ResultGood, Neutral 이하는 ResultBad로 TalkScene 복귀
- [x] 씬 반영 — TalkScene.unity 생성, SceneA.unity 작업대 전용 구조로 재생성
- [x] Build Settings — TalkScene(buildIndex 0), SceneA(buildIndex 1) 등록
- [x] 임시 전환 테스트 입력 — SceneA에서 G/B 키로 ResultGood/ResultBad TalkScene 복귀

**미완료:**
- [ ] 오른쪽 드로잉 패널 연결 (캔버스 + 팔레트 + 툴바)
- [ ] 한글 폰트 적용
- [ ] NameTag (화자 이름 탭)
- [ ] 손님 교체 (다음 손님 등장 흐름)

---

### 드로잉 씬 (DrawingCanvas)

**완료:**
- [x] 마우스 드로잉 (클릭 + 드래그, 선 보간 처리)
- [x] 브러시 크기 조절 (`SetBrushSize`)
- [x] 색상 변경 (`SetBrushColor`)
- [x] 지우개 (`SetEraser` — 배경색으로 덮어쓰기)
- [x] 캔버스 초기화 (`ClearCanvas`)
- [x] Undo — Cmd+Z (macOS) / Ctrl+Z (Windows), 최대 20단계
- [x] 툴바 (DrawingToolbar.cs — Submit, Reset, 도구 버튼)
- [x] Submit → GameManager.OnSubmit() 연동

**미완료:**
- [ ] 팔레트 UI (색상 선택 버튼들)
- [ ] 브러시 종류 (연필/펜/마커 등)
- [ ] 손님 참고 이미지 표시 (우측 상단 또는 별도 영역)
- [ ] 타임 제한 표시 (PM 형식 시간 UI)
- [ ] 격자/점선 보조 오버레이 (설정 토글)

---

## 시스템 현황

### 채점 시스템 (ScoreCalculator.cs)
- [x] 8×8 코스 그리드 + 슬라이딩 윈도우(±2칸) 픽셀 유사도 계산
- [ ] 손님별 기준 이미지 연동 (CustomerData.referenceImage)

### 반응 시스템 (ReactionSystem.cs / ReactionUI.cs)
- [x] 5단계 반응 (진상/불만/애매/만족/매우만족)
- [x] 단계별 대사 풀 + 수입 계산
- [x] 반응 대사 하단 대사창(DialogueUI)에 출력
- [x] 3초 후 캔버스 초기화 → Narrator 상태 복귀
- [ ] 반응 연출 (두근두근 줌인 등 비주얼 강화)

### 게임 상태 관리 (GameManager.cs)
- [x] GameState (Narrator / Drawing) 전환
- [x] 하루 데이터 구조 (수입, 손님 수)
- [ ] 하루 루프 전체 흐름 연결
- [ ] 저녁 정산 화면

### 대사 시스템
- [x] NarratorController.cs — 대사 시퀀스, 엔터로 진행, 드로잉 화면 전환
- [x] SceneADialogue.cs — 감정/포즈 연동 대사 진행
- [x] DialogueUI.cs — 화면 하단 고정 대사창
- [ ] 조건 태그 기반 대사 풀 시스템 (방문 횟수, 시간대, 친밀도 등)
- [ ] 다국어 / 한글 폰트

### 손님 데이터 (CustomerData.cs)
- [x] ScriptableObject 구조 (이름, 참고 이미지, 대사, 반응 등급)
- [x] SampleCustomer.asset 자동 생성
- [ ] 실제 손님 에셋 제작 (현재 샘플 1개만 존재)

### 씬 빌더 (Editor 스크립트)
- [x] SceneABuilder.cs — Yorki/Build Scene A (SceneA.unity 자동 생성)
- [x] DrawingSceneBuilder.cs — Yorki/Build Drawing Scene
- [x] GameSceneBuilder.cs — Yorki/Setup Game Scene
- [x] YorkiSetup.cs — 8방향 idle/walk 애니메이션 클립 + AnimatorController 자동 생성

### 씬 목록
| 씬 | 상태 | 설명 |
|----|------|------|
| SceneA.unity | 진행 중 | 손님 대화 + 드로잉 메인 씬 |
| SceneB_Test.unity | 미사용 | 8방향 이동 테스트용 (Coffee Talk 전환으로 미사용) |

### 기타 스크립트
- [x] YorkiMovement.cs — 8방향 이동 (Coffee Talk 방식 전환으로 현재 미사용)
- [x] NPCWander.cs — NPC 배회 AI (현재 미사용)
- [x] FadeIn.cs — 알파값 0→1 페이드인

---


## 알려진 문제점

### 스프라이트 품질

| 문제 | 내용 | 심각도 | 상태 |
|------|------|--------|------|
| 누끼 불완전 | 일부 스프라이트 배경 제거가 제대로 안 됨 | 높음 | 미해결 |
| ~~캐릭터 위치/크기 불일치~~ | ~~크롭 rect가 컷마다 달라 전환 시 튐~~ | ~~높음~~ | **해결 (rect 통일)** |
| 의자 다리 잘림 | 일부 컷에서 의자 하단이 잘려있음 | 중간 | 미해결 |
| 컷 전환 테두리 깜빡임 | AI 생성 이미지 특유의 외곽선 차이로 미세한 깜빡임 | 낮음 | 미해결 |

### 남은 정렬 이슈

AI로 뽑은 컷은 매번 테두리/디테일이 미세하게 달라서 완벽 일치는 불가.
idle/talk 쌍은 **몸통 자세 동일 + 입 모양만 차이**가 이상적이나, 현재 컷들은 자세도 약간 다름.
최종적으로 수동 정렬이 필요할 수 있음 (Photopea 등 활용).

---

## 다음 작업 순서 (우선순위)

### 1단계 — SceneA 완성
1. 드로잉 패널을 SceneA 오른쪽에 연결 (DrawingCanvas + DrawingToolbar)
2. 팔레트 UI 구현 (색상 선택 버튼)
3. 대사 종료 → 드로잉 모드 전환 흐름
4. 드로잉 제출 → 반응 대사 → 다음 손님 흐름
5. 한글 폰트 적용

### 2단계 — 하루 루프
1. 아침 시작 → SceneA (손님 대화 + 그리기) 반복
2. 저녁 정산 화면 (수입 + 손님 수 표시)
3. 하루 종료 → 다음 날 시작

### 3단계 — 콘텐츠 채우기
1. 실제 손님 에셋 제작 (CustomerData 다수)
2. 조건 태그 기반 대사 풀 구축
3. 타임 제한 시스템
4. 채점-손님 이미지 연동

### 4단계 — 비주얼 / 연출
1. 반응 연출 강화 (줌인 등)
2. 낮/밤 분위기 차이
3. 자리 선택 맵뷰
