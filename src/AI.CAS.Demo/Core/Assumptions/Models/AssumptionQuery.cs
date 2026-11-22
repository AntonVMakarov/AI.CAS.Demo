using AI.CAS.Core.Ast;
using AI.CAS.Core.Domains;

namespace AI.CAS.Core.Assumptions;

/// <summary>
/// Представляет запрос к механизму предположений.
/// Запрос хранит общий <see cref="AssumptionAtom"/> и тем самым задает математическое утверждение, истинность
/// которого должен оценить механизм предположений относительно текущего набора фактов.
/// </summary>
/// <remarks>
/// <para>
/// В отличие от <see cref="AssumptionFact"/> запрос не содержит ожидаемого истинностного значения.
/// Его роль состоит только в том, чтобы передать движку один валидный ключ математического утверждения. Движок может затем
/// найти точный факт, применить поставщики литералов, доменную решетку, граф импликаций или алгебраические правила.
/// </para>
/// </remarks>
public sealed record AssumptionQuery
{
    /// <summary>
    /// Создает запрос из уже проверенного атома.
    /// Такой путь используется новым кодом, который явно разделяет модель утверждения и публичный
    /// трехзначный ответ <see cref="AssumptionTruthValue"/>.
    /// </summary>
    /// <param name="atom">
    /// Валидный атом математического утверждения.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если атом отсутствует.
    /// </exception>
    public AssumptionQuery(AssumptionAtom atom)
    {
        ArgumentNullException.ThrowIfNull(atom);

        Atom = atom;
    }


    /// <summary>
    /// Создает запрос к механизму предположений через старую совместимую форму полей.
    /// Конструктор сохраняется для существующих вызовов <see cref="IAssumptionEngine.Evaluate"/>, но теперь
    /// немедленно делегирует построение <see cref="AssumptionAtom.Create(AstNode, AssumptionPredicateKind, AstNode?, IDomain?)"/>.
    /// Поэтому недопустимые сочетания полей отклоняются при создании запроса, а не в каждом отдельном поставщике.
    /// </summary>
    /// <param name="subject">
    /// Выражение, для которого требуется узнать истинность предиката.
    /// Значение должно рассматриваться как неизменяемый узел дерева выражения.
    /// </param>
    /// <param name="predicate">
    /// Вид проверяемого предиката.
    /// </param>
    /// <param name="comparedTo">
    /// Второе выражение для бинарного предиката.
    /// Значение должно быть задано только для равенства и неравенства.
    /// </param>
    /// <param name="domain">
    /// Область, относительно которой формулируется запрос.
    /// Значение должно быть задано только для доменного членства.
    /// </param>
    /// <param name="interval">
    /// Типизированный интервал для запроса <see cref="AssumptionPredicateKind.InInterval"/>.
    /// Значение должно отсутствовать для старых унарных, бинарных и доменных запросов.
    /// </param>
    public AssumptionQuery(
        AstNode subject,
        AssumptionPredicateKind predicate,
        AstNode? comparedTo = null,
        IDomain? domain = null,
        ScalarInterval? interval = null)
        : this(AssumptionAtom.Create(subject, predicate, comparedTo, domain, interval))
    {
    }


    /// <summary>
    /// Возвращает атом математического утверждения, истинность которого запрашивается.
    /// Все точные сравнения запросов должны использовать именно это значение, чтобы fact/query/condition
    /// сравнивали одну и ту же модель утверждения.
    /// </summary>
    public AssumptionAtom Atom { get; }
}
