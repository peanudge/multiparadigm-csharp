using System.Collections.Generic;
using System.Threading.Tasks;

public class TaskHelpers
{
	public static Task<T> Delay<T>(int time, T value)
		=> Task.Delay(time).ContinueWith((_) => value);


	public enum TaskStatus
	{
		Pending = 0,
		Fulfilled = 1,
		Rejected = 2,
	}

	public record SettleResult<T>(TaskStatus Status, T? Value, string? Reason);

	public static async Task<SettleResult<T>> SettleTask<T>(Task<T> task)
	{
		try
		{
			var result = await task;
			return new SettleResult<T>(TaskStatus.Fulfilled, result, null);
		}
		catch (Exception error)
		{
			return new SettleResult<T>(TaskStatus.Rejected, default, error.Message);
		}
	}

	public static async Task<SettleResult<T>[]> AllSettled<T>(Task<T>[] tasks)
	{
		return await Task.WhenAll(tasks.Select(SettleTask));
	}


	public static Task<TResult> WhenAnySuccess<TResult>(Task<TResult>[] tasks)
	{
		List<Task<TResult>> remainedTasks = [.. tasks];
		List<Task<TResult>> failedTasks = [];

		while (remainedTasks.Count > 0)
		{
			var completedTask = Task.WhenAny(remainedTasks).Result;
			if (completedTask.IsCompletedSuccessfully)
			{
				return completedTask;
			}

			failedTasks.Add(completedTask);
			remainedTasks.Remove(completedTask);
		}

		throw new Exception("All tasks were rejected");
	}

	public static async Task<TResult[]> FromAsync<TResult>(IEnumerable<Task<TResult>> iterable)
	{
		List<TResult> results = [];
		foreach (var task in iterable)
		{
			results.Add(await task);
		}
		return results.ToArray();
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


public static class TaskExtensions
{

	public static Task<TResult2> Then<TResult1, TResult2>(
		this Task<TResult1> task,
		Func<TResult1, TResult2> resolve)
	{
		return task.ContinueWith((preTask) =>
		{
			return resolve(preTask.Result);
		}, TaskContinuationOptions.OnlyOnRanToCompletion);
	}

	public static Task<TResult2> Then<TResult1, TResult2>(
			this Task<TResult1> task,
			Func<TResult1, Task<TResult2>> resolve)
	{
		return task.ContinueWith((preTask) =>
		{
			return resolve(preTask.Result).Result;
		}, TaskContinuationOptions.OnlyOnRanToCompletion);
	}
}
