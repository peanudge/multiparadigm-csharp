using FxCs;

public static class PaymentExample
{
	private static async Task<T> Delay<T>(int time, T value)
	{
		await Task.Delay(time);
		return value;
	}

	public record Payment(string PgUid, long StoreOrderId, int Amount);
	private record PaymentCancelResult(int Code, string Message, string PgUid);
	private class PgApi
	{
		private Payment[][] _pgDataPaymentPages = [
			[
				new Payment("PG11", 1, 15000),
				new Payment("PG12", 2, 25000),
				new Payment("PG13", 3, 10000),
			],
			[
				new Payment("PG14", 4, 25000),
				new Payment("PG15", 5, 45000),
				new Payment("PG16", 6, 15000),
			],
			[
				new Payment("PG17", 7, 20000),
				new Payment("PG18", 8, 30000),
			]
		];

		public async Task<Payment[]> GetPayments(int page)
		{
			WriteLine($"결제 내역 요청: https://pg.com/payment?page={page}");
			await Task.Delay(500);

			var payments = _pgDataPaymentPages.Length >= page ? _pgDataPaymentPages[page - 1] : [];
			WriteLine($"{payments.Length}개: {string.Join(",", payments.Select(p => p.PgUid))}");
			return payments;
		}

		public async Task<PaymentCancelResult> CancelPayment(string pgUid)
		{
			WriteLine($"취소 요청: {pgUid}");
			await Task.Delay(300);
			return new PaymentCancelResult(
				Code: 200,
				Message: $"{pgUid}: 취소 및 환불 완료",
				PgUid: pgUid
			);
		}

		public async Task<int> GetPageCount()
		{
			WriteLine("페이지 카운트 요청: http://pg.com/payments/page-count");
			await Task.Delay(50);
			return _pgDataPaymentPages.Length;
		}
	}

	private record Order(long Id, int Amount, bool IsPaid);
	private class StoreDB
	{
		/// <summary>
		/// 결제완료된 주문 데이터를 필터링하여 반환
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public async Task<Order[]> GetOrders(long[] ids)
		{
			if (ids.Length > 5)
			{
				throw new Exception($"ID 개수 초과: 최대 5가지까지 요청할 수 있습니다. (전달된 개수: {ids.Length})");
			}

			WriteLine($"SELECT * FROM orders WHERE IN ({string.Join(",", ids)}) AND is_paid = true;");
			await Task.Delay(100);
			return [
				new Order(Id: 1, Amount: 15000, IsPaid: true),
				new Order(Id: 2, Amount: 10000, IsPaid: true),
				new Order(Id: 3, Amount: 45000, IsPaid: true),
				new Order(Id: 4, Amount: 20000, IsPaid: true),
				new Order(Id: 5, Amount: 30000, IsPaid: true),
			];
		}
	}

	public static async Task<Payment[]> SyncPayments()
	{
		var RATE_LIMIT = 2;
		var pgApi = new PgApi();
		var totalPage = await pgApi.GetPageCount();

		var payments = await Enumerable.Range(1, totalPage)
			.Chunk(RATE_LIMIT) // instead of concurrent function
			.Select(pages => pages.Select(pgApi.GetPayments))
			.Select(Task.WhenAll)
			.ToAsyncEnumerable()
			.SelectMany(paymentsGroup => paymentsGroup.Flat())
			.ToArray();

		var storeDb = new StoreDB();
		var orders = await payments
			.Select(p => p.StoreOrderId)
			.Chunk(5)
			.ToAsyncEnumerable()
			.Select(storeDb.GetOrders)
			.Flat()
			.ToArray();

		var ordersById = orders.Select(order => order.Id).ToHashSet();

		await payments
		   .ToAsyncEnumerable()
		   .Reject(p => ordersById.Contains(p.StoreOrderId))
		   .ForEach(async p =>
		   {
			   var cancelResult = await pgApi.CancelPayment(p.PgUid);
			   WriteLine(cancelResult.Message);
		   });

		return payments;
	}

	private static IEnumerable<DateTimeOffset> InfinityTime()
	{
		while (true)
		{
			yield return DateTimeOffset.Now;
		}
	}

	public static async Task RunScheduler()
	{
		await InfinityTime()
			.ToAsyncEnumerable()
			.ForEach(async now =>
			{
				WriteLine();
				WriteLine($"{now} - Scheduled");
				WriteLine();
				await Task.WhenAll([Task.Delay(10000), SyncPayments()]);
			});
	}
}


