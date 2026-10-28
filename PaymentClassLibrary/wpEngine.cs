using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class WpResult
{
    public string Steps = "";   // пошаговые преобразования
    public string Wp = "";   // итоговое предусловие (формула)
    public string Defined = "";   // условия определённости
    public string Hoare = "";   // триада Хоара
}

public static class WpEngine
{
    private static StringBuilder log;
    private static StringBuilder definedLog;

    public static WpResult Analyze(string code, string post)
    {
        log = new StringBuilder();
        definedLog = new StringBuilder();

        log.AppendLine("Анализ фрагмента");

        Parser postParser = new Parser(post);
        Node postNode = postParser.ParseExpression();
        postParser.ExpectEnd();

        Parser codeParser = new Parser(code);
        Stmt program = codeParser.ParseProgram();
        codeParser.ExpectEnd();

        log.AppendLine("Код:  " + code);
        log.AppendLine("Post: " + postNode);
        log.AppendLine();
        log.AppendLine("Считаем wp (с конца к началу):");

        Node wp = CalculateWp(program, postNode, 0);

        WpResult result = new WpResult();
        result.Steps = log.ToString();
        result.Wp = wp.ToString();
        result.Defined = definedLog.Length == 0 ? "Ограничений определённости нет" : definedLog.ToString();
        result.Hoare = "{ " + wp + " }  " + code + "  { " + post + " }";
        return result;
    }

    private static Node CalculateWp(Stmt stmt, Node post, int depth)
    {
        string indent = new string(' ', depth * 2);

        if (stmt is Assign a)
        {
            Node result = Substitute(post, a.Name, a.Expression);
            Node defined = Defined(a.Expression);
            if (defined != null)
            {
                result = And(defined, result);
                definedLog.AppendLine(defined.ToString());
            }
            log.AppendLine(indent + "Присваивание " + a.Name + " := " + a.Expression);
            log.AppendLine(indent + "  wp = " + result);
            return result;
        }

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

        if (stmt is Conditional c)
        {
            log.AppendLine(indent + "Ветвление if (" + c.Condition + ")");
            Node thenWp = CalculateWp(c.Then, post, depth + 1);
            Node elseWp = CalculateWp(c.Else, post, depth + 1);
            Node result = Or(And(c.Condition, thenWp), And(Not(c.Condition), elseWp));
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

    private static Node Substitute(Node node, string name, Node replacement)
    {
        if (node.Kind == "id" && node.Value == name) return replacement;
        if (node.Children.Count == 0) return node;
        return new Node(node.Kind, node.Value,
            node.Children.Select(c => Substitute(c, name, replacement)).ToList());
    }

    private static Node Defined(Node expression)
    {
        List<Node> conditions = new List<Node>();
        CollectDefinedness(expression, conditions);
        if (conditions.Count == 0) return null;
        return conditions.Aggregate(And);
    }

    private static void CollectDefinedness(Node n, List<Node> conditions)
    {
        foreach (Node child in n.Children) CollectDefinedness(child, conditions);
        if (n.Kind == "binary" && n.Value == "/")
            conditions.Add(new Node("binary", "!=", new List<Node> { n.Children[1], Num("0") }));
        if (n.Kind == "call" && n.Value == "sqrt")
            conditions.Add(new Node("binary", ">=", new List<Node> { n.Children[0], Num("0") }));
    }

    private static Node Num(string value) { return new Node("number", value); }
    private static Node And(Node a, Node b) { return new Node("binary", "&&", new List<Node> { a, b }); }
    private static Node Or(Node a, Node b) { return new Node("binary", "||", new List<Node> { a, b }); }
    private static Node Not(Node a) { return new Node("unary", "!", new List<Node> { a }); }
}

internal sealed class Node
{
    public string Kind { get; }
    public string Value { get; }
    public List<Node> Children { get; }

    public Node(string kind, string value, List<Node> children = null)
    {
        Kind = kind;
        Value = value;
        Children = children ?? new List<Node>();
    }

    public override string ToString()
    {
        if (Kind == "id" || Kind == "number" || Kind == "bool") return Value;
        if (Kind == "call") return Value + "(" + Children[0] + ")";
        if (Kind == "unary") return Value == "!" ? "!(" + Children[0] + ")" : "-" + Children[0];
        if (Kind == "binary") return "(" + Children[0] + " " + Value + " " + Children[1] + ")";
        return Value;
    }
}

internal abstract record Stmt;
internal sealed record Assign(string Name, Node Expression) : Stmt;
internal sealed record Sequence(List<Stmt> Statements) : Stmt;
internal sealed record Conditional(Node Condition, Stmt Then, Stmt Else) : Stmt;

internal sealed class Parser
{
    private readonly List<string> tokens;
    private int position;
    private string Current { get { return position < tokens.Count ? tokens[position] : "<EOF>"; } }

    public Parser(string input)
    {
        tokens = Regex.Matches(input,
            @"\s*(?:(\d+(?:\.\d+)?)|([A-Za-z_][A-Za-z0-9_]*)|(:=|==|!=|<=|>=|&&|\|\||[{}();,+*/<>!-]))")
            .Select(m => m.Groups[1].Success ? m.Groups[1].Value
                       : m.Groups[2].Success ? m.Groups[2].Value
                       : m.Groups[3].Value).ToList();
        string compact = Regex.Replace(input, @"\s+", "");
        if (compact != string.Concat(tokens))
            throw new FormatException("Не удалось разобрать ввод.");
    }

    public void ExpectEnd()
    {
        if (Current != "<EOF>") throw new FormatException("Лишний токен: " + Current);
    }

    private string Take()
    {
        string t = Current;
        if (t == "<EOF>") throw new FormatException("Неожиданный конец ввода.");
        position++;
        return t;
    }
    private bool Match(string token) { if (Current != token) return false; position++; return true; }
    private void Need(string token) { if (!Match(token)) throw new FormatException("Ожидался '" + token + "', получено '" + Current + "'."); }

    public Stmt ParseProgram()
    {
        List<Stmt> list = new List<Stmt>();
        while (Current != "<EOF>" && Current != "}")
        {
            list.Add(ParseStatement());
            if (Current == ";") { while (Match(";")) { } }
            else if (Current != "<EOF>" && Current != "}")
                throw new FormatException("Нужна точка с запятой.");
        }
        if (list.Count == 0) throw new FormatException("Код пуст.");
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
        if (!Regex.IsMatch(name, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            throw new FormatException("Ожидалось имя переменной.");
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
            if (double.TryParse(token, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out _))
                left = new Node("number", token);
            else if (Regex.IsMatch(token, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                if ((token == "abs" || token == "sqrt") && Match("("))
                { Node arg = ParseExpression(); Need(")"); left = new Node("call", token, new List<Node> { arg }); }
                else if (token == "true" || token == "false") left = new Node("bool", token);
                else left = new Node("id", token);
            }
            else throw new FormatException("Неожиданный токен: " + token);
        }
        while (Precedence(Current) >= minPrecedence && Precedence(Current) >= 0)
        {
            string op = Take();
            Node right = ParseExpression(Precedence(op) + 1);
            left = new Node("binary", op, new List<Node> { left, right });
        }
        return left;
    }

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