using AI.CAS.Core.Assumptions;
using AI.CAS.Core.Ast;

namespace AI.CAS.Demo.Test.Core.Assumptions;

[TestClass]
public class AssumptionsAtomTest
{
    [TestMethod]
    public void Test()
    {
        // Создаём переменную x:
        AstNode x = new AstVariableNode("x");

        // Создаём утверждение x > 0
        AssumptionAtom xPositive = AssumptionAtom.Positive(x);       
    }
}
