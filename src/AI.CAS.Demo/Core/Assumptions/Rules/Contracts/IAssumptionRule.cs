using AI.CAS.Core.Cancellations;

namespace AI.CAS.Core.Assumptions;

/// <summary>
/// Интерфейс правил, отвечающих на вопросы движка предположений.
/// </summary>
public interface IAssumptionRule
{
    AssumptionTruthValue Evaluate(
        AssumptionQuery query, 
        AssumptionSet assumptions, 
        IAssumptionEngine engine, 
        IM4uCancellationToken? ct = null);
}
