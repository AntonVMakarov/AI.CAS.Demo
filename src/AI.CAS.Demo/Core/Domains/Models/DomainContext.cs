namespace AI.CAS.Core.Domains;

/// <summary>
/// Представляет контекст для математической области.
/// Контекст хранит текущую область вычисления и не выполняет дополнительных математических выводов.
/// </summary>
public sealed record DomainContext
{
    /// <summary>
    /// Создает контекст с комплексной областью по умолчанию.
    /// Комплексная область выбрана как консервативный широкий контекст, потому что она не добавляет
    /// вещественных, положительных или целочисленных предположений к входному выражению.
    /// </summary>
    public DomainContext() : this(KnownDomains.Complex)
    {
    }


    /// <summary>
    /// Создает контекст с явно заданной математической областью.
    /// </summary>
    /// <param name="domain">
    /// Текущая область вычисления.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если область отсутствует.
    /// </exception>
    public DomainContext(IDomain domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        Domain = domain;
    }


    /// <summary>
    /// Возвращает текущую математическую область вычисления.
    /// Значение не должно трактоваться как доказательство принадлежности всех подвыражений этой области.
    /// </summary>
    public IDomain Domain { get; }
}