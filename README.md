# ItemStack Container

아이템 스택을 슬롯 단위로 담는 고정 크기 컨테이너입니다. UI는 포함하지 않으며, 추가/제거/스왑/조회 같은 핵심 로직만 제공합니다. 크기는 생성 시점에 고정되고, 정렬이나 압축 같은 정책은 패키지가 강제하지 않으며 `Slots`를 이용해 호출부에서 직접 구현합니다. 멤버로 들고 사용으로 쓰는 걸 전제로 설계했습니다.

[ Usage ](#usage) <br>
[ API ](#api)

<br>
<br>
<br>


## 🔧Usage

<br>

#### Item 정의
`IItem`을 구현해 아이템의 정체성(`Key`)과 최대 스택 수(`MaxStackCount`)를 정의합니다.

```cs

public class WeaponItem : IItem
{
    public string Key { get; private set; }
    public int MaxStackCount { get; private set; }

    public WeaponItem(string key, int maxStackCount)
    {
        Key = key;
        MaxStackCount = maxStackCount;
    }
}

```

<br>
<br>

#### 기본 사용

```cs

var container = new ItemStackContainer<WeaponItem>(20);

var leftover = container.Add(sword, 1);
container.Remove(sword, 1);
container.Swap(0, 3);

```

<br>
<br>

#### 슬롯 직접 접근

```cs

var slot = container.Get(3);
if (slot != null && !slot.IsEmpty)
{
    // ...
}

```

<br>
<br>

#### 슬롯 비우기 / 꺼내기

```cs

container.Clear(3);      // 특정 슬롯만 비움
container.ClearAll();    // 전체 비움

var taken = container.Take(3);   // 특정 슬롯 내용을 꺼내면서 비움
var all = container.TakeAll();   // 채워진 슬롯 전체를 꺼내면서 비움

```

<br>
<br>

#### 변경 감지
`OnSlotChanged(index)`로 변경된 슬롯 인덱스를 통지받습니다. 어떤 종류의 변경인지는 알려주지 않으므로, 구독 측에서 `Slots[index]`를 다시 읽어 판단합니다.

```cs

container.OnSlotChanged += (index) =>
{
    var slot = container.Slots[index];
    if (slot.IsEmpty)
    {
        // 슬롯 비워짐
    }
};

```

<br>
<br>

#### 프로젝트별 규칙 추가
상속이 아니라 조합으로 사용합니다. 타입 제한, 무게 제한 같은 자체 규칙은 이 컨테이너를 감싸는 클래스에서 `Add` 호출 전에 직접 검사합니다.

```cs

public class PlayerInventory
{
    private ItemStackContainer<WeaponItem> _container;

    public PlayerInventory(int capacity)
    {
        _container = new ItemStackContainer<WeaponItem>(capacity);
    }

    public int Add(WeaponItem item, int count)
    {
        if (item.Key == "quest_only")
        {
            return count;
        }

        return _container.Add(item, count);
    }
}

```

<br>
<br>
<br>


## 📖API

#### IItem
**`Key`** : 아이템 정체성을 나타내는 고유 문자열입니다.<br>
**`MaxStackCount`** : 한 슬롯에 쌓일 수 있는 최대 수량입니다.<br>

<br>

#### ItemStack\<TItem\>
**`Item`** : 이 스택이 담고 있는 아이템입니다. 빈 슬롯이면 null입니다.<br>
**`Count`** : 현재 수량입니다.<br>
**`IsEmpty`** : 수량이 0 이하면 true를 반환합니다.<br>

<br>

#### ItemStackContainer\<T\>
**`Slots`** : 현재 슬롯 목록입니다. 정렬/압축 등은 이 목록을 참고해 호출부에서 직접 구현합니다.<br>
**`Capacity`** : 슬롯 개수입니다. 생성자에서 고정되며 이후 변경할 수 없습니다.<br>
**`OnSlotChanged`** : 슬롯 인덱스가 변경될 때 호출되는 이벤트입니다.<br>
**`Get(index)`** : 해당 인덱스의 슬롯을 반환합니다. 인덱스가 유효하지 않으면 null을 반환합니다.<br>
**`Add(item, count)`** : 아이템을 추가합니다. 처리하지 못하고 남은 수량을 반환합니다.<br>
**`Add(stack)`** : ItemStack을 그대로 추가합니다.<br>
**`Remove(item, count)`** : 아이템을 제거합니다. 제거하지 못하고 남은 수량을 반환합니다.<br>
**`Remove(stack)`** : ItemStack을 그대로 제거합니다.<br>
**`Take(index)`** : 해당 슬롯의 내용을 꺼내면서 슬롯을 비웁니다. 인덱스가 유효하지 않거나 비어있으면 null을 반환합니다.<br>
**`TakeAll()`** : 채워진 슬롯 전체를 꺼내면서 모두 비웁니다.<br>
**`Clear(index)`** : 해당 슬롯을 비웁니다.<br>
**`ClearAll()`** : 모든 슬롯을 비웁니다.<br>
**`Swap(indexA, indexB)`** : 두 슬롯의 내용을 1:1로 교환합니다.<br>
