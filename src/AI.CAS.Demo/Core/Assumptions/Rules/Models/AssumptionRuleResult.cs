using AI.CAS.Core.Assumptions;

/// <summary>
/// Представляет внутренний результат работы одного правила знаний.
/// Тип отделяет неприменимость, незавершенное доказательство, конфликт и определенное доказательство.
/// </summary>
internal sealed record AssumptionRuleResult(AssumptionRuleResultKind Kind, AssumptionTruthValue TruthValue, string RuleId)
{
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
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleId, nameof(ruleId));

        if (truthValue == AssumptionTruthValue.Unknown)
        {
            throw new ArgumentException("Доказанное утверждение не может хранить публичное значение Unknown.", nameof(truthValue));
        }

        return new AssumptionRuleResult(AssumptionRuleResultKind.Proved, truthValue, ruleId);
    }


    
}