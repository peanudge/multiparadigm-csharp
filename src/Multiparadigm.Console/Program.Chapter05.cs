using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using FxCs;

using static IterableHelpers;

public static partial class Program
{
	static Team[] _teams = [
		new Team("Bears",  [
			new Player("Luka", 32),
			new Player("Anthony", 28),
			new Player("Kevin", 15),
			new Player("Jaylen", 14)
		]),
		new Team("Lions",  [
			new Player("Stephan", 37),
			new Player("Zach", 20),
			new Player("Nikola", 19),
			new Player("Austin", 22)
		]),
		new Team("Wolves",  [
			new Player("Jayson", 37),
			new Player("Klay", 20),
			new Player("Andrew", 19),
			new Player("Dennis", 22)
		]),
	];
	public static void Two_Demension()
	{
		// 5.1.1 2차원 배열 숫자 다루기
		int[][] numbers = [
			[1,2],
			[3,4,5],
			[6,7,8],
			[9,10]
		];

		var oddSquareSum = numbers
			.Flat()
			.Where(a => a % 2 == 1)
			.Select(a => a * a)
			.Aggregate(0, (a, b) => a + b);

		WriteLine(oddSquareSum);
	}


	// 5.1.2 농구팀 데이터 다루기
	private record Player(string Name, int Score);
	private record Team(string Name, Player[] Players);
	public static void HandleBasketBallData()
	{
		var totalHighScores1 = _teams.ToFx()
			.FlatMap(team => team.Players)
			.Filter(player => player.Score > 30)
			.Map(player => player.Score)
			.Reduce((a, b) => a + b, 0);

		WriteLine(totalHighScores1);

		var totalHighScores2 = _teams
			.SelectMany(team => team.Players)
			.Where(player => player.Score > 30)
			.Select(player => player.Score)
			.Aggregate(0, (a, b) => a + b);

		WriteLine(totalHighScores2);
	}

	// 5.1.3 commerce data
	// private record Product(
	// 	string Name,
	// 	int Price,
	// 	int Quantity,
	// 	bool Selected
	// );

	// private static Product[] Products = [
	// 	new Product("티셔츠", 10000, 1, true),
	// 	new Product("셔츠", 30000, 2, false),
	// 	new Product("바지", 15000, 2, true),
	// ];

	// public static void HandleCommerceData1()
	// {
	// 	WriteLine(SumSelectedQuantities(Products));
	// 	WriteLine(CalcSelectedPrices(Products));
	// }

	// private static int SumSelectedQuantities(Product[] products)
	// 	=> products
	// 		.Where(prd => prd.Selected)
	// 		.Select(prd => prd.Quantity)
	// 		.Aggregate(0, (a, b) => a + b);

	// private static int CalcSelectedPrices(Product[] products)
	// 	=> products
	// 		.Where(prd => prd.Selected)
	// 		.Select(prd => prd.Price * prd.Quantity)
	// 		.Aggregate(0, (a, b) => a + b);

	// 5.1.4. 커머스 데이터 다루기 2
	private record struct Option(string Name, int Price, int Quantity);
	private record struct Product(string Name, int Price, bool Selected, Option[] Options);
	public static void HandleCommerceData2()
	{
		Product[] products = [
			new Product(
				"티셔츠",
				10000,
				true,
				Options: [
					new Option(Name: "L", Price: 0, Quantity: 3),
					new Option(Name: "XL", Price: 1000, Quantity: 2),
					new Option(Name: "2XL", Price: 3000, Quantity: 2),
				]
			),
			new Product(
				"셔츠",
				30000,
				false,
				Options: [
					new Option(Name: "L", Price: 0, Quantity: 2),
					new Option(Name: "XL", Price: 1000, Quantity: 5),
					new Option(Name: "2XL", Price: 3000, Quantity: 4),
				]
			),
			new Product(
				"바지",
				15000,
				true,
				Options: [
					new Option(Name: "L", Price: 500, Quantity: 3),
					new Option(Name: "2XL", Price: 3000, Quantity: 5),
				]
			)
		];
		// WriteLine(CalcTotalPrice(products));
		// WriteLine(SumSelectedQuantities2(products));
		// WriteLine(CalcSelectedPrice2(products));

		products.Pipe(
			products => products.Where(prd => prd.Selected),
			products => CalcTotalPrice(products.ToArray())
		);

	}

