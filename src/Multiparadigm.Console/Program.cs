// 5.1.1
// Two_Demension();

// 5.1.2
// HandleBasketBallData();

// 5.1.3
// HandleCommerceData1();
// HandleCommerceData2();


// 5.2.1
// await FunctionCompositionUsingPipe();
// ZipFunction();

// 5.2.5 Collatz Conjecture
// WriteLine(CollatzConjecture(1));
// WriteLine(CollatzConjecture(4));
// WriteLine(CollatzConjecture(5));

// InsteadBreakExample();



await Enumerable.Range(1, 10)
	.Select(n => Delay(1000, n))
	.ToAsyncEnumerable()
	.Select(n => n * 2)
	.ForEach(WriteLine);
