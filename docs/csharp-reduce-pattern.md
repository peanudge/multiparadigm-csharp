# 대표적인 리스트 프로세싱 패턴인 "Reduce"

25.04.30 23:50

이번에는 리스트 프로세싱 패턴 중 map-reduce (변형-누적) 패턴의 `reduce(...)` 함수를 C#으로 직접 구현해보는 과정을 진행해보겠습니다.

"멀티패러다임 플밍" 책에서는 Typescript의 함수 오버로딩을 소개하면서 Built-In으로 제공하는 array.reduce(...) 의 시그니처를 똑같이 제공하는 과정을 보여줍니다. 이 과정에서 "reduce의 에러 관리" 라는 특수한 예외 상황에 어떻게 대응해야하는지를 경험해볼 수 있습니다.

JS/TS 로 만든 결과물과 자체적으로 오버로딩을 제공하는 C# 언어로 만든 결과물을 한번 비교하고 차이를 느껴보시죠!

## 값을 계속 누적해주는 Reduce 함수 - C#

우선, 간단히 초기값을 항상 명시적으로 넣어주는 방식으로 만들어보겠습니다.

```cs

static Acc Reduce<A, Acc>(Func<Acc, A, Acc> f, Acc acc, IEnumerable<A> iterable)
{
   foreach (var a in iterable)
   {
       acc = f(acc, a);
   }
   return acc;
}
```

책에서 했던 것처럼 초기값을 받지않고 첫번째 요소를 초기값으로 누적해주는 함수 형태도 제공해보겠습니다.

첫번째 요소를 초기값으로 쓰려면 특별한 예외에 대해 어떻게 처리할 지 고민해야합니다. 처리방식에 여러 방향이 있겠지만 JS array.reduce 방식을 그대로 따라가겠습니다.

첫 요소가 존재하지 않을 경우 예외를 던진다.

(TS로 구현체와는 달리 C# 는 그냥 동일한 이름의 함수를 다른 시그니처로 정의할 수 있어 오버로딩이 더 자연스럽습니다. )

위와같이 특별한 상황을 다루려면 첫 요소만 뽑아서 체크하고 나머지는 초기값을 받는 시그니처를 가진 함수로 처리하는 것이 자연스러워보입니다.

여기서 iterable에서 iterator를 얻어서 반복하는 예제를 연습한 보람이 느껴지는 순간이 옵니다!

(굳이, 편리하게 foreach나 전개연산으로 하지않고 왜 iterator를 직접 돌리는 방법이 필요한지를 아는 순간)

```cs
static A Reduce<A>(Func<A, A, A> f, IEnumerable<A> iterable)
{
    var iterator = iterable.GetEnumerator();
    if (!iterator.MoveNext())
    {
        throw new InvalidOperationException("no element");
    }

    return BaseReduce(f, iterator.Current, iterator);
}

static Acc Reduce<A, Acc>(Func<Acc, A, Acc> f, Acc acc, IEnumerable<A> iterable)
{
    return BaseReduce(f, acc, iterable.GetEnumerator());
}

static Acc BaseReduce<A, Acc>(Func<Acc, A, Acc> f, Acc acc, IEnumerator<A> iterator)
{
    while (iterator.MoveNext())
    {
        acc = f.Invoke(acc, iterator.Current);
    }
    return acc;
}
```

위와같이 지연 이터레이터를 잘 활용한다면 초기값을 명시적으로 전달하는 방식과 생략하는 방식 둘 다를 지원하는 `Reduce(...)` 함수를 C#으로 정의할 수 있습니다.

## C#의 Enumerable.Aggregate

대부분의 멀티패러다임 언어에서 누적기를 제공합니다. C# 도 저희가 구현한 Reduce와 거의 동일한 함수를 Aggregate 라는이름으로 제공합니다.

물론, 초기값 유무에 따른 다른 Aggregate 구현체를 오버로딩 함수들로 제공합니다.

## 만든 Reduce를 활용

아래는 C#를 이용해 TS/JS로 만든 예제들과 최대한 유사한 방식으로 구현하려 노력해보았습니다.

```cs
int[] array = [1, 2, 3];
var sum = Reduce((acc, a) => acc + a, 0, array);
WriteLine(sum);

string[] strings = ["a", "b", "c"];
var abc = Reduce((acc, a) => $"{acc}{a}", string.Empty, strings);
WriteLine(abc);

int[] array2 = [1, 2, 3];
var sum2 = Reduce((a, b) => a + b, array2);

string[] words = ["hello", "beautiful", "world"];

WriteLine(Reduce((a, b) => $"{a} {b}", words));

// C# Enumerable.Aggregate
WriteLine(words.Aggregate((a, b) => $"{a} {b}"));
```

멀티패러다임을 잘 반영한 코드들은 마치 의사코드(pseudo code)를 활용하는 것처럼 다른 언어 소프트웨어 엔지니어들과도 무리 없이 소통할 수 있는 방향으로 나아갈 수 있게 해주지 않을까요?

## 정리

저희가 책을 통해 연습하는 실습들이 SDK 제작하는 과정에 참여하는 듯한 느낌을 주지않나요? 정말 경험하기 드문 과정을 통해 통찰력을 기를 수 있는 것 같습니다.
