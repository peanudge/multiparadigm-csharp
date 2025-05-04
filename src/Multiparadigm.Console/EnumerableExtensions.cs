public static class EnumerableExtensions
{
	public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> f)
	{
		foreach (var item in source) { f(item); }
	}


	public static List<TSource> FxSort<TSource>(this List<TSource> source, Comparison<TSource> comparison)
	{
		source.Sort(comparison);
		return source;
	}
}
