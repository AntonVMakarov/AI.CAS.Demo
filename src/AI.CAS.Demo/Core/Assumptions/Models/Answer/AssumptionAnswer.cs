namespace AI.CAS.Core.Assumptions;

/// <summary>
/// Представляет публичный объяснимый ответ механизма предположений.
/// Ответ сохраняет старую трехзначную истинность и добавляет причину ответа. 
/// </summary>
public sealed record AssumptionAnswer
{
    /// <summary>
    /// Публичная трехзначная истинность ответа.
    /// </summary>
    public AssumptionTruthValue TruthValue { get; }


    /// <summary>
    /// Более точная причина ответа.
    /// </summary>
    public AssumptionAnswerKind Kind { get; }


    /// <summary>
    /// Закрытый конструктор с параметрами.
    /// </summary>
    /// <param name="truthValue">Трехзначный ответ</param>
    /// <param name="kind">Более точная причина ответа</param>
    private AssumptionAnswer(AssumptionTruthValue truthValue, AssumptionAnswerKind kind)
    {
        TruthValue = truthValue;
        Kind = kind;
    }


    public static AssumptionAnswer Proved(AssumptionTruthValue truthValue)
    {
        if (truthValue is not AssumptionTruthValue.True and not AssumptionTruthValue.False)
        {
            // Доказанное утверждение должно быть либо true либо false:
            throw new ArgumentOutOfRangeException(nameof(truthValue));
        }

        return new AssumptionAnswer(truthValue, AssumptionAnswerKind.Proved);
    }


    public static AssumptionAnswer NoEvidence() => new AssumptionAnswer(AssumptionTruthValue.Unknown, AssumptionAnswerKind.NoEvidence);


    public static AssumptionAnswer Inconclusive() => new AssumptionAnswer(AssumptionTruthValue.Unknown, AssumptionAnswerKind.Inconclusive);


    public static AssumptionAnswer Conflict() => new AssumptionAnswer(AssumptionTruthValue.Unknown, AssumptionAnswerKind.Conflict);
}
