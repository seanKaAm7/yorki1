using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static TalkScenePhase currentTalkPhase = TalkScenePhase.PreDraw;

    public enum GameState { Narrator, Drawing }
    public GameState currentState { get; private set; }

    [Header("손님 데이터")]
    public CustomerData currentCustomer;

    [Header("씬 레퍼런스")]
    public DrawingCanvas drawingCanvas;
    public RawImage customerImage;
    public ReactionUI reactionUI;
    public GameObject toolbar;
    public NarratorController narratorController;

    [Header("하루 데이터")]
    public int todayEarnings = 0;
    public int customersServed = 0;
    public int mentalHealth = 5;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetState(GameState.Narrator);
    }

    // 내레이터 화면 ↔ 드로잉 화면 전환
    public void SetState(GameState state)
    {
        currentState = state;
        bool drawing = (state == GameState.Drawing);

        if (drawingCanvas != null) drawingCanvas.gameObject.SetActive(drawing);
        if (customerImage != null) customerImage.gameObject.SetActive(drawing);
        if (toolbar != null)       toolbar.SetActive(drawing);

        if (state == GameState.Narrator)
            narratorController?.RestartSequence();
    }

    // 손님 등장 → 드로잉 화면으로 전환
    public void StartDrawing()
    {
        LoadCustomer(currentCustomer);
        SetState(GameState.Drawing);
        DialogueUI.Instance?.Clear();
    }

    void LoadCustomer(CustomerData data)
    {
        if (data == null) return;
        if (customerImage != null && data.referenceImage != null)
            customerImage.texture = data.referenceImage;
    }

    public void OnSubmit()
    {
        if (currentCustomer == null || drawingCanvas == null) return;

        Texture2D playerTex = drawingCanvas.GetDrawTexture();
        int score = ScoreCalculator.Calculate(playerTex, currentCustomer);
        ReactionResult result = ReactionSystem.Evaluate(score, currentCustomer);

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

    public void ShowLegacyReaction(ReactionResult result)
    {
        if (reactionUI != null && currentCustomer != null)
            reactionUI.Show(result, currentCustomer.customerName);
    }
}
