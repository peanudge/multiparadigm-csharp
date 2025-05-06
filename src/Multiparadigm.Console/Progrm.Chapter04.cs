
using FxCs;

public static partial class Program
{
	public static Task<T> Delay<T>(int time, T value)
		=> Task.Delay(time).ContinueWith((_) => value);

	// https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/chaining-tasks-by-using-continuation-tasks#about-continuations
	public static void DelayExample()
	{
		WriteLine("Start");
		var task = Delay(1000, "Hello, World")
			.ContinueWith(antecedent =>
			{
				WriteLine(antecedent.Result);
				WriteLine("End");
			});

		task.Wait();
	}

	public static void PromiseRaceExample()
	{
		/*
		const promise1 = new Promise((resolve) => setTimeout(resolve, 500, 'one'));
		const promise2 = new Promise((resolve) => setTimeout(resolve, 100, 'two'));
		await Promise.race([promise1, promise2]).then(value => { console.log(value) });
		*/

		Task.WhenAny([Delay(500, "one"), Delay(100, "Two")])
			.ContinueWith((whenAnyTask) =>
			{
				WriteLine(whenAnyTask.Result.Result);
			}).Wait();
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
}
