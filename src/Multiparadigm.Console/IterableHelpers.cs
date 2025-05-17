


using System.Threading.Tasks;

public static class IterableHelpers
{

	public static Func<T> Constant<T>(T a) => () => a;

	public static T Identity<T>(T arg) => arg;

	public static IEnumerable<int> Naturals(int end = int.MaxValue)
	{
		var n = 1;
		while (n <= end)
		{
			yield return n++;
		}
	}

	public static IEnumerable<T> Reverse<T>(T[] array)
	{
		var idx = array.Length;
		while (idx > 0)
		{
			yield return array[--idx];
		}
	}

	public static IEnumerable<B> Map<A, B>(Func<A, B> f, IEnumerable<A> iterable)
	{
		foreach (var value in iterable)
		{
			yield return f.Invoke(value);
		}
	}

	public static Func<IEnumerable<A>, IEnumerable<B>> Map<A, B>(Func<A, B> f)
	{
		return (IEnumerable<A> iterable) => Map(f, iterable);
	}

	public static async IAsyncEnumerable<B> Map<A, B>(Func<A, B> f, IAsyncEnumerable<A> asyncIterable)
	{
		await foreach (var value in asyncIterable)
		{
			yield return f.Invoke(value);
		}
	}

	public static async IAsyncEnumerable<B> Map<A, B>(Func<A, Task<B>> f, IAsyncEnumerable<A> asyncIterable)
	{
		await foreach (var value in asyncIterable)
		{
			yield return await f.Invoke(value);
		}
	}


	public static void ForEach<A>(Action<A> f, IEnumerable<A> iterable)
	{
		foreach (var value in iterable)
		{
			f.Invoke(value);
		}
	}

	public static IEnumerable<A> Filter<A>(Func<A, bool> f, IEnumerable<A> iterable)
	{
		foreach (var value in iterable)
		{
			if (f(value))
			{
				yield return value;
			}
		}
	}

	public static Func<IEnumerable<A>, IEnumerable<A>> Filter<A>(Func<A, bool> f)
	{
		return (IEnumerable<A> iterable) => Filter(f, iterable);
	}

	public static async IAsyncEnumerable<A> Filter<A>(Func<A, bool> f, IAsyncEnumerable<A> asyncIterable)
	{
		await foreach (var a in asyncIterable)
		{
			if (f(a))
			{
				yield return a;
			}
		}
	}

	public static async IAsyncEnumerable<A> Filter<A>(Func<A, Task<bool>> f, IAsyncEnumerable<A> asyncIterable)
	{
		await foreach (var a in asyncIterable)
		{
			if (await f(a))
			{
				yield return a;
			}
		}
	}

	public static A Reduce<A>(Func<A, A, A> f, IEnumerable<A> iterable)
	{
		var iterator = iterable.GetEnumerator();
		if (!iterator.MoveNext())
		{
			throw new InvalidOperationException("no elements");
		}

		return BaseReduce(f, iterator.Current, iterator);
	}

	public static Acc Reduce<A, Acc>(Func<Acc, A, Acc> f, Acc acc, IEnumerable<A> iterable)
	{
		return BaseReduce(f, acc, iterable.GetEnumerator());
	}

	private static Acc BaseReduce<A, Acc>(Func<Acc, A, Acc> f, Acc acc, IEnumerator<A> iterator)
	{
		while (iterator.MoveNext())
		{
			acc = f.Invoke(acc, iterator.Current);
		}
		return acc;
	}

	public static async Task<Acc> Reduce<A, Acc>(Func<Acc, A, Acc> f, Acc acc, IAsyncEnumerable<A> asyncIterable)
	{
		await foreach (var a in asyncIterable)
		{
			acc = f(acc, a);
		}
		return acc;
	}

	public static async Task<Acc> Reduce<A, Acc>(Func<Acc, A, Task<Acc>> f, Acc acc, IAsyncEnumerable<A> asyncIterable)
	{
		await foreach (var a in asyncIterable)
		{
			acc = await f(acc, a);
		}
		return acc;
	}

	public static IEnumerable<A> Take<A>(int limit, IEnumerable<A> iterable)
	{
		var iterator = iterable.GetEnumerator();
		while (limit > 0 && iterator.MoveNext())
		{
			yield return iterator.Current;
			limit--;
		}
	}

	public static A? Head<A>(IEnumerable<A> iterable)
	{
		var iterator = iterable.GetEnumerator();
		return iterator.MoveNext() ? iterator.Current : default;
	}

	public static A? Find<A>(Func<A, bool> func, IEnumerable<A> iterable)
		=> Head(Filter(func, iterable));

	public static IEnumerable<A> Concat<A>(params IEnumerable<A>[] iterables)
	{
		foreach (var iterable in iterables)
		{
			var iterator = iterable.GetEnumerator();
			while (iterator.MoveNext())
			{
				yield return iterator.Current;
			}
		}
	}


	public static bool Every<A>(Func<A, bool> func, IEnumerable<A> iterable)
		=> Reduce((a, b) => a && b, true,
			Take(1,
				Filter(a => !a,
					Map(func, iterable))));

	public static bool Some<A>(Func<A, bool> func, IEnumerable<A> iterable)
		=> Reduce((a, b) => a || b, false,
			Take(1,
				Filter(a => a,
					Map(func, iterable))));


}
