using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// Учебный символьный движок слабейшего предусловия (WP), .NET 8.
// Поддерживает присваивания, последовательности, if-else, + - * /, унарный минус,
// сравнения, &&, ||, !, abs(x), sqrt(x). Циклы намеренно не поддерживаются.

internal static class Program
{
    private static void Main()
    {
        //Console.OutputEncoding = System.Text.Encoding.UTF8;
        RunPreset("1. Максимум из двух чисел",
            "if (x1 >= x2) { max := x1; } else { max := x2; }",
            "max >= x1 && max >= x2");

        RunPreset("2. Квадратное уравнение (один корень)",
            "if (D >= 0 && a != 0) { x1 := (-b + sqrt(D)) / (2 * a); } else { x1 := -999; }",
            "(D >= 0 && a != 0 && a * x1 * x1 + b * x1 + c == 0) || ((D < 0 || a == 0) && x1 == -999)");

        RunPreset("3. Последовательность присваиваний",
            "x := x + 10; y := x + 1;",
            "y == x + 1 && x > 15");

        Console.WriteLine("\n=== ПРОВЕРКА ПРИМЕРОВ WP ===");
        RunSelfTests();

        Console.WriteLine("\nВведите собственный фрагмент (пустая строка завершает ввод).");
        Console.WriteLine("Формат: x := x + 1; y := x * 2;  ИЛИ  if (x > 0) { y := x; } else { y := 0; }");
        Console.Write("Код: ");
        string? code = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(code)) return;
        Console.Write("Постусловие (Post): ");
        string? post = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(post)) return;
        try { Analyze("Пользовательский фрагмент", code, post); }
        catch (Exception ex) { Console.WriteLine($"Ошибка: {ex.Message}"); }
    }

    private static void RunPreset(string title, string code, string post)
    {
        try { Analyze(title, code, post); }
        catch (Exception ex) { Console.WriteLine($"Ошибка в пресете «{title}»: {ex.Message}"); }
    }

    private static void Analyze(string title, string code, string postText)
    {
        //вывод заголовка
        Console.WriteLine("\n" + new string('=', 76));
        Console.WriteLine(title);
        Console.WriteLine(new string('=', 76));

        //для пост
        var postParser = new Parser(postText);
        //парсер разбирает выражение и возвращает объект Node
        //объект Node это корень дерева выражения
        Node post = postParser.ParseExpression();
        //проверяет что парсер обработал весь код, чтобы не было ошибок
        postParser.ExpectEnd();
        //для кода
        var codeParser = new Parser(code);
        //Stmt это абстрактное представление оператора или последовательности операторов
        Stmt program = codeParser.ParseProgram();
        codeParser.ExpectEnd();

        Console.WriteLine($"Фрагмент: {code}");
        Console.WriteLine($"Post:     {post}");
        Console.WriteLine("\nПошаговое вычисление wp (с конца к началу):");
        //возвращает дерево нового условия wp
        Node wp = CalculateWp(program, post, 0);
        Console.WriteLine("\nИтоговое предусловие wp:");
        Console.WriteLine(wp);
        Console.WriteLine("\nПояснение: входные значения должны удовлетворять этому условию, чтобы после выполнения фрагмента постусловие было истинно.");
        Console.WriteLine("Триада Хоара:");
        Console.WriteLine($"{{ {wp} }}  {code}  {{ {post} }}");
    }

    /// <summary>
    /// Основная логика движка
    /// </summary>
    /// <param name="stmt">текущий оператор программы</param>
    /// <param name="post">условие, которое должно быть истинно после выполнения этого оператора</param>
    /// <param name="depth">глубина вложенности для форматирования вывода</param>
    /// <returns>дерево вычисленного предусловия</returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static Node CalculateWp(Stmt stmt, Node post, int depth)
    {
        //создает отступ из пробелов в зависимости от вложенности операторов
        //нужно чтобы пошаговый вывод показывал вложенность операторов
        string indent = new string(' ', depth * 2);
        switch (stmt)
        {
            //Присваивание
            //a это переменная, через которую код получает доступ к присваиванию
            //a.Name - имя переменной
            //a.Expression - дерево выражений
            case Assign a:
            {
                //В пост условии заменяет переменную на то что находится справа от присваивания :=
                Node result = Substitute(post, a.Name, a.Expression);
                //Проверяет условие определенности, чтобы не было деления на 0 или корня из -1
                Node defined = Defined(a.Expression);
                //если условие определенности есть то это оператор И
                if (defined != null) result = And(defined, result);
                    //вывод присваивания
                    Console.WriteLine($"{indent}Через присваивание {a.Name} := {a.Expression}:");
                    //вывод текущего шага
                    Console.WriteLine($"{indent}  wp = {result}");
                return result;
            }//Последовательность операторов
            case Sequence s:
            {
                //начать с постусловия
                Node current = post;
                //s.Statement - список операторов, из которых состоит последовательность
                //цикл проходит операторов в обратном порядке
                for (int i = s.Statements.Count - 1; i >= 0; i--)
                {
                    //номер текущего шага
                    Console.WriteLine($"{indent}Оператор {i + 1} с конца:");
                    //передает текущий оператор и текущее условие в себя + глубина
                    current = CalculateWp(s.Statements[i], current, depth + 1);
                }
                return current;
            }//Ветвление if else
            //В c находится условие и 2 ветки
            case Conditional c:
            {
                //вывод условие ветвления
                Console.WriteLine($"{indent}Ветвление if ({c.Condition}):");
                //вычисление предусловия для ветки then
                Node thenWp = CalculateWp(c.Then, post, depth + 1);
                //вычисление предусловия для ветки else
                Node elseWp = CalculateWp(c.Else, post, depth + 1);
                //соединение двух веток по правилу Хоара
                Node result = Or(And(c.Condition, thenWp), And(Not(c.Condition), elseWp));
                //Проверяет условие определенности, чтобы не было деления на 0 или корня из -1 
                Node? conditionDefined = Defined(c.Condition);
                //если условие определенности есть то это оператор И
                if (conditionDefined != null) result = And(conditionDefined, result);
                //вывод правило ветвления
                Console.WriteLine($"{indent}  wp(if) = ({c.Condition} && ({thenWp})) || (!( {c.Condition} ) && ({elseWp}))");
                //вывод ограничения определенности
                if (conditionDefined != null) Console.WriteLine($"{indent}  Ограничения определённости условия: {conditionDefined}");
                return result;
            }
            default: throw new InvalidOperationException("Неизвестный тип оператора.");
        }
    }
    /// <summary>
    /// В пост условии заменяет переменную на то что находится справа от присваивания := 
    /// </summary>
    /// <param name="node">дерево в котором ищется переменная</param>
    /// <param name="name">имя переменной для замены</param>
    /// <param name="replacement">дерево выражения которым она заменяется</param>
    /// <returns></returns>
    private static Node Substitute(Node node, string name, Node replacement)
    {
        //Если текущий узел является индетификатором и его имя совпадает с переданным name, возвращается дерево замены
        if (node.Kind == "id" && node.Value == name) return replacement;
        //Если у узла нет дочерних узлов, то он возвращается без изменений
        if (node.Children.Count == 0) return node;
        //Создает новый узел того же вида и с тем же оператором, но его дочерние узлы обрабатываются Substitute() рекурсией
        //Select() обрабатывает каждый дочерний узел
        //ToList() собрает результаты в список
        return new Node(node.Kind, node.Value, node.Children.Select(c => Substitute(c, name, replacement)).ToList());
    }

    /// <summary>
    /// проверка на определенности выражения
    /// </summary>
    /// <param name="expression">выражение</param>
    /// <returns>null, если выражение определенное, иначе дерево ограничений</returns>
    private static Node? Defined(Node expression)
    {
        //создание пустого списка условий
        var conditions = new List<Node>();
        //рекурсивный поиск возможный неопределенностей внутри выражения
        CollectDefinedness(expression, conditions);
        if (conditions.Count == 0) return null;
        //объединение строк в оператор И
        return conditions.Aggregate(And);
    }

    /// <summary>
    /// проверка условия неопределенности
    /// </summary>
    /// <param name="n"></param>
    /// <param name="conditions"></param>
    private static void CollectDefinedness(Node n, List<Node> conditions)
    {
        //перебирает дочерние узлы и рекурсивно проеряет
        foreach (var child in n.Children) CollectDefinedness(child, conditions);
        //является ли узел операцией деления
        //binary означает что оператор имеет два операнда справа и слева
        if (n.Kind == "binary" && n.Value == "/")
            //если обнаружено деление то в список добавляем условие
            //знаменатель != 0
            //n.Children[1] правый опренад - знаменатель
            //Num("0") создание узла числа 0
            conditions.Add(new Node("binary", "!=", new List<Node> { n.Children[1], Num("0") }));
        //является ли узел операцией корня
        if (n.Kind == "call" && n.Value == "sqrt")
            //добавление ограничения число >= 0, где число хранится в n.Children[0]
            conditions.Add(new Node("binary", ">=", new List<Node> { n.Children[0], Num("0") }));
    }
    /// <summary>
    /// Создает узел числа
    /// </summary>
    /// <param name="value">число для которого создаётся узел</param>
    /// <returns>объект Node, где Knid = "number", а  Value = переданное число</returns>
    private static Node Num(string value) => new("number", value);
    /// <summary>
    /// создает узел логического И между дочерними элементами
    /// </summary>
    /// <param name="a">дочерний элемент 1</param>
    /// <param name="b">дочерний элемент 2</param>
    /// <returns></returns>
    private static Node And(Node a, Node b) => new("binary", "&&", new List<Node> { a, b });
    /// <summary>
    /// создает узел логического ИЛИ между дочерними элементами
    /// </summary>
    /// <param name="a">дочерний элемент 1</param>
    /// <param name="b">дочерний элемент 2</param>
    /// <returns></returns>
    private static Node Or(Node a, Node b) => new("binary", "||", new List<Node> { a, b });
    /// <summary>
    /// создает узел отрицания
    /// </summary>
    /// <param name="a">элемент для отрицания</param>
    /// <returns></returns>
    private static Node Not(Node a) => new("unary", "!", new List<Node> { a });

    //бесполезные тесты чтобы просто были
    private static void RunSelfTests()
    {
        //проверяем ожидаемые символьные результаты для базовых правил
        Check("Последовательность", "x := x + 1; y := x * 2;", "y > 20 && x < 50",
            "(((x + 1) * 2) > 20 && (x + 1) < 50)");
        Check("Ветвление", "if (x > 0) { y := x; } else { y := 0; }", "y >= 0",
            "((x > 0 && x >= 0) || (!(x > 0) && 0 >= 0))");
        Check("Деление", "x := a / b;", "x > 1", "(b != 0 && (a / b) > 1)");
    }
    //тоже надо убрать
    private static void Check(string title, string code, string post, string expected)
    {
        try
        {
            var pp = new Parser(post); Node p = pp.ParseExpression(); pp.ExpectEnd();
            var cp = new Parser(code); Stmt s = cp.ParseProgram(); cp.ExpectEnd();
            TextCapture.Begin(); Node actual = CalculateWp(s, p, 0); TextCapture.End();
            bool ok = Normalize(actual.ToString()) == Normalize(expected);
            Console.WriteLine($"{(ok ? "PASS" : "CHECK")}: {title}");
            if (!ok) Console.WriteLine($"  Получено: {actual}\n  Ожидалось: {expected}");
        }
        catch (Exception ex) { TextCapture.End(); Console.WriteLine($"FAIL: {title}: {ex.Message}"); }
    }
    /// <summary>
    /// убирает различие в пробелах и круглых скобках перед сравнением строк
    /// </summary>
    /// <param name="s">строка для обработки</param>
    /// <returns>строка где убраны все последовательности пробельных символов</returns>
    private static string Normalize(string s) => Regex.Replace(s, @"\s+", "").Replace("(", "").Replace(")", "");
}

