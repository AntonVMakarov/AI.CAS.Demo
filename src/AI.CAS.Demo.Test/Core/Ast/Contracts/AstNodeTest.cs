using AI.CAS.Core.Ast;

namespace AI.CAS.Demo.Test;

[TestClass]
public class AstNodeTest
{
    [TestMethod]
    public void Test1()
    {
        // Создаём числовое значение 19:
        AstNumber int19 = new AstInteger(19);
        // и соответствующий узел:
        AstNumberNode int19Node = new AstNumberNode(int19);

        // Создаём числовое значение -15:
        AstNumber intMinus15 = new AstInteger(-15);
        // и соответствующий узел:
        AstNumberNode intMinus15Node = new AstNumberNode(intMinus15);

        // Создаём числовое значение 3:
        AstNumber int3 = new AstInteger(3);
        // и соответствующий узел:
        AstNumberNode int3Node = new AstNumberNode(int3);

        // Создаём узел '+', содержащий все числовые значения, созданные ранее:
        AstPlusNode plus = new AstPlusNode([int19Node, intMinus15Node, int3Node]);

        // Теперь проверяем результат:
        // Проверяем количество созданных узлов:
        Assert.AreEqual(3, plus.Count);
        // И тип узла:
        Assert.IsInstanceOfType(plus, typeof(AstPlusNode));

        // Проверяем тип и значение узла:
        // Первый = 19:
        Assert.IsInstanceOfType(plus[0], typeof(AstNumberNode));
        Assert.AreEqual(new AstNumberNode(new AstInteger(19)), plus[0]);

        // Второй = -15:
        Assert.IsInstanceOfType(plus[1], typeof(AstNumberNode));
        Assert.AreEqual(new AstNumberNode(new AstInteger(-15)), plus[1]);

        // Третий = 3:
        Assert.IsInstanceOfType(plus[2], typeof(AstNumberNode));
        Assert.AreEqual(new AstNumberNode(new AstInteger(3)), plus[2]);
    }
}
