using System;
using System.Data;
using System.Globalization;

/// <summary>
/// Решает неравенства вида "выражение OP константа" относительно переменной balance
/// и возвращает целое значение balance, удовлетворяющее неравенству.
///
/// Примеры:
///   "((((balance - 1) - 2) - 3) >= 5000)"        -> 5006
///   "(((((balance - 1) - 2) - 3) - 4) >= 5000)"  -> 5010
/// </summary>
public static class BalanceSolver
{
    public static decimal Solve(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression is empty.", nameof(expression));

        var (op, idx) = FindComparison(expression);
        if (op == null)
            throw new ArgumentException("No comparison operator found.", nameof(expression));

        // Разделяем на левую и правую части и балансируем скобки
        string left = Balance(expression.Substring(0, idx).Trim());
        string right = Balance(expression.Substring(idx + op.Length).Trim());

        // Представляем каждую сторону в виде A*x + B:  f(0) = B,  f(1) = A + B
        decimal l0 = Eval(left, 0m), l1 = Eval(left, 1m);
        decimal r0 = Eval(right, 0m), r1 = Eval(right, 1m);

        // (l1-l0)*x + l0  OP  (r1-r0)*x + r0
        // =>  A*x  OP  B
        decimal A = (l1 - l0) - (r1 - r0);
        decimal B = r0 - l0;

        return SolveLinear(op, A, B);
    }

    private static (string, int) FindComparison(string expr)
    {
        foreach (var op in new[] { ">=", "<=", "==", "!=" })
        {
            int i = expr.IndexOf(op, StringComparison.Ordinal);
            if (i >= 0) return (op, i);
        }
        foreach (var op in new[] { ">", "<" })
        {
            int i = expr.IndexOf(op, StringComparison.Ordinal);
            if (i >= 0) return (op, i);
        }
        return (null, -1);
    }

    private static decimal SolveLinear(string op, decimal A, decimal B)
    {
        // Если A == 0, неравенство не зависит от balance
        if (A == 0m)
        {
            bool holds = op switch
            {
                ">=" => 0m >= B,
                ">" => 0m > B,
                "<=" => 0m <= B,
                "<" => 0m < B,
                "==" => 0m == B,
                "!=" => 0m != B,
                _ => throw new ArgumentException("Unknown operator: " + op)
            };
            return holds ? decimal.MaxValue : decimal.MinValue;
        }

        // Если A < 0 — умножаем обе части на -1 и меняем знак неравенства
        string sop = op;
        if (A < 0m)
        {
            A = -A; B = -B;
            sop = sop switch
            {
                ">=" => "<=",
                "<=" => ">=",
                ">" => "<",
                "<" => ">",
                _ => sop
            };
        }

        // Теперь A > 0:  balance  sop  B/A
        decimal T = B / A;
        return sop switch
        {
            ">=" => Math.Ceiling(T),        // минимальное целое >= T
            ">" => Math.Floor(T) + 1m,     // минимальное целое > T
            "<=" => Math.Floor(T),          // максимальное целое <= T
            "<" => Math.Ceiling(T) - 1m,   // максимальное целое < T
            "==" => T,
            "!=" => throw new NotSupportedException("'!=' has no single answer."),
            _ => throw new ArgumentException("Unknown operator: " + op)
        };
    }

    /// <summary>Делает набор скобок в строке сбалансированным.</summary>
    private static string Balance(string s)
    {
        int depth = 0, minDepth = 0;
        foreach (char c in s)
        {
            if (c == '(') depth++;
            else if (c == ')') { depth--; if (depth < minDepth) minDepth = depth; }
        }

        string result = s;
        if (minDepth < 0)
            result = new string('(', -minDepth) + result;

        int addClose = depth - minDepth;
        if (addClose > 0)
            result += new string(')', addClose);

        return result;
    }

    private static decimal Eval(string expr, decimal x)
    {
        // Подставляем значение balance = x
        string replaced = expr.Replace("balance",
            x.ToString(CultureInfo.InvariantCulture));

        // Вычисляем как арифметическое выражение
        var dt = new DataTable();
        object res = dt.Compute(replaced, string.Empty);
        return Convert.ToDecimal(res, CultureInfo.InvariantCulture);
    }
}