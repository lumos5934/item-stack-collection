using UnityEngine;

namespace LLib
{
    [System.Serializable]
    public class ItemConfig
    {
        public string key;
        public string name;
        public int maxStack = 1;
    }

    public class ExampleContainerTest : MonoBehaviour
    {
        public ExampleContainer container;

        public ItemConfig addItemConfig;
        public int addAmount = 1;
        public int removeIndex = 0;
        public int removeAmount = 1;



        private void TryCreateContainer()
        {
            if (container != null)
                return;
        
            container = new ExampleContainer(SlotLayout.Fixed, 5);
        }
    
        public void Add()
        {
            TryCreateContainer();
        
            var item = new ExampleItem(
                addItemConfig.key,
                addItemConfig.name,
                addItemConfig.maxStack
            );

            container.Add(item, addAmount);
        }

        public void Remove()
        {
            TryCreateContainer();
        
            container.RemoveAt(removeIndex, removeAmount);
        }

        public void Clear()
        {
            container?.Clear();
        }
    }
}