	private static int SumSelectedQuantities2(Product[] products)
		=> products
			.Where(prd => prd.Selected)
			.SelectMany(prd => prd.Options)
			.Select(option => option.Quantity)
			.Aggregate(0, (a, b) => a + b);

	// private static int CalcSelectedPrice2(Product[] products)
	// 	=> products
	// 		.Where(prd => prd.Selected)
	// 		.SelectMany(CalcProductOptionPrices)
	// 		.Aggregate(0, (a, b) => a + b);

	private static IEnumerable<int> CalcProductOptionPrices(Product product)
		=> product.Options.Select(opt => (product.Price + opt.Price) * opt.Quantity);

	private static int CalcTotalPrice(Product[] products)
		=> products
			.SelectMany(CalcProductOptionPrices)
			.Aggregate(0, (a, b) => a + b);

	private static int CalcSelectedPrice2(Product[] products)
		=> CalcTotalPrice(products.Where(prd => prd.Selected).ToArray());



	// 5.2.1 Pipe Function
	// h(x) => f(g(x))
	// 1. Method Chain (OOP)
	// 2. Pipe Method (FP)
	public static async Task FunctionCompositionUsingPipe()
	{
		var add = (int a) => (int b) => a + b;
		WriteLine(add(5)(10));

		var result1 = 10.Pipe(
			add(10),
			add(10)
		);

		WriteLine(result1);

		var result2 = PipeExtensions.Pipe(
			new string[] { "1", "2", "3" },
			texts => Map(n => int.Parse(n), texts),
			nums => Filter(n => n == 1, nums)
		);

		result2.ForEach(WriteLine);

		var result = await PipeExtensions.Pipe(
			Task.FromResult(5),
			async task => await task + 10,
			async task =>
			{
				await Task.Delay(1000);
				return await task;
			},
			async task => await task - 5
		);

		var arr = new int[] { 1, 2, 3, 4, 5 };
		await PipeExtensions.Pipe(
			arr,
			Fx.ToAsync,
			arr => Map(a => a + 10, arr),
			arr => Filter(a => a % 2 == 0, arr),
			arr => Fx.FromAsync(arr)
		).ContinueWith(
			task => task.Result.ForEach(WriteLine)
		);
	}


	public static void ZipFunction()
	{
		Fx.From(["Jiho", "Suhyuen", "Jinjoo"])
			.Pipe(
				names => Fx.Zip(names, Naturals()),
				names => Map(tuple => $"{tuple.Item1}: {tuple.Item2}", names)
			)
			.ForEach(WriteLine);
	}

	//5.2.5 콜라츠 추측

	public static IEnumerable<int> Count() => Naturals();
	public static IEnumerable<A> RepeatApply<A>(Func<A, A> func, A acc)
	{
		while (true) yield return acc = func(acc);
	}

	public static int NextCollatzValue(int num) => num % 2 == 0 ? num / 2 : num * 3 + 1;

	public static int CollatzConjecture(int num)
	{
		var stopWatch = new Stopwatch();
		stopWatch.Start();

		var collatzCount = PipeExtensions.Pipe(
			RepeatApply(NextCollatzValue, num),
			_ => Enumerable.Zip(Count(), _),
			_ => Enumerable.Where(_, tuple => tuple.Second == 1),
			_ => Enumerable.FirstOrDefault(_),
			collatz => collatz!,
			collatz => collatz.First
		);

		stopWatch.Stop();
		WriteLine($"Collatze {num} => {collatzCount} ({stopWatch.ElapsedMilliseconds} ms)");

		return collatzCount;
	}

	public static void InsteadBreakExample()
	{
		Fx.From([1, 2, 3, 0, 0, 5, 6])
			.TakeWhile(a =>
			{
				WriteLine($"TakeWhile: {a}, {a >= 1}");
				return a >= 1;
			})
			.ForEach(WriteLine);

		Fx.From([0, 10, 1, 3, 5, 0, 4, 2])
			.TaskUntilInclusive(a =>
			{
				WriteLine($"TakeUntilInclusive: {a}, {a == 5}");
				return a == 5;
			})
			.ForEach(WriteLine);
	}

