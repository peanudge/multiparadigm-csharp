# JS/TS와 조금 다른 C#에서의 반복자 패턴

25.04.20 17:57 (Edited)

멀티 패러다임 종이 책을 구매하고 다시 정독을 하면서 C#으로 동일한 컨텐츠를 즐겨보자는 마음으로 1장을 펼쳤습니다!

TS/JS 의 Iteration Protocol과 유사하지만 C#은 Iterator의 형태가 조금씩 다름을 볼 수 있습니다.

반복자 타입 이름이Iterator 가 아니라 Enumerator 공식문서에서도 "iterator 패턴을 c#에서 구현하는 방법"으로 Enumerator를 소개하는 것을 보면 단순히 언어 개발 단계에서 용어 결정이 지금까지 이어온 것이라고 합니다. (Enumerate는 순회에 대한 더욱 집합론적 수학 표현)
여기서는 편의상 Enumerator라고 표현하겠습니다.
C#의 Enumerator는 MoveNext() 호출 후 직접 값을 꺼내야한다. (즉, JS의 itertor의 응답값으로 다음 값을 주는 형태는 아님) 직접 Enumerator를 구현할 경우, 꼭 MoveNext()를 적어도 한번은 호출해줘야 유의미한 요소를 순회할 수 있음을 암시합니다.

```cs
var iterator = Natural().GetEnumerator();

Console.WriteLine(iterator.Current); // 0
Console.WriteLine($"next: {iterator.MoveNext()}, value: {iterator.Current}"); // 1
Console.WriteLine($"next: {iterator.MoveNext()}, value: {iterator.Current}"); // 2
Console.WriteLine($"next: {iterator.MoveNext()}, value: {iterator.Current}"); // 3
Console.WriteLine($"next: {iterator.MoveNext()}, value: {iterator.Current}"); // 4

// JS/TS의 Generator 방식
IEnumerable<int> Natural()
{
    var n = 0;
    while (true)
    {
        yield return ++n;
    }
}
```

이어서 1.1.2 ArrayLike로부터 Iterator 생성하기 에서 ArrayLikeIterator를 구현해보았습니다.

```cs
public class ArrayLikeIterator : IEnumerator<uint>
{
private int \_position = -1;

    private readonly uint[] _array;

    public ArrayLikeIterator(uint[] array)
    {
        _array = array;
    }

    public uint Current => _array[_position];

    public bool MoveNext()
    {
        _position++;
        return _position < _array.Length;
    }

    public void Reset() { _position = -1; }

    object IEnumerator.Current => Current;
    public void Dispose() { }

}
```

뭔가 이상합니다. 위와 같은 C# Enumerator 의 특성은 몇가지 의문을 줍니다. MoveNext() 를 호출하기전에 Current를 조회하면 어떻게 되야하나? 예외를 던져야하나? (대부분의 구현체에서 예외를 던짐) 아니면 첫번째 요소를 던져도되나?

이러한 의문들은 공식문서를 자세히 확인해보니 비로소 해결됩니다.

```
IEnumerator.Current is undefined under any of the following conditions:

The enumerator is positioned before the first element in the collection, immediately after the enumerator is created. MoveNext must be called to advance the enumerator to the first element of the collection before reading the value of Current.
The last call to MoveNext returned false, which indicates the end of the collection.
The enumerator is invalidated due to changes made in the collection, such as adding, modifying, or deleting elements.
```

Enumerator가 생성된 초기에는 초기 요소 이전을 가리켜야한다고 합니다. 위 3가지 조건 또한 C#에서 지켜져야할 iteration protocol이라고 볼수 있습니다.

IEnumerator.Reset() 의 존재를 보고 Iterator는 한번 반복되고 사라지는 존재가 아니었나라는 의문이 생겼습니다.
호환성을 위해 구현되었을 뿐,구현이 필수는 아니라고 합니다.

정리하자면,

TS/JS, C# 두 언어에서 모두 Iterator 기본 개념을 잘 준수하고 있어서 언어의 장벽없이 같은 개념을 쉽게 학습할 수 있었습니다. 하지만 이 글에서 다뤘듯이 C#에서 Iterator인 Enumerator를 구현시 추가적으로 주의해야할 부분이 있었습니다.

MoveNext() 를 값 조회전에 꼭 호출해줘야하고 그렇지않고 값 조회를 시도하면 대부분의 구현체들은 예외를 던집니다.
다음 단계로 지연평가되는 map함수를 C# 버젼으로 만들면서 일급함수와 고차함수를 활용해보겠습니다. 배울게 많이 있을듯합니다.
