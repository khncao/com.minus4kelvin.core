using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.InventorySystem {
[CreateAssetMenu(menuName="ScriptableObjects/ItemTierTable")]
public class ItemTierTable : ScriptableObject
{
    [System.Serializable]
    public class ItemRateAmount {
        public Item item;
        [Range(0, 999)]
        public int amount = 1;
        [Range(0, 1f)]
        public float rate = 1f;
    }
    [System.Serializable]
    public class ItemTierCollection {
        public string id;
        public bool carriesOver;
        public List<ItemRateAmount> items;
    }


    public List<ItemTierCollection> collections;

    public Inventory GetItemsFromRandomTier() {
        return GetItemsUpToTier(Random.Range(0, collections.Count));
    }
    public Inventory GetItemsUpToTier(int tierIndex) {
        if(tierIndex >= collections.Count) {
            Debug.LogWarning("requested tier exceeding max");
            tierIndex = collections.Count - 1;
        }
        Inventory items = new Inventory(16);

        for(int i = 0; i <= tierIndex; ++i) {
            if(i != tierIndex && !collections[i].carriesOver)
                continue;

            for(int j = 0; j < collections[i].items.Count; ++j) {
                int count = 0;
                for(int k = 0; k < collections[i].items[j].amount; ++k) {
                    if(SpawnItem(collections[i].items[j].rate))
                        count++;
                }
                // if(Game.Progression.CheckUnlocked(collectionsTable[i].items[j].unlockable)) {
                    items.AddItemAmount(collections[i].items[j].item, count, false);
                // }
            }
        }
        // Debug.Log(items.items.Length);

        return items;
    }
    bool SpawnItem(float rate) {
        float rand = Random.Range(0, 1f);
        return rand < rate;
    }
}
}