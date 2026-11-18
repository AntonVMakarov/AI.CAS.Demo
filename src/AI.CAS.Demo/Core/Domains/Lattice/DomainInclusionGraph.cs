namespace AI.CAS.Core.Domains;

/// <summary>
/// Описывает небольшую и явно ограниченную решетку включений математических областей.
/// Класс отвечает только на один вопрос: является ли область-источник подобластью целевой области.
/// Он не проверяет принадлежность конкретного выражения области, не анализирует арифметическую форму Ast
/// и не связывает домены с унарными предикатами вроде <c>Integer(x)</c> или <c>Real(x)</c>.
///
/// Такое разделение важно для поэтапного развития механизма предположений. Пока разрешено только
/// безопасное включение между явно заданными фактами <c>DomainMember</c>, например <c>x ∈ Integer</c>
/// может поддержать запрос <c>x ∈ Real?</c> или <c>x ∈ ExtendedReal?</c>. Однако факт унарного предиката
/// <c>Integer(x)</c> все еще не является доменным фактом и не должен обрабатываться этим классом или поставщиком
/// доменной решетки.
///
/// Решетка намеренно использует <see cref="DomainKind"/>, а не <see cref="IDomain.DisplayName"/>.
/// Отображаемое имя предназначено для диагностики и отладки, поэтому оно не должно становиться источником
/// математической истины. Например, две области с одинаковым человекочитаемым именем не обязаны быть
/// одной и той же семантической областью, если их <see cref="DomainKind"/> различается.
/// </summary>
internal sealed class DomainInclusionGraph
{
    private static readonly IReadOnlyDictionary<DomainKind, IReadOnlyList<DomainKind>> DirectSuperdomains =
        new Dictionary<DomainKind, IReadOnlyList<DomainKind>>
        {
            [DomainKind.Natural] = [DomainKind.Integer],
            [DomainKind.Integer] = [DomainKind.Rational],
            [DomainKind.Rational] = [DomainKind.Real],
            [DomainKind.Real] = [DomainKind.Complex, DomainKind.ExtendedReal]
        };


    /// <summary>
    /// Проверяет, является ли область-источник подобластью целевой области.
    /// Направление параметров намеренно отражено в именах: <paramref name="sourceDomain"/> — более узкая
    /// или равная область, из которой приходит известный факт, <paramref name="targetDomain"/> — область,
    /// членство в которой требуется доказать.
    ///
    /// Примеры чтения:
    ///     <c>IsSubdomainOf(Integer, Real)</c> возвращает <c>true</c>, потому что каждое целое число вещественно.
    ///     <c>IsSubdomainOf(Real, Integer)</c> возвращает <c>false</c>, потому что не каждое вещественное число целое.
    ///     <c>IsSubdomainOf(Real, Real)</c> возвращает <c>true</c>, потому что тождественное включение безопасно.
    ///
    /// Числовая решетка ветвится в точке <see cref="DomainKind.Real"/>: конечные вещественные числа
    /// входят и в <see cref="DomainKind.Complex"/>, и в <see cref="DomainKind.ExtendedReal"/>, но комплексная
    /// область и расширенная вещественная область остаются несравнимыми между собой. Это важно, потому что
    /// <c>ExtendedReal</c> содержит направленные бесконечности, а <c>Complex</c> описывает обычные конечные
    /// комплексные числа.
    ///
    /// Области <see cref="DomainKind.Boolean"/>, <see cref="DomainKind.Matrix"/>,
    /// <see cref="DomainKind.PolynomialRing"/>, <see cref="DomainKind.LocalSeries"/> и
    /// <see cref="DomainKind.Unknown"/> остаются вне числовой цепочки. Для них разрешено только тождественное
    /// включение по <see cref="DomainKind"/>, потому что более сильные отношения требуют отдельных доменных
    /// контрактов и не должны появляться как побочный эффект этой небольшой решетки.
    /// </summary>
    /// <param name="sourceDomain">
    /// Область из уже известного явного факта, например <c>Integer</c> в факте <c>x ∈ Integer</c>.
    /// </param>
    /// <param name="targetDomain">
    /// Область из запроса, например <c>Real</c> в запросе <c>x ∈ Real?</c>.
    /// </param>
    /// <returns>
    /// <c>true</c>, если источник является той же областью или безопасной подобластью цели, иначе <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если область-источник или целевая область отсутствует.
    /// </exception>
    public bool IsSubdomainOf(IDomain sourceDomain, IDomain targetDomain)
    {
        ArgumentNullException.ThrowIfNull(sourceDomain);
        ArgumentNullException.ThrowIfNull(targetDomain);

        // Сначала обрабатываем тождественное включение.
        // Это не добавляет нового математического знания: оно только фиксирует, что область всегда
        // является подобластью самой себя. Такой случай безопасен и для числовых областей, и для областей,
        // которые пока специально оставлены вне числовой цепочки, например Matrix или Unknown.
        if (sourceDomain.Kind == targetDomain.Kind)
        {
            return true;
        }

        // Создаём очередь, работающую по приципу FIFO.
        Queue<DomainKind> pending = new Queue<DomainKind>();

        // Создаем hashSet.
        HashSet<DomainKind> visited = new HashSet<DomainKind>();

        // Добавляем объект в очередь.
        pending.Enqueue(sourceDomain.Kind);

        // Цикл работает, пока в очереде есть объекты.
        while (pending.Count > 0)
        {
            // Извлекаем объект из начала очереди.
            DomainKind current = pending.Dequeue();

            if (!visited.Add(current))
            {
                continue;
            }

            if (!DirectSuperdomains.TryGetValue(current, out IReadOnlyList<DomainKind>? superdomains))
            {
                continue;
            }

            foreach (DomainKind superdomain in superdomains)
            {
                // Ветвление решетки выражено именно здесь:
                // Natural -> Integer -> Rational -> Real -> Complex.
                //                                      \
                //                                       -> ExtendedReal.
                // Обход идет только вверх по явно заданным ребрам, поэтому Complex и ExtendedReal остаются
                // несравнимыми. Для них нет ребра ни в одну сторону, и алгоритм не придумывает общего ранга.
                if (superdomain == targetDomain.Kind)
                {
                    return true;
                }

                // Кладем новый объект в очередь.
                pending.Enqueue(superdomain);
            }
        }

        return false;
    }
}
