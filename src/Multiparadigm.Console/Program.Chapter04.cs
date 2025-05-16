using System.Diagnostics;
using FxCs;
using static TaskHelpers;
using static FxCs.Fx;

public static partial class Program
{
	public static Task<T> Delay<T>(int time, T value)
		=> Task.Delay(time).ContinueWith((_) => value);

	// https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/chaining-tasks-by-using-continuation-tasks#about-continuations
	public static async Task DelayExample()
	{
		var result = await Delay(1000, "Hello, World")
			.Then(data => Delay(1000, "Hello, World"))
			.Then(d => Task.FromException(new Exception("Heloo")))
			.Then(data => data + "!!!");

		WriteLine(result);

	}

	public static async Task PromiseRaceExample()
	{
		/*
		const promise1 = new Promise((resolve) => setTimeout(resolve, 500, 'one'));
		const promise2 = new Promise((resolve) => setTimeout(resolve, 100, 'two'));
		await Promise.race([promise1, promise2]).then(value => { console.log(value) });
		*/

		var result = await Task
			.WhenAny([Delay(500, "one"), Delay(100, "Two")])
			.Then((firstTask) => firstTask.Result);

		WriteLine(result);
	}


	public static T GetRandomValue<T>(T a, T b)
		=> new Random().Next(maxValue: 2) == 0 ? a : b;

	public record User(string Name);

	public static Task<User[]> GetFriends()
		=> Delay(GetRandomValue(60, 6_000), new[] { new User("Marty"), new User("Michael"), new User("Sarah") });

	public static async Task IOTimeoutExample()
	{
		var getFriendsTask = GetFriends();
		var timeoutTask = Delay(5000, "Timeout");
		var firstCompleted = await Task.WhenAny([getFriendsTask, timeoutTask]);

		if (firstCompleted == getFriendsTask)
		{
			var friends = await getFriendsTask;
			var result = friends.ToFx()
				.Map(friend => $"<li>{friend.Name}</li>")
				.Reduce((a, b) => $"{a}{b}");

			WriteLine($"친구 목록 렌더링: {result}");
		}
		else
		{
			WriteLine("현재 네트워크 환경이 좋지 않습니다.");
		}
	}

	public static void ToggleLoadingIndicator(bool show)
	{
		if (show)
		{
			WriteLine("Append Loading...");
		}
		else
		{
			WriteLine("Remove Loading...");
		}
	}

	public static async Task RenderFriendsPicker()
	{
		var friendsTask = GetFriends();
		var slowLoadingTask = Delay(100, "isSlow");
		var resultTask = await Task.WhenAny([friendsTask, slowLoadingTask]);
		if (resultTask == slowLoadingTask)
		{
			ToggleLoadingIndicator(true);
			await friendsTask;
			ToggleLoadingIndicator(false);
		}

		var friends = await friendsTask;
		WriteLine("친구 목록 렌더링: " + friends.ToFx().Map(friend => $"<li>{friend.Name}</li>").Reduce((a, b) => $"{a}{b}"));
	}

	public record File(string Name, string Body, int Size);

	public static Task<File> GetFile(string name, int size = 1000)
	{
		WriteLine($"{name} 시작");
		return Delay(size, new File(name, "...", size));
	}

	public static async Task PromiseAllExample()
	{
		var stopWatch = new Stopwatch();
		stopWatch.Start();
		try
		{
			var files = await Task.WhenAll([
				Task.FromException<File>(new Exception("파일 다운로드 실패")),
				GetFile("img.png", 500),
				GetFile("book.pdf", 1000),
				GetFile("index.html", 1500),
			]);
			files.ForEach(WriteLine);
		}
		catch (Exception e)
		{
			WriteLine(e.Message);
		}
		finally
		{
			stopWatch.Stop();
			WriteLine("Elapsed Time: " + stopWatch.ElapsedMilliseconds);
		}
	}


	public static async Task PromiseAllSettledExample()
	{
		var results = await AllSettled([
			Task.FromException<File>(new Exception("파일 다운로드 실패")),
			GetFile("img.png", 500),
			GetFile("book.pdf", 1000),
			GetFile("index.html", 1500),
		]);

		results.ForEach(WriteLine);
	}

	public static Task<Result> DelayReject<Result>(int time, string error)
	{
		return Task.Delay(time)
			.ContinueWith<Result>(pre =>
			{
				throw new Exception(error);
			});
	}

	public static async Task PromiseAnyExample()
	{
		var stopWatch = new Stopwatch();
		stopWatch.Start();

		var file = await WhenAnySuccess([
			GetFile("img.png", 500),
			GetFile("book.pdf", 1000),
			GetFile("index.html", 1500),
			Task.FromException<File>(new Exception("파일 다운로드 실패")),
		]);

		stopWatch.Stop();
		WriteLine(stopWatch.Elapsed);
		WriteLine(file);
	}

