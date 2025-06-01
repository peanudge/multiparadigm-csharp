using static TaggedTemplate;

public partial class Program
{
	public static void TaggedTemplateExample()
	{
		var a = "A";
		var b = "B";
		var c = "C";

		Html_Naive($"<b>{a}</b><i>{b}</i><em>{c}</em>");
		Html_Navie($"");
	}

	public static void EsacpeHtmlText()
	{
		// WriteLine("\t" + HtmlHelper.Escape("<script>alert(\"XSS\")</script>"));
		// WriteLine("\t" + HtmlHelper.Escape("Hello & World! \"Have\" a nice day <3"));

		var a = "<script>alert(\"XSS\")</script>";
		var b = "Hello & Welcome!";

		WriteLine(
			Html_Naive($"""
				<ul>
					<li>${a}</li>
					<li>${b}</li>
				</ul>
				"""));
	}


	private record Menu(string Name, decimal Price);

	public static void NestDataComponentExample()
	{
		var menu = new Menu("Choco Latte & Cooki", 8_000);
		var menuHtml = (Menu menu) => Html_Naive($"<li>{menu.Name} ({menu.Price})</li>");
		var a = "<script>alert(\"XSS\")</script>";
		var b = "Hello & Welcome!";

		var result = Html_Naive($"""
		<ul>
			<li>${a}</li>
			<li>${b}</li>
			{menuHtml(menu)}
			{Html_Naive($"<li>{Html_Navie("<b>3단계 중첩</b>")}</li>")}
		</ul>
		""");
		WriteLine(result);
	}


	public static void NestDataComponentExample_Solution()
	{
		var menu = new Menu("Choco Latte & Cooki", 8_000);
		var menuHtml = (Menu m) => Html($"<li>{m.Name} ({m.Price})</li>");
		var a = "<script>alert(\"XSS\")</script>";
		var b = "Hello & Welcome!";

		var result = Html($"""
		<ul>
			<li>${a}</li>
			<li>${b}</li>
			{menuHtml(menu)}
			{Html($"<li>{Html("<b>3단계 중첩</b>")}</li>")}
		</ul>
		""");
		WriteLine(result.ToHtml());
	}

}
