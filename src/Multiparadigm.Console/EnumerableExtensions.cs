public static class EnumerableExtensions
{
	public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> f)
	{
		foreach (var v in source)
		{
			f(v);
		}
	}
}
