using System.Collections;
using System.Threading.Tasks;

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

	public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
	{
		foreach (var value in source) yield return await Task.FromResult(value);
	}


	public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<Task<T>> source)
	{
		foreach (var task in source) yield return await task;
	}
}

public static class AsyncEnumerableExtensions
{
	public static async IAsyncEnumerable<R> Select<T, R>(this IAsyncEnumerable<T> source, Func<T, R> func)
	{
		await foreach (var value in source)
		{
			yield return func(value);
		}
	}

	public static async IAsyncEnumerable<R> Select<T, R>(this IAsyncEnumerable<T> source, Func<T, Task<R>> func)
	{
		await foreach (var value in source)
		{
			yield return await func(value);
		}
	}

	public static async IAsyncEnumerable<T> Reject<T>(this IAsyncEnumerable<T> source, Func<T, bool> func)
	{
		await foreach (var item in source)
		{
			if (!func(item))
			{
				yield return item;
			}
		}
	}

	public static async IAsyncEnumerable<T> Take<T>(this IAsyncEnumerable<T> source, int limit)
	{
		var iterator = source.GetAsyncEnumerator();
		while (limit > 0 && await iterator.MoveNextAsync())
		{
			yield return iterator.Current;
			limit--;
		}
	}

	public static async IAsyncEnumerable<T> TakeWhile<T>(this IAsyncEnumerable<T> source, Func<T, bool> func)
	{
		var iterator = source.GetAsyncEnumerator();
		while (await iterator.MoveNextAsync())
		{
			if (!func(iterator.Current)) break;
			yield return iterator.Current;
		}
	}

	public static async IAsyncEnumerable<T> TakeUntilInclusive<T>(this IAsyncEnumerable<T> source, Func<T, bool> func)
	{
		var iterator = source.GetAsyncEnumerator();
		while (await iterator.MoveNextAsync())
		{
			yield return iterator.Current;
			if (func(iterator.Current)) break;
		}
	}

	public static async IAsyncEnumerable<R> SelectMany<T, R>(this IAsyncEnumerable<T> source, Func<T, IEnumerable<R>> func)
	{
		await foreach (var a in source)
		{
			var sub = func(a);
			foreach (var item in sub)
			{
				yield return item;
			}
		}
	}

	public static async IAsyncEnumerable<T> Flat<T>(this IAsyncEnumerable<T[]> source)
	{
		await foreach (var items in source)
		{
			foreach (var item in items)
			{
				yield return item;
			}
		}
	}

	public static async Task ForEach<T>(this IAsyncEnumerable<T> source, Action<T> action)
	{
		await foreach (var value in source)
		{
			action(value);
		}
	}

	public static async Task<T[]> ToArray<T>(this IAsyncEnumerable<T> source)
	{
		List<T> array = new();
		await foreach (var value in source)
		{
			array.Add(value);
		}
		return array.ToArray();
	}
}