/// <summary>
/// класс чтобы хранить выражения
/// sealed - ненаследуется
/// </summary>
internal sealed class Node
{
    //вид узла
    /// <summary>
    /// "number" - число
    /// "id" - имя переменной
    /// "binary" - бинарная операция
    /// "unary" - унарная операция
    /// "call" - вызов фукнции
    /// </summary>
    public string Kind { get; }
    public string Value { get; }
    //содержит дочерние узлы, то что справа и слева от операции +
    public List<Node> Children { get; }
    /// <summary>
    /// Конструктр
    /// </summary>
    /// <param name="kind">тип узла</param>
    /// <param name="value">его значение</param>
    /// <param name="children">список дочерних узлов если есть</param>
    public Node(string kind, string value, List<Node>? children = null)
    {    
        Kind = kind;
        //может быть имя фукнции
        Value = value;
        //если он не равен null то ниче, иначе создать пустой список
        //в children может находится аргумент фукнции
        Children = children ?? new List<Node>();
    }
    public override string ToString()
    {
        if (Kind is "id" or "number") return Value;
        //для вызова функции sqrt()
        if (Kind == "call") return $"{Value}({Children[0]})";
        //проверка унарный оператор это ! или -
        if (Kind == "unary") return Value == "!" ? $"!({Children[0]})" : $"-{Children[0]}";
        //Дя бинарного оператора собирается выражение из левого операнда, оператора и правого операнда
        if (Kind == "binary") return $"({Children[0]} {Value} {Children[1]})";
        //если узел не попал в предыдущие проверки
        return Value;
    }
}

