using static TemplateEngine;

public partial class Program
{
	public static void TaggedTemplateExample()
	{
		var a = "A";
		var b = "B";
		var c = "C";
		WriteLine(Html_Naive($"<b>{a}</b><i>{b}</i><em>{c}</em>"));
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
					<li>{a}</li>
					<li>{b}</li>
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
			<li>{a}</li>
			<li>{b}</li>
			{menuHtml(menu)}
			{Html_Naive($"<li>{Html_Naive("<b>3단계 중첩</b>")}</li>")}
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
			<li>{a}</li>
			<li>{b}</li>
			{menuHtml(menu)}
			{Html($"<li>{Html("<b>3단계 중첩</b>")}</li>")}
		</ul>
		""");
		WriteLine(result.ToHtml());
	}

	public static void HtmlFromArrayExample()
	{
		var menuHtml = (Menu menu) => Html($"<li>{menu.Name} ({menu.Price})</li>");
		var menus = new Menu[]{
			new Menu("아메리카노", 4500),
			new Menu("카푸치노", 5000),
			new Menu("라떼 & 쿠키 세트", 8000),
		};

		var menuBoardHtml = (Menu[] menus) => Html($"""
		<div>
			<h1>메뉴 목록</h1>
			<ul>
				{menus.Select(menuHtml).Aggregate("", (a, b) => a + b.ToHtml())}
			</ul>
		</div>
		""");
		WriteLine(menuBoardHtml(menus).ToHtml());
	}

	public static void HtmlFromArrayExample_Solution_1()
	{
		var menuHtml = (Menu menu) => Html($"<li>{menu.Name} ({menu.Price})</li>");
		var menus = new Menu[]{
			new Menu("아메리카노", 4500),
			new Menu("카푸치노", 5000),
			new Menu("라떼 & 쿠키 세트", 8000),
		};

		// INFO: HtmlComponent간의 결합 방법을 제공해서 Escape를 구분.
		var menuBoardHtml = (Menu[] menus) => Html($"""
		<div>
			<h1>메뉴 목록</h1>
			<ul>
				{menus.Select(menuHtml).Aggregate((a, b) => Html($"{a}{b}"))}
			</ul>
		</div>
		""");

		WriteLine(menuBoardHtml(menus).ToHtml());
	}

	public static void HtmlFromArrayExample_Solution_2()
	{
		var menuHtml = (Menu menu) => Html($"<li>{menu.Name} ({menu.Price})</li>");
		var menus = new Menu[]{
			new Menu("아메리카노", 4500),
			new Menu("카푸치노", 5000),
			new Menu("라떼 & 쿠키 세트", 8000),
		};

		// INFO: HtmlComponent간의 결합 방법을 제공해서 Escape를 구분.
		var menuBoardHtml = (Menu[] menus) => Html($"""
		<div>
			<h1>메뉴 목록</h1>
			<ul>
				{menus.Select(menuHtml)}
			</ul>
		</div>
		""");

		WriteLine(menuBoardHtml(menus).ToHtml());
	}

	public static async Task RunTasksWithPoolByGpt()
	{
		Func<Task<string>>[] tasks = [
			CreateAsyncTask("A", 1000),
			CreateAsyncTask("B", 500),
			CreateAsyncTask("C", 800),
			CreateAsyncTask("D", 300),
			CreateAsyncTask("E", 1200),
		];

		var poolSize = 2;
		var results = await RunTasksWithPool_ByGPT(tasks, poolSize);
		WriteLine($"Results: {string.Join(", ", results)}");
	}


	static Func<Task<string>> CreateAsyncTask(string name, int ms)
	{
		return async () =>
		{
			WriteLine($"Started: {name}");
			await Task.Delay(ms);
			WriteLine($"Finshed: {name}");
			return name;
		};
	}


	//https://kwangyulseo.wordpress.com/2015/06/25/%ec%99%9c-%eb%b3%80%ec%88%98%ea%b0%80-%eb%82%98%ec%81%9c%ea%b0%80/
	static async Task<T[]> RunTasksWithPool_ByGPT<T>(Func<Task<T>>[] funcs, int poolSize)
	{
		T[] results = new T[funcs.Length];
		List<Task> activeTasks = [];

		for (var i = 0; i < funcs.Length; i++)
		{
			var taskFactory = funcs[i];
			var task = Task.Run(async () =>
			{
				var n = i;
				results[n] = await taskFactory();
				WriteLine($"results[{n}]: {results[n]}");
			});

			activeTasks.Add(task);

			if (activeTasks.Count() >= poolSize)
			{
				var finshedTask = await Task.WhenAny(activeTasks);
				activeTasks.Remove(finshedTask);
			}
		}

		await Task.WhenAll(activeTasks);
		return results;
	}
}
