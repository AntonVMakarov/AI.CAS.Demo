namespace AI.CAS.Core.Domains;

/// <summary>
/// Представляет простую скалярную математическую область.
/// Тип используется как минимальная реализация <see cref="IDomain"/> для стандартных областей,
/// которые нужны первому фасадному слою без отдельного описания параметров кольца, поля или пространства.
/// </summary>
public sealed record ScalarDomain : IDomain
{
    /// <summary>
    /// Создает скалярную область с заданным видом и отображаемым именем.
    /// Конструктор не выполняет математических проверок, потому что область на этом этапе является
    /// декларативным объектом контекста, но он защищает контракт от пустого диагностического имени.
    /// </summary>
    /// <param name="kind">
    /// Общий вид области.
    /// </param>
    /// <param name="displayName">
    /// Отображаемое техническое имя области.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Возникает, если отображаемое имя отсутствует или состоит только из пробельных символов.
    /// </exception>
    public ScalarDomain(DomainKind kind, string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        Kind = kind;
        DisplayName = displayName;
    }


    /// <summary>
    /// Возвращает общий вид скалярной области.
    /// </summary>
    public DomainKind Kind { get; }


    /// <summary>
    /// Возвращает отображаемое техническое имя скалярной области.
    /// </summary>
    public string DisplayName { get; }
}
