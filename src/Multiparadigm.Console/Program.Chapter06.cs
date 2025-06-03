using System.Diagnostics;
using static TemplateEngine;

public partial class Program
{
	public static void TaggedTemplateExample()
	{
		var a = "A";
		var b = "B";
		var c = "C";
		WriteLine(Html_Naive($"<b>{a}</b><i>{b}</i><em>{c}</em>"));
	}

	public static void EsacpeHtmlText()
	{
		// WriteLine("\t" + HtmlHelper.Escape("<script>alert(\"XSS\")</script>"));
		// WriteLine("\t" + HtmlHelper.Escape("Hello & World! \"Have\" a nice day <3"));

		var a = "<script>alert(\"XSS\")</script>";
		var b = "Hello & Welcome!";

		WriteLine(
			Html_Naive($"""
				<ul>
					<li>{a}</li>
					<li>{b}</li>
				</ul>
				"""));
	}


	private record Menu(string Name, decimal Price);

	public static void NestDataComponentExample()
	{
		var menu = new Menu("Choco Latte & Cooki", 8_000);
		var menuHtml = (Menu menu) => Html_Naive($"<li>{menu.Name} ({menu.Price})</li>");
		var a = "<script>alert(\"XSS\")</script>";
		var b = "Hello & Welcome!";

		var result = Html_Naive($"""
		<ul>
			<li>{a}</li>
			<li>{b}</li>
			{menuHtml(menu)}
			{Html_Naive($"<li>{Html_Naive("<b>3단계 중첩</b>")}</li>")}
		</ul>
		""");
		WriteLine(result);
	}


	public static void NestDataComponentExample_Solution()
	{
		var menu = new Menu("Choco Latte & Cooki", 8_000);
		var menuHtml = (Menu m) => Html($"<li>{m.Name} ({m.Price})</li>");
		var a = "<script>alert(\"XSS\")</script>";
		var b = "Hello & Welcome!";

		var result = Html($"""
		<ul>
			<li>{a}</li>
			<li>{b}</li>
			{menuHtml(menu)}
			{Html($"<li>{Html("<b>3단계 중첩</b>")}</li>")}
		</ul>
		""");
		WriteLine(result.ToHtml());
	}

	public static void HtmlFromArrayExample()
	{
		var menuHtml = (Menu menu) => Html($"<li>{menu.Name} ({menu.Price})</li>");
		var menus = new Menu[]{
			new Menu("아메리카노", 4500),
			new Menu("카푸치노", 5000),
			new Menu("라떼 & 쿠키 세트", 8000),
		};

		var menuBoardHtml = (Menu[] menus) => Html($"""
		<div>
			<h1>메뉴 목록</h1>
			<ul>
				{menus.Select(menuHtml).Aggregate("", (a, b) => a + b.ToHtml())}
			</ul>
		</div>
		""");
		WriteLine(menuBoardHtml(menus).ToHtml());
	}

	public static void HtmlFromArrayExample_Solution_1()
	{
		var menuHtml = (Menu menu) => Html($"<li>{menu.Name} ({menu.Price})</li>");
		var menus = new Menu[]{
			new Menu("아메리카노", 4500),
			new Menu("카푸치노", 5000),
			new Menu("라떼 & 쿠키 세트", 8000),
		};

		// INFO: HtmlComponent간의 결합 방법을 제공해서 Escape를 구분.
		var menuBoardHtml = (Menu[] menus) => Html($"""
		<div>
			<h1>메뉴 목록</h1>
			<ul>
				{menus.Select(menuHtml).Aggregate((a, b) => Html($"{a}{b}"))}
			</ul>
		</div>
		""");

		WriteLine(menuBoardHtml(menus).ToHtml());
	}

	public static void HtmlFromArrayExample_Solution_2()
	{
		var menuHtml = (Menu menu) => Html($"<li>{menu.Name} ({menu.Price})</li>");
		var menus = new Menu[]{
			new Menu("아메리카노", 4500),
			new Menu("카푸치노", 5000),
			new Menu("라떼 & 쿠키 세트", 8000),
		};

		// INFO: HtmlComponent간의 결합 방법을 제공해서 Escape를 구분.
		var menuBoardHtml = (Menu[] menus) => Html($"""
		<div>
			<h1>메뉴 목록</h1>
			<ul>
				{menus.Select(menuHtml)}
			</ul>
		</div>
		""");

		WriteLine(menuBoardHtml(menus).ToHtml());
	}

