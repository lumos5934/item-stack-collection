using System;

namespace LLib.Inventory
{
    [Serializable]
    public class ItemStack<TItem> where TItem : class, IItem
    {
        public TItem Item { get; private set; }
        public int Count { get; private set; }

        public bool IsEmpty => Count <= 0;
        
        
        public ItemStack(TItem item, int count)
        {
            Item = item;
            Count = count;
        }

        public bool CanMerge(TItem other)
        {
            if (Item == null || other == null)
            {
                return false;
            }

            return Item.Key == other.Key;
        }

        public void Fill(TItem item, int count)
        {
            Item = item;
            Count = count;
        }

        public void Clear()
        {
            Item = null;
            Count = 0;
        }

        public int AddCount(int amount)
        {
            var newCount = Count + amount;
            if (newCount > Item.MaxStackCount)
            {
                var overflow = newCount - Item.MaxStackCount;
                Count = Item.MaxStackCount;
                return overflow;
            }

            Count = newCount;
            return 0;
        }

        public int RemoveCount(int amount)
        {
            var removed = amount;
            if (removed > Count)
            {
                removed = Count;
            }

            Count -= removed;
            return removed;
        }
    }
}