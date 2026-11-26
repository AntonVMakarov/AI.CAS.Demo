using AI.CAS.Core.Cancellations;

namespace AI.CAS.Core.Assumptions;

/// <summary>
/// Отвечает на запросы только по точным явно сохраненным фактам из <see cref="AssumptionSet"/>.
/// Это правило не выводит следствия и не ищет математически эквивалентные выражения.
/// </summary>
internal sealed class ExplicitFactRule : IAssumptionRule
{
    /// <summary>
    /// Уникальный идентификатор правила.
    /// </summary>
    public string RuleId => "explicit.fact.rule";


    /// <summary>
    /// Ищет точные совпадения запроса среди явно сохраненных фактов.
    /// Совпадение проверяется по субъекту, предикату, второму выражению и области.
    /// </summary>
    /// <param name="query">
    /// Запрос, для которого выполняется точный поиск.
    /// </param>
    /// <param name="assumptions">
    /// Снимок явно известных фактов.
    /// </param>
    /// <param name="ct">
    /// Необязательный токен отмены. Проверяется внутри цикла по фактам, потому что список может стать большим.
    /// </param>
    /// <returns>
    /// Определенный ответ для согласованных точных фактов, <c>Uknown</c> в противополжном случае.
    /// </returns>    
    public AssumptionRuleResult Evaluate(AssumptionQuery query, AssumptionSet assumptions, IM4uCancellationToken? ct = null)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(assumptions);

        bool hasUnknownFacts = false;
        bool hasTrueFacts = false;
        bool hasFalseFacts = false;

        // Проходим по списку фактов:
        foreach (AssumptionFact fact in assumptions.Facts)
        {
            // Проверяем токен отмены:
            ct?.ThrowIfCancellationRequested();

            // Если условия в запросе совпадают с условиями в заданном факте:
            if (query.Atom == fact.Atom)
            {
                // Анализируем совпадающий с нашим запросом факт:
                switch (fact.TruthValue)
                {
                    case AssumptionTruthValue.True:
                        hasTrueFacts = true;
                        break;
                    case AssumptionTruthValue.False:
                        hasFalseFacts = true;
                        break;
                    case AssumptionTruthValue.Unknown:
                        hasUnknownFacts = true;
                        break;
                }
            }
        }

        // Если у нас противоречие:
        if (hasTrueFacts && hasFalseFacts) 
        {
            return AssumptionRuleResult.Conflict(RuleId);
        }

        // Если у нас среди совпавших фактов есть true:
        // (Получается, что false нет, иначе это было бы учтено выше):
        if(hasTrueFacts)
        {
            return AssumptionRuleResult.Proved(AssumptionTruthValue.True, RuleId);
        }

        // Если у нас среди совпадающих фактов есть false:
        // (Получается, что true нет, иначе это было бы учтено выше):
        if(hasFalseFacts)
        {
            return AssumptionRuleResult.Proved(AssumptionTruthValue.False, RuleId);
        }

        // Если у нас среди совпадающих фактов есть unknown:
        // (Получается, что true и false нет, иначе это было бы учтено выше):
        if(hasUnknownFacts)
        {
            return AssumptionRuleResult.Inconclusive(RuleId);
        }

        // Если мы сюда попали, то вообще у нас нет никаких фактов, поэтому:
        return AssumptionRuleResult.NoAnswer(RuleId);
    }
}
