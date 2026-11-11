using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Результат анализа фрагмента: пошаговые преобразования, итоговое предусловие,
/// условия определённости и триада Хоара
/// </summary>
public class WpResult
{
    public string Steps = "";   // пошаговые преобразования
    public string Wp = "";   // итоговое предусловие (формула)
    public string Defined = "";   // условия определённости
    public string Hoare = "";   // триада Хоара
}
/// <summary>
/// Движок слабейшего предусловия (WP)
/// Логирует шаги и условия определённости в статические StringBuilder
/// </summary>
public static class WpEngine
{
    //накопитель пошагового лога
    private static StringBuilder log;
    //накопитель условий определённости
    private static StringBuilder definedLog;

    /// <summary>
    /// Точка входа: разбирает код и постусловие, считает wp и собирает результат
    /// </summary>
    /// <param name="code">фрагмент программы</param>
    /// <param name="post">постусловие</param>
    /// <returns>объект WpResult с логом, wp, условиями определённости и триадой Хоара</returns>
    public static WpResult Analyze(string code, string post)
    {
        //сброс логов при новом анализе
        log = new StringBuilder();
        definedLog = new StringBuilder();

        log.AppendLine("Анализ фрагмента");

        //парсинг постусловия: разбираем выражение и проверяем, что ввода не осталось
        Parser postParser = new Parser(post);
        Node postNode = postParser.ParseExpression();
        postParser.ExpectEnd();

        //парсинг кода: строим дерево операторов
        Parser codeParser = new Parser(code);
        Stmt program = codeParser.ParseProgram();
        codeParser.ExpectEnd();

        log.AppendLine("Код:  " + code);
        log.AppendLine("Post: " + postNode);
        log.AppendLine();
        log.AppendLine("Считаем wp (с конца к началу):");

        //вычисление wp: начинаем с постусловия и идём от конца к началу
        Node wp = CalculateWp(program, postNode, 0);

        //сборка результата
        WpResult result = new WpResult();
        result.Steps = log.ToString();
        result.Wp = wp.ToString();
        //если ограничений не было то пишем об этом
        result.Defined = definedLog.Length == 0 ? "Ограничений определённости нет" : definedLog.ToString();
        result.Hoare = "{ " + wp + " }  " + code + "  { " + post + " }";
        return result;
    }
    /// <summary>
    /// Основная логика движка: вычисляет wp для оператора относительно постусловия
    /// </summary>
    /// <param name="stmt">текущий оператор программы</param>
    /// <param name="post">условие, которое должно быть истинно после выполнения этого оператора</param>
    /// <param name="depth">глубина вложенности для форматирования вывода</param>
    /// <returns>дерево вычисленного предусловия</returns>
    private static Node CalculateWp(Stmt stmt, Node post, int depth)
    {
        //отступ из пробелов в зависимости от вложенности
        string indent = new string(' ', depth * 2);

        //Присваивание
        if (stmt is Assign a)
        {
            //в постусловии заменяем переменную на правую часть присваивания
            Node result = Substitute(post, a.Name, a.Expression);
            //проверяем определённость выражения (деление на 0, корень из отрицательного)
            Node defined = Defined(a.Expression);
            //если ограничение есть — добавляем его через логическое И
            if (defined != null)
            {
                result = And(defined, result);
                definedLog.AppendLine(defined.ToString());
            }
            log.AppendLine(indent + "Присваивание " + a.Name + " := " + a.Expression);
            log.AppendLine(indent + "  wp = " + result);
            return result;
        }

        //Последовательность операторов — идём с конца к началу
        if (stmt is Sequence s)
        {
            Node current = post;
            for (int i = s.Statements.Count - 1; i >= 0; i--)
            {
                log.AppendLine(indent + "Оператор " + (i + 1) + " с конца:");
                current = CalculateWp(s.Statements[i], current, depth + 1);
            }
            return current;
        }

        //Ветвление if-else по правилу Хоара
        if (stmt is Conditional c)
        {
            log.AppendLine(indent + "Ветвление if (" + c.Condition + ")");
            //wp для ветки then
            Node thenWp = CalculateWp(c.Then, post, depth + 1);
            //wp для ветки else
            Node elseWp = CalculateWp(c.Else, post, depth + 1);
            //соединение веток: (C && wp_then) || (!C && wp_else)
            Node result = Or(And(c.Condition, thenWp), And(Not(c.Condition), elseWp));
            //ограничение определённости самого условия
            Node conditionDefined = Defined(c.Condition);
            if (conditionDefined != null)
            {
                result = And(conditionDefined, result);
                definedLog.AppendLine(conditionDefined.ToString());
            }
            return result;
        }

        throw new InvalidOperationException("Неизвестный тип оператора.");
    }