	public static async Task ExecuteWithLimitExample()
	{
		var stopWatch = new Stopwatch();
		stopWatch.Start();

		var files = await ExecuteWithLimit_OnlyCSharp([
			() => GetFile("img.png"),
			() => GetFile("book.pdf"),
			() => GetFile("index.html"),
			() => GetFile("img.png"),
			() => GetFile("book.pdf"),
			() => GetFile("index.html"),
			() => GetFile("img.png"),
			() => GetFile("book.pdf"),
			() => GetFile("index.html"),
			() => GetFile("img.png"),
			() => GetFile("book.pdf"),
			() => GetFile("index.html"),
		], 3);
		stopWatch.Stop();
		WriteLine(stopWatch.Elapsed);
		files.ForEach(WriteLine);
	}

	private static Task<TResult[]> ExecuteWithLimit<TResult>(
		Func<Task<TResult>>[] fs,
		int limit)
	{
		return fs.ToFx() // [() => P<T>, () => P<T>, ...]
			.Map(f => f()) // [P<T>, P<T>, P<T>, ...] - Run Async Func
			.Chunk(limit) // [[P<T>, P<T>, P<T>], ...] - Group
			.Map(Task.WhenAll) // [P<T, T, T>, ...] - Wrap WhenAll for waiting 3 tasks
			.To(FromAsync) // P<[[T,T,T],[T,T,T], ... ]> - Get result of tasks
			.Then(tasks => tasks.SelectMany(x => x).ToArray()); // Flatten to 1 demension array
	}


	private static Task<TResult[]> ExecuteWithLimit_OnlyCSharp<TResult>(
		Func<Task<TResult>>[] fs,
		int limit)
	{
		return fs
			.Select(f => f())
			.Chunk(limit)
			.Select(Task.WhenAll)
			.To(FromAsync)
			.Then(tasks => tasks.SelectMany(x => x).ToArray());
	}

	public static async Task<TResult[]> ExecuteWithLimit_GPT<TResult>(Func<Task<TResult>>[] fs, int limit)
	{
		if (fs == null) throw new ArgumentNullException(nameof(fs));
		if (limit <= 0) throw new ArgumentOutOfRangeException(nameof(limit), "limit는 1 이상이어야 합니다.");

		var results = new List<TResult>();

		for (int i = 0; i < fs.Length; i += limit)
		{
			var batch = fs.Skip(i).Take(limit).ToArray();
			var tasks = batch.Select(f => f()).ToArray();
			var batchResults = await Task.WhenAll(tasks);
			results.AddRange(batchResults);
		}

		return results.ToArray();
	}

	public static async Task AsyncTypesExample()
	{

		var words = await FromAsync([
			Delay(100, "Hello"),
			Delay(100, "World")
		]);
		WriteLine(string.Join(" ", words));

		// https://learn.microsoft.com/en-us/archive/msdn-magazine/2019/november/csharp-iterating-with-async-enumerables-in-csharp-8#c

		var asyncIterable = Fx.ToAsync([1]);
		var asyncIterator = asyncIterable.GetAsyncEnumerator();
		await asyncIterator.MoveNextAsync();
		WriteLine(asyncIterator.Current);

		await foreach (var a in Fx.ToAsync([1, 2, 3, 4]))
		{
			WriteLine(a);
		}

		// var asyncIterable2 = ToAsync([Task.FromResult(2)]);
		// var asyncIterator2 = asyncIterable2.GetAsyncEnumerator();
		// await asyncIterator2.MoveNextAsync();
		// WriteLine(asyncIterator2.Current);

		// await foreach (var a in ToAsync([1, 2]))
		// {
		// 	WriteLine(a);
		// }

		// ToAsync의 필요성? IAsyncEunmerable -> IEnumerable
		// 238page b/c IEnumerable -> IAsyncEnumerable conversion.
		// 비동기적으로 값을 다룰 것임.
		await foreach (var a in ToAsync([Task.FromResult(1), Task.FromResult(2)]))
		{
			WriteLine(a);
		}

		// await foreach (var value in MapAsync(a => a.ToUpper(), FetchStrings()))
		// {
		// 	WriteLine(value);
		// }

		// await foreach (var a in MapAsync(a => a * 2, Numbers()))
		// {
		// 	WriteLine(a);
		// }

		await foreach (var a in
			Map(a => Delay(1000, a),
				Filter(a => a % 2 == 1,
					ToAsync(Enumerable.Range(1, 10)))))
		{
			WriteLine(a);
		}

		await foreach (var a in FetchStrings())
		{
			WriteLine(a);
		}
	}

	private static async IAsyncEnumerable<string> FetchStrings()
	{
		yield return await Delay(500, "A");
		yield return await Delay(200, "B");
	}

	// private static async IAsyncEnumerable<int> Numbers()
	// {
	// 	yield return 1;
	// 	yield return 2;
	// }

