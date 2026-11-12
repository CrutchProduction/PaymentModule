using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public static class BalanceExpressionSolver
{
    /// <summary>
    /// Возвращает минимальное целое значение balance, при котором выражение истинно.
    /// Если выражение истинно при всех достаточно малых balance — возвращает decimal.MinValue.
    /// </summary>
    public static decimal Solve(string expression)
    {
        var parser = new Parser(expression);
        var root = parser.Parse();

        var comparisons = new List<CompareNode>();
        root.Collect(comparisons);

        var candidates = new SortedSet<decimal> { 0m };

        foreach (var cmp in comparisons)
        {
            var diff = cmp.Left - cmp.Right; // A * balance + B
            if (diff.A != 0m)
            {
                decimal t = -diff.B / diff.A;
                decimal f = Math.Floor(t);
                decimal c = Math.Ceiling(t);

                for (decimal d = f - 2m; d <= c + 2m; d++)
                    candidates.Add(d);
            }
        }

        decimal min = candidates.First();
        if (root.Eval(min - 1m))
            return decimal.MinValue; // минимума нет — истинно для всех малых balance

        foreach (var c in candidates)
        {
            if (root.Eval(c)) return c;
            if (root.Eval(c + 1m)) return c + 1m;
        }

        decimal max = candidates.Last();
        if (root.Eval(max + 1m)) return max + 1m;

        throw new InvalidOperationException("Выражение не имеет решения.");
    }

    /// <summary>
    /// Просто вычисляет выражение при известном balance.
    /// </summary>
    public static bool Evaluate(string expression, decimal balance)
    {
        var parser = new Parser(expression);
        return parser.Parse().Eval(balance);
    }

    private sealed class Linear
    {
        public decimal A { get; }
        public decimal B { get; }

        public Linear(decimal a, decimal b) { A = a; B = b; }

        public static Linear Const(decimal c) => new Linear(0m, c);
        public static Linear Balance => new Linear(1m, 0m);

        public decimal Eval(decimal x) => A * x + B;

        public static Linear operator +(Linear l, Linear r) =>
            new Linear(l.A + r.A, l.B + r.B);

        public static Linear operator -(Linear l, Linear r) =>
            new Linear(l.A - r.A, l.B - r.B);

        public static Linear operator -(Linear l) =>
            new Linear(-l.A, -l.B);

        public static Linear operator *(Linear l, Linear r)
        {
            if (l.A == 0m && r.A == 0m) return Const(l.B * r.B);
            if (l.A == 0m) return new Linear(r.A * l.B, r.B * l.B);
            if (r.A == 0m) return new Linear(l.A * r.B, l.B * r.B);

            throw new NotSupportedException("Нелинейное умножение не поддерживается.");
        }

        public static Linear operator /(Linear l, Linear r)
        {
            if (r.A != 0m)
                throw new NotSupportedException("Деление на выражение с balance не поддерживается.");
            if (r.B == 0m) throw new DivideByZeroException();

            return new Linear(l.A / r.B, l.B / r.B);
        }
    }

    private abstract class BoolNode
    {
        public abstract bool Eval(decimal x);
        public abstract void Collect(List<CompareNode> list);
    }

    private sealed class NotNode : BoolNode
    {
        private readonly BoolNode _inner;
        public NotNode(BoolNode inner) { _inner = inner; }

        public override bool Eval(decimal x) => !_inner.Eval(x);
        public override void Collect(List<CompareNode> list) => _inner.Collect(list);
    }

    private sealed class AndNode : BoolNode
    {
        private readonly BoolNode _left, _right;
        public AndNode(BoolNode l, BoolNode r) { _left = l; _right = r; }

        public override bool Eval(decimal x) => _left.Eval(x) && _right.Eval(x);

        public override void Collect(List<CompareNode> list)
        {
            _left.Collect(list);
            _right.Collect(list);
        }
    }

    private sealed class OrNode : BoolNode
    {
        private readonly BoolNode _left, _right;
        public OrNode(BoolNode l, BoolNode r) { _left = l; _right = r; }

        public override bool Eval(decimal x) => _left.Eval(x) || _right.Eval(x);

        public override void Collect(List<CompareNode> list)
        {
            _left.Collect(list);
            _right.Collect(list);
        }
    }

    private sealed class CompareNode : BoolNode
    {
        public Linear Left { get; }
        public Linear Right { get; }
        public string Op { get; }

        public CompareNode(Linear left, Linear right, string op)
        {
            Left = left;
            Right = right;
            Op = op;
        }

        public override bool Eval(decimal x)
        {
            decimal l = Left.Eval(x);
            decimal r = Right.Eval(x);

            return Op switch
            {
                ">=" => l >= r,
                "<=" => l <= r,
                ">" => l > r,
                "<" => l < r,
                "==" => l == r,
                "!=" => l != r,
                _ => throw new InvalidOperationException("Unknown operator: " + Op)
            };
        }

        public override void Collect(List<CompareNode> list) => list.Add(this);
    }

    private sealed class Parser
    {
        private readonly List<string> _tokens;
        private int _pos;

        public Parser(string expression)
        {
            _tokens = Tokenize(expression);
            _pos = 0;
        }

        public BoolNode Parse()
        {
            var node = ParseOr();

            if (_pos != _tokens.Count)
                throw new FormatException("Unexpected token: " + Peek());

            return node;
        }

        private string Peek() => _pos < _tokens.Count ? _tokens[_pos] : null;

        private bool Match(string token)
        {
            if (Peek() == token)
            {
                _pos++;
                return true;
            }
            return false;
        }

        private void Expect(string token)
        {
            if (!Match(token))
                throw new FormatException($"Expected '{token}', got '{Peek()}'");
        }

        private BoolNode ParseOr()
        {
            var left = ParseAnd();
            while (Match("||"))
                left = new OrNode(left, ParseAnd());
            return left;
        }

        private BoolNode ParseAnd()
        {
            var left = ParseNot();
            while (Match("&&"))
                left = new AndNode(left, ParseNot());
            return left;
        }

        private BoolNode ParseNot()
        {
            if (Match("!"))
                return new NotNode(ParseNot());

            return ParsePrimaryBool();
        }

        private BoolNode ParsePrimaryBool()
        {
            if (Peek() == "(")
            {
                int save = _pos;
                _pos++;

                try
                {
                    var inner = ParseOr();
                    Expect(")");
                    return inner;
                }
                catch
                {
                    _pos = save;
                }
            }

            return ParseComparison();
        }

        private BoolNode ParseComparison()
        {
            var left = ParseArith();

            string op = Peek();
            if (op == ">=" || op == "<=" || op == ">" ||
                op == "<" || op == "==" || op == "!=")
            {
                _pos++;
                var right = ParseArith();
                return new CompareNode(left, right, op);
            }

            throw new FormatException("Expected comparison operator, got: " + op);
        }

        private Linear ParseArith()
        {
            var left = ParseTerm();

            while (Peek() == "+" || Peek() == "-")
            {
                string op = Peek();
                _pos++;
                var right = ParseTerm();
                left = op == "+" ? left + right : left - right;
            }

            return left;
        }

        private Linear ParseTerm()
        {
            var left = ParseFactor();

            while (Peek() == "*" || Peek() == "/")
            {
                string op = Peek();
                _pos++;
                var right = ParseFactor();
                left = op == "*" ? left * right : left / right;
            }

            return left;
        }

        private Linear ParseFactor()
        {
            if (Match("+")) return ParseFactor();
            if (Match("-")) return -ParseFactor();

            if (Match("("))
            {
                var inner = ParseArith();
                Expect(")");
                return inner;
            }

            string tok = Peek();
            if (tok == null)
                throw new FormatException("Unexpected end of expression");

            if (tok == "balance")
            {
                _pos++;
                return Linear.Balance;
            }

            if (decimal.TryParse(tok, NumberStyles.Number,
                CultureInfo.InvariantCulture, out var value))
            {
                _pos++;
                return Linear.Const(value);
            }

            throw new FormatException("Unexpected token: " + tok);
        }

        private static List<string> Tokenize(string s)
        {
            var tokens = new List<string>();
            int i = 0;

            while (i < s.Length)
            {
                char ch = s[i];

                if (char.IsWhiteSpace(ch))
                {
                    i++;
                    continue;
                }

                if (char.IsDigit(ch) || ch == '.')
                {
                    int start = i;
                    while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.'))
                        i++;

                    tokens.Add(s.Substring(start, i - start));
                    continue;
                }

                if (char.IsLetter(ch))
                {
                    int start = i;
                    while (i < s.Length && char.IsLetterOrDigit(s[i]))
                        i++;

                    tokens.Add(s.Substring(start, i - start));
                    continue;
                }

                if (i + 1 < s.Length)
                {
                    string two = s.Substring(i, 2);
                    if (two == "&&" || two == "||" ||
                        two == ">=" || two == "<=" ||
                        two == "==" || two == "!=")
                    {
                        tokens.Add(two);
                        i += 2;
                        continue;
                    }
                }

                if ("+-*/()!><".IndexOf(ch) >= 0)
                {
                    tokens.Add(ch.ToString());
                    i++;
                    continue;
                }

                throw new FormatException($"Unexpected character '{ch}' at position {i}");
            }

            return tokens;
        }
    }
}