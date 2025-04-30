namespace FxCs;

public static class Helpers
{
	public static FxIterable<T> Fx<T>(IEnumerable<T> iterable)
		=> new FxIterable<T>(iterable);
}

public class FxIterable<A>
{
	private readonly IEnumerable<A> _iterable;

	public FxIterable(IEnumerable<A> iterable)
	{
		_iterable = iterable;
	}

	public FxIterable<B> Map<B>(Func<A, B> f)
		=> Helpers.Fx(IterableHelpers.Map(f, _iterable));

	public FxIterable<A> Filter(Func<A, bool> f)
		=> Helpers.Fx(IterableHelpers.Filter(f, _iterable));

	public void ForEach(Action<A> f)
		=> IterableHelpers.ForEach(f, _iterable);

	// TODO: ToEnumerable()
}
