using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Items {
[CreateAssetMenu(menuName="Data/Items/ItemTierTable")]
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
        public Conditions conditions;
        public bool doesNotCarryOver;
        public List<ItemRateAmount> items;
    }

    public List<ItemTierCollection> collections;

    public Inventory GetItemsFromRandomTier(Inventory inv, System.Predicate<Item> predicate = null) {
        return GetItemsUpToTier(inv, Random.Range(0, collections.Count), predicate);
    }

    public Inventory GetInventoryByIds(Inventory inv, List<string> ids, System.Predicate<Item> predicate = null) {
        foreach(var id in ids) {
            var collection = FindCollection(id);
            if(collection != null) {
                PopulateInventory(ref inv, collection, predicate);
            }
            else 
                Debug.LogWarning($"Collection id: {id} not found");
        }
        return inv;
    }

    public Inventory GetItemsUpToTier(Inventory inv, int tierIndex, System.Predicate<Item> predicate = null) {
        if(tierIndex >= collections.Count) {
            Debug.LogWarning("requested tier exceeding max");
            tierIndex = collections.Count - 1;
        }

        for(int i = 0; i <= tierIndex; ++i) {
            if(i != tierIndex && collections[i].doesNotCarryOver)
                continue;
            if(!collections[i].conditions.CheckCompleteReqs())
                continue;

            PopulateInventory(ref inv, collections[i], predicate);
        }

        return inv;
    }

    void PopulateInventory(ref Inventory inv, ItemTierCollection collection, System.Predicate<Item> predicate = null) {
        for(int j = 0; j < collection.items.Count; ++j) {
            if(collection.items[j].item is ItemConditional condItem
            && !condItem.CheckConditions())
                continue;
            if(predicate != null && !predicate.Invoke(collection.items[j].item))
                continue;

            int count = 0;
            for(int k = 0; k < collection.items[j].amount; ++k) {
                if(Random.value < collection.items[j].rate)
                    count++;
            }

            inv.AddItemAmount(collection.items[j].item, count, false);
        }
    }

    ItemTierCollection FindCollection(string id) {
        return collections.Find(x=>x.id == id);
    }
}
}