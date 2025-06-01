using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated#compilation-of-interpolated-strings
// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/improved-interpolated-strings#the-handler-pattern
// This attribute is used by the compiler to determine if a type is a valid interpolated string handler type.

public class TaggedTemplate
{
	public static string Html_Naive(NaiveHtmlInterpolatedStringHandler builder) => builder.GetFormattedText();
	public static string Html_Navie(string msg) => msg;

	public static HtmlComponent Html(HtmlInterpolatedStringHandler handler)
		=> new HtmlComponent(handler.Strs, handler.Vals);
	public static HtmlComponent Html(string text) => new HtmlComponent([text], []);
}


public class HtmlComponent
{
	private string[] _strs;
	private object[] _vals;

	public HtmlComponent(string[] strs, object[] vals)
	{
		_strs = strs;
		_vals = vals;
	}

	// 구조적인 문제는 객체지향적으로 해결한다는 의미는 구조적인 문제에 객체지향의 특성들이 굉장히 유용합니다.
	// Encapsulation, Abstraction, Override
	// 즉, 여기서는 Escape하는 방법은 언제든지 교체할 수 있습니다. (access modifier가 private인것을 주목한다면요)
	private string Escape(object raw)
		=> raw is HtmlComponent htmlComponent
			? htmlComponent.ToHtml()
			: HtmlHelper.Escape(raw);

	public string ToHtml()
	{
		return _vals
			.Select(Escape)
			.Append(string.Empty)
			.Zip(_strs, (val, str) => new string[] { str, val })
			.SelectMany(pair => pair)
			.Aggregate((a, b) => a + b);
	}
}

[InterpolatedStringHandler]
public ref struct HtmlInterpolatedStringHandler
{
	List<string> _strs;
	public string[] Strs => _strs.ToArray();
	List<object> _vals;
	public object[] Vals => _vals.ToArray();
	public HtmlInterpolatedStringHandler(int _, int formattedCount)
	{
		_strs = new List<string>(capacity: formattedCount + 1);
		_vals = new List<object>(capacity: formattedCount);
	}
	public void AppendLiteral(string s) => _strs.Add(s);
	public void AppendFormatted<T>(T t) => _vals.Add(t!);
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


#region Navie Approach
[InterpolatedStringHandler]
public ref struct NaiveHtmlInterpolatedStringHandler
{
	List<string> _strs;
	List<string> _vals;

	public NaiveHtmlInterpolatedStringHandler(int _, int formattedCount)
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
#endregion
