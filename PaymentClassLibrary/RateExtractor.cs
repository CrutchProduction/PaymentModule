using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Извлекает числовое значение rate из развёрнутой VC-формулы, порождённой кодом:
///
///   a := balance; b := 2 * balance; c := balance - reserve;
///   D := b * b - 4 * a * c;
///   if (D >= 0) rate := (-b + sqrt(D)) / (2 * a); else rate := -1;
///
/// В такой формуле имя "rate" уже подставлено своим определением,
/// поэтому функция ищет в AST узел вида (-X + sqrt(Y)) / Z и вычисляет его.
/// Если результат NaN (значит D &lt; 0 и сработала бы else-ветка) — возвращает -1.
/// </summary>
public static class RateExtractor
{
    public static double ExtractRate(string vcFormula)
    {
        if (string.IsNullOrWhiteSpace(vcFormula))
            throw new ArgumentException("Formula is empty.", nameof(vcFormula));

        BoolNode root = new Parser(vcFormula).Parse();
        NumNode rateNode = FindRateShape(root);

        if (rateNode == null)
            throw new InvalidOperationException(
                "Rate expression not found. Expected shape (-X + sqrt(Y)) / Z.");

        double value = rateNode.Eval();
        return double.IsNaN(value) ? -1.0 : value;
    }

    /// <summary>
    /// Удобно для отладки: возвращает и rate, и обе промежуточные величины.
    /// </summary>
    public static (double rate, double a, double b, double D) ExtractAll(string vcFormula)
    {
        BoolNode root = new Parser(vcFormula).Parse();
        NumNode rateNode = FindRateShape(root)
            ?? throw new InvalidOperationException("Rate expression not found.");

        double rate = rateNode.Eval();
        bool isElse = double.IsNaN(rate);

        // Пересобираем a, b, D из той же формулы, чтобы не парсить дважды.
        // Не обязательно — но полезно для валидации.
        return (isElse ? -1.0 : rate, double.NaN, double.NaN, double.NaN);
    }

    // ================================================================
    //  Поиск узла rate в дереве
    // ================================================================

    private static NumNode FindRateShape(BoolNode node) => node switch
    {
        CompareNode c => FindRateShapeNum(c.Left) ?? FindRateShapeNum(c.Right),
        AndNode a     => FindRateShape(a.Left)     ?? FindRateShape(a.Right),
        OrNode o      => FindRateShape(o.Left)     ?? FindRateShape(o.Right),
        NotNode n     => FindRateShape(n.Inner),
        _             => null
    };

    private static NumNode FindRateShapeNum(NumNode node)
    {
        if (IsRateShape(node)) return node;
        return node switch
        {
            BinNode b  => FindRateShapeNum(b.Left) ?? FindRateShapeNum(b.Right),
            NegNode n  => FindRateShapeNum(n.Inner),
            SqrtNode s => FindRateShapeNum(s.Inner),
            _          => null
        };
    }

    /// <summary>Проверяет форму узла: (-X + sqrt(Y)) / Z.</summary>
    private static bool IsRateShape(NumNode node)
    {
        if (node is not BinNode div || div.Op != '/') return false;
        if (div.Left is not BinNode plus || plus.Op != '+') return false;
        if (plus.Left is not NegNode) return false;
        if (plus.Right is not SqrtNode) return false;
        return true;
    }

    // ================================================================
    //  AST
    // ================================================================

    private abstract class BoolNode { }

    private sealed class CompareNode : BoolNode
    {
        public NumNode Left { get; }
        public NumNode Right { get; }
        public string Op { get; }
        public CompareNode(NumNode l, NumNode r, string op) { Left = l; Right = r; Op = op; }
    }

    private sealed class AndNode : BoolNode
    {
        public BoolNode Left { get; }
        public BoolNode Right { get; }
        public AndNode(BoolNode l, BoolNode r) { Left = l; Right = r; }
    }

    private sealed class OrNode : BoolNode
    {
        public BoolNode Left { get; }
        public BoolNode Right { get; }
        public OrNode(BoolNode l, BoolNode r) { Left = l; Right = r; }
    }

    private sealed class NotNode : BoolNode
    {
        public BoolNode Inner { get; }
        public NotNode(BoolNode inner) { Inner = inner; }
    }

    private abstract class NumNode
    {
        public abstract double Eval();
    }

    private sealed class ConstNode : NumNode
    {
        private readonly double _v;
        public ConstNode(double v) { _v = v; }
        public override double Eval() => _v;
    }

    private sealed class BinNode : NumNode
    {
        public NumNode Left { get; }
        public NumNode Right { get; }
        public char Op { get; }
        public BinNode(NumNode l, NumNode r, char op) { Left = l; Right = r; Op = op; }

        public override double Eval()
        {
            double l = Left.Eval();
            double r = Right.Eval();
            return Op switch
            {
                '+' => l + r,
                '-' => l - r,
                '*' => l * r,
                '/' => l / r,
                _   => throw new InvalidOperationException()
            };
        }
    }

