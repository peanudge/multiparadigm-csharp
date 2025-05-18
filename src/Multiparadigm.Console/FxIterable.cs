namespace FxCs;

using System.Collections;
using System.Threading;
using System.Threading.Tasks;

public static class Fx
{
	public static FxIterable<T> From<T>(IEnumerable<T> iterable) => new FxIterable<T>(iterable);
	public static FxAsyncIterable<T> From<T>(IAsyncEnumerable<T> iterable) => new FxAsyncIterable<T>(iterable);
	public static FxIterable<TSource> ToFx<TSource>(this IEnumerable<TSource> source) => From(source);
	public static FxAsyncIterable<TSource> ToFx<TSource>(this IAsyncEnumerable<TSource> source) => From(source);

	public static IEnumerable<B> Map<A, B>(Func<A, B> func, IEnumerable<A> iterable)
	{
		foreach (var value in iterable)
		{
			yield return func(value);
		}
	}

	public static async IAsyncEnumerable<B> Map<A, B>(Func<A, B> func, IAsyncEnumerable<A> asyncIterable)
	{
		await foreach (var value in asyncIterable)
		{
			yield return func(value);
		}
	}

	public static async IAsyncEnumerable<B> Map<A, B>(Func<A, Task<B>> func, IAsyncEnumerable<A> asyncIterable)
	{
		await foreach (var value in asyncIterable)
		{
			yield return await func(value);
		}
	}

	public static void ForEach<A>(Action<A> func, IEnumerable<A> iterable)
	{
		foreach (var value in iterable)
		{
			func(value);
		}
	}

	public static IEnumerable<A> Filter<A>(Func<A, bool> func, IEnumerable<A> iterable)
	{
		foreach (var value in iterable)
		{
			if (func(value))
			{
				yield return value;
			}
		}
	}

	public static async IAsyncEnumerable<A> Filter<A>(Func<A, bool> func, IAsyncEnumerable<A> asyncIterable)
	{
		await foreach (var value in asyncIterable)
		{
			if (func(value))
			{
				yield return value;
			}
		}
	}

	public static async IAsyncEnumerable<A> Filter<A>(Func<A, Task<bool>> func, IAsyncEnumerable<A> asyncIterable)
	{
		await foreach (var value in asyncIterable)
		{
			if (await func(value))
			{
				yield return value;
			}
		}
	}

	public static A Reduce<A>(Func<A, A, A> func, IEnumerable<A> iterable)
	{
		var iterator = iterable.GetEnumerator();
		if (!iterator.MoveNext())
		{
			throw new InvalidOperationException("no elements");
		}

		return BaseReduce(func, iterator.Current, iterator);
	}

	public static Acc Reduce<A, Acc>(Func<Acc, A, Acc> func, Acc acc, IEnumerable<A> iterable)
	{
		return BaseReduce(func, acc, iterable.GetEnumerator());
	}

