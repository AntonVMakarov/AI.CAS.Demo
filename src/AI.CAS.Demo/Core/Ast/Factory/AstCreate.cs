using System.Collections.Immutable;
using System.Numerics;

namespace AI.CAS.Core.Ast;

/// <summary>
/// Предоставляет рекомендуемый фасад структурного создания типизированных узлов Ast.
/// Фасад централизует минимальную проверку null, имен и коллекций, а также владеет каноническим структурным
/// уплощением прямых однотипных ассоциативных узлов для Plus, Times, And и Or.
/// Числовые удобные маршруты делегируют <see cref="AstNumberCreate"/>, чтобы инварианты точных рациональных
/// и приближенных вещественных значений оставались в одном слое модели значений и не дублировались в фасаде узлов 
/// выражений. Остальные пути создания остаются структурными: фасад намеренно не парсит строки, не вычисляет выражения, 
/// не удаляет повторы, не сортирует операнды и не применяет алгебраические или логические тождества.
/// </summary>
public static class AstCreate
{
    /// <summary>
    /// Создает числовой узел из готового типизированного числового значения.
    /// </summary>
    /// <param name="number">
    /// Числовое значение Ast.
    /// </param>
    /// <returns>
    /// Новый числовой лист.
    /// </returns>
    public static AstNumberNode NumberNode(AstNumber number)
    {
        // Делегируем проверку null конструктору числового узла.
        return new AstNumberNode(number);
    }


    public static AstNumberNode OneNode() => NumberNode(new AstInteger(1));
    public static AstNumberNode TwoNode() => NumberNode(new AstInteger(2));
    public static AstNumberNode FourNode() => NumberNode(new AstInteger(4));


    /// <summary>
    /// Создает числовой узел с точным целым значением.
    /// Это удобный путь фазы 1 поверх <see cref="AstInteger"/> и <see cref="BigInteger"/>.
    /// </summary>
    /// <param name="integer">
    /// Точное целое значение без ограничения размера.
    /// </param>
    /// <returns>
    /// Новый числовой лист с данными <see cref="AstInteger"/>.
    /// </returns>
    public static AstNumberNode IntegerNode(BigInteger integer)
    {
        // Создаем типизированные данные точного целого числа через единый маршрут числовой фабрики.
        AstInteger number = AstNumberCreate.Integer(integer);

        // Оборачиваем число в лист AST.
        return new AstNumberNode(number);
    }


    /// <summary>
    /// Создает числовой узел с точным рациональным значением.
    /// Метод делегирует нормализацию <see cref="AstNumberCreate.Rational(BigInteger, BigInteger)"/>, поэтому результат
    /// может содержать <see cref="AstInteger"/>, если дробь после сокращения имеет знаменатель один.
    /// </summary>
    /// <param name="numerator">
    /// Числитель точной рациональной дроби.
    /// </param>
    /// <param name="denominator">
    /// Знаменатель точной рациональной дроби; ноль запрещен числовой фабрикой.
    /// </param>
    /// <returns>
    /// Новый числовой лист с каноническими точными числовыми данными.
    /// </returns>
    public static AstNumberNode RationalNode(BigInteger numerator, BigInteger denominator)
    {
        // Создаем каноническое точное числовое значение через числовую фабрику, а не через конструктор AstRational напрямую.
        AstNumber number = AstNumberCreate.Rational(numerator, denominator);

        // Оборачиваем канонические числовые данные в атомарный лист AST.
        return new AstNumberNode(number);
    }


    /// <summary>
    /// Создает числовой узел с приближенным десятичным вещественным значением.
    /// Метод принимает семантические компоненты будущей цели адаптера и не парсит исходную строку вроде <c>1.2500</c>
    /// или <c>1e-30</c>. Нормализация величины и проверка точности принадлежат <see cref="AstNumberCreate"/>.
    /// </summary>
    /// <param name="significand">
    /// Значащая часть приближенной величины.
    /// </param>
    /// <param name="decimalExponent">
    /// Десятичный показатель в формуле <c>significand * 10^decimalExponent</c>.
    /// </param>
    /// <param name="precisionDigits">
    /// Положительное количество заявленных значащих десятичных цифр.
    /// </param>
    /// <returns>
    /// Новый числовой лист с каноническими приближенными числовыми данными.
    /// </returns>
    public static AstNumberNode ApproximateRealNode(BigInteger significand, int decimalExponent, int precisionDigits)
    {
        // Создаем каноническое приближенное значение через числовую фабрику, чтобы нормализация завершающих нулей не дублировалась.
        AstApproximateReal number = AstNumberCreate.ApproximateReal(significand, decimalExponent, precisionDigits);

        // Оборачиваем канонические приближенные числовые данные в атомарный лист AST.
        return new AstNumberNode(number);
    }


