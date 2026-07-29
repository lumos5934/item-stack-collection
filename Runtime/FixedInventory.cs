using System.Collections.Generic;

namespace LLib.Inventory
{
    public class FixedInventory<TItem> : InventoryBase<TItem> where TItem : class, IItem
    {
        public int Capacity { get; private set; }

        public FixedInventory(int slotCount)
        {
            _slots = new List<ItemStack<TItem>>(slotCount);
            for (var i = 0; i < slotCount; i++)
            {
                _slots.Add(new ItemStack<TItem>(null, 0));
            }
        }

        public void SetCapacity(int value)
        {
            if (value < 0)
            {
                value = 0;
            }
            if (value > _slots.Count)
            {
                value = _slots.Count;
            }

            Capacity = value;
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

            for (var i = 0; i < Capacity && remaining > 0; i++)
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
            for (var i = 0; i < Capacity && remaining > 0; i++)
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

        public override void Clear()
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                _slots[i].Clear();
                RaiseSlotChanged(i);
            }
        }
        
        public bool Swap(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= Capacity || indexB < 0 || indexB >= Capacity)
                return false;
            
            if (!CanSwap(indexA, indexB))
                return false;

            var temp = _slots[indexA];
            _slots[indexA] = _slots[indexB];
            _slots[indexB] = temp;

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