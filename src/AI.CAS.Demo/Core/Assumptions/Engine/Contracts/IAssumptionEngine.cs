using AI.CAS.Core.Cancellations;

namespace AI.CAS.Core.Assumptions;

/// <summary>
/// Описывает механизм ответа на запросы к набору предположений.
/// Интерфейс отделяет данные предположений от стратегии вывода.
/// </summary>
public interface IAssumptionEngine
{
    /// <summary>
    /// Вычисляет истинностное значение запроса относительно заданного набора предположений.
    /// Реализация обязана возвращать <see cref="AssumptionTruthValue.Unknown"/>, если не может доказать
    /// истинность или ложность утверждения в рамках своих явно заявленных возможностей.
    /// </summary>
    /// <param name="query">
    /// Запрос к механизму предположений.
    /// </param>
    /// <param name="assumptions">
    /// Набор известных предположений.
    /// </param>
    /// <param name="ct">
    /// Необязательный токен отмены долгого вычисления.
    /// </param>
    /// <returns>
    /// Трехзначное истинностное значение запроса.
    /// </returns>
    AssumptionTruthValue Evaluate(AssumptionQuery query, AssumptionSet assumptions, IM4uCancellationToken? ct = null);


    /// <summary>
    /// Вычисляет объяснимый ответ на запрос относительно заданного набора предположений.
    /// Метод возвращает ту же публичную истинность, что и <see cref="Evaluate(AssumptionQuery, AssumptionSet, IM4uCancellationToken?)"/>,
    /// но дополнительно сообщает причину ответа, идентификаторы правил, участвующих в доказательстве. Реализация не
    /// должна запускать отдельный второй пайплайн: Evaluate обязан быть проекцией <c>Explain(...).TruthValue</c>.
    /// </summary>
    /// <param name="query">
    /// Запрос к механизму предположений.
    /// </param>
    /// <param name="assumptions">
    /// Набор известных предположений.
    /// </param>
    /// <param name="ct">
    /// Необязательный токен отмены долгого вычисления.
    /// </param>
    /// <returns>
    /// Объяснимый ответ механизма предположений.
    /// </returns>
    AssumptionAnswer Explain(AssumptionQuery query, AssumptionSet assumptions, IM4uCancellationToken? ct = null);
}