    /// <summary>
    /// В дереве заменяет переменную name на дерево replacement
    /// </summary>
    /// <param name="node">дерево, в котором ищется переменная</param>
    /// <param name="name">имя переменной для замены</param>
    /// <param name="replacement">дерево выражения, которым она заменяется</param>
    private static Node Substitute(Node node, string name, Node replacement)
    {
        //если узел - идентификатор с нужным именем, возвращаем замену
        if (node.Kind == "id" && node.Value == name) return replacement;
        //листья возвращаем без изменений
        if (node.Children.Count == 0) return node;
        //иначе рекурсивно обходим детей и строим новый узел
        return new Node(node.Kind, node.Value,
            node.Children.Select(c => Substitute(c, name, replacement)).ToList());
    }

    /// <summary>
    /// Проверка определённости выражения
    /// </summary>
    /// <param name="expression">выражение</param>
    /// <returns>null, если выражение определённое, иначе дерево ограничений</returns>
    private static Node Defined(Node expression)
    {
        //список найденных ограничений
        List<Node> conditions = new List<Node>();
        //рекурсивный поиск неопределённостей
        CollectDefinedness(expression, conditions);
        if (conditions.Count == 0) return null;
        //объединяем ограничения через логическое И
        return conditions.Aggregate(And);
    }

    /// <summary>
    /// Рекурсивно собирает ограничения определённости: деление и sqrt
    /// </summary>
    private static void CollectDefinedness(Node n, List<Node> conditions)
    {
        //сначала обходим детей
        foreach (Node child in n.Children) CollectDefinedness(child, conditions);
        //деление: знаменатель != 0
        if (n.Kind == "binary" && n.Value == "/")
            conditions.Add(new Node("binary", "!=", new List<Node> { n.Children[1], Num("0") }));
        //корень: аргумент >= 0
        if (n.Kind == "call" && n.Value == "sqrt")
            conditions.Add(new Node("binary", ">=", new List<Node> { n.Children[0], Num("0") }));
    }

    //создаёт узел числа
    private static Node Num(string value) { return new Node("number", value); }
    //создаёт узел логического И
    private static Node And(Node a, Node b) { return new Node("binary", "&&", new List<Node> { a, b }); }
    //создаёт узел логического ИЛИ
    private static Node Or(Node a, Node b) { return new Node("binary", "||", new List<Node> { a, b }); }
    //создаёт узел отрицания
    private static Node Not(Node a) { return new Node("unary", "!", new List<Node> { a }); }
}


/// <summary>
/// Класс для хранения узлов дерева выражений
/// sealed — не наследуется
/// </summary>
internal sealed class Node
{
    //вид узла: "number", "id", "binary", "unary", "call", "bool"
    public string Kind { get; }
    //значение узла (имя переменной, оператор, число)
    public string Value { get; }
    //дочерние узлы (операнды)
    public List<Node> Children { get; }

    /// <summary>
    /// Конструктор узла
    /// </summary>
    /// <param name="kind">тип узла</param>
    /// <param name="value">значение</param>
    /// <param name="children">список дочерних узлов, если есть</param>
    public Node(string kind, string value, List<Node> children = null)
    {
        Kind = kind;
        Value = value;
        //если children не передан — создаём пустой список
        Children = children ?? new List<Node>();
    }

    public override string ToString()
    {
        //узел у которых нет дочерних узлов числа, идентификаторы, bool
        if (Kind == "id" || Kind == "number" || Kind == "bool") return Value;
        //вызов функции
        if (Kind == "call") return Value + "(" + Children[0] + ")";
        //унарный оператор: ! или -
        if (Kind == "unary") return Value == "!" ? "!(" + Children[0] + ")" : "-" + Children[0];
        //бинарный оператор: (левый операнд, оператор, правый операнд)
        if (Kind == "binary") return "(" + Children[0] + " " + Value + " " + Children[1] + ")";
        return Value;
    }
}

//abstract — экземпляры создавать нельзя
//record — хранение данных с поддержкой сравнения и копирования
internal abstract record Stmt;
//присваивание: имя := выражение
internal sealed record Assign(string Name, Node Expression) : Stmt;
//последовательность операторов
internal sealed record Sequence(List<Stmt> Statements) : Stmt;
//ветвление if-else
internal sealed record Conditional(Node Condition, Stmt Then, Stmt Else) : Stmt;

/// <summary>
/// Парсер: читает строку и строит объекты Node и Stmt
/// </summary>
internal sealed class Parser
{
    //список токенов
    private readonly List<string> tokens;
    //текущая позиция в списке токенов
    private int position;
    //текущий токен или <EOF>
    private string Current { get { return position < tokens.Count ? tokens[position] : "<EOF>"; } }

