using AI.CAS.Core.Ast;
using AI.CAS.Core.Cancellations;

namespace AI.CAS.Core.Assumptions;

/// <summary>
/// Правило для ответов на запросы движка предположений по поводу целых чисел.
/// </summary>
internal sealed class IntegerNumberRule : IAssumptionRule
{
    public AssumptionTruthValue Evaluate(AssumptionQuery query, AssumptionSet assumptions, IAssumptionEngine engine, IM4uCancellationToken? ct = null)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(assumptions);

        // У нас субьект - это целое число:
        if ((query.Atom.Subject is AstNumberNode numberNode) && (numberNode.Number is AstInteger number))
        {
            // Если нам требуется доказать утверждение, что:

            // у нас целое положительное число:
            if (query.Atom.Predicate == AssumptionPredicateKind.Positive)
            {
                // Проверяем, если у нас целое число является положительным, тогда возвращаем
                // true. В противном случае оно является отрицательным или равным нулю и тогда ответ false:
                return (number.Integer.Sign > 0) ? AssumptionTruthValue.True : AssumptionTruthValue.False;
            }
            
            // у нас целое отрицательное число:
            if(query.Atom.Predicate == AssumptionPredicateKind.Negative)
            {
                // Проверяем, если у нас целое число является отрицательным, тогда возвращаем
                // true. В противном случае оно является положительным или равным нулю и тогда ответ false:
                return (number.Integer.Sign < 0) ? AssumptionTruthValue.True : AssumptionTruthValue.False;
            }

            // у нас целое число равно нулю:
            if (query.Atom.Predicate == AssumptionPredicateKind.Zero)
            {
                return (number.Integer.IsZero) ? AssumptionTruthValue.True : AssumptionTruthValue.False;
            }

            // у нас целое число не равное нулю:
            if (query.Atom.Predicate == AssumptionPredicateKind.NonZero) 
            {
                return (!number.Integer.IsZero) ? AssumptionTruthValue.True : AssumptionTruthValue.False;
            }

            // у нас целое неположительное (т.е. отрицательное или нуль) число: 
            if(query.Atom.Predicate == AssumptionPredicateKind.NonPositive)
            {
                return ((number.Integer.Sign < 0) || (number.Integer == 0)) ? AssumptionTruthValue.True : AssumptionTruthValue.False;
            }

            // у нас целое неотрицательное (т.е. положительное или нуль) число:
            if(query.Atom.Predicate == AssumptionPredicateKind.NonNegative)
            {
                return ((number.Integer.Sign > 0) || (number.Integer == 0)) ? AssumptionTruthValue.True : AssumptionTruthValue.False;
            }

            // у нас конечное число:
            if (query.Atom.Predicate == AssumptionPredicateKind.Finite) return AssumptionTruthValue.True;

            // у нас не конечное число:
            if (query.Atom.Predicate == AssumptionPredicateKind.NonFinite) return AssumptionTruthValue.False;

            // у нас целое число:
            if (query.Atom.Predicate == AssumptionPredicateKind.Integer) return AssumptionTruthValue.True;
        }

        // Тут - ответ не знаю:
        return AssumptionTruthValue.Unknown;
    }
}
