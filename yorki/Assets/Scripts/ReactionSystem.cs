public class ReactionResult
{
    public ReactionLevel level;
    public int payment;
    public bool mentalDamage; // 진상 손님일 때 true
}

public static class ReactionSystem
{
    public static ReactionResult Evaluate(int score, CustomerData customer)
    {
        ReactionLevel level = customer.GetReactionLevel(score);
        int payment = customer.GetPayment(level);

        bool mentalDamage = (level == ReactionLevel.VeryUnsatisfied)
            || (customer.type == CustomerType.Jerk && level == ReactionLevel.Unsatisfied);

        return new ReactionResult
        {
            level = level,
            payment = payment,
            mentalDamage = mentalDamage
        };
    }

    public static bool IsGoodResult(ReactionLevel level)
    {
        return level == ReactionLevel.Satisfied || level == ReactionLevel.VerySatisfied;
    }
}
