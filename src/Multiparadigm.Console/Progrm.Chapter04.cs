using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using FxCs;
using static TaskHelpers;

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
}
