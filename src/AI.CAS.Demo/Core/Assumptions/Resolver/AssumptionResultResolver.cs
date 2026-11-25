using AI.CAS.Core.Assumptions;

internal sealed class AssumptionResultResolver
{
    /// <summary>
    /// Объединяет результаты правил знаний в итоговое публичное значение.
    /// </summary>
    /// <param name="results">
    /// Последовательность результатов правил знаний.
    /// </param>
    /// <returns>
    /// Согласованный результат.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если последовательность результатов отсутствует.
    /// </exception>
    public AssumptionAnswer Resolve(IEnumerable<AssumptionRuleResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        bool hasTrue = false;
        bool hasFalse = false;
        bool hasInconclusive = false;
        bool hasConflict = false;

        foreach (AssumptionRuleResult result in results)
        {
            switch (result.Kind)
            {
                case AssumptionRuleResultKind.Proved:
                    hasTrue |= result.TruthValue == AssumptionTruthValue.True;
                    hasFalse |= result.TruthValue == AssumptionTruthValue.False;
                    break;

                case AssumptionRuleResultKind.Inconclusive:
                    hasInconclusive = true;
                    break;

                case AssumptionRuleResultKind.Conflict:
                    hasConflict = true;
                    break;
            }
        }

        if (hasConflict || (hasTrue && hasFalse))
        {
            return AssumptionAnswer.Conflict(
                results.Select(r => r.RuleId).ToArray()
                );
        }

        if (hasTrue || hasFalse)
        {
            return AssumptionAnswer.Proved(
                hasTrue ? AssumptionTruthValue.True : AssumptionTruthValue.False,
                results.Select(r => r.RuleId).ToArray()
                );
        }

        return hasInconclusive ? AssumptionAnswer.Inconclusive(results.Select(r => r.RuleId).ToArray()) : AssumptionAnswer.NoEvidence();
    }
}