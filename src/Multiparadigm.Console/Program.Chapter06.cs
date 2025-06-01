using System.Diagnostics;

public partial class Program
{
	public static void TaggedTemplateExample()
	{
		var a = "A";
		var b = "B";
		var c = "C";

		TaggedTemplate.Html($"<b>{a}</b><i>{b}</i><em>{c}</em>");
		TaggedTemplate.Html($"");
	}

	public static void EsacpeHtmlText()
	{
		// WriteLine("\t" + HtmlHelper.Escape("<script>alert(\"XSS\")</script>"));
		// WriteLine("\t" + HtmlHelper.Escape("Hello & World! \"Have\" a nice day <3"));

		var a = "<script>alert(\"XSS\")</script>";
		var b = "Hello & Welcome!";

		WriteLine(
			TaggedTemplate.Html($"""
								<ul>
									<li>${a}</li>
									<li>${b}</li>
								</ul>
								"""));
	}
}
