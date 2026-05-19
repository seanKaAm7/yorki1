using UnityEngine;

public enum CustomerType { Normal, Picky, Relaxed, Jerk }

public enum ReactionLevel { VeryUnsatisfied, Unsatisfied, Neutral, Satisfied, VerySatisfied }

[CreateAssetMenu(fileName = "NewCustomer", menuName = "Yorki/Customer Data")]
public class CustomerData : ScriptableObject
{
    [Header("기본 정보")]
    public string customerName = "손님";
    public CustomerType type = CustomerType.Normal;
    public Texture2D referenceImage;

    [Header("게임 설정")]
    public int basePay = 10;

    [Header("존 가중치 (8x8 = 64칸)")]
    [Tooltip("중요 부위는 2.0, 배경은 0.5 권장")]
    public float[] zoneWeights = new float[64];

    void OnValidate()
    {
        if (zoneWeights == null || zoneWeights.Length != 64)
        {
            zoneWeights = new float[64];
            for (int i = 0; i < 64; i++)
                zoneWeights[i] = 1.0f;
        }
    }

    public int ScoreOffset()
    {
        if (type == CustomerType.Picky)   return 15;
        if (type == CustomerType.Relaxed) return -15;
        if (type == CustomerType.Jerk)    return 20;
        return 0;
    }

    public ReactionLevel GetReactionLevel(int score)
    {
        int adjusted = score - ScoreOffset();
        if (adjusted >= 87) return ReactionLevel.VerySatisfied;
        if (adjusted >= 70) return ReactionLevel.Satisfied;
        if (adjusted >= 50) return ReactionLevel.Neutral;
        if (adjusted >= 25) return ReactionLevel.Unsatisfied;
        return ReactionLevel.VeryUnsatisfied;
    }

    public int GetPayment(ReactionLevel level)
    {
        if (level == ReactionLevel.VerySatisfied) return Mathf.RoundToInt(basePay * 2.0f);
        if (level == ReactionLevel.Satisfied)     return Mathf.RoundToInt(basePay * 1.5f);
        if (level == ReactionLevel.Neutral)       return basePay;
        if (level == ReactionLevel.Unsatisfied)   return Mathf.RoundToInt(basePay * 0.5f);
        return 0;
    }
}
