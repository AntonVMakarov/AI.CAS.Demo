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
/// <para>
/// Свойства <see cref="Subject"/>, <see cref="Predicate"/>, <see cref="ComparedTo"/>, <see cref="Domain"/> и
/// <see cref="Interval"/> оставлены как совместимый доступ только для чтения к полям атома. Новый код должен
/// предпочитать <see cref="Atom"/>, потому что именно атом является единственным общим представлением утверждения
/// без истинностного значения.
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


    /// <summary>
    /// Возвращает выражение, для которого выполняется запрос.
    /// Свойство является совместимым представлением <see cref="AssumptionAtom.Subject"/> только для чтения.
    /// </summary>
    public AstNode Subject => Atom.Subject;


    /// <summary>
    /// Возвращает вид проверяемого предиката.
    /// Свойство является совместимым представлением <see cref="AssumptionAtom.Predicate"/> только для чтения.
    /// </summary>
    public AssumptionPredicateKind Predicate => Atom.Predicate;


    /// <summary>
    /// Возвращает второе выражение для бинарного предиката, если оно было задано.
    /// Свойство является совместимым представлением <see cref="AssumptionAtom.ComparedTo"/> только для чтения.
    /// </summary>
    public AstNode? ComparedTo => Atom.ComparedTo;


    /// <summary>
    /// Возвращает область, относительно которой формулируется запрос, если она была задана.
    /// Свойство является совместимым представлением <see cref="AssumptionAtom.Domain"/> только для чтения.
    /// </summary>
    public IDomain? Domain => Atom.Domain;


    /// <summary>
    /// Возвращает типизированный интервал запроса, если запрос проверяет интервальную принадлежность.
    /// Это свойство существует для совместимого доступа к <see cref="AssumptionAtom.Interval"/> и не является
    /// отдельным строковым представлением условия.
    /// </summary>
    public ScalarInterval? Interval => Atom.Interval;
}
