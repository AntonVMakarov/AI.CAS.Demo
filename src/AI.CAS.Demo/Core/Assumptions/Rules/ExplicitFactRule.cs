using AI.CAS.Core.Cancellations;

namespace AI.CAS.Core.Assumptions;

/// <summary>
/// Отвечает на запросы только по точным явно сохраненным фактам из <see cref="AssumptionSet"/>.
/// Это правило не выводит следствия и не ищет математически эквивалентные выражения.
/// </summary>
internal sealed class ExplicitFactRule : IAssumptionRule
{
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
    /// <param name="engine">
    /// Экземпляр объекта движка. Данное правило его не использует.
    /// </param>
    /// <param name="ct">
    /// Необязательный токен отмены. Проверяется внутри цикла по фактам, потому что список может стать большим.
    /// </param>
    /// <returns>
    /// Определенный ответ для согласованных точных фактов, <c>Uknown</c> в противополжном случае.
    /// </returns>    
    public AssumptionTruthValue Evaluate(AssumptionQuery query, AssumptionSet assumptions, IAssumptionEngine engine, IM4uCancellationToken? ct = null)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(assumptions);

        // Проходим по списку фактов:
        foreach (AssumptionFact fact in assumptions.Facts)
        {
            // Если условия в запросе совпадают с условиями в заданном факте, 
            // возращает истинностное значение факта:
            if (query.Atom == fact.Atom) return fact.TruthValue;
        }

        // Если мы не нашли подходящих фактов:
        return AssumptionTruthValue.Unknown;
    }
}