    /// <summary>
    /// Создает лист переменной с заданным именем.
    /// </summary>
    /// <param name="name">
    /// Имя переменной, которое должно содержать хотя бы один непробельный символ.
    /// </param>
    /// <returns>
    /// Новый лист переменной.
    /// </returns>
    public static AstVariableNode VarNode(string? name)
    {
        // Создаем переменную через типизированный узел, который проверяет минимальный контракт имени.
        return new AstVariableNode(name);
    }
    

    /// <summary>
    /// Создает атомарную символьную константу <c>pi</c>.
    /// Фасадный метод нужен мосту из старого дерева, чтобы известная константа <c>pi</c> не превращалась в обычную
    /// переменную и не получала десятичное приближение на уровне нейтрального Ast.
    /// </summary>
    /// <returns>
    /// Новый лист <see cref="AstPiNode"/>.
    /// </returns>
    public static AstPiNode PiNode()
    {
        // Создаем конкретный атом pi без дочерних узлов и без численного приближения.
        return new AstPiNode();
    }

    
    /// <summary>
    /// Создает бинарное вычитание.
    /// </summary>
    /// <param name="left">
    /// Уменьшаемое выражение.
    /// </param>
    /// <param name="right">
    /// Вычитаемое выражение.
    /// </param>
    /// <returns>
    /// Новый узел вычитания.
    /// </returns>
    public static AstMinusNode MinusNode(AstNode left, AstNode right)
    {
        // Создаем бинарное вычитание без преобразования в сумму с унарным минусом.
        return new AstMinusNode(left, right);
    }


    /// <summary>
    /// Создает n-арное сложение из массива параметров.
    /// </summary>
    /// <param name="terms">
    /// Два или более слагаемых в исходном порядке.
    /// </param>
    /// <returns>
    /// Новый узел суммы.
    /// </returns>
    public static AstPlusNode PlusNode(params AstNode[] terms)
    {
        // Преобразуем массив параметров в неизменяемую коллекцию с явной проверкой null самого массива.
        ImmutableArray<AstNode> immutableTerms = ToImmutableChildren(terms, nameof(terms));

        // Уплощаем только прямые вложенные суммы, сохраняя исходный порядок слагаемых и не применяя правило x + 0 = x.
        ImmutableArray<AstNode> flattenedTerms = FlattenAssociativeChildren<AstPlusNode>(immutableTerms, nameof(terms));

        // Создаем итоговую сумму через сырой конструктор, потому что каноническая подготовка детей уже выполнена фасадом.
        return new AstPlusNode(flattenedTerms);
    }


    /// <summary>
    /// Создает n-арное сложение из готовой неизменяемой коллекции.
    /// </summary>
    /// <param name="terms">
    /// Два или более слагаемых в исходном порядке.
    /// </param>
    /// <returns>
    /// Новый узел суммы.
    /// </returns>
    public static AstPlusNode PlusNode(ImmutableArray<AstNode> terms)
    {
        // Уплощаем прямые вложенные суммы так же, как перегрузка с массивом параметров, чтобы оба публичных пути совпадали.
        ImmutableArray<AstNode> flattenedTerms = FlattenAssociativeChildren<AstPlusNode>(terms, nameof(terms));

        // Создаем итоговую сумму без дополнительной математической нормализации.
        return new AstPlusNode(flattenedTerms);
    }


    /// <summary>
    /// Создает n-арное умножение из массива параметров.
    /// </summary>
    /// <param name="factors">
    /// Два или более множителей в исходном порядке.
    /// </param>
    /// <returns>
    /// Новый узел произведения.
    /// </returns>
    public static AstTimesNode TimesNode(params AstNode[] factors)
    {
        // Преобразуем массив параметров в неизменяемую коллекцию, сохраняя порядок множителей.
        ImmutableArray<AstNode> immutableFactors = ToImmutableChildren(factors, nameof(factors));

        // Уплощаем только прямые вложенные произведения, не используя правила умножения на ноль или единицу.
        ImmutableArray<AstNode> flattenedFactors = FlattenAssociativeChildren<AstTimesNode>(immutableFactors, nameof(factors));

        // Создаем итоговое произведение из уже подготовленной последовательности множителей.
        return new AstTimesNode(flattenedFactors);
    }


    /// <summary>
    /// Создает n-арное умножение из готовой неизменяемой коллекции.
    /// </summary>
    /// <param name="factors">
    /// Два или более множителей в исходном порядке.
    /// </param>
    /// <returns>
    /// Новый узел произведения.
    /// </returns>
    public static AstTimesNode TimesNode(ImmutableArray<AstNode> factors)
    {
        // Уплощаем прямые вложенные произведения так же, как перегрузка с массивом параметров.
        ImmutableArray<AstNode> flattenedFactors = FlattenAssociativeChildren<AstTimesNode>(factors, nameof(factors));

        // Создаем итоговое произведение без сортировки и без вычисления числовых множителей.
        return new AstTimesNode(flattenedFactors);
    }


