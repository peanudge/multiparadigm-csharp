# C#은 이터러블과 어느정도까지 상호작용할까?

25.04.28 23:09 (Edited)

멀티패러다임을 책의 1.3.2 언어와 이터러블의 상호작용 를 공부하면서 Iterable protocol을 준수한 결과물들이 언어의 다양한 기능과 상호작용하는 동작을 보면 굉장히 우아하다는 느낌을 받았었습니다.

그래서 C++++ 라는 언어에서는 얼마나 언어차원에서 Iterable와 상호작용하는지 궁금했습니다. 그래서 직접 만든 iterable을 생성해 언어 기능들과 결합 시켜보려합니다.

```cs
ForEach(WriteLine,
    Map(x => x * 10,
        Filter(x => x % 2 == 1,
            Naturals(5))));
```

이런 결과물을 얻는 것은 멀티패러다임 플밍 책만 따라하면 쉽게 얻을 수 있었습니다. 저는 여기에 더해서 이미 제공중이던 C#의 System.Linq 도구들과 결합하면서 그 활용도를 넓혀보려합니다.

## LINQ도구들과 결합하기

인동님의 “C#은 애초부터 멀티패러다임 언어였다”는 말을 다시 이해하게되었습니다. C# 버젼의 멀티패러다임 예제들을 만들다보면 이미 도구들은 충분히 준비되어있었다는 것을 다시 깨닫게 됩니다. 결합의 시작 부분과 끝 부분을 언어에서 제공하는 Enumerable 클래스의 확장 method들을 결합해봅시다.- Natural() 를 Enumerable.Range() 로 교체- 출력 처리를 직접 만든 ForEach(f, iterable)이 아닌 Enumerable.ForEach() 로 교체

```cs
​Map(x => x \* 10, Enumerable.Range(1, 10)).ToList().ForEach(WriteLine);
```

흥미롭지 않나요? 책에서 배웠던 것들이 너무 깔끔하게 기존 C# 언어가 제공되는 것들과 너무 자연스럽게 이어집니다.

중간 `Map(..)` Iterator 함수만 저희가 구현한 것이죠. 이런 언어 장벽을 뛰어넘는 예제들임을 눈으로 확인할 때마다 멀티패러다임 프로그래밍 책 중후반부에 나오는 가치 있는 예제들을 충분히 유사하게 구현할 수 있을 것 같은 용기가 생깁니다.

## FxIterable 과 같은 녀석 - Enumerable

위 예제를 만들면서 직접 구현한 `Natural()` 과 얼마나 비슷할까? 궁금해서 `Enumerable.Range()` 구현체를 살펴봤습니다.

잠깐! 책을 통해 구현해봤던 FxIterable 이라는 class와 유사하게 생긴 것들이 많이 보입니다. Method Signature은 조금 다르지만 본질적으로 목표는 유사해보입니다. 모든 언어가 하나를 향해가는 느낌이 듭니다.

.NET 생태계 차원에서, 아니 SDK 차원에서 FxIterable와 유사한 것을 제공하는 모습은 멀티패러다임이 옳은 길임을 확신하게 됩니다.

## JS/TS의 Spread 연산자 같은 C#의 Spread Element 표현

JS에서는 애용하고 편리한 Spread Syntax 가 있다면! 조금 유사한 C#에서는 열거형 데이터를 생성할 때 사용되는 Spread Element 라는 것이 있습니다. (C#12 부터)

```cs
int[] numbers = [.. Map(x => x * 10, Filter(x => x % 2 == 1, Naturals(5)))];
```

concatenate 작업를 간단하게 선언적인 표현으로 처리할 수 있어서 익숙해진다면 유용해보입니다.

다만, spread 되어 나올 결과 값을 명확히 지정해줘야해서 강타입 언어의 특성을 보여줍니다. TS는 array 타입으로 바로 추론을 해줍니다. C#은 아마 spread element를 통해 다양한 타입(array, List, Span ...etc)으로 결과를 만들 수 있기에 바로 추론을 해주지 못하는 모습을 보입니다. make sense!

> ....can be assigned to many different collection types.

## 정리하면...

C#으로 멀티패러다임 플밍 공부하기를 진행 하면서 아직 책의 2장도 못 넘어갔는데 배울게 굉장히 많습니다.

"언어를 이렇게 진지하게 공부해본 적이 있었는가?" 스스로 회고하게 됩니다. :)
