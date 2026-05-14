using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static TalkScenePhase currentTalkPhase = TalkScenePhase.PreDraw;

    [Header("손님 데이터")]
    public CustomerData currentCustomer;

    [Header("씬 레퍼런스")]
    public DrawingCanvas drawingCanvas;

    [Header("하루 데이터")]
    public int todayEarnings = 0;
    public int customersServed = 0;
    public int mentalHealth = 5;

    void Awake()
    {
        Instance = this;
    }

    public void OnSubmit()
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
