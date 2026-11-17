using AI.CAS.Core.Ast;

using System.Collections.Immutable;

/// <summary>
/// Представляет n-арное умножение.
/// Узел хранит множители в исходном порядке и не применяет сортировку, удаление единиц, поглощение нулем или уплощение.
/// </summary>
public sealed class AstTimesNode : AstNaryNode
{
    /// <summary>
    /// Создает n-арное произведение из двух или более множителей.
    /// </summary>
    /// <param name="factors">
    /// Упорядоченные множители произведения.
    /// </param>
    public AstTimesNode(ImmutableArray<AstNode> factors) : base(factors)
    {
        // Базовый n-арный класс уже проверил количество множителей и отсутствие null.
    }


    /// <summary>
    /// Возвращает множители произведения в исходном порядке.
    /// Свойство является семантическим псевдонимом <see cref="AstNode.Children"/>.
    /// TODO: А зачем над две переменные <see cref="AstTimesNode.Factors"/> и <see cref="AstNode.Children"/> 
    /// обозначающие одно и тоже?
    /// </summary>
    public ImmutableArray<AstNode> Factors
    {
        get
        {
            // Возвращаем ту же неизменяемую коллекцию, чтобы равенство с учетом порядка видело тех же детей.
            return Children;
        }
    }


    /// <summary>
    /// Создает новое произведение с заданными множителями.
    /// </summary>
    /// <param name="children">
    /// Новые множители в исходном порядке.
    /// </param>
    /// <returns>
    /// Новый <see cref="AstTimesNode"/>.
    /// </returns>
    protected override AstNaryNode Rebuild(ImmutableArray<AstNode> children)
    {
        // Возвращаем новое произведение без уплощения и без алгебраических упрощений.
        return new AstTimesNode(children);
    }
}