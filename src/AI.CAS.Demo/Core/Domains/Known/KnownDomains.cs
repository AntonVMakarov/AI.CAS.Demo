namespace AI.CAS.Core.Domains;

/// <summary>
/// Статический класс, содержащий общие предопределённые экземпляры стандартных математических областей.
/// Эти значения являются часто используемыми математическими областями, но сами по себе они не запускают проверку 
/// принадлежности и не добавляют скрытых предположений.
/// </summary>
public static class KnownDomains
{
    /// <summary>
    /// Возвращает стандартную область комплексных чисел.
    /// </summary>
    public static ScalarDomain Complex { get; } = new ScalarDomain(DomainKind.Complex, "Complex");


    /// <summary>
    /// Возвращает стандартную область вещественных чисел.
    /// </summary>
    public static ScalarDomain Real { get; } = new ScalarDomain(DomainKind.Real, "Real");


    /// <summary>
    /// Возвращает стандартную область расширенной вещественной прямой.
    /// Область является отдельной ветвью от вещественных чисел: конечные вещественные значения входят в нее,
    /// но комплексная область и расширенная вещественная область остаются несравнимыми.
    /// </summary>
    public static ScalarDomain ExtendedReal { get; } = new ScalarDomain(DomainKind.ExtendedReal, "ExtendedReal");


    /// <summary>
    /// Возвращает стандартную область рациональных чисел.
    /// </summary>
    public static ScalarDomain Rational { get; } = new ScalarDomain(DomainKind.Rational, "Rational");


    /// <summary>
    /// Возвращает стандартную область целых чисел.
    /// </summary>
    public static ScalarDomain Integer { get; } = new ScalarDomain(DomainKind.Integer, "Integer");


    /// <summary>
    /// Возвращает стандартную область натуральных чисел.
    /// Значение не фиксирует вопрос включения нуля. Этот вопрос должен решаться более точным
    /// доменным контрактом в следующих фазах.
    /// </summary>
    public static ScalarDomain Natural { get; } = new ScalarDomain(DomainKind.Natural, "Natural");


    /// <summary>
    /// Возвращает стандартную булеву область.
    /// </summary>
    public static ScalarDomain Boolean { get; } = new ScalarDomain(DomainKind.Boolean, "Boolean");


    /// <summary>
    /// Возвращает неизвестную область.
    /// Это безопасная область для запросов, где текущий фасад не должен делать выводов о допустимых значениях.
    /// </summary>
    public static ScalarDomain Unknown { get; } = new ScalarDomain(DomainKind.Unknown, "Unknown");
}
