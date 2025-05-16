using FxCs;

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
		WriteLine(CalcTotalPrice(products));
		WriteLine(SumSelectedQuantities2(products));
		WriteLine(CalcSelectedPrice2(products));
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
}