    /// <summary>
    /// Конструктор: разбивает входную строку на токены регулярным выражением
    /// </summary>
    /// <param name="input">исходный текст</param>
    public Parser(string input)
    {
        //регулярка выделяет числа, идентификаторы и операторы
        tokens = Regex.Matches(input,
            @"\s*(?:(\d+(?:\.\d+)?)|([A-Za-z_][A-Za-z0-9_]*)|(:=|==|!=|<=|>=|&&|\|\||[{}();,+*/<>!-]))")
            .Select(m => m.Groups[1].Success ? m.Groups[1].Value
                       : m.Groups[2].Success ? m.Groups[2].Value
                       : m.Groups[3].Value).ToList();
        //проверка: без пробелов исходник должен совпасть со склейкой токенов
        string compact = Regex.Replace(input, @"\s+", "");
        if (compact != string.Concat(tokens))
            throw new FormatException("Не удалось разобрать ввод.");
    }

    /// <summary>
    /// Проверяет, что весь ввод разобран (не осталось лишних токенов)
    /// </summary>
    public void ExpectEnd()
    {
        if (Current != "<EOF>") throw new FormatException("Лишний токен: " + Current);
    }

    //взять текущий токен и сдвинуть позицию
    private string Take()
    {
        string t = Current;
        if (t == "<EOF>") throw new FormatException("Неожиданный конец ввода.");
        position++;
        return t;
    }
    //если текущий токен совпадает — сдвинуть и вернуть true
    private bool Match(string token) { if (Current != token) return false; position++; return true; }
    //требовать наличие токена
    private void Need(string token) { if (!Match(token)) throw new FormatException("Ожидался '" + token + "', получено '" + Current + "'."); }

    /// <summary>
    /// Разбирает программу: последовательность операторов
    /// </summary>
    public Stmt ParseProgram()
    {
        List<Stmt> list = new List<Stmt>();
        while (Current != "<EOF>" && Current != "}")
        {
            list.Add(ParseStatement());
            //пропускаем точки с запятой
            if (Current == ";") { while (Match(";")) { } }
            //если не конец и не }, нужна точка с запятой
            else if (Current != "<EOF>" && Current != "}")
                throw new FormatException("Нужна точка с запятой.");
        }
        if (list.Count == 0) throw new FormatException("Код пуст.");
        //один оператор возвращаем как есть, несколько — как Sequence
        return list.Count == 1 ? list[0] : new Sequence(list);
    }

    /// <summary>
    /// Разбирает один оператор: if-else или присваивание
    /// </summary>
    private Stmt ParseStatement()
    {
        if (Match("if"))
        {
            Need("("); Node condition = ParseExpression(); Need(")");
            Need("{"); Stmt thenStmt = ParseProgram(); Need("}");
            Need("else"); Need("{"); Stmt elseStmt = ParseProgram(); Need("}");
            return new Conditional(condition, thenStmt, elseStmt);
        }
        //имя переменной
        string name = Take();
        if (!Regex.IsMatch(name, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            throw new FormatException("Ожидалось имя переменной.");
        Need(":="); Node expr = ParseExpression();
        return new Assign(name, expr);
    }

    /// <summary>
    /// Разбирает выражение методом Пратта (учёт приоритетов операций)
    /// </summary>
    /// <param name="minPrecedence">минимальный приоритет для текущего уровня</param>
    public Node ParseExpression(int minPrecedence = 0)
    {
        Node left;
        //унарный !
        if (Match("!")) left = new Node("unary", "!", new List<Node> { ParseExpression(7) });
        //унарный -
        else if (Match("-")) left = new Node("unary", "-", new List<Node> { ParseExpression(7) });
        //выражение в скобках
        else if (Match("(")) { left = ParseExpression(); Need(")"); }
        else
        {
            string token = Take();
            //число
            if (double.TryParse(token, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out _))
                left = new Node("number", token);
            //идентификатор или вызов функции
            else if (Regex.IsMatch(token, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                if ((token == "abs" || token == "sqrt") && Match("("))
                { Node arg = ParseExpression(); Need(")"); left = new Node("call", token, new List<Node> { arg }); }
                else if (token == "true" || token == "false") left = new Node("bool", token);
                else left = new Node("id", token);
            }
            else throw new FormatException("Неожиданный токен: " + token);
        }
        //обрабатываем бинарные операторы по приоритету
        while (Precedence(Current) >= minPrecedence && Precedence(Current) >= 0)
        {
            string op = Take();
            Node right = ParseExpression(Precedence(op) + 1);
            left = new Node("binary", op, new List<Node> { left, right });
        }
        return left;
    }

    /// <summary>
    /// Приоритет оператора: чем больше число, тем выше приоритет
    /// </summary>
    private static int Precedence(string op)
    {
        switch (op)
        {
            case "||": return 1;
            case "&&": return 2;
            case "==": case "!=": case "<": case ">": case "<=": case ">=": return 3;
            case "+": case "-": return 4;
            case "*": case "/": return 5;
            default: return -1;
        }
    }
}