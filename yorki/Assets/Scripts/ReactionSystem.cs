using UnityEngine;

public class ReactionResult
{
    public ReactionLevel level;
    public string dialogue;
    public int payment;
    public bool mentalDamage; // 진상 손님일 때 true
}

public static class ReactionSystem
{
    static readonly string[][] dialogues = new string[][]
    {
        // VeryUnsatisfied — 진상
        new string[] {
            "...일부러 이렇게 그린 거예요?",
            "환불해줘요. 진심으로.",
            "이게 뭐예요?? 장난해요?",
            "돈 돌려줘요. 지금 당장.",
            "이걸 그림이라고... 어이없어서."
        },
        // Unsatisfied — 불만
        new string[] {
            "음... 뭔가 아쉽네요.",
            "좀 더 잘 그릴 수 있지 않았나요?",
            "고맙긴 한데... 별로예요 솔직히.",
            "기대했던 것보다는 좀 그렇네요.",
            "다음엔 더 잘 부탁해요."
        },
        // Neutral — 애매
        new string[] {
            "음... 뭐, 그냥저냥이네요.",
            "나쁘진 않은데 딱히 좋지도 않고.",
            "고맙습니다 ㅎㅎ - 그리신 지 얼마 됐어요?",
            "괜찮아요. 나쁘지 않아요.",
            "흠... 나름 봐줄 만하네요."
        },
        // Satisfied — 만족
        new string[] {
            "오, 꽤 잘 그렸는데요!",
            "좋은데요! 마음에 들어요.",
            "와, 생각보다 잘 그리시네요.",
            "잘 그렸어요. 고마워요.",
            "이 정도면 충분히 만족해요!"
        },
        // VerySatisfied — 매우만족
        new string[] {
            "완전 마음에 들어요!! 대박이다.",
            "세상에, 이렇게 잘 그리는 사람은 처음 봤어요!",
            "진짜 너무 잘 그렸다. 액자에 걸어둘 거예요.",
            "와 대박... 진짜 저 맞아요? 너무 잘 그렸다!",
            "이거 그림으로 파셔도 되겠는데요?! 정말 최고예요."
        }
    };

    public static ReactionResult Evaluate(int score, CustomerData customer)
    {
        ReactionLevel level = customer.GetReactionLevel(score);
        int payment = customer.GetPayment(level);

        string dialogue = GetDialogue(level);

        bool mentalDamage = (level == ReactionLevel.VeryUnsatisfied)
            || (customer.type == CustomerType.Jerk && level == ReactionLevel.Unsatisfied);

        return new ReactionResult
        {
            level = level,
            dialogue = dialogue,
            payment = payment,
            mentalDamage = mentalDamage
        };
    }

    static string GetDialogue(ReactionLevel level)
    {
        int idx = (int)level;
        string[] pool = dialogues[idx];
        return pool[Random.Range(0, pool.Length)];
    }
}
