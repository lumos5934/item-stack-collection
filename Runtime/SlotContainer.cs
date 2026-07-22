using System;
using System.Collections.Generic;

namespace LLib
{
    public enum SlotLayout
    {
        Fixed,
        Compact
    }


    public class SlotContainer<TIItem, TKey> where TIItem : IItem<TKey>
    {
        public readonly SlotLayout Layout;
        
        private readonly List<ItemSlot<TIItem>> _slots;
        private readonly int _capacity;

        private readonly Dictionary<int, ItemSlot<TIItem>> _changedSlots = new();

        public IReadOnlyList<ItemSlot<TIItem>> Slots => _slots;

        public int Capacity => _capacity;
        public int UsedSlotCount { get; private set; }
        public int EmptySlotCount => _capacity < 0 ? 0 : _capacity - UsedSlotCount;
        public bool IsEmpty => UsedSlotCount == 0;
        public bool IsFull => _capacity >= 0 && UsedSlotCount >= _capacity;
        public bool HasEmptySlot => _capacity < 0 || UsedSlotCount < _capacity;

        public event Action<IReadOnlyDictionary<int, ItemSlot<TIItem>>> SlotsChanged;
        

        public SlotContainer(SlotLayout layout, int capacity = -1)
        {
            Layout = layout;
            _capacity = capacity;

            _slots = new List<ItemSlot<TIItem>>();

            if (_capacity >= 0)
            {
                for (int i = 0; i < _capacity; i++)
                {
                    _slots.Add(CreateSlot());
                }
            }
        }


        protected virtual ItemSlot<TIItem> CreateSlot()
        {
            return new ItemSlot<TIItem>();
        }

        public ItemSlot<TIItem> GetSlot(int index)
        {
            return _slots[index];
        }

        public int Add(TIItem item, int amount)
        {
            BeginChange();

            int result = OnAdd(item, amount);

            InvokeChanged();

            return result;
        }

        public int Add(IReadOnlyList<(TIItem item, int amount)> items)
        {
            BeginChange();

            int total = 0;

            for (int i = 0; i < items.Count; i++)
            {
                total += OnAdd(
                    items[i].item,
                    items[i].amount);
            }

            InvokeChanged();

            return total;
        }

        public int RemoveAt(int index, int amount)
        {
            BeginChange();

            int result = OnRemoveAt(index, amount);

            InvokeChanged();

            return result;
        }
        
        public int RemoveAt(IReadOnlyList<(int index, int amount)> removes)
        {
            BeginChange();

            int total = 0;

            for (int i = 0; i < removes.Count; i++)
            {
                total += OnRemoveAt(
                    removes[i].index,
                    removes[i].amount);
            }

            InvokeChanged();

            return total;
        }

        public void Clear()
        {
            BeginChange();

            for (int i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];

                if (!slot.IsEmpty)
                {
                    UsedSlotCount--;
                }

                slot.Clear();

                MarkChanged(i);
            }


            UsedSlotCount = 0;

            InvokeChanged();
        }
        
        private void BeginChange()
        {
            _changedSlots.Clear();
        }

        private void MarkChanged(int index)
        {
            _changedSlots[index] = _slots[index];
        }

        private void InvokeChanged()
        {
            if (_changedSlots.Count == 0)
                return;

            SlotsChanged?.Invoke(_changedSlots);

            _changedSlots.Clear();
        }

        protected virtual int OnAdd(TIItem item, int amount)
        {
            int remaining = amount;

            // 기존 슬롯 스택 증가
            for (int i = 0; i < _slots.Count; i++)
            {
                if (remaining <= 0)
                    break;
                
                var slot = _slots[i];
                
                if (!slot.IsEmpty && IsSameItem(slot.Item, item))
                {
                    int canAdd = Math.Min(remaining, slot.Item.MaxStack - slot.Count);
                    if (canAdd > 0)
                    {
                        slot.Add(canAdd);

                        remaining -= canAdd;

                        MarkChanged(i);
                    }
                }
            }

            // Fixed 슬롯
            if (Layout == SlotLayout.Fixed)
            {
                for (int i = 0; i < _slots.Count; i++)
                {
                    if (remaining <= 0)
                        break;
                    
                    var slot = _slots[i];
                    
                    if (slot.IsEmpty)
                    {
                        int canAdd = Math.Min(
                            remaining,
                            item.MaxStack);


                        slot.Set(item, canAdd);

                        remaining -= canAdd;

                        UsedSlotCount++;

                        MarkChanged(i);
                    }
                }
            }

            // Compact 슬롯 생성
            if (Layout == SlotLayout.Compact || _capacity < 0)
            {
                while (remaining > 0)
                {
                    if (_capacity >= 0 && _slots.Count >= _capacity)
                        break;
                    
                    var slot = CreateSlot();
                    
                    int canAdd = Math.Min(remaining, item.MaxStack);
                    
                    slot.Set(item, canAdd);
                    
                    _slots.Add(slot);

                    remaining -= canAdd;

                    UsedSlotCount++;
                    
                    MarkChanged(_slots.Count - 1);
                }
            }

            return amount - remaining;
        }

        protected virtual int OnRemoveAt(int index, int amount)
        {
            var slot = _slots[index];
            if (slot.IsEmpty)
                return 0;

            int removed = slot.Remove(amount);

            if (slot.IsEmpty)
            {
                UsedSlotCount--;
            }

            MarkChanged(index);

            if (Layout == SlotLayout.Compact && slot.IsEmpty)
            {
                _slots.RemoveAt(index);
                
                for (int i = index; i < _slots.Count; i++)
                {
                    MarkChanged(i);
                }
            }

            return removed;
        }

        protected virtual bool IsSameItem(TIItem a, TIItem b)
        {
            return EqualityComparer<TKey>.Default.Equals(
                a.Key,
                b.Key);
        }
    }
}