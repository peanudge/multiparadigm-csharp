using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated#compilation-of-interpolated-strings
// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/improved-interpolated-strings#the-handler-pattern
// This attribute is used by the compiler to determine if a type is a valid interpolated string handler type.

public class TaggedTemplate
{
	public static string Html(HtmlInterpolatedStringHandler builder) => builder.GetFormattedText();
	public static string Html(string msg) => msg;
}

[InterpolatedStringHandler]
public ref struct HtmlInterpolatedStringHandler
{
	List<string> _strs;
	List<string> _vals;

	public HtmlInterpolatedStringHandler(int _, int formattedCount)
	{
		_strs = new List<string>(capacity: formattedCount + 1);
		_vals = new List<string>(capacity: formattedCount);
	}

	public void AppendLiteral(string s)
	{
		_strs.Add(s);
	}

	public void AppendFormatted<T>(T t)
	{
		_vals.Add(t?.ToString()!);
	}

	internal string GetFormattedText()
	{
		Debug.Assert(_strs.Count - 1 == _vals.Count || _strs.Count == _vals.Count);
		return _vals
			.Select(HtmlHelper.Escape)
			.Append(string.Empty)
			.Zip(_strs, (val, str) => new string[] { str, val })
			.SelectMany(pair => pair)
			.Aggregate((a, b) => a + b);
	}
}

public static class HtmlHelper
{
	private static Dictionary<string, string> EscapeMap = new()
	{
		{"&", "&amp;"},
		{"<", "&lt;"},
		{">", "&gt;"},
		{"\"", "&quot;"},
		{"'", "&#x27;"},
		{"`", "&#x60;"}
	};

	private static Regex EscapableRegExp = new("(?:" + string.Join("|", EscapeMap.Keys) + ")");

	public static string Escape(object input)
	{
		var str = $"{input}";
		return EscapableRegExp.IsMatch(str)
			? EscapableRegExp.Replace(str, (match) => EscapeMap[match.Value])
			: str;
	}
}
