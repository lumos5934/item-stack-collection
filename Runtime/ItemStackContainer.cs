using System;
using System.Collections.Generic;

namespace LLib
{
    public class ItemStackContainer<T> where T : class, IItem
    {
        private List<ItemStack<T>> _slots;

        public IReadOnlyList<ItemStack<T>> Slots => _slots;
        public int Capacity => _slots.Count;

        public event Action<int> OnSlotChanged;
        
        
        public ItemStackContainer(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));
            
            _slots = new List<ItemStack<T>>(capacity);
            for (var i = 0; i < capacity; i++)
            {
                _slots.Add(new ItemStack<T>());
            }
        }


        public ItemStack<T> Get(int index)
        {
            if (!IsValidIndex(index))
                return null;
            
            return _slots[index];
        }

        public int Add(ItemStack<T> stack)
        {
            if (stack == null)
                return 0;

            return Add(stack.Item, stack.Count);
        }
        
        public int Add(T item, int count)
        {
            if (item == null || count <= 0)
                return count;

            var remaining = MergeIntoExistingStacks(item, count);
            if (remaining <= 0)
                return 0;

            for (var i = 0; i < _slots.Count && remaining > 0; i++)
            {
                var slot = _slots[i];
                if (!slot.IsEmpty)
                {
                    continue;
                }

                var stackCount = remaining;
                if (stackCount > item.MaxStackCount)
                {
                    stackCount = item.MaxStackCount;
                }

                slot.Fill(item, stackCount);
                remaining -= stackCount;
                RaiseSlotChanged(i);
            }

            return remaining;
        }

        public int Remove(ItemStack<T> stack)
        {
            if (stack == null)
                return 0;

            return Remove(stack.Item, stack.Count);
        }
        
        public int Remove(T item, int count)
        {
            if (item == null || count <= 0)
                return count;

            var remaining = count;
            for (var i = 0; i < _slots.Count && remaining > 0; i++)
            {
                var slot = _slots[i];
                if (!slot.CanMerge(item))
                {
                    continue;
                }

                remaining -= slot.RemoveCount(remaining);
                if (slot.IsEmpty)
                {
                    slot.Clear();
                }

                RaiseSlotChanged(i);
            }
            

            return remaining;
        }
        
        public List<ItemStack<T>> TakeAll()
        {
            var result = new List<ItemStack<T>>();

            for (var i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot.IsEmpty)
                    continue;

                result.Add(slot);
                _slots[i] = new ItemStack<T>();
                RaiseSlotChanged(i);
            }
            
            return result;
        }
        
        public ItemStack<T> Take(int index)
        {
            if (!IsValidIndex(index) || _slots[index].IsEmpty)
                return null;
            
            var result = _slots[index];
            
            _slots[index] = new ItemStack<T>();
            
            RaiseSlotChanged(index);

            return result;
        }
        
        public void ClearAll()
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsEmpty)
                    continue;
                
                _slots[i].Clear();
                RaiseSlotChanged(i);
            }
        }
        
        public void Clear(int index)
        {
            if (!IsValidIndex(index) || _slots[index].IsEmpty)
                return;
            
            _slots[index].Clear();
            RaiseSlotChanged(index);
        }
        
        
        public bool Swap(int indexA, int indexB)
        {
            if (!IsValidIndex(indexA) || !IsValidIndex(indexB) || indexA == indexB)
                return false;

            var temp = _slots[indexA];
            _slots[indexA] = _slots[indexB];
            _slots[indexB] = temp;

            RaiseSlotChanged(indexA);
            RaiseSlotChanged(indexB);
            return true;
        }
        
        private int MergeIntoExistingStacks(T item, int count)
        {
            var remaining = count;
            for (var i = 0; i < _slots.Count && remaining > 0; i++)
            {
                var slot = _slots[i];
                if (!slot.CanMerge(item))
                {
                    continue;
                }

                var overflow = slot.AddCount(remaining);
                if (overflow < remaining)
                {
                    RaiseSlotChanged(i);
                }

                remaining = overflow;
            }

            return remaining;
        }
        
        private void RaiseSlotChanged(int index)
        {
            OnSlotChanged?.Invoke(index);
        }
        
        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < _slots.Count;
        }
    }
}

