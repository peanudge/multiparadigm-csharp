# C#으로 구현한 Iterable 고차함수들

25.04.26 00:06 (Edited)

JS/TS 로 책을 완독하고 꼭 주 언어로 책을 다시 학습하고 싶다는 열망이 생겼었는데... 첫 도전으로 멀티패러다임에 언어의 장벽이 없이 1장을 잘 마무리했습니다.

.NET이 JS/TS에 비해 "언어와 이터러블의 상호작용"이 부족해보이는 부분도 잇었습니다. e.g. 구조분해 할당이 불가능한점.

그래도 IEnumerable 즉, 이터레이터 프로토콜이 언어 전반에 잘 반영되어있어서 아래와 같이 책에 나온 예시들과 거의 동일한 코드들을 만들수 있었습니다.

```cs

static IEnumerable<int> Naturals(int end = int.MaxValue)
{
    var n = 1;
    while (n <= end)
    {
        yield return n++;
    }
}

static IEnumerable<T> Reverse<T>(T[] array)
{
    var idx = array.Length;
    while (idx > 0)
    {
        yield return array[--idx];
    }
}

static IEnumerable<B> Map<A, B>(Func<A, B> f, IEnumerable<A> iterable)
{
    foreach (var value in iterable)
    {
        yield return f(value);
    }
}

static void ForEach<A>(Action<A> f, IEnumerable<A> iterable)
{
    foreach (var value in iterable)
    {
        f(value);
    }
}

static IEnumerable<A> Filter<A>(Func<A, bool> f, IEnumerable<A> iterable)
{
    foreach (var value in iterable)
    {
        if (f(value))
        {
            yield return value;
        }
    }
}
```

마지막으로 최종적으로 IEnumerable 로 소통하는 함수들을 조합하는 고차함수!

```cs
ForEach(WriteLine,
  Map(x => x * 10,
    Filter(x => x % 2 == 1,
      Naturals(5))));
```

멀티패러다임 프로그래밍에서 "Typescript"가 빠진 것이 맞다는 느낌이 드네요. 7장까지 다해보고 판단해야겠지만요.

- 추가로 재밌는 점은 .NET에서 Generator라는 이름이 아닌 "Iterator Method" 라는 호칭으로 불리네요. 그리고 Iterator Method는 Iterator를 만들어줄 수도 있고 Iterable을 만들어줄 수도 있습니다. 꽤 많이 다른 도구들을 제공하는 모습을 보여줍니다.\* 참고로 C#의 "var"는 js의 "var" 와 전혀 관련없음 주의..
