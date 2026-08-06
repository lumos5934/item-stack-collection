using System.Collections.Generic;

namespace LLib
{
    public class ItemStackArray<TItem> : ItemStackCollection<TItem> where TItem : class, IItem
    {
        public ItemStackArray(int capacity)
        {
            _stacks = new List<ItemStack<TItem>>(capacity);
            for (var i = 0; i < capacity; i++)
            {
                _stacks.Add(new ItemStack<TItem>(null, 0));
            }
        }


        public override int Add(TItem item, int count)
        {
            if (item == null || count <= 0)
            {
                return count;
            }
            if (!CanAdd(item, count))
            {
                return count;
            }

            var remaining = MergeIntoExistingStacks(item, count);
            if (remaining <= 0)
            {
                return 0;
            }

            for (var i = 0; i < _stacks.Count && remaining > 0; i++)
            {
                var slot = _stacks[i];
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

        public override int Remove(TItem item, int count)
        {
            if (item == null || count <= 0)
            {
                return count;
            }
            if (!CanRemove(item, count))
            {
                return count;
            }

            var remaining = count;
            for (var i = 0; i < _stacks.Count && remaining > 0; i++)
            {
                var slot = _stacks[i];
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

        public override void Clear()
        {
            for (var i = 0; i < _stacks.Count; i++)
            {
                _stacks[i].Clear();
                RaiseSlotChanged(i);
            }
        }
        
        public bool Swap(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= _stacks.Count || indexB < 0 || indexB >= _stacks.Count)
                return false;
            
            if (!CanSwap(indexA, indexB))
                return false;

            var temp = _stacks[indexA];
            _stacks[indexA] = _stacks[indexB];
            _stacks[indexB] = temp;

            RaiseSlotChanged(indexA);
            RaiseSlotChanged(indexB);
            return true;
        }

        protected virtual bool CanSwap(int indexA, int indexB)
        {
            return true;
        }
    }
}