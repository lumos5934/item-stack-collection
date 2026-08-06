using System;
using System.Collections.Generic;

namespace LLib
{
    public abstract class ItemStackCollection<TItem> where TItem : class, IItem
    {
        protected List<ItemStack<TItem>> _stacks;

        public IReadOnlyList<ItemStack<TItem>> Stacks => _stacks;

        public event Action<int> OnStackChanged;

        protected void RaiseSlotChanged(int index)
        {
            OnStackChanged?.Invoke(index);
        }

        protected virtual bool CanAdd(TItem item, int count)
        {
            return true;
        }

        protected virtual bool CanRemove(TItem item, int count)
        {
            return true;
        }

        public abstract int Add(TItem item, int count);
        public abstract int Remove(TItem item, int count);
        public abstract void Clear();

        public int Add(ItemStack<TItem> stack)
        {
            if (stack == null)
            {
                return 0;
            }

            return Add(stack.Item, stack.Count);
        }

        public int Remove(ItemStack<TItem> stack)
        {
            if (stack == null)
            {
                return 0;
            }

            return Remove(stack.Item, stack.Count);
        }

        protected int MergeIntoExistingStacks(TItem item, int count)
        {
            var remaining = count;
            for (var i = 0; i < _stacks.Count && remaining > 0; i++)
            {
                var slot = _stacks[i];
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
    }
}