	private static Acc BaseReduce<A, Acc>(Func<Acc, A, Acc> func, Acc acc, IEnumerator<A> iterator)
	{
		while (iterator.MoveNext())
		{
			acc = func.Invoke(acc, iterator.Current);
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
		=> Take(limit, iterable.GetEnumerator());

	public static IEnumerable<A> Take<A>(int limit, IEnumerator<A> iterator)
	{
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

	public static IEnumerable<A[]> Chunk<A>(int size, IEnumerable<A> iterable)
	{
		var iterator = iterable.GetEnumerator();
		while (true)
		{
			var arr = Take(size, iterator).ToArray();
			if (arr.Length != 0) yield return arr;
			if (arr.Length < size) break;
		}
	}

	public static IEnumerable<(A, B)> Zip<A, B>(IEnumerable<A> firstIterable, IEnumerable<B> secondIterable)
	{
		var firstIterator = firstIterable.GetEnumerator();
		var secondIterator = secondIterable.GetEnumerator();
		while (firstIterator.MoveNext() && secondIterator.MoveNext())
		{
			yield return (firstIterator.Current, secondIterator.Current);
		}
	}

	public static IEnumerable<B> FlatMap<A, B>(Func<A, IEnumerable<B>> f, IEnumerable<A> iterable)
	{
		foreach (var item in iterable)
		{
			var subIterable = f(item);
			foreach (var value in subIterable)
			{
				yield return value;
			}
		}
	}

	public static async IAsyncEnumerable<T> ToAsync<T>(IEnumerable<T> iterable)
	{
		var iterator = iterable.GetEnumerator();
		while (iterator.MoveNext())
		{
			yield return await Task.FromResult(iterator.Current);
		}
	}

	public static async IAsyncEnumerable<T> ToAsync<T>(IEnumerable<Task<T>> iterable)
	{
		var iterator = iterable.GetEnumerator();
		while (iterator.MoveNext())
		{
			yield return await iterator.Current;
		}
	}

	public static async Task<TResult[]> FromAsync<TResult>(IAsyncEnumerable<TResult> iterable)
	{
		List<TResult> results = [];
		await foreach (var value in iterable)
		{
			results.Add(value);
		}
		return results.ToArray();
	}
}

public class FxIterable<A> : IEnumerable<A>
{
	private readonly IEnumerable<A> _iterable;

	public FxIterable(IEnumerable<A> iterable)
	{
		_iterable = iterable;
	}

	public IEnumerator<A> GetEnumerator() => _iterable.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public FxIterable<B> Map<B>(Func<A, B> func)
		=> Fx.Map(func, this).ToFx();

	public FxIterable<A> Filter(Func<A, bool> func)
		=> Fx.Filter(func, this).ToFx();

	public FxIterable<A> Reject(Func<A, bool> func)
		=> Filter(a => !func(a));

	public void ForEach(Action<A> func)
		=> Fx.ForEach(func, this);

	public Acc Reduce<Acc>(Func<Acc, A, Acc> func, Acc acc)
		=> Fx.Reduce(func, acc, this);

	public A Reduce(Func<A, A, A> func)
		=> Fx.Reduce(func, this);

	public R To<R>(Func<IEnumerable<A>, R> converter)
		=> converter(this);

	public FxIterable<B> Chain<B>(Func<IEnumerable<A>, IEnumerable<B>> func)
		=> func(this).ToFx();

	public FxIterable<A> Take(int limit)
		=> Fx.Take(limit, this).ToFx();

	public bool Every(Func<A, bool> func)
		=> AccumulateWith((a, b) => a && b, true, a => !a, func);

	public bool Some(Func<A, bool> func)
		=> AccumulateWith((a, b) => a || b, false, a => a, func);

	private bool AccumulateWith(Func<bool, bool, bool> accumulator, bool seed, Func<bool, bool> taking, Func<A, bool> func)
		=> Map(func)
			.Filter(taking)
			.Take(1)
			.Reduce(accumulator, seed);

	public FxIterable<A[]> Chunk(int size)
		=> Fx.Chunk(size, this).ToFx();

	public FxIterable<B> FlatMap<B>(Func<A, IEnumerable<B>> f)
		=> Fx.FlatMap(f, _iterable).ToFx();

}

public static class FxIterableExtensions
{
	public static FxAsyncIterable<T> ToAsync<T>(this FxIterable<T> source)
		=> Fx.ToAsync(source).ToFx();

	public static FxAsyncIterable<T> ToAsync<T>(this FxIterable<Task<T>> source)
		=> Fx.ToAsync(source).ToFx();

}


public class FxAsyncIterable<A> : IAsyncEnumerable<A>
{
	private readonly IAsyncEnumerable<A> _iterable;

	public FxAsyncIterable(IAsyncEnumerable<A> iterable)
	{
		_iterable = iterable;
	}

	public IAsyncEnumerator<A> GetAsyncEnumerator(CancellationToken cancellationToken = default)
	{
		return _iterable.GetAsyncEnumerator();
	}

	public FxAsyncIterable<B> Map<B>(Func<A, B> f)
		=> Fx.Map(f, this).ToFx();

	public FxAsyncIterable<B> Map<B>(Func<A, Task<B>> f)
		=> Fx.Map(f, this).ToFx();

	public FxAsyncIterable<A> Filter(Func<A, bool> f)
		=> Fx.Filter(f, this).ToFx();

	public FxAsyncIterable<A> Filter(Func<A, Task<bool>> f)
		=> Fx.Filter(f, this).ToFx();

	public Task<A[]> ToArray()
		=> Fx.FromAsync(this);

	public Task<Acc> Reduce<Acc>(Func<Acc, A, Acc> func, Acc acc)
		=> Fx.Reduce(func, acc, this);

	public Task<Acc> Reduce<Acc>(Func<Acc, A, Task<Acc>> func, Acc acc)
		=> Fx.Reduce(func, acc, this);
}