	public static async Task RunTasksWithPoolByGpt()
	{
		Func<Task<string>>[] tasks = [
			CreateAsyncTask("A", 1000),
			CreateAsyncTask("B", 500),
			CreateAsyncTask("C", 800),
			CreateAsyncTask("D", 300),
			CreateAsyncTask("E", 1200),
		];

		var poolSize = 2;
		var results = await RunTasksWithPool_ByGPT(tasks, poolSize);
		WriteLine($"Results: {string.Join(", ", results)}");
	}


	static Func<Task<string>> CreateAsyncTask(string name, int ms, bool broken = false)
	{
		return async () =>
		{
			// WriteLine($"Started: {name}");
			await Task.Delay(ms);
			// WriteLine($"Finshed: {name}");
			if (broken)
			{
				throw new Exception("No");
			}
			return name;
		};
	}


	//https://kwangyulseo.wordpress.com/2015/06/25/%ec%99%9c-%eb%b3%80%ec%88%98%ea%b0%80-%eb%82%98%ec%81%9c%ea%b0%80/
	static async Task<T[]> RunTasksWithPool_ByGPT<T>(Func<Task<T>>[] funcs, int poolSize)
	{
		T[] results = new T[funcs.Length];
		List<Task> activeTasks = [];

		for (var i = 0; i < funcs.Length; i++)
		{
			var taskFactory = funcs[i];
			var task = Task.Run(async () =>
			{
				var n = i;
				results[n] = await taskFactory();
				WriteLine($"results[{n}]: {results[n]}");
			});

			activeTasks.Add(task);

			if (activeTasks.Count() >= poolSize)
			{
				var finshedTask = await Task.WhenAny(activeTasks);
				activeTasks.Remove(finshedTask);
			}
		}

		await Task.WhenAll(activeTasks);
		return results;
	}



	public static async Task RunTasksWithPool_MultiParadigm()
	{
		Func<Task<string>>[] tasks = [
			CreateAsyncTask("A", 1000),
			CreateAsyncTask("B", 500),
			CreateAsyncTask("C", 800),
			CreateAsyncTask("D", 300),
			CreateAsyncTask("E", 1200),
		];

		var poolSize = 2;
		var results = await RunTasksWithPool(tasks, poolSize);
		WriteLine($"Results: {string.Join(", ", results)}");
	}

	private static async Task<T[]> RunTasksWithPool<T>(Func<Task<T>>[] fs, int poolSize)
	{
		var tasks = fs.Select(f => new TaskRunner<T>(f)).ToArray();
		List<TaskRunner<T>> pool = [];
		foreach (var nextTask in tasks)
		{
			// Pool에 작업을 PoolSize 만큼 추가
			pool.Add(nextTask);
			if (pool.Count < poolSize) continue;
			// 현재 풀에 있는 작업을 시작하고 하나가 끝날 때까지 대기
			await Task.WhenAny(pool.Select((task) => task.Run()));
			// 완료된 작업 제거
			var completedTask = pool.FirstOrDefault(task => task.IsDone);
			if (completedTask is not null)
			{
				pool.Remove(completedTask);
			}
		}
		return await Task.WhenAll(tasks.Select(task => task.Task));
	}

	private class TaskRunner<T>
	{
		private Func<Task<T>> _f;
		private Task<T>? _task = null;
		public Task<T> Task => _task ?? Run();
		private bool _isDone = false;
		public bool IsDone => _isDone;

		public TaskRunner(Func<Task<T>> f)
		{
			_f = f;
		}

		public Task<T> Run()
		{
			if (_task != null)
			{
				return _task;
			}

			return _task = _f().ContinueWith(t =>
			{
				_isDone = true;
				return t.Result;
			});
		}
	}


	public static async Task InfinityTaskPoolExample()
	{
		var result = await new TaskPool<int>(
				fs: InfinityRange().Select(CrawlingFn),
				poolSize: 5)
			.RunAllSettled();

		result.ForEach((t) => { WriteLine($"Task {t.Result} - {t.Status}"); });
	}
	private static IEnumerable<int> InfinityRange() => Enumerable.Range(1, int.MaxValue);
	private static Func<Task<int>> CrawlingFn(int page) => () => Crawling(page);

