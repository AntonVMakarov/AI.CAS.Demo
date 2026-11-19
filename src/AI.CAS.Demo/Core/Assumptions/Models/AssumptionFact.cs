using AI.CAS.Core.Ast;
using AI.CAS.Core.Domains;

namespace AI.CAS.Core.Assumptions;

/// <summary>
/// Представляет один явно заданный факт предположений для дерева выражения.
/// Факт хранит общий <see cref="AssumptionAtom"/> и истинностное значение, которое вызывающий код утверждает
/// в текущем <see cref="AssumptionSet"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AssumptionFact"/> остается отдельной ролью, несмотря на общий атом утверждения. Атом говорит,
/// какое утверждение сформулировано, а факт говорит, что это утверждение является входным знанием контекста
/// предположений. Поэтому факт не должен смешиваться с <see cref="AssumptionQuery"/> или <see cref="SemanticCondition"/>,
/// даже если все три типа содержат одинаковый атом.
/// </para>
/// <para>
/// Класс не выполняет математическую проверку истинности. Например, можно явно передать конфликтующие факты
/// <c>Positive(x) = True</c> и <c>Positive(x) = False</c>. Контейнер обязан сохранить входные данные, а
/// обнаружение конфликта относится к поставщику явных фактов и внутреннему согласователю результатов.
/// </para>
/// </remarks>
public sealed record AssumptionFact
{
    /// <summary>
    /// Создает факт предположения из уже проверенного атома и истинностного значения.
    /// Это основной путь для нового кода, потому что модель утверждения создается и проверяется отдельно
    /// от роли входного знания.
    /// </summary>
    /// <param name="atom">
    /// Валидный атом математического утверждения.
    /// </param>
    /// <param name="truthValue">
    /// Трехзначное значение истинности факта.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если атом отсутствует.
    /// </exception>
    public AssumptionFact(AssumptionAtom atom, AssumptionTruthValue truthValue)
    {
        ArgumentNullException.ThrowIfNull(atom);

        Atom = atom;
        TruthValue = truthValue;
    }


    /// <summary>
    /// Создает факт предположения через старую совместимую форму полей.
    /// Конструктор делегирует создание атома <see cref="AssumptionAtom.Create(AstNode, AssumptionPredicateKind, AstNode?, IDomain?)"/>,
    /// поэтому все структурные инварианты модели утверждения проверяются в одном месте.
    /// </summary>
    /// <param name="subject">
    /// Выражение, к которому относится факт.
    /// Значение должно рассматриваться как неизменяемый узел дерева выражения.
    /// </param>
    /// <param name="predicate">
    /// Вид предиката, который утверждается или отрицается для выражения.
    /// </param>
    /// <param name="truthValue">
    /// Трехзначное значение истинности факта.
    /// </param>
    /// <param name="comparedTo">
    /// Второе выражение для бинарных предикатов равенства и неравенства.
    /// Значение должно быть пустым для унарных и доменных предикатов.
    /// </param>
    /// <param name="domain">
    /// Математическая область для доменного предиката.
    /// Значение должно быть пустым для унарных и бинарных предикатов.
    /// </param>
    /// <param name="interval">
    /// Типизированный интервал для интервального предиката.
    /// Значение должно быть пустым для старых унарных, бинарных и доменных предикатов.
    /// </param>
    public AssumptionFact(
        AstNode subject,
        AssumptionPredicateKind predicate,
        AssumptionTruthValue truthValue,
        AstNode? comparedTo = null,
        IDomain? domain = null,
        ScalarInterval? interval = null)
        : this(
            AssumptionAtom.Create(subject, predicate, comparedTo, domain, interval),
            truthValue)
    {
    }


    /// <summary>
    /// Возвращает атом математического утверждения, которому факт приписывает истинностное значение.
    /// Точные сравнения фактов и запросов должны сравнивать этот атом, а не заново собирать ключ из отдельных полей.
    /// </summary>
    public AssumptionAtom Atom { get; }


    /// <summary>
    /// Возвращает истинностное значение факта.
    /// Значение <see cref="AssumptionTruthValue.Unknown"/> допустимо как явно сохраненная входная неопределенность,
    /// но поставщик явных фактов должен отличать ее от отсутствия совпадающих фактов.
    /// </summary>
    public AssumptionTruthValue TruthValue { get; }


    /// <summary>
    /// Возвращает выражение, к которому относится факт.
    /// Свойство является совместимым представлением <see cref="AssumptionAtom.Subject"/> только для чтения.
    /// </summary>
    public AstNode Subject => Atom.Subject;


    /// <summary>
    /// Возвращает вид предиката, который утверждается или отрицается фактом.
    /// Свойство является совместимым представлением <see cref="AssumptionAtom.Predicate"/> только для чтения.
    /// </summary>
    public AssumptionPredicateKind Predicate => Atom.Predicate;


    /// <summary>
    /// Возвращает второе выражение для бинарного предиката, если оно было задано.
    /// Свойство является совместимым представлением <see cref="AssumptionAtom.ComparedTo"/> только для чтения.
    /// </summary>
    public AstNode? ComparedTo => Atom.ComparedTo;


    /// <summary>
    /// Возвращает область, относительно которой сформулирован факт, если она была задана.
    /// Свойство является совместимым представлением <see cref="AssumptionAtom.Domain"/> только для чтения.
    /// </summary>
    public IDomain? Domain => Atom.Domain;


    /// <summary>
    /// Возвращает типизированный интервал факта, если факт формулирует
    /// <see cref="AssumptionPredicateKind.InInterval"/>.
    /// Свойство является совместимым представлением <see cref="AssumptionAtom.Interval"/> только для чтения.
    /// </summary>
    public ScalarInterval? Interval => Atom.Interval;
}