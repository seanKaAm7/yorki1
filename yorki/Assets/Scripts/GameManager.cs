using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static TalkScenePhase currentTalkPhase = TalkScenePhase.PreDraw;

    [Header("손님 데이터 (채점용)")]
    public CustomerData currentCustomer;

    [Header("하루 에피소드 큐 (대사/표정)")]
    public CustomerEpisodeData[] episodeQueue;
    public int currentEpisodeIndex = 0;
    public bool introMonologueShown = false;

    [Header("씬 레퍼런스")]
    public DrawingCanvas drawingCanvas;

    [Header("하루 데이터")]
    public int todayEarnings = 0;
    public int customersServed = 0;
    public int mentalHealth = 5;

    public CustomerEpisodeData CurrentEpisode
    {
        get
        {
            if (episodeQueue == null || episodeQueue.Length == 0) return null;
            if (currentEpisodeIndex < 0 || currentEpisodeIndex >= episodeQueue.Length) return null;
            return episodeQueue[currentEpisodeIndex];
        }
    }

    public void AdvanceToNextEpisode() // 큐의 다음 손님으로 인덱스 진행. 마지막 손님 이후 호출되면 CurrentEpisode가 null이 됨.
    {
        currentEpisodeIndex++;
    }

    void Awake() // 영속 싱글턴. 두 번째 인스턴스는 자기 자신을 파괴해서 기존 큐 상태를 보존.
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void OnSubmit() // 그리기 제출 버튼을 눌렀을 때 호출. DrawingCanvas에서 점수를 계산하고 ReactionSystem으로 반응 평가를 받은 후 수입과 정신 건강에 반영. 그리고 SceneTransition을 통해 TalkScene으로 돌아가면서 결과에 따라 다음 대화 단계 설정.
    {
        if (drawingCanvas == null) return;

        CustomerData customer = currentCustomer;
        if (customer == null)
            customer = Resources.Load<CustomerData>("Customers/SampleCustomer");
        if (customer == null) return;

        Texture2D playerTex = drawingCanvas.GetFlattenedTextureForScoring();
        int score = ScoreCalculator.Calculate(playerTex, customer);
        ReactionResult result = ReactionSystem.Evaluate(score, customer);

        todayEarnings += result.payment;
        customersServed++;
        if (result.mentalDamage) mentalHealth--;

        Debug.Log($"[GameManager] 점수: {score} | 반응: {result.level} | 수입: +€{result.payment}");

        TalkScenePhase nextPhase = IsGoodResult(result.level)
            ? TalkScenePhase.ResultGood
            : TalkScenePhase.ResultBad;

        SceneTransition.EnsureInstance().SceneAToTalkScene(nextPhase);
    }

    static bool IsGoodResult(ReactionLevel level)
    {
        return level == ReactionLevel.Satisfied || level == ReactionLevel.VerySatisfied;
    }
}
