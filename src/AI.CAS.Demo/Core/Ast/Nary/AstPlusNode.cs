using AI.CAS.Core.Ast;

using System.Collections.Immutable;

/// <summary>
/// Представляет n-арное сложение.
/// Узел хранит слагаемые в исходном порядке и не применяет коммутативную сортировку, удаление нулей или уплощение.
/// </summary>
public sealed class AstPlusNode : AstNaryNode
{
    /// <summary>
    /// Создает n-арную сумму из двух или более слагаемых.
    /// </summary>
    /// <param name="terms">
    /// Упорядоченные слагаемые суммы.
    /// </param>
    public AstPlusNode(ImmutableArray<AstNode> terms) : base(terms)
    {
        // Базовый n-арный класс уже проверил количество слагаемых и отсутствие null.
    }


    /// <summary>
    /// Возвращает слагаемые суммы в исходном порядке.
    /// Свойство является семантическим псевдонимом <see cref="AstNode.Children"/>.
    /// TODO: А зачем над две переменные <see cref="AstPlusNode.Terms"/> и <see cref="AstNode.Children"/> 
    /// обозначающие одно и тоже?
    /// </summary>
    public ImmutableArray<AstNode> Terms
    {
        get
        {
            // Возвращаем ту же неизменяемую коллекцию, чтобы семантический псевдоним не расходился со структурным контрактом.
            return Children;
        }
    }


    /// <summary>
    /// Создает новую сумму с заданными слагаемыми.
    /// </summary>
    /// <param name="children">
    /// Новые слагаемые в исходном порядке.
    /// </param>
    /// <returns>
    /// Новый <see cref="AstPlusNode"/>.
    /// </returns>
    protected override AstNaryNode Rebuild(ImmutableArray<AstNode> children)
    {
        // Возвращаем новую сумму без уплощения и без алгебраических упрощений.
        return new AstPlusNode(children);
    }
}