	private static async Task<int> Crawling(int page)
	{
		WriteLine($"{page} 페이지 분석 시작");
		await Task.Delay(5000);
		WriteLine($"{page} 페이지 저장 완료");
		return page;
	}

	private class TaskPool<T>
	{
		private IEnumerator<TaskRunner<T>> _tasks;
		private int _poolSize;
		private List<TaskRunner<T>> _pool = [];

		public TaskPool(IEnumerable<Func<Task<T>>> fs, int poolSize)
		{
			_tasks = fs.Select(f => new TaskRunner<T>(f)).GetEnumerator();
			_poolSize = poolSize;
		}

		public void SetPoolSize(int poolSize) { _poolSize = poolSize; }

		private bool CanExpandPool()
		{
			return _pool.Count < _poolSize;
		}

		public async Task<Task<T>[]> Run(Action<Exception>? errorHandler = null)
		{
			List<TaskRunner<T>> resultTasks = [];
			while (true)
			{
				var hasNextTask = _tasks.MoveNext();
				if (hasNextTask)
				{
					_pool.Add(_tasks.Current);
					resultTasks.Add(_tasks.Current);
					if (CanExpandPool()) continue;
				}

				if (!hasNextTask && _pool.Count == 0) break;

				var completedTask = await Task.WhenAny(_pool.Select(task => task.Run()));
				try
				{
					await completedTask;
				}
				catch (AggregateException ex)
				{
					errorHandler?.Invoke(ex.InnerExceptions[0]);
				}

				_pool.Remove(_pool.First(task => task.IsDone));
			}

			return resultTasks.Select(task => task.Task).ToArray();
		}

		public async Task<T[]> RunAll()
		{
			var tasks = await Run(errorHandler: (error) => { throw error; });
			return await Task.WhenAll(tasks);
		}

		public Task<Task<T>[]> RunAllSettled() => Run(error => { /* Nothing */ });
	}


	public static async Task RunAllTestExample()
	{
		Func<Task<string>>[] taskFuncs = [
			CreateAsyncTask("A", 1000),
			() => CreateAsyncTask("B", 1000, broken: true)(),
			CreateAsyncTask("C", 1000),
			CreateAsyncTask("D", 1000),
			CreateAsyncTask("E", 1000),
		];

		try
		{
			var result = await new TaskPool<string>(taskFuncs, 2).RunAll();
			WriteLine(string.Join(", ", result));
		}
		catch (Exception e)
		{
			WriteLine(e.Message);
		}
	}

	public static async Task RunAllSettledTestExample()
	{
		var stopWatch = new Stopwatch();
		stopWatch.Start();
		Func<Task<string>>[] taskFuncs = [
			CreateAsyncTask("A", 1000),
			() => CreateAsyncTask("B", 300, broken: true)(),
			CreateAsyncTask("C", 1000),
			CreateAsyncTask("D", 1000),
			CreateAsyncTask("E", 1000),
		];

		var tasks = await new TaskPool<string>(taskFuncs, 3).RunAllSettled();
		stopWatch.Stop();
		WriteLine($"Elapsed: {stopWatch.ElapsedMilliseconds} ms");
		tasks.ForEach(task => WriteLine(task.Status));
	}

	private static Func<Task<int>> CreateCrawler(int page)
		=> () => page == 7
			? Task.FromException<int>(new Exception($"{page}"))
			: Crawling(page);

	public static async Task RunAllTestExample2()
	{
		try
		{
			await new TaskPool<int>(InfinityRange().Select(CreateCrawler), 5).RunAll();
		}
		catch (Exception e)
		{
			WriteLine($"Crawling 중간에 실패! ({e.Message} Page)");
		}
	}

	public static async Task RunAllSettledTestExample2()
	{
		var stopWatch = new Stopwatch();
		stopWatch.Start();
		WriteLine($"{stopWatch.Elapsed} - Start");
		var taskPool = new TaskPool<int>(InfinityRange().Select(CreateCrawler), 2);
		_ = taskPool.RunAllSettled();

		await Task.Delay(10_000);

		taskPool.SetPoolSize(10);
		WriteLine($"{stopWatch.Elapsed} - PoolSize Changed To {5}");

		await Task.Delay(10_000);

		WriteLine($"{stopWatch.Elapsed} - End");
	}
}