/// <summary>
/// abstract - создавать экземпляры класса нельзя
/// record - хранение данных с поддержкой ряда операций сравнения и копирования
/// </summary>
internal abstract record Stmt;
//присваивание
internal sealed record Assign(string Name, Node Expression) : Stmt;
/// <summary>
/// последовательность операторов
/// </summary>
/// <param name="Statements">содержит 2 объекта Assign</param>
internal sealed record Sequence(List<Stmt> Statements) : Stmt;
/// <summary>
/// для if else
/// </summary>
/// <param name="Condition">дерево условия</param>
/// <param name="Then">оператор или последовательность операторов истинного условия</param>
/// <param name="Else">оператор или последовательность операторов ложного условия</param>
internal sealed record Conditional(Node Condition, Stmt Then, Stmt Else) : Stmt;

//читает символы и строит объекты Node и Stmt
internal sealed class Parser
{
    private readonly List<string> _tokens;
    private int _position;
    private string Current => _position < _tokens.Count ? _tokens[_position] : "<EOF>";
    public Parser(string input)
    {
        _tokens = Regex.Matches(input, @"\s*(?:(\d+(?:\.\d+)?)|([A-Za-z_][A-Za-z0-9_]*)|(:=|==|!=|<=|>=|&&|\|\||[{}();,+*/<>!-]))")
            .Select(m => m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Success ? m.Groups[2].Value : m.Groups[3].Value).ToList();
        string compact = Regex.Replace(input, @"\s+", "");
        string tokenCompact = string.Concat(_tokens);
        if (compact != tokenCompact) throw new FormatException("Не удалось разобрать часть ввода. Проверьте символы и синтаксис.");
    }
    public void ExpectEnd() { if (Current != "<EOF>") throw new FormatException($"Лишний токен: {Current}"); }
    private string Take() { string t = Current; if (t == "<EOF>") throw new FormatException("Неожиданный конец ввода."); _position++; return t; }
    private bool Match(string token) { if (Current != token) return false; _position++; return true; }
    private void Need(string token) { if (!Match(token)) throw new FormatException($"Ожидался токен '{token}', получено '{Current}'."); }

