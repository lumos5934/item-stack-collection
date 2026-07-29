# Inventory System

아이템 스택과 그것을 담는 인벤토리를 관리합니다. UI는 포함하지 않으며, 아이템 스택 데이터와 추가/제거/스왑/조회 같은 핵심 로직만 제공합니다. 빈 슬롯을 허용하는 슬롯형(FixedInventory)과 빈 슬롯 없이 항상 압축된 상태를 유지하는 압축형(DynamicInventory)을 상황에 맞게 선택해서 사용할 수 있습니다.

[ Usage ](#usage) <br>
[ API ](#api) <br>


<br>
<br>
<br>


## 🔧Usage

<br>

#### Item 정의
`IItem`을 구현해 아이템의 정체성(`Id`)과 최대 스택 수(`MaxStackCount`)를 정의합니다.

```cs

public class WeaponItem : IItem
{
    public string Id { get; private set; }
    public int MaxStackCount { get; private set; }

    public WeaponItem(string id, int maxStackCount)
    {
        Id = id;
        MaxStackCount = maxStackCount;
    }
}

```

<br>
<br>

#### FixedInventory 사용
슬롯 개수가 고정되고 빈 슬롯이 존재하는 인벤토리입니다. (플레이어 인벤토리, 장비창 등)

```cs

var inventory = new FixedInventory<WeaponItem>(20);

var leftover = inventory.Add(sword, 1);
inventory.Remove(sword, 1);
inventory.Swap(0, 3);

// 슬롯 개수 축소 (예: 디버프로 인벤토리 칸 잠금)
inventory.SetCurCapacity(15);

```

<br>
<br>

#### DynamicInventory 사용
빈 슬롯 없이 항상 압축된 상태를 유지하는 인벤토리입니다. (창고, 우편함 등 정렬만 필요한 경우)

```cs

var inventory = new DynamicInventory<WeaponItem>();

inventory.Add(sword, 1);
inventory.Remove(sword, 1);

// 특정 위치로 재배치
inventory.Insert(0, new ItemStack<WeaponItem>(sword, 1));

```

<br>
<br>

#### 슬롯 변경 감지
`OnSlotChanged(index)`로 변경된 슬롯 인덱스를 통지받습니다. 어떤 종류의 변경인지는 알려주지 않으므로, 구독 측에서 `Slots[index]`를 다시 읽어 판단합니다.

```cs

inventory.OnSlotChanged += (index) =>
{
    var slot = inventory.Slots[index];
    if (slot.IsEmpty)
    {
        // 슬롯 비워짐
    }
};

```

<br>
<br>

#### 프로젝트별 규칙 추가
`CanAdd`, `CanRemove`(base), `CanSwap`(Fixed), `CanInsert`(Dynamic) 훅을 override해 무게 제한, 타입 제한, 슬롯 잠금 같은 규칙을 얹을 수 있습니다.

```cs

public class PlayerInventory : FixedInventory<WeaponItem>
{
    public PlayerInventory(int capacity) : base(capacity)
    {
    }

    protected override bool CanAdd(WeaponItem item, int count)
    {
        if (item.Id == "quest_only")
        {
            return false;
        }

        return true;
    }
}

```

<br>
<br>
<br>


## 📖API

#### IItem
**`Id`** : 아이템 정체성을 나타내는 고유 문자열입니다.<br>
**`MaxStackCount`** : 한 슬롯에 쌓일 수 있는 최대 수량입니다.<br>

<br>

#### ItemStack\<TItem\>
**`Item`** : 이 스택이 담고 있는 아이템입니다. 빈 슬롯이면 null입니다.<br>
**`Count`** : 현재 수량입니다.<br>
**`IsEmpty`** : 수량이 0 이하면 true를 반환합니다.<br>
**`CanMerge(other)`** : 같은 아이템으로 병합 가능한지 여부를 반환합니다.<br>
**`Fill(item, count)`** : 빈 슬롯을 새 아이템으로 채웁니다.<br>
**`Clear()`** : 슬롯을 빈 상태로 되돌립니다.<br>
**`AddCount(amount)`** : 수량을 더합니다. MaxStackCount를 초과해 담지 못한 나머지 수량을 반환합니다.<br>
**`RemoveCount(amount)`** : 수량을 뺍니다. 실제로 제거된 수량을 반환합니다.<br>

<br>

#### InventoryBase\<TItem\>
**`Slots`** : 현재 슬롯 목록입니다. Find류 메서드 없이 이 목록을 직접 순회해서 사용합니다.<br>
**`OnSlotChanged`** : 슬롯 인덱스가 변경될 때 호출되는 이벤트입니다.<br>
**`Add(item, count)`** : 아이템을 추가합니다. 처리하지 못하고 남은 수량을 반환합니다.<br>
**`Add(stack)`** : ItemStack을 그대로 추가합니다.<br>
**`Remove(item, count)`** : 아이템을 제거합니다. 제거하지 못하고 남은 수량을 반환합니다.<br>
**`Remove(stack)`** : ItemStack을 그대로 제거합니다.<br>

<br>

#### FixedInventory\<TItem\> : InventoryBase\<TItem\>
**`Capacity`** : 배열의 물리적 크기입니다. 불변입니다.<br>
**`MaxCapacity`** : Capacity 이하로 조정 가능한 사용 가능 상한입니다.<br>
**`CurCapacity`** : MaxCapacity 이하로 조정 가능한 실사용 상한입니다. Add/Swap은 이 값을 기준으로 동작합니다.<br>
**`SetMaxCapacity(value)`** : MaxCapacity를 설정합니다. CurCapacity가 이보다 크면 함께 낮춥니다.<br>
**`SetCurCapacity(value)`** : CurCapacity를 설정합니다.<br>
**`Swap(indexA, indexB)`** : 두 슬롯의 내용을 1:1로 교환합니다.<br>

<br>

#### DynamicInventory\<TItem\> : InventoryBase\<TItem\>
**`Insert(index, stack)`** : 리스트 상 위치를 재배치합니다. 음수면 맨 앞, 최대 초과면 맨 뒤, 중간이면 밀어서 삽입합니다.<br>

<br>
<br>
<br>
