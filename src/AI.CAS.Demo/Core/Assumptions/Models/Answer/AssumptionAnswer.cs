using System.Collections.ObjectModel;

namespace AI.CAS.Core.Assumptions;

/// <summary>
/// Представляет публичный объяснимый ответ механизма предположений.
/// Ответ сохраняет старую трехзначную истинность и добавляет причину ответа. 
/// </summary>
public sealed class AssumptionAnswer
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
    /// Список правил, которые предоставили доказательства.
    /// </summary>
    public IReadOnlyList<string> RuleIds { get; }


    /// <summary>
    /// Закрытый конструктор с параметрами.
    /// </summary>
    /// <param name="truthValue">Трехзначный ответ</param>
    /// <param name="kind">Более точная причина ответа</param>
    private AssumptionAnswer(AssumptionTruthValue truthValue, AssumptionAnswerKind kind, IReadOnlyList<string> ruleIds)
    {
        TruthValue = truthValue;
        Kind = kind;
        RuleIds = CopyRuleIds(ruleIds);
    }


    public static AssumptionAnswer Proved(AssumptionTruthValue truthValue, IReadOnlyList<string> rulesIds)
    {
        if (truthValue is not AssumptionTruthValue.True and not AssumptionTruthValue.False)
        {
            // Доказанное утверждение должно быть либо true либо false:
            throw new ArgumentOutOfRangeException(nameof(truthValue));
        }

        return new AssumptionAnswer(truthValue, AssumptionAnswerKind.Proved, rulesIds);
    }


    public static AssumptionAnswer NoEvidence() => new AssumptionAnswer(AssumptionTruthValue.Unknown, AssumptionAnswerKind.NoEvidence, Array.Empty<string>());


    public static AssumptionAnswer Inconclusive(IReadOnlyList<string> ruleIds) => new AssumptionAnswer(AssumptionTruthValue.Unknown, AssumptionAnswerKind.Inconclusive, ruleIds);


    public static AssumptionAnswer Conflict(IReadOnlyList<string> ruleIds) => new AssumptionAnswer(AssumptionTruthValue.Unknown, AssumptionAnswerKind.Conflict, ruleIds);


    /// <summary>
    /// Создает защищенный снимок уникальных идентификаторов правил, которые предоставили доказательства.
    /// </summary>
    /// <param name="ruleIds">
    /// Исходный список уникальных идентификаторов правил, которые предоставили доказательства.
    /// Исходная последовательность правил.
    /// </param>
    /// <returns>
    /// Неизменяемый список правил.
    /// </returns>
    private static IReadOnlyList<string> CopyRuleIds(IReadOnlyList<string> ruleIds)
    {
        ArgumentNullException.ThrowIfNull(ruleIds);

        if (ruleIds.Count == 0)
        {
            return Array.Empty<string>();
        }

        List<string> copiedRuleIds = new List<string>(ruleIds.Count);

        foreach (string? ruleId in ruleIds)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(ruleId, nameof(ruleId));

            // Одно и тоже правило не добавляем по нескольку раз:
            if (!copiedRuleIds.Contains(ruleId))
            {
                copiedRuleIds.Add(ruleId);
            }
        }

        return new ReadOnlyCollection<string>(copiedRuleIds);
    }
}