    public Stmt ParseProgram()
    {
        var list = new List<Stmt>();
        while (Current != "<EOF>" && Current != "}")
        {
            list.Add(ParseStatement());
            if (Current == ";")
            {
                while (Match(";")) { }
            }
            else if (Current != "<EOF>" && Current != "}")
            {
                throw new FormatException($"Между операторами нужна точка с запятой. Получено: '{Current}'.");
            }
        }
        if (list.Count == 0) throw new FormatException("Фрагмент программы пуст.");
        return list.Count == 1 ? list[0] : new Sequence(list);
    }
    private Stmt ParseStatement()
    {
        if (Match("if"))
        {
            Need("("); Node condition = ParseExpression(); Need(")");
            Need("{"); Stmt thenStmt = ParseProgram(); Need("}");
            Need("else"); Need("{"); Stmt elseStmt = ParseProgram(); Need("}");
            return new Conditional(condition, thenStmt, elseStmt);
        }
        string name = Take();
        if (!Regex.IsMatch(name, @"^[A-Za-z_][A-Za-z0-9_]*$")) throw new FormatException($"Ожидалось имя переменной, получено '{name}'.");
        Need(":="); Node expr = ParseExpression();
        return new Assign(name, expr);
    }

    public Node ParseExpression(int minPrecedence = 0)
    {
        Node left;
        if (Match("!")) left = new Node("unary", "!", new List<Node> { ParseExpression(7) });
        else if (Match("-")) left = new Node("unary", "-", new List<Node> { ParseExpression(7) });
        else if (Match("(")) { left = ParseExpression(); Need(")"); }
        else
        {
            string token = Take();
            if (double.TryParse(token, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _)) left = new Node("number", token);
            else if (Regex.IsMatch(token, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                if ((token == "abs" || token == "sqrt") && Match("(")) { Node arg = ParseExpression(); Need(")"); left = new Node("call", token, new List<Node> { arg }); }
                else if (token is "true" or "false") left = new Node("bool", token);
                else left = new Node("id", token);
            }
            else throw new FormatException($"Неожиданный токен в выражении: '{token}'.");
        }
        while (Precedence(Current) >= minPrecedence && Precedence(Current) >= 0)
        {
            string op = Take(); int precedence = Precedence(op);
            Node right = ParseExpression(precedence + 1);
            left = new Node("binary", op, new List<Node> { left, right });
        }
        return left;
    }
    private static int Precedence(string op) => op switch
    {
        "||" => 1, "&&" => 2, "==" or "!=" or "<" or ">" or "<=" or ">=" => 3,
        "+" or "-" => 4, "*" or "/" => 5, _ => -1
    };
}

// Перехватчик нужен, чтобы автотесты не печатали подробный трейс повторно.
internal static class TextCapture
{
    private static System.IO.TextWriter? _original;
    public static void Begin() { _original = Console.Out; Console.SetOut(System.IO.TextWriter.Null); }
    public static void End() { if (_original != null) { Console.SetOut(_original); _original = null; } }
}
