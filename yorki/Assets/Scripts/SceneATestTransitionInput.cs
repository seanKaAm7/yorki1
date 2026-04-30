using UnityEngine;

// UI가 붙기 전까지 SceneA -> TalkScene 결과 복귀를 검증하기 위한 임시 입력.
// G: 좋은 반응, B: 나쁜 반응. 실제 Submit UI 연결 후 제거 예정.
public class SceneATestTransitionInput : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            SceneTransition.EnsureInstance().SceneAToTalkScene(TalkScenePhase.ResultGood);

        if (Input.GetKeyDown(KeyCode.B))
            SceneTransition.EnsureInstance().SceneAToTalkScene(TalkScenePhase.ResultBad);
    }
}
