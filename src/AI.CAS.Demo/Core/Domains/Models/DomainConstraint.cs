using AI.CAS.Core.Ast;

namespace AI.CAS.Core.Domains;

/// <summary>
/// Представляет доменное ограничение для конкретного выражения.
/// Ограничение связывает неизменяемый <see cref="AstNode"/> с математической областью, но не проверяет,
/// истинно ли это ограничение в текущем математическом контексте.
/// </summary>
public sealed record DomainConstraint
{
    /// <summary>
    /// Создает доменное ограничение для заданного выражения.
    /// </summary>
    /// <param name="subject">
    /// Неизменяемое дерево выражения, к которому относится ограничение.
    /// </param>
    /// <param name="domain">
    /// Математическая область, с которой связывается выражение.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если выражение или область отсутствует.
    /// </exception>
    public DomainConstraint(AstNode subject, IDomain domain)
    {
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(domain);

        Subject = subject;
        Domain = domain;
    }


    /// <summary>
    /// Возвращает неизменяемое дерево выражения, к которому относится доменное ограничение.
    /// </summary>
    public AstNode Subject { get; }


    /// <summary>
    /// Возвращает математическую область, с которой связано выражение.
    /// </summary>
    public IDomain Domain { get; }
}