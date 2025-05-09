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
		=> Delay(size, new File(name, "...", size));

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
}
