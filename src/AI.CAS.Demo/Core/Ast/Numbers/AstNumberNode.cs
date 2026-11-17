using System.Collections.Immutable;
using System.Text;

namespace AI.CAS.Core.Ast;

/// <summary>
/// Представляет числовой лист Ast.
/// Узел хранит типизированное числовое значение <see cref="AstNumber"/> и не имеет детей. Конкретная числовая форма
/// остается внутри объекта числовых данных, чтобы будущие рациональные или приближенные числа можно было добавить без
/// превращения узла в строковый или нетипизированный контейнер.
/// </summary>
public sealed class AstNumberNode : AstNode
{
    /// <summary>
    /// Создает числовой лист с заданным неизменяемым числовым значением.
    /// </summary>
    /// <param name="number">
    /// Числовое значение Ast, которое не должно быть <see langword="null"/>.
    /// </param>
    public AstNumberNode(AstNumber number)
    {
        // Проверяем числовые данные на null, потому что числовой лист без значения не имеет математического смысла.
        Number = number ?? throw new ArgumentNullException(nameof(number));
    }


    /// <summary>
    /// Возвращает типизированное числовое значение листа.    
    /// </summary>
    public AstNumber Number { get; }


    /// <summary>
    /// Возвращает пустую неизменяемую коллекцию детей, потому что число является атомарным листом выражения.
    /// </summary>
    public override ImmutableArray<AstNode> Children
    {
        get
        {
            // Возвращаем каноническую пустую неизменяемую коллекцию без выделения детей.
            return ImmutableArray<AstNode>.Empty;
        }
    }


    /// <summary>
    /// Создает новый числовой лист с тем же числовым значением после проверки пустого набора детей.
    /// Листовая пересборка возвращает новый объект, чтобы общий контракт пересборки был одинаковым для листьев и составных узлов.
    /// </summary>
    /// <param name="children">
    /// Новая коллекция детей, которая для числового листа обязана быть пустой.
    /// </param>
    /// <returns>
    /// Новый <see cref="AstNumberNode"/> с тем же числовым значением.
    /// </returns>
    public override AstNode WithChildren(ImmutableArray<AstNode> children)
    {
        // Проверяем, что вызывающий общий обход не пытается превратить лист в составной узел.
        ValidateChildren(children, 0, nameof(children));

        // Возвращаем новый лист того же конкретного типа и с теми же неизменяемыми числовыми данными.
        return new AstNumberNode(Number);
    }


    /// <summary>
    /// Сравнивает собственное числовое значение двух числовых листьев.
    /// </summary>
    /// <param name="other">
    /// Другой узел того же конкретного CLR-типа.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, если числовые значения равны по контракту <see cref="AstNumber"/>.
    /// </returns>
    protected override bool PayloadEquals(AstNode other)
    {
        // Приводим узел к AstNumberNode после проверки конкретного типа базовым AstNode.
        AstNumberNode otherNumberNode = (AstNumberNode)other;

        // Сравниваем типизированные числовые значения, а не строковые представления.
        return Number.Equals(otherNumberNode.Number);
    }


    /// <summary>
    /// Добавляет числовое значение в структурный хеш узла.
    /// </summary>
    /// <param name="hashCode">
    /// Накопитель хеша для текущего узла.
    /// </param>
    protected override void AddPayloadHashCode(ref HashCode hashCode)
    {
        // Добавляем числовые данные, чтобы разные числа давали разные хеши при одинаковой пустой структуре.
        hashCode.Add(Number);
    }


    /// <summary>
    /// Печатает число в структурной отладочной форме через его собственный хук: <c>2</c>, <c>Rational[1, 2]</c>,
    /// <c>Real[1.25, 3]</c>.
    /// </summary>
    /// <param name="builder">
    /// Буфер формы.
    /// </param>
    internal override void AppendDebugPayload(StringBuilder builder)
    {
        Number.AppendDebugPayload(builder);
    }


    /// <summary>
    /// Возвращает <see langword="true"/>: число печатается своим значением без обертки <c>Number[...]</c>.
    /// </summary>
    internal override bool PrintsDebugPayloadWithoutKind => true;
}