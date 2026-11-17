using System.Collections.Immutable;
using System.Text;

namespace AI.CAS.Core.Ast;

/// <summary>
/// Представляет символьную переменную как лист Ast.
/// Узел хранит имя переменной без парсинга и без нормализации. Имя переменной не пустое
/// и не состоит целиком из пробельных символов.
/// </summary>
public sealed class AstVariableNode : AstNode
{
    /// <summary>
    /// Создает лист переменной с заданным именем.
    /// </summary>
    /// <param name="name">
    /// Имя переменной, которое должно содержать хотя бы один непробельный символ.
    /// </param>
    public AstVariableNode(string? name)
    {
        // Проверяем имя на минимальную структурную корректность, не вводя политику уровня парсера.
        Name = ValidateName(name, nameof(name));
    }


    /// <summary>
    /// Возвращает имя переменной.
    /// Имя является семантической нагрузкой листа и участвует в структурном равенстве и хеше.
    /// </summary>
    public string Name { get; }


    /// <summary>
    /// Возвращает пустую неизменяемую коллекцию детей, потому что переменная является атомарным листом.
    /// </summary>
    public override ImmutableArray<AstNode> Children
    {
        get
        {
            // Возвращаем пустую коллекцию как единственно допустимую структуру переменной.
            return ImmutableArray<AstNode>.Empty;
        }
    }


    /// <summary>
    /// Создает новый лист переменной с тем же именем после проверки пустого набора детей.
    /// </summary>
    /// <param name="children">
    /// Новая коллекция детей, которая для переменной обязана быть пустой.
    /// </param>
    /// <returns>
    /// Новый <see cref="AstVariableNode"/> с тем же именем.
    /// </returns>
    public override AstNode WithChildren(ImmutableArray<AstNode> children)
    {
        // Проверяем, что листовой узел не получает детей через общий API пересборки.
        ValidateChildren(children, 0, nameof(children));

        // Возвращаем новый объект того же конкретного семантического типа, сохраняя имя без нормализации.
        return new AstVariableNode(Name);
    }


    /// <summary>
    /// Сравнивает имена двух переменных одного конкретного CLR-типа.
    /// </summary>
    /// <param name="other">
    /// Другой узел переменной.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, если имена переменных совпадают посимвольно.
    /// </returns>
    protected override bool PayloadEquals(AstNode other)
    {
        // Приводим узел к AstVariableNode после проверки конкретного типа базовым AstNode.
        AstVariableNode otherVariableNode = (AstVariableNode)other;

        // Сравниваем имена строго и порядково, чтобы структурное равенство не зависело от культуры выполнения.
        return string.Equals(Name, otherVariableNode.Name, StringComparison.Ordinal);
    }


    /// <summary>
    /// Добавляет имя переменной в структурный хеш узла.
    /// </summary>
    /// <param name="hashCode">
    /// Накопитель хеша для текущего узла.
    /// </param>
    protected override void AddPayloadHashCode(ref HashCode hashCode)
    {
        // Добавляем имя переменной той же порядковой семантикой, которой пользуется строковый хеш в .NET.
        hashCode.Add(Name, StringComparer.Ordinal);
    }


    /// <summary>
    /// Печатает имя переменной в структурной отладочной форме как есть.
    /// </summary>
    /// <param name="builder">
    /// Буфер формы.
    /// </param>
    internal override void AppendDebugPayload(StringBuilder builder)
    {
        builder.Append(Name);
    }


    /// <summary>
    /// Возвращает <see langword="true"/>: переменная печатается одним именем <c>x</c>.
    /// </summary>
    internal override bool PrintsDebugPayloadWithoutKind => true;
}
