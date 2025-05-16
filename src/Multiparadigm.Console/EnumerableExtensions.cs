public static class EnumerableExtensions
{
	// public R To<R>(Func<IEnumerable<A>, R> converter)
	// => converter(this);
	public static R To<TSource, R>(
		this IEnumerable<TSource> source,
		Func<IEnumerable<TSource>, R> converter)
		=> converter(source);

	public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> f)
	{
		foreach (var item in source) { f(item); }
	}


	public static List<TSource> FxSort<TSource>(this List<TSource> source, Comparison<TSource> comparison)
	{
		source.Sort(comparison);
		return source;
	}


	public static IEnumerable<T> Flat<T>(this IEnumerable<IEnumerable<T>> source)
	{
		foreach (var a in source)
		{
			foreach (var b in a)
			{
				yield return b;
			}
		}
	}
}