    /// <summary>
    /// Создает бинарное деление.
    /// </summary>
    /// <param name="numerator">
    /// Числитель.
    /// </param>
    /// <param name="denominator">
    /// Знаменатель.
    /// </param>
    /// <returns>
    /// Новый узел деления.
    /// </returns>
    public static AstDivNode DivNode(AstNode numerator, AstNode denominator)
    {
        // Создаем деление без сокращения и без проверки знаменателя на ноль.
        return new AstDivNode(numerator, denominator);
    }


    /// <summary>
    /// Создает бинарную степень.
    /// </summary>
    /// <param name="baseNode">
    /// Основание степени.
    /// </param>
    /// <param name="exponent">
    /// Показатель степени.
    /// </param>
    /// <returns>
    /// Новый узел степени.
    /// </returns>
    public static AstPowNode PowNode(AstNode baseNode, AstNode exponent)
    {
        // Создаем степень без правил x^0, x^1 или раскрытия степеней.
        return new AstPowNode(baseNode, exponent);
    }
    

    /// <summary>
    /// Создает точную дробь <c>1/2</c> как числовой лист Ast.
    /// Метод нужен формулам рядов, тригонометрическим интервалам и производным, где половина является точным
    /// рациональным числом, а не десятичным приближением и не старой строковой записью.
    /// </summary>
    /// <returns>
    /// Новый числовой узел с каноническим точным значением <c>1/2</c>.
    /// </returns>
    public static AstNumberNode HalfNode()
    {
        // Создаем точную рациональную половину через числовую фабрику Ast, чтобы сохранить числовую каноничность.
        return RationalNode(BigInteger.One, new BigInteger(2));
    }


    /// <summary>
    /// Создает структурное выражение <c>pi/2</c>.
    /// Фасад не вводит отдельную константу половины пи, потому что базовый Ast хранит только примитивные символические
    /// атомы и операции. Нормализация или специальное распознавание <c>pi/2</c> принадлежит более высокому CAS-слою.
    /// </summary>
    /// <returns>
    /// Узел деления <c>pi/2</c>.
    /// </returns>
    public static AstDivNode HalfPiNode()
    {
        // Создаем pi как символический атом, а не как переменную с именем "pi".
        AstNode numerator = PiNode();

        // Создаем точную целую двойку как знаменатель, чтобы выражение оставалось exact.
        AstNode denominator = IntegerNode(2);

        // Возвращаем структурную дробь pi/2 без вычисления десятичного значения.
        return DivNode(numerator, denominator);
    }
    

    /// <summary>
    /// Создает квадратный корень <c>sqrt(argument)</c> как типизированный радикал.
    /// </summary>
    /// <param name="argument">
    /// Подкоренное выражение.
    /// </param>
    /// <returns>
    /// Новый узел квадратного корня.
    /// </returns>
    public static AstSqrtNode SqrtNode(AstNode argument)
    {
        // Создаем sqrt как конкретный CLR-тип известной математической функции.
        return new AstSqrtNode(argument);
    }


    /// <summary>
    /// Преобразует массив параметров детей в неизменяемую коллекцию.
    /// Метод нужен, чтобы фасад одинаково обрабатывал null-массив и не отдавал наружу изменяемую коллекцию.
    /// </summary>
    /// <param name="children">
    /// Массив детей, переданный методу с параметрами переменной длины.
    /// </param>
    /// <param name="parameterName">
    /// Имя параметра публичного метода фасада.
    /// </param>
    /// <returns>
    /// Неизменяемая коллекция детей в исходном порядке.
    /// </returns>
    private static ImmutableArray<AstNode> ToImmutableChildren(AstNode[]? children, string parameterName)
    {
        // Проверяем сам массив параметров, потому что вызов может явно передать null вместо списка детей.
        if (children is null)
        {
            // Сообщаем имя параметра фасада, на котором возникло отсутствующее значение.
            throw new ArgumentNullException(parameterName);
        }

        // Копируем массив в ImmutableArray, чтобы последующие изменения массива вызывающего кода не меняли AST.
        return ImmutableArray.Create(children);
    }


