namespace LLib
{
    public class ItemSlot<TItem>
    {
        public TItem Item { get; private set; }
        public int Count { get; private set; }

        public bool IsEmpty => Item == null || Count <= 0;

        
        internal void Set(TItem item, int count)
        {
            Item = item;
            Count = count;
        }

        internal void Add(int amount)
        {
            Count += amount;
        }

        internal int Remove(int amount)
        {
            int removed = System.Math.Min(amount, Count);
            Count -= removed;

            if (Count <= 0)
                Clear();

            return removed;
        }

        internal void Clear()
        {
            Item = default;
            Count = 0;
        }
    }
}


