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
}
