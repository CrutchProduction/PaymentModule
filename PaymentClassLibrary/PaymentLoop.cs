using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace PaymentClassLibrary.Loops
{
    public sealed record LoopStep(int Index, int Result, int VariantBefore, int VariantAfter,
        bool InvariantBefore, bool InvariantAfter, bool VariantOk);
    public sealed record LoopAnalysis(int Result, IReadOnlyList<LoopStep> Steps,
        WpResult InvariantWp, WpResult VariantWp, WpResult InitializationWp, bool Initialization, bool Preservation,
        bool Exit, bool Termination);

    /// <summary>Проверяет выполнение выбранного цикла на заданных платежах, не доказывает теоремы.</summary>
    public static class PaymentLoop
    {
        public static string Body(int mode) => mode switch
        {
            0 => "res := res + a[j]; j := j + 1;",
            1 => "if (a[j] > T) { res := res + 1; } else { res := res; }; j := j + 1;",
            2 => "res := max(res, a[j]); j := j + 1;",
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };

        public static LoopAnalysis Analyze(int[] a, int mode, int threshold, string invariant, string variant)
        {
            if (a == null || a.Length == 0) throw new ArgumentException("Добавьте хотя бы один платёж.");
            if (a.Any(x => x < 0)) throw new ArgumentException("Суммы должны быть неотрицательными.");
            string body = Body(mode);
            Node inv = Parse(invariant), t = Parse(variant);
            Node builtIn = Parse(mode switch {
                0 => "res == sum(a, 0, j) && 0 <= j && j <= n",
                1 => "res == count_gt(a, 0, j, T) && 0 <= j && j <= n",
                _ => "res == max_prefix(a, 0, j) && 1 <= j && j <= n"
            });
            int j = mode == 2 ? 1 : 0, res = mode == 2 ? a[0] : 0;
            bool initialization = Holds(inv, a, threshold, j, res);
            int previous = Variant(t, a, threshold, j, res);
            var steps = new List<LoopStep> {
                new(j, res, previous, previous, initialization, initialization, previous >= 0)
            };
            WpResult initWp = WpEngine.Analyze(mode == 2 ? "res := a[0]; j := 1;" : "res := 0; j := 0;", invariant);
            initialization &= Holds(Parse(initWp.Wp), a, threshold, 0, 0);
            WpResult invWp = WpEngine.Analyze(body, invariant);
            WpResult tWp = WpEngine.Analyze(body, "(" + variant + ") >= 0 && (" + variant + ") < old_t");
            Node invCondition = Parse(invWp.Wp), tCondition = Parse(tWp.Wp);
            bool preservation = true, termination = previous >= 0;
            while (j < a.Length)
            {
                bool before = Holds(inv, a, threshold, j, res);
                previous = Variant(t, a, threshold, j, res);
                bool wpOk = Holds(invCondition, a, threshold, j, res);
                bool tOk = Holds(tCondition, a, threshold, j, res, previous);
                // Ожидаемые ошибки пользовательских формул показываем статусами, без остановки отладчика.
                Debug.Assert(Holds(builtIn, a, threshold, j, res), "Инвариант алгоритма перед шагом.");
                res = mode switch {
                    0 => checked(res + a[j]),
                    1 => res + (a[j] > threshold ? 1 : 0),
                    _ => Math.Max(res, a[j])
                };
                j++;
                bool after = Holds(inv, a, threshold, j, res);
                int next = Variant(t, a, threshold, j, res);
                bool variantOk = previous >= 0 && next >= 0 && next < previous && tOk;
                Debug.Assert(Holds(builtIn, a, threshold, j, res), "Инвариант алгоритма после шага.");
                Debug.Assert(a.Length - j >= 0, "Вариант встроенного алгоритма неотрицателен.");
                Debug.WriteLine($"j={j}: Inv {before} -> {after}, t {previous} -> {next}");
                preservation &= before && wpOk && after;
                termination &= variantOk;
                steps.Add(new(j, res, previous, next, before, after, variantOk));
            }
            int expected = mode switch {
                0 => a.Aggregate(0, (sum, x) => checked(sum + x)),
                1 => a.Count(x => x > threshold),
                _ => a.Max()
            };
            return new(res, steps, invWp, tWp, initWp, initialization, preservation,
                Holds(inv, a, threshold, j, res) && res == expected, termination);
        }

        public static bool CheckInvariant(string formula, int[] a, int threshold, int j, int res) =>
            Holds(Parse(formula), a, threshold, j, res);

        private static Node Parse(string formula)
        {
            if (string.IsNullOrWhiteSpace(formula)) throw new FormatException("Формула не должна быть пустой.");
            var parser = new Parser(formula);
            Node node = parser.ParseExpression();
            parser.ExpectEnd();
            return node;
        }
        private static bool Holds(Node node, int[] a, int threshold, int j, int res, int old = 0)
        {
            decimal value = Eval(node, a, threshold, j, res, old);
            if (value != 0 && value != 1) throw new FormatException("Условие должно возвращать true или false.");
            return value == 1;
        }
        private static int Variant(Node node, int[] a, int threshold, int j, int res)
        {
            decimal value = Eval(node, a, threshold, j, res, 0);
            if (value != decimal.Truncate(value) || value < int.MinValue || value > int.MaxValue)
                throw new FormatException("Вариант должен быть целым числом.");
            return (int)value;
        }
        private static decimal Eval(Node node, int[] a, int threshold, int j, int res, int old)
        {
            decimal E(Node x) => Eval(x, a, threshold, j, res, old);
            int I(Node x)
            {
                decimal v = E(x);
                if (v != decimal.Truncate(v) || v < int.MinValue || v > int.MaxValue)
                    throw new FormatException("Индекс должен быть целым числом.");
                return (int)v;
            }
            if (node.Kind == "number") return decimal.Parse(node.Value, CultureInfo.InvariantCulture);
            if (node.Kind == "bool") return node.Value == "true" ? 1 : 0;
            if (node.Kind == "id") return node.Value switch {
                "n" => a.Length, "j" => j, "res" => res, "T" => threshold, "old_t" => old,
                _ => throw new FormatException("Неизвестная переменная: " + node.Value)
            };
            if (node.Kind == "index")
            {
                int index = I(node.Children[0]);
                if (node.Value != "a" || index < 0 || index >= a.Length)
                    throw new FormatException("Индекс массива вне границ.");
                return a[index];
            }
            if (node.Kind == "unary")
                return node.Value == "!" ? (E(node.Children[0]) == 0 ? 1 : 0) : -E(node.Children[0]);
            if (node.Kind == "binary")
            {
                decimal l = E(node.Children[0]);
                if (node.Value == "&&" && l == 0) return 0;
                if (node.Value == "||" && l != 0) return 1;
                decimal r = E(node.Children[1]);
                return node.Value switch {
                    "+" => l+r, "-" => l-r, "*" => l*r,
                    "/" => r == 0 ? throw new FormatException("Деление на ноль.") : l/r,
                    "==" => l==r?1:0, "!=" => l!=r?1:0, "<" => l<r?1:0, "<=" => l<=r?1:0,
                    ">" => l>r?1:0, ">=" => l>=r?1:0, "&&" => r!=0?1:0, "||" => r!=0?1:0,
                    _ => throw new FormatException("Неизвестный оператор.")
                };
            }
            if (node.Kind == "call")
            {
                var args = node.Children;
                if (node.Value == "abs" && args.Count == 1) return Math.Abs(E(args[0]));
                if (node.Value == "sqrt" && args.Count == 1)
                {
                    decimal x = E(args[0]);
                    if (x < 0) throw new FormatException("Корень из отрицательного числа.");
                    return (decimal)Math.Sqrt((double)x);
                }
                if (node.Value == "max" && args.Count == 2) return Math.Max(E(args[0]), E(args[1]));
                bool count = node.Value == "count_gt";
                if ((node.Value == "sum" || node.Value == "max_prefix" || count) && args.Count == (count?4:3))
                {
                    if (args[0].Kind != "id" || args[0].Value != "a")
                        throw new FormatException("Первый аргумент должен быть массивом a.");
                    int from = I(args[1]), end = I(args[2]);
                    if (from < 0 || end < from || end > a.Length || (node.Value == "max_prefix" && from == end))
                        throw new FormatException("Неверные границы префикса.");
                    decimal result = node.Value == "max_prefix" ? a[from] : 0;
                    decimal limit = count ? E(args[3]) : 0;
                    for (int i = from; i < end; i++)
                        result = node.Value == "sum" ? result+a[i] : count ?
                            result+(a[i]>limit?1:0) : Math.Max(result,a[i]);
                    return result;
                }
                throw new FormatException("Неизвестная функция или неверное число аргументов: " + node.Value);
            }
            throw new FormatException("Неподдерживаемое выражение.");
        }
    }
}