using AI.CAS.Core.Ast;
using AI.CAS.Core.Cancellations;

namespace AI.CAS.Core.Assumptions;

/// <summary>
/// Правило для ответов на запросы движка предположений по поводу целых чисел.
/// </summary>
internal sealed class IntegerNumberRule : IAssumptionRule
{
    /// <summary>
    /// Уникальный идентификатор правила.
    /// </summary>
    public override string RuleId => "integer.number.rule";


    /// <summary>
    /// Выполняет правило.
    /// </summary>
    /// <param name="query">Запрос на проверку предположения.</param>
    /// <param name="assumptions">Набор истин.</param>
    /// <param name="ct">Необязательный токен отмены.</param>
    /// <returns>
    /// Результат работы правила.
    /// </returns>
    public override AssumptionRuleResult Evaluate(AssumptionQuery query, AssumptionSet assumptions, IM4uCancellationToken? ct = null)
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
                return 
                    AssumptionRuleResult.Proved(
                        (number.Integer.Sign > 0) ? AssumptionTruthValue.True : AssumptionTruthValue.False,
                        RuleId);
            }
            
            // у нас целое отрицательное число:
            if(query.Atom.Predicate == AssumptionPredicateKind.Negative)
            {
                // Проверяем, если у нас целое число является отрицательным, тогда возвращаем
                // true. В противном случае оно является положительным или равным нулю и тогда ответ false:
                return
                    AssumptionRuleResult.Proved(
                        (number.Integer.Sign < 0) ? AssumptionTruthValue.True : AssumptionTruthValue.False,
                        RuleId);
            }

            // у нас целое число равно нулю:
            if (query.Atom.Predicate == AssumptionPredicateKind.Zero)
            {
                return
                    AssumptionRuleResult.Proved(
                        (number.Integer.IsZero) ? AssumptionTruthValue.True : AssumptionTruthValue.False,
                        RuleId);
            }

            // у нас целое число не равное нулю:
            if (query.Atom.Predicate == AssumptionPredicateKind.NonZero) 
            {
                return 
                    AssumptionRuleResult.Proved(
                        (!number.Integer.IsZero) ? AssumptionTruthValue.True : AssumptionTruthValue.False, 
                        RuleId);
            }

            // у нас целое неположительное (т.е. отрицательное или нуль) число: 
            if(query.Atom.Predicate == AssumptionPredicateKind.NonPositive)
            {
                return 
                    AssumptionRuleResult.Proved(
                        ((number.Integer.Sign < 0) || (number.Integer == 0)) ? AssumptionTruthValue.True : AssumptionTruthValue.False, 
                        RuleId);
            }

            // у нас целое неотрицательное (т.е. положительное или нуль) число:
            if(query.Atom.Predicate == AssumptionPredicateKind.NonNegative)
            {
                return 
                    AssumptionRuleResult.Proved(
                        ((number.Integer.Sign > 0) || (number.Integer == 0)) ? AssumptionTruthValue.True : AssumptionTruthValue.False, 
                        RuleId);
            }

            // у нас конечное число:
            if (query.Atom.Predicate == AssumptionPredicateKind.Finite) return AssumptionRuleResult.Proved(AssumptionTruthValue.True, RuleId);

            // у нас не конечное число:
            if (query.Atom.Predicate == AssumptionPredicateKind.NonFinite) return AssumptionRuleResult.Proved(AssumptionTruthValue.False, RuleId);

            // у нас целое число:
            if (query.Atom.Predicate == AssumptionPredicateKind.Integer) return AssumptionRuleResult.Proved(AssumptionTruthValue.True, RuleId);
        }

        // Тут - ответ не знаю:
        return AssumptionRuleResult.NoAnswer(RuleId);
    }
}