    /// <summary>
    /// Возвращает детей n-арного ассоциативного узла в канонической структурной форме для публичного фасада
    /// <see cref="AstCreate"/>.
    /// Метод применим только к тем семействам, для которых явно разрешено однотипное ассоциативное
    /// уплощение: <see cref="AstPlusNode"/>, <see cref="AstTimesNode"/>, <see cref="AstAndNode"/> и
    /// <see cref="AstOrNode"/>.
    /// Уплощение является строго структурным: выражение вида Plus(a, Plus(b, c), d) превращается в
    /// Plus(a, b, c, d), но Plus(a, UMinus(Plus(b, c))) остается с внутренней суммой под унарным минусом,
    /// потому что другой CLR-тип образует семантическую границу.
    /// </summary>
    /// <typeparam name="TAssociativeNode">
    /// Точный CLR-тип ассоциативного узла, прямые вложенные экземпляры которого нужно раскрыть в последовательность
    /// детей внешнего узла.
    /// </typeparam>
    /// <param name="children">
    /// Исходные дети внешнего вызова фабрики в порядке, заданном вызывающим кодом.
    /// </param>
    /// <param name="parameterName">
    /// Имя публичного параметра фабрики, которое должно попасть в диагностическое исключение при нарушении
    /// верхнего структурного контракта.
    /// </param>
    /// <returns>
    /// Неизменяемая последовательность детей после раскрытия прямых и транзитивных вложенных узлов того же типа.
    /// Порядок ссылок сохраняется слева направо, а сами дочерние узлы не клонируются и не изменяются.
    /// </returns>
    private static ImmutableArray<AstNode> FlattenAssociativeChildren<TAssociativeNode>(
        ImmutableArray<AstNode> children,
        string parameterName)
        where TAssociativeNode : AstNaryNode
    {
        // Проверяем default-коллекцию до чтения Length, потому что default ImmutableArray не является допустимым списком детей AST.
        if (children.IsDefault)
        {
            // Сообщаем об ошибке как о нарушении контракта аргумента фабрики, сохраняя прежнюю категорию структурной валидации.
            throw new ArgumentException("Коллекция детей n-арного узла AST должна быть явно задана.", parameterName);
        }

        // Проверяем верхнюю арность до уплощения, чтобы AstCreate.PlusNode(Plus(a, b)) не превращался в допустимую сумму.
        if (children.Length < 2)
        {
            // Сохраняем контракт фазы 1: публичный вызов n-арной фабрики обязан иметь минимум два верхних операнда.
            throw new ArgumentException("N-арный узел AST должен иметь как минимум двух верхних детей.", parameterName);
        }

        // Создаем рабочую стековую структуру, чтобы раскрывать глубокую ассоциативную цепочку без рекурсии и риска переполнить стек CLR.
        Stack<AstNode> pendingChildren = new();

        // Загружаем верхних детей справа налево, потому что стек возвращает элементы в обратном порядке добавления.
        for (int index = children.Length - 1; index >= 0; index--)
        {
            // Читаем очередного верхнего ребенка для отдельной проверки null и последующего помещения в стек.
            AstNode child = children[index];

            // Проверяем null на верхнем уровне до обхода, чтобы ошибка оставалась диагностикой недопустимых детей AST.
            if (child is null)
            {
                // Сообщаем имя публичного параметра фабрики, а не внутреннее имя вспомогательного метода.
                throw new ArgumentException("Дети n-арного узла AST не могут быть null.", parameterName);
            }

            // Помещаем ребенка в стек так, чтобы последующее извлечение восстановило исходный порядок слева направо.
            pendingChildren.Push(child);
        }

        // Накапливаем результат в изменяемом локальном списке, потому что многократное склеивание ImmutableArray было бы лишним.
        List<AstNode> flattenedChildren = new(children.Length);

        // Обрабатываем рабочий стек до исчерпания всех прямых и раскрытых однотипных ассоциативных узлов.
        while (pendingChildren.Count > 0)
        {
            // Берем следующий узел в порядке исходного обхода слева направо.
            AstNode current = pendingChildren.Pop();

            // Сравниваем точный CLR-тип, чтобы не пересекать смысловые границы и не раскрывать другие семейства AST.
            if (current.GetType() == typeof(TAssociativeNode))
            {
                // Берем детей вложенного однотипного узла как уже валидную неизменяемую последовательность.
                ImmutableArray<AstNode> nestedChildren = current.Children;

                // Помещаем вложенных детей справа налево, чтобы после извлечения они сохранили свой исходный порядок.
                for (int index = nestedChildren.Length - 1; index >= 0; index--)
                {
                    // Добавляем вложенного ребенка в рабочий стек без клонирования и без обхода через AstCreate.
                    pendingChildren.Push(nestedChildren[index]);
                }

                // Переходим к следующему элементу стека, потому что сам вложенный узел уже заменен его детьми.
                continue;
            }

            // Сохраняем любой узел другого типа как семантическую границу и не заглядываем внутрь его детей.
            flattenedChildren.Add(current);
        }

        // Материализуем ровно одну неизменяемую коллекцию результата, которую затем получает сырой конструктор узла.
        return flattenedChildren.ToImmutableArray();
    }
}