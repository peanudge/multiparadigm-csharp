

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

	public static void ForEach<A>(Action<A> f, IEnumerable<A> iterable)
	{
		foreach (var value in iterable)
		{
			f(value);
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
