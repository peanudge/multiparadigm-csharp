using System.Collections;

namespace FxCs;

public static class Fx
{
	public static FxIterable<T> From<T>(IEnumerable<T> iterable) => new FxIterable<T>(iterable);
}

public static class EnumerableExtension
{
	public static FxIterable<TSource> ToFx<TSource>(this IEnumerable<TSource> source)
		=> Fx.From(source);
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

	public FxIterable<B> Map<B>(Func<A, B> f)
		=> IterableHelpers.Map(f, _iterable).ToFx();

	public FxIterable<A> Filter(Func<A, bool> f)
		=> IterableHelpers.Filter(f, _iterable).ToFx();

	public FxIterable<A> Reject(Func<A, bool> f)
		=> Filter(a => !f(a));

	public void ForEach(Action<A> f)
		=> IterableHelpers.ForEach(f, _iterable);

	public Acc Reduce<Acc>(Func<Acc, A, Acc> f, Acc acc)
		=> IterableHelpers.Reduce(f, acc, _iterable);

	public A Reduce(Func<A, A, A> f)
		=> IterableHelpers.Reduce(f, _iterable);

	public R To<R>(Func<IEnumerable<A>, R> converter)
		=> converter(_iterable);

	public FxIterable<B> Chain<B>(Func<IEnumerable<A>, IEnumerable<B>> f)
		=> f(_iterable).ToFx();
}
