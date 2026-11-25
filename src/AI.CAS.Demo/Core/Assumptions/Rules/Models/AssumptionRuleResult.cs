using AI.CAS.Core.Assumptions;

/// <summary>
/// Представляет внутренний результат работы одного правила знаний.
/// Тип отделяет неприменимость, незавершенное доказательство, конфликт и определенное доказательство.
/// </summary>
internal sealed record AssumptionRuleResult
{
    /// <summary>
    /// Публичная трехзначная истинность ответа.
    /// </summary>
    public AssumptionTruthValue TruthValue { get; }


    /// <summary>
    /// Более точная причина ответа.
    /// </summary>
    public AssumptionRuleResultKind Kind { get; }

    
    /// <summary>
    /// Правило, которое предоставило ответ.
    /// </summary>
    public string RuleId { get; } = null!;


    /// <summary>
    /// Закрытый конструктор с параметрами.
    /// </summary>
    /// <param name="truthValue">Трехзначная логика результата</param>
    /// <param name="kind">Более точная причина ответа</param>
    /// <param name="ruleId">Правило, которое предоставило ответ</param>
    private AssumptionRuleResult(AssumptionTruthValue truthValue, AssumptionRuleResultKind kind, string ruleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleId, nameof(ruleId));

        TruthValue = truthValue;
        Kind = kind;
        RuleId = ruleId;
    }


    /// <summary>
    /// Создаёт доказанный результат правила (знания).
    /// </summary>
    /// <param name="truthValue">
    /// Доказанное значение результата.
    /// </param>
    /// <param name="ruleId">
    /// Идентификатор правила.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static AssumptionRuleResult Proved(AssumptionTruthValue truthValue, string ruleId)
    {
        if (truthValue is not AssumptionTruthValue.True and not AssumptionTruthValue.False)
        {
            // Доказанное утверждение должно быть либо true либо false:
            throw new ArgumentOutOfRangeException(nameof(truthValue));
        }

        return new AssumptionRuleResult(truthValue, AssumptionRuleResultKind.Proved, ruleId);
    }


    public static AssumptionRuleResult NoAnswer(string ruleId)
        => new AssumptionRuleResult(AssumptionTruthValue.Unknown, AssumptionRuleResultKind.NoAnswer, ruleId);


    public static AssumptionRuleResult Inconclusive(string ruleId)
        => new AssumptionRuleResult(AssumptionTruthValue.Unknown, AssumptionRuleResultKind.Inconclusive, ruleId);


    public static AssumptionRuleResult Conflict(string ruleId)
        => new AssumptionRuleResult(AssumptionTruthValue.Unknown, AssumptionRuleResultKind.Conflict, ruleId);
    
}