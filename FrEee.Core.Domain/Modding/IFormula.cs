using FrEee.Serialization;
using FrEee.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace FrEee.Modding;

[DoNotCopy]
public interface IFormula : IComparable
{
    object Context { get; }
    bool IsDynamic { get; }
    bool IsLiteral { get; }
    string Text { get; set; }

    object Value { get; }

    object Evaluate(object host, IDictionary<string, object> variables = null);

    Formula<string> ToStringFormula(CultureInfo c = null);

    IFormula<T> ToFormula<T>()
		where T : IComparable, IConvertible;
}

public interface IFormula<out T> : IFormula
{
    new T Value { get; }

    new T Evaluate(object host, IDictionary<string, object> variables = null);
}