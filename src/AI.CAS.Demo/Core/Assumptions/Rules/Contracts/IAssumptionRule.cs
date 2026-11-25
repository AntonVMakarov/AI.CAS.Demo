using AI.CAS.Core.Cancellations;

namespace AI.CAS.Core.Assumptions;

/// <summary>
/// Интерфейс правил, отвечающих на вопросы движка предположений.
/// </summary>
internal abstract class IAssumptionRule
{
    /// <summary>
    /// Уникальный идентификатор правила.
    /// </summary>
    public abstract string RuleId { get; }


    /// <summary>
    /// Выполняет правило.
    /// </summary>
    /// <param name="query">Запрос на проверку предположения.</param>
    /// <param name="assumptions">Набор истин.</param>
    /// <param name="ct">Необязательный токен отмены.</param>
    /// <returns>
    /// Результат работы правила.
    /// </returns>
    public abstract AssumptionRuleResult Evaluate(AssumptionQuery query, AssumptionSet assumptions, IM4uCancellationToken? ct = null);
}