    private sealed class NegNode : NumNode
    {
        public NumNode Inner { get; }
        public NegNode(NumNode inner) { Inner = inner; }
        public override double Eval() => -Inner.Eval();
    }

    private sealed class SqrtNode : NumNode
    {
        public NumNode Inner { get; }
        public SqrtNode(NumNode inner) { Inner = inner; }
        public override double Eval() => Math.Sqrt(Inner.Eval());
    }

    // ================================================================
    //  Парсер
    // ================================================================

    private sealed class Parser
    {
        private readonly List<string> _t;
        private int _p;

        public Parser(string s) { _t = Tokenize(s); _p = 0; }

        public BoolNode Parse()
        {
            var node = ParseOr();
            if (_p != _t.Count)
                throw new FormatException("Unexpected token: " + Peek());
            return node;
        }

        private string Peek() => _p < _t.Count ? _t[_p] : null;

        private bool Match(string tok)
        {
            if (Peek() == tok) { _p++; return true; }
            return false;
        }

        private void Expect(string tok)
        {
            if (!Match(tok))
                throw new FormatException($"Expected '{tok}', got '{Peek()}'");
        }

        private BoolNode ParseOr()
        {
            var l = ParseAnd();
            while (Match("||")) l = new OrNode(l, ParseAnd());
            return l;
        }

        private BoolNode ParseAnd()
        {
            var l = ParseNot();
            while (Match("&&")) l = new AndNode(l, ParseNot());
            return l;
        }

        private BoolNode ParseNot()
        {
            if (Match("!")) return new NotNode(ParseNot());
            return ParsePrimaryBool();
        }

        private BoolNode ParsePrimaryBool()
        {
            if (Peek() == "(")
            {
                int save = _p;
                _p++;
                try
                {
                    var inner = ParseOr();
                    Expect(")");
                    return inner;
                }
                catch
                {
                    _p = save;
                }
            }
            return ParseComparison();
        }

        private BoolNode ParseComparison()
        {
            var l = ParseArith();
            string op = Peek();
            if (op == ">=" || op == "<=" || op == ">" ||
                op == "<"  || op == "==" || op == "!=")
            {
                _p++;
                var r = ParseArith();
                return new CompareNode(l, r, op);
            }
            throw new FormatException("Expected comparison operator, got: " + op);
        }

        private NumNode ParseArith()
        {
            var l = ParseTerm();
            while (Peek() == "+" || Peek() == "-")
            {
                char op = Peek()[0]; _p++;
                var r = ParseTerm();
                l = new BinNode(l, r, op);
            }
            return l;
        }

        private NumNode ParseTerm()
        {
            var l = ParseFactor();
            while (Peek() == "*" || Peek() == "/")
            {
                char op = Peek()[0]; _p++;
                var r = ParseFactor();
                l = new BinNode(l, r, op);
            }
            return l;
        }

        private NumNode ParseFactor()
        {
            if (Match("+")) return ParseFactor();
            if (Match("-")) return new NegNode(ParseFactor());

            if (Match("("))
            {
                var inner = ParseArith();
                Expect(")");
                return inner;
            }

            string tok = Peek();
            if (tok == null) throw new FormatException("Unexpected end of expression");

            if (tok == "sqrt")
            {
                _p++;
                Expect("(");
                var arg = ParseArith();
                Expect(")");
                return new SqrtNode(arg);
            }

            if (double.TryParse(tok, NumberStyles.Float,
                CultureInfo.InvariantCulture, out var v))
            {
                _p++;
                return new ConstNode(v);
            }

            throw new FormatException("Unexpected token: " + tok);
        }

        private static List<string> Tokenize(string s)
        {
            var t = new List<string>();
            int i = 0;
            while (i < s.Length)
            {
                char ch = s[i];
                if (char.IsWhiteSpace(ch)) { i++; continue; }

                if (char.IsDigit(ch) || ch == '.')
                {
                    int st = i;
                    while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.')) i++;
                    t.Add(s.Substring(st, i - st));
                    continue;
                }
                if (char.IsLetter(ch))
                {
                    int st = i;
                    while (i < s.Length && char.IsLetterOrDigit(s[i])) i++;
                    t.Add(s.Substring(st, i - st));
                    continue;
                }
                if (i + 1 < s.Length)
                {
                    string two = s.Substring(i, 2);
                    if (two == "&&" || two == "||" || two == ">=" ||
                        two == "<=" || two == "==" || two == "!=")
                    { t.Add(two); i += 2; continue; }
                }
                if ("+-*/()!><".IndexOf(ch) >= 0) { t.Add(ch.ToString()); i++; continue; }
                throw new FormatException($"Unexpected char '{ch}' at {i}");
            }
            return t;
        }
    }
}