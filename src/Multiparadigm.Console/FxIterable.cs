using System.Collections;

namespace FxCs;

public static class Fx
{
	public static FxIterable<T> From<T>(IEnumerable<T> iterable) => new FxIterable<T>(iterable);
	public static FxIterable<TSource> ToFx<TSource>(this IEnumerable<TSource> source) => From(source);
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
		=> IterableHelpers.Map(func, _iterable).ToFx();

	public FxIterable<A> Filter(Func<A, bool> func)
		=> IterableHelpers.Filter(func, _iterable).ToFx();

	public FxIterable<A> Reject(Func<A, bool> func)
		=> Filter(a => !func(a));

	public void ForEach(Action<A> func)
		=> IterableHelpers.ForEach(func, _iterable);

	public Acc Reduce<Acc>(Func<Acc, A, Acc> func, Acc acc)
		=> IterableHelpers.Reduce(func, acc, _iterable);

	public A Reduce(Func<A, A, A> func)
		=> IterableHelpers.Reduce(func, _iterable);

	public R To<R>(Func<IEnumerable<A>, R> converter)
		=> converter(_iterable);

	public FxIterable<B> Chain<B>(Func<IEnumerable<A>, IEnumerable<B>> func)
		=> func(_iterable).ToFx();

	public FxIterable<A> Take(int limit)
		=> IterableHelpers.Take(limit, _iterable).ToFx();
}
