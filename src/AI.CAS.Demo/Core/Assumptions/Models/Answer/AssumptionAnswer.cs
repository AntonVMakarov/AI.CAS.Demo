namespace AI.CAS.Core.Assumptions;

/// <summary>
/// Представляет публичный объяснимый ответ механизма предположений.
/// Ответ сохраняет старую трехзначную истинность и добавляет причину ответа. 
/// </summary>
/// <param name="TruthValue">
/// Публичная трехзначная истинность ответа.
/// </param>
/// <param name="Kind">
/// Более точная причина ответа.
/// </param>
public sealed record AssumptionAnswer(AssumptionTruthValue TruthValue, AssumptionAnswerKind Kind)
{
    public static AssumptionAnswer Proved(AssumptionTruthValue truthValue)
    {
        if (truthValue == AssumptionTruthValue.Unknown)
        {
            throw new ArgumentException("Доказанное утверждение не может хранить публичное значение Unknown.", nameof(truthValue));
        }

        return new AssumptionAnswer(truthValue, AssumptionAnswerKind.Proved);
    }


    public static AssumptionAnswer NoEvidence(AssumptionTruthValue truthValue) 
    {
        return new AssumptionAnswer(AssumptionTruthValue.Unknown, AssumptionAnswerKind.NoEvidence);
    }


    public static AssumptionAnswer Inconclusive()
    {
        return new AssumptionAnswer(AssumptionTruthValue.Unknown, AssumptionAnswerKind.Inconclusive);
    }


    public static AssumptionAnswer Conflict()
    {
        return new AssumptionAnswer(AssumptionTruthValue.Unknown, AssumptionAnswerKind.Conflict);
    }
}
