using System.Collections.Generic;

namespace LLib
{
    public class ExampleContainer : SlotContainer<ExampleItem, string>
    {
        private readonly Dictionary<string, List<int>> _index = new();

        public ExampleContainer(SlotLayout layout, int capacity = -1) : base(layout, capacity)
        {
        }

        /*protected override void OnSlotChanged(int index, ItemStack<ExampleItem> slot)
        {
            if (slot.IsEmpty)
            {
                RebuildIndex();
                return;
            }

            var key = slot.Item.Key;
            if (!_index.TryGetValue(key, out var list))
            {
                list = new List<int>();
                _index[key] = list;
            }

            if (!list.Contains(index))
            {
                list.Add(index);
            }
        }*/

        public List<int> FindSlots(string key)
        {
            if (_index.TryGetValue(key, out var list))
                return list;

            return new List<int>();
        }

        public ItemSlot<ExampleItem> FindFirst(string key)
        {
            var slots = FindSlots(key);

            if (slots.Count == 0)
                return null;

            return GetSlot(slots[0]);
        }

        public void RebuildIndex()
        {
            _index.Clear();

            for (int i = 0; i < Slots.Count; i++)
            {
                var slot = GetSlot(i);

                if (slot.IsEmpty)
                    continue;

                var key = slot.Item.Key;

                if (!_index.TryGetValue(key, out var list))
                    _index[key] = list = new List<int>();

                list.Add(i);
            }
        }
    }
}

