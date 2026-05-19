using UnityEngine;
using UnityEngine.UI;

// 게임 시작 시 1회만 살아있는 영속 객체.
// 두 씬(TalkScene / SceneA) 사이를 따라다니며 손님 + 배경(CustomerStage)을 보유.
// 씬 전환 시 위치는 SceneTransition이 제어.
public class PersistentBootstrap : MonoBehaviour
{
    public static PersistentBootstrap Instance { get; private set; }

    [Header("자식 참조 (빌더에서 자동 연결)")]
    public Canvas           persistentCanvas;
    public RectTransform    customerStage;
    public RectTransform    background;
    public CustomerDisplay  customerDisplay;

    void Awake() // 싱글톤 패턴 구현. 이미 인스턴스가 존재하면 자신을 파괴하고, 그렇지 않으면 인스턴스로 설정하고 씬 전환 시에도 파괴되지 않도록 함.
    {
        // 동일 객체가 이미 살아있으면(예: TalkScene 재진입) 본인 파괴
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
