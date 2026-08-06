using System.Collections.Generic;

namespace LLib
{
    public class ItemStackList<TItem> : ItemStackCollection<TItem> where TItem : class, IItem
    {
        public ItemStackList()
        {
            _stacks = new List<ItemStack<TItem>>();
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
            while (remaining > 0)
            {
                var stackCount = remaining;
                if (stackCount > item.MaxStackCount)
                {
                    stackCount = item.MaxStackCount;
                }

                _stacks.Add(new ItemStack<TItem>(item, stackCount));
                RaiseSlotChanged(_stacks.Count - 1);
                remaining -= stackCount;
            }

            return 0;
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
            for (var i = _stacks.Count - 1; i >= 0 && remaining > 0; i--)
            {
                var slot = _stacks[i];
                if (!slot.CanMerge(item))
                {
                    continue;
                }

                remaining -= slot.RemoveCount(remaining);
                if (slot.IsEmpty)
                {
                    _stacks.RemoveAt(i);
                }

                RaiseSlotChanged(i);
            }

            return remaining;
        }

        public void Insert(int index, ItemStack<TItem> stack)
        {
            if (stack == null)
            {
                return;
            }
            if (!CanInsert(index, stack))
            {
                return;
            }

            if (index < 0)
            {
                index = 0;
            }
            if (index > _stacks.Count)
            {
                index = _stacks.Count;
            }

            _stacks.Insert(index, stack);
            RaiseSlotChanged(index);
        }

        protected virtual bool CanInsert(int index, ItemStack<TItem> stack)
        {
            return true;
        }
        
        public override void Clear()
        {
            var removedCount = _stacks.Count;
            _stacks.Clear();

            for (var i = removedCount - 1; i >= 0; i--)
            {
                RaiseSlotChanged(i);
            }
        }
    }
}