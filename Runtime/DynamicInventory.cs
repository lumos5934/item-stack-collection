using System.Collections.Generic;

namespace LLib.Inventory
{
    public class DynamicInventory<TItem> : InventoryBase<TItem> where TItem : class, IItem
    {
        public DynamicInventory()
        {
            _slots = new List<ItemStack<TItem>>();
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

                _slots.Add(new ItemStack<TItem>(item, stackCount));
                RaiseSlotChanged(_slots.Count - 1);
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
            for (var i = _slots.Count - 1; i >= 0 && remaining > 0; i--)
            {
                var slot = _slots[i];
                if (!slot.CanMerge(item))
                {
                    continue;
                }

                remaining -= slot.RemoveCount(remaining);
                if (slot.IsEmpty)
                {
                    _slots.RemoveAt(i);
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
            if (index > _slots.Count)
            {
                index = _slots.Count;
            }

            _slots.Insert(index, stack);
            RaiseSlotChanged(index);
        }

        protected virtual bool CanInsert(int index, ItemStack<TItem> stack)
        {
            return true;
        }
    }
}