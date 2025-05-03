namespace FxCs;

public static class FxFactory
{
	public static FxIterable<T> Fx<T>(IEnumerable<T> iterable) => new FxIterable<T>(iterable);
}

public class FxIterable<A>
{
	private readonly IEnumerable<A> _iterable;

	public FxIterable(IEnumerable<A> iterable)
	{
		_iterable = iterable;
	}

	public FxIterable<B> Map<B>(Func<A, B> f)
		=> FxFactory.Fx(IterableHelpers.Map(f, _iterable));

	public FxIterable<A> Filter(Func<A, bool> f)
		=> FxFactory.Fx(IterableHelpers.Filter(f, _iterable));

	public void ForEach(Action<A> f)
		=> IterableHelpers.ForEach(f, _iterable);

	public Acc Reduce<Acc>(Func<Acc, A, Acc> f, Acc acc)
		=> IterableHelpers.Reduce(f, acc, _iterable);

	public A Reduce(Func<A, A, A> f)
		=> IterableHelpers.Reduce(f, _iterable);


}