	// 비동기를 타입으로 다룬다는 것의 의미??
	// 컴파일 타임에 비동기 처리를 예상할 수 있다.
	public static void MapAsyncExamplesAtCompileTime()
	{
		IEnumerable<string> iter1 = Map(
			(a) => $"{a:'N'}",
			[1, 2]);

		IEnumerable<Task<string>> iter2 = Map(
			a => Task.FromResult($"{a:N}"),
			[1, 2]);

		IAsyncEnumerable<string> iter3 = Map(
			a => $"{a:N}",
			ToAsync([1, 2]));

		IAsyncEnumerable<string> iter4 = Map(
			a => Task.FromResult($"{a:N}"),
			ToAsync([1, 2]));

		IAsyncEnumerable<string> iter5 = Map(
			a => $"{a:N}",
			ToAsync([Task.FromResult(1), Task.FromResult(2)])
		);

		IAsyncEnumerable<string> iter6 = Map(
			a => Task.FromResult($"{a:N}"),
			ToAsync([Task.FromResult(1), Task.FromResult(2)])
		);
	}

	public static async Task MapSyncAndAsyncExamples()
	{
		// Map(a => a * 10, [1, 2]).ForEach(WriteLine);

		// await foreach (var a in Map(a => Delay(100, a * 10), ToAsync([1, 2])))
		// {
		// 	WriteLine(a);
		// }

		// var numbers1 = await FromAsync(Map(a => Delay(100, a * 10), ToAsync([1, 2])));
		// numbers1.ForEach(WriteLine);

		var numbers2 = await Task.WhenAll(Map(a => Delay(100, a * 10), [1, 2]));
		numbers2.ForEach(WriteLine);
	}

	public static void FilterAsyncExamplesAtCompileTime()
	{
		var iter1 = Filter(a => a % 2 == 1, [1, 2]);

		// Compile Error:
		// Cannot implicitly convert type 'System.Threading.Tasks.Task<bool>' to 'bool'
		// var iter2 = Filter(a => Task.FromResult(a % 2 == 1), [1,2]);

		var iter3 = Filter(a => a % 2 == 1, ToAsync([1, 2]));
		var iter4 = Filter(a => Task.FromResult(a % 2 == 1), ToAsync([1, 2]));
	}

	public static async Task FilterSyncAndAsyncExamples()
	{
		var isOdd = (int a) => a % 2 == 1;
		ForEach(WriteLine, Map(a => a * 10, Filter(isOdd, Enumerable.Range(1, 4))));

		var iter2 = Map(a => $"{a:N}", Filter(a => Delay(100, isOdd(a)), ToAsync(Enumerable.Range(1, 4))));

		await foreach (var a in iter2)
		{
			WriteLine(a);
		}
		WriteLine("End");

		var numbers = await Fx.FromAsync(
			Map(a => Delay(100, a * 10),
				ToAsync(Filter(isOdd, Enumerable.Range(1, 4)))));

		numbers.ForEach(WriteLine);
	}


	private static bool IsOdd(int n)
		=> n % 2 == 1;

	public static async Task FxAsyncIterableExample()
	{
		var arr1 = Enumerable.Range(1, 4).ToFx()
			.Filter(IsOdd)
			.Map(a => a * 10)
			.ToArray();

		WriteLine(string.Join(", ", arr1));

		var iter2 = Enumerable.Range(1, 4).ToFx()
			.ToAsync()
			.Filter(a => Delay(100, IsOdd(a)))
			.Map(a => $"{a:N}");

		await foreach (var a in iter2)
		{
			WriteLine(a);
		}
		WriteLine("End");

		var arr3 = await Enumerable.Range(1, 4).ToFx()
			.Filter(IsOdd)
			.ToAsync()
			.Map(a => Delay(100, a * 10))
			.ToArray();

		WriteLine(string.Join(", ", arr3));

		// var sum1 = await Enumerable.Range(1, 4).ToFx()
		//    .Filter(IsOdd)
		//    .Map(a => Delay(100, a * 10))
		//    .ToAsync()
		//    .Reduce((acc, a) => acc + a, 0);

		var sum2 = await Enumerable.Range(1, 4).ToFx()
			.Filter(IsOdd)
			.Map(a => Delay(100, a * 10))
			.To(iterable => Fx.ToAsync(iterable).ToFx())
			.Reduce((acc, a) => acc + a, 0);

		WriteLine(sum2);

		var sum3 = await Enumerable.Range(1, 4).ToFx()
			.Filter(IsOdd)
			.Map(a => Delay(100, a * 10))
			.ToAsync()
			.Reduce((acc, a) => acc + a, 0);

		WriteLine(sum3);

		var sum4 = await Enumerable.Range(1, 4).ToFx()
			.Filter(IsOdd)
			.Map(a => Delay(100, a * 10))
			.To(FromAsync)
			.Then(task => task.ToFx().Reduce((acc, a) => acc + a, 0));

		WriteLine(sum4);
	}



}
