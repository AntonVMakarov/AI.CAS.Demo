using AI.CAS.Core.Cancellations;
using System.Collections.ObjectModel;

namespace AI.CAS.Core.Assumptions;

/// <summary>
/// Движок механизма предположений.
///
/// Главная идея класса:
///     "Лучше вернуть Unknown, чем сделать математически опасный вывод".
///
/// Для работы класс использует небольшой упорядоченный список правил (знаний).
///
/// 1. <see cref="ExplicitFactRule"/> читает точные явно заданные факты.
/// 2. <see cref="IntegerNumberRule"/> отвечает на очевидные свойства целых чисел.
///
/// На данном этапе возвращается первый полученный ответ. Это приводит к зависимости от 
/// порядка правил, что не очень хорошо и в будущем потребуется Resolver, который будет согласовывать результаты
/// независимых источников.
/// </summary>
public sealed class AssumptionEngine : IAssumptionEngine
{
    /// <summary>
    /// Список правил (знаний) для работы механизма предположений.
    /// </summary>
    private readonly IReadOnlyList<IAssumptionRule> _rules;
    

    /// <summary>
    /// Создает механизм предположений с правилами (знаниями) по умолчанию.
    /// Порядок правил фиксируется в одном месте, чтобы в будущем можно было добавлять новые источники
    /// знания без разрастания фасада и без изменения публичного контракта <see cref="IAssumptionEngine"/>.
    /// </summary>
    public AssumptionEngine() : this(CreateDefaultConfiguration())
    {
    }


    /// <summary>
    /// Создает механизм с явно заданными правилами знаний.
    /// Это основной конструктор: остальные перегрузки сводятся к нему.
    /// </summary>
    /// <param name="rules">
    /// Упорядоченный список правил знаний.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если список правил (знаний) отсутствует.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Возникает, если список правил (знаний) содержит отсутствующий элемент.
    /// </exception>
    internal AssumptionEngine(IReadOnlyList<IAssumptionRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        List<IAssumptionRule> copiedRules = new List<IAssumptionRule>(rules.Count);

        foreach (IAssumptionRule? rule in rules)
        {
            if (rule is null)
            {
                throw new ArgumentException("Пайплайн предположений не должен содержать отсутствующее правило.", nameof(rules));
            }

            copiedRules.Add(rule);
        }

        _rules = new ReadOnlyCollection<IAssumptionRule>(copiedRules);
    }


    /// <summary>
    /// Оценивает истинность запроса относительно набора предположений.
    /// </summary>
    /// <param name="query">
    /// Запрос к движку предположений.
    /// </param>
    /// <param name="assumptions">
    /// Набор явно известных фактов. Если дополнительных фактов нет, вызывающий код должен передать
    /// <see cref="AssumptionSet.Empty"/>, а не <c>null</c>.
    /// </param>
    /// <param name="ct">
    /// Необязательный токен отмены.
    /// </param>
    /// <returns>
    /// Трехзначный ответ: True, False или Unknown.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если query или assumptions равны null.
    /// </exception>
    public AssumptionTruthValue Evaluate(AssumptionQuery query, AssumptionSet assumptions, IM4uCancellationToken? ct = null)
    {
        // Проверяем токен отмены:
        ct?.ThrowIfCancellationRequested();

        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(assumptions);

        // Пока мы ничего не знаем:
        AssumptionTruthValue answer = AssumptionTruthValue.Unknown;

        // Теперь пробуем применить каждое правило:
        foreach (IAssumptionRule rule in _rules)
        {
            // Проверяем токен отмены:
            ct?.ThrowIfCancellationRequested();

            // Пробуем применить правило:
            answer = rule.Evaluate(query, assumptions, this, ct);

            // Если правило смогло доказать истинность или ложность утверждения, тогда
            // сразу возвращаем результат:
            if (answer != AssumptionTruthValue.Unknown) return answer;
        }

        // Возвращаем результат:
        return answer;
    }
  

    /// <summary>
    /// Создает стандартный порядок поставщиков для консервативного фасада.
    /// </summary>
    /// <returns>
    /// Упорядоченный список поставщиков предположений по умолчанию.
    /// </returns>
    private static IReadOnlyList<IAssumptionRule> CreateDefaultConfiguration()
    {
        // Правило для ответа на запросы только по точным явно сохраненным фактам:
        ExplicitFactRule explicitRule = new ExplicitFactRule();

        // Правило для ответа на запросы по числовым значениям:
        IntegerNumberRule numberRule = new IntegerNumberRule();

        // TODO: Сюда нужно добавлять новые правила:

        IAssumptionRule[] rules =
        [
            explicitRule,
            numberRule,
        ];

        // Возвращаем список правил:
        return rules;
    }
}
