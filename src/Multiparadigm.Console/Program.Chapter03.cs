using System.Security.Cryptography.X509Certificates;
using FxCs;
using static IterableHelpers;

public static partial class Program
{
	static void ListProcessingExamplePratice()
	{
		var numbers = Enumerable.Range(1, 9).ToArray();
		WriteLine("Imperative :");
		WriteLine(SumOfSquaresOfOddNumbers_Imperative(3, numbers));
		WriteLine("ListProcessing :");
		WriteLine(SumOfSquaresOfOddNumbers_ListProcessing(3, numbers));
		WriteLine("ListProcessing_MethodChaining :");
		WriteLine(SumOfSquaresOfOddNumbers_MethodChaingListProcessing(3, numbers));
		WriteLine("ListProcessing_LINQ :");
		WriteLine(SumOfSquaresOfOddNumbers_LINQ(3, numbers));

	}

	static int SumOfSquaresOfOddNumbers_Imperative(int limit, int[] list)
	{
		int acc = 0;
		foreach (var value in list)
		{
			if (value % 2 == 1)
			{
				var b = value * value;
				acc += b;
				if (--limit == 0) break;
			}
		}
		return acc;
	}

	static int SumOfSquaresOfOddNumbers_ListProcessing(int limit, int[] list)
		=> Reduce((a, b) => a + b, 0,
			Take(limit,
				Map(n => n * n,
					Filter(n => n % 2 == 1, list))));

	static int SumOfSquaresOfOddNumbers_MethodChaingListProcessing(int limit, int[] list)
		=> list
			.ToFx()
			.Filter(n => n % 2 == 1)
			.Map(n => n * n)
			.Take(limit)
			.Reduce((a, b) => a + b);

	static int SumOfSquaresOfOddNumbers_LINQ(int limit, int[] list)
		=> list
			.Where(n => n % 2 == 1)
			.Select(n => n * n)
			.Take(limit)
			.Aggregate(seed: 0, func: (a, b) => a + b);


	public static void CurryingExample()
	{
		// Partial Application
		var add = (int a) => (int b) => a + b;
		var addFive = add(5);
		WriteLine(addFive(10));

		var f = (int x) => x + 1;
		var g = (int x) => x * 2;
		var h = (int x) => x - 3;
		var func = (int x) => f(g(h(x)));
		WriteLine(func(1));
	}

	public static void OrderOfExecIterator()
	{
		Fx.From([1, 2, 3, 4, 5])
			.Filter(a => a % 2 == 1)
			.Map(a => a * a)
			.Take(2)
			.ForEach(n =>
			{
				WriteLine($"result: {n}");
				WriteLine("---");
			});
	}

	public static void Find_ListProcessing()
	{
		var result = Find(a => a > 2, [1, 2, 3]);
		WriteLine(result);

		// var isOdd = (int? a) => a % 2 == 1;
		// var result2 = Find<int?>(isOdd, [2, 4, 6]);
		// WriteLine(result2); // null

		var isOdd = (int a) => a % 2 == 1;
		var result2 = Find(isOdd, [2, 4, 6]);
		WriteLine(result2); // default is 0!! -> C# not allow to represent generic as nullable b/c primitive value type is not allowed to be null. 

		var desserts = new List<Dessert>()
		{
			new Dessert("Chocolate", 5000),
			new Dessert("Latte", 3500),
			new Dessert("Coffee", 3000),
		};

		var dessert = Find((item) => item.Price > 2000, desserts);
		WriteLine(dessert?.Name ?? "T^T");

		// Non-Null Assertion
		var dessert2 = Find((item) => item.Price < double.PositiveInfinity, desserts)!;
		WriteLine(dessert2.Name);
	}

	record Dessert(string Name, int Price);

	public static void Every_ListProcessing()
	{
		var isOdd = (int a) => a % 2 == 1;
		var allOdd = Fx.From([1, 3]).Every(isOdd);
		WriteLine($"All elements are odd: {allOdd}");
	}

	public static void Some_ListProcessing()
	{
		var isOdd = (int a) => a % 2 == 1;
		var anyOdd = Fx.From([1, 2, 3]).Some(isOdd);
		WriteLine($"Any elements is odd: {anyOdd}");
	}

	public static void Concat_ListProcessing()
	{
		int[] arr = [1, 2, 3, 4, 5];

		var result = Fx
			.From([6, 7, 8, 9])
			.Concat(arr)
			.Take(3);

		WriteLine(string.Join(",", result));

		WriteLine("===========");

		List<int> arr2 = [1, 2, 3, 4, 5];
		arr2.Insert(index: 0, 0); // Shift
		WriteLine(string.Join(",", arr2));

		// Use Concat instead of Unshift
		List<int> arr3 = [1, 2, 3, 4, 5];
		WriteLine(string.Join(",", Fx.From([0]).Concat(arr3)));

		WriteLine("===========");
	}
}