	public static void MapReducePatterns()
	{
		Func<string, string> decodeUri = System.Web.HttpUtility.UrlDecode;
		Func<string, string> encodeUri = System.Web.HttpUtility.UrlEncode;

		var queryString = "key1=value1%7Cvalue2&key2=123";

		var queryObject = queryString
			.Split("&")
			.Select(param => param.Split("="))
			.Where(entry => entry.Length == 2)
			.Select(entry => entry.Select(decodeUri).ToArray())
			.ToDictionary(entry => entry[0], entry => entry[1]);

		WriteLine($"{JsonSerializer.Serialize(queryObject)}");

		var deserializedQueryString = queryObject
			.Select(kvp => (encodeUri(kvp.Key), encodeUri(kvp.Value)))
			.Select(entry => string.Join("=", [entry.Item1, entry.Item2]))
			.Aggregate((a, b) => $"{a}&{b}"); // Empty params will be thrown

		WriteLine(deserializedQueryString);
	}



	private record struct TreeNode(int Id, TreeNode[] children);
	public static void NestedMapPatterns()
	{
		// Tree Node Transform
		var tree = new TreeNode[]
		{
			new TreeNode(Id: 1, children: [
				new TreeNode(Id: 2, []),
				new TreeNode(Id: 3, [])
			]),
			new TreeNode(Id: 4, children: [
				new TreeNode(Id: 5, [])
			]),
		};

		var transformedTree = tree
			.Select(node => new
			{
				Name = $"parent-{node.Id}",
				Children = node.children.Select(child => new { Name = $"child-{child.Id}" })
			})
			.ToArray();

		WriteLine(JsonSerializer.Serialize(transformedTree, new JsonSerializerOptions() { WriteIndented = true }));
	}


	private static int[] GetMonthEndDates(DateTime monthEnd)
		=> monthEnd.DayOfWeek == DayOfWeek.Saturday
			? []
			: Enumerable
				.Range(monthEnd.Day - (int)monthEnd.DayOfWeek, (int)monthEnd.DayOfWeek + 1)
				.ToArray();

	public static int[][] GenerateCalendar(DateTime prevMonthEnd, DateTime currentMonthEnd)
		=> Enumerable.Empty<int>()
			.Concat(GetMonthEndDates(prevMonthEnd))
			.Concat(Enumerable.Range(1, currentMonthEnd.Day))
			.Concat(Enumerable.Range(1, (int)DayOfWeek.Saturday - (int)currentMonthEnd.DayOfWeek))
			.Chunk(7)
			.ToArray();

	private static string FormatCalendar(int[][] calendarWeeks)
		=> calendarWeeks
			.Select(weeks => weeks.Select(day => day < 10 ? $" {day}" : $"{day}"))
			.Select(weeks => string.Join(" ", weeks))
			.StringJoin("\n");
	// .Aggregate((a, b) => $"{a}\n{b}");

	private static DateTime EndDateOfMonth(int year, int month)
		=> new DateTime(
			year: year,
			month: month,
			day: DateTime.DaysInMonth(year, month)
		);

	public static void RenderCalendar(int year, int month)
	{
		var result = PipeExtensions.Pipe(
			GenerateCalendar(EndDateOfMonth(year, month - 1), EndDateOfMonth(year, month)),
			FormatCalendar
		);

		WriteLine(result);
	}

	public static void IteratorForEachPatterns()
	{
		Enumerable.Range(1, 5)
			.Select(x => x * 2)
			.ForEach(x => WriteLine($"Processed: {x}"));
	}

	public static async Task ChunkFlatPatterns()
	{
		var values = await Enumerable.Range(1, 100)
			.Chunk(5)
			.ToAsyncEnumerable()
			.SelectMany(nums => nums)
			.ToArray();

		values.ForEach(WriteLine);
	}


	private record Comment(int Id, string Text, Comment[] Replies);
	public static void MapFlatPatterns()
	{
		Comment[] comments = [
			new Comment(1, "First comment", [
				new Comment(11, "Reply 1-1", [])
			]),
			new Comment(2, "Second comment", []),
			new Comment(3, "Third comment", [
				new Comment(31, "Reply 3-1", []),
				new Comment(32, "Reply 3-2", [])
			]),
		];

		comments
			.Select(comment => Enumerable.Empty<Comment>().Concat([comment]).Concat(comment.Replies))
			.SelectMany(comments => comments)
			.ForEach(WriteLine);
	}
}
