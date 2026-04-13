using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("손님 데이터")]
    public CustomerData currentCustomer;

    [Header("씬 레퍼런스")]
    public DrawingCanvas drawingCanvas;
    public RawImage customerImage;
    public ReactionUI reactionUI;

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
        LoadCustomer(currentCustomer);
    }

    void LoadCustomer(CustomerData data)
    {
        if (data == null) return;
        if (customerImage != null && data.referenceImage != null)
            customerImage.texture = data.referenceImage;
    }

    // Submit 버튼에서 호출
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

        if (reactionUI != null)
            reactionUI.Show(result, currentCustomer.customerName);
    }
}
