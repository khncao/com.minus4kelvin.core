using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace m4k.Items {
[Serializable]
public class InventoryCollection {
    public string id;
    public SerializableDictionary<string, Inventory> inventories;

    public InventoryCollection(string id) {
        this.id = id;
        inventories = new SerializableDictionary<string, Inventory>();
    }

    public bool TryAddInventory(string id, Inventory inv) {
        if(inventories.ContainsKey(id)) return false;
        else
            inventories.Add(id, inv);
        return true;
    }

    public bool TryGetInventory(string id, out Inventory inv) {
        return inventories.TryGetValue(id, out inv);
    }
}

[Serializable]
public class Inventory: UnityEngine.ISerializationCallbackReceiver
{
    [NonSerialized]
    public ItemInstance[] items; 

    [SerializeField]
    List<ItemData> _items;
    
    [SerializeField]
    long currency;
    [SerializeField]
    int maxSize;

    [NonSerialized]
    public List<ItemInstance> totalItemsList = new List<ItemInstance>();
    [NonSerialized]
    public Action onChange;
    [NonSerialized]
    public Action<long, long> onCurrencyChange;
    [NonSerialized]
    public GameObject owner;

    public string id { get; set; }
    public long Currency { get { return currency; }}
    public bool keepZeroItems { get; set; }
    public int MaxSize { get { return maxSize; }}


    public Inventory(int maxSize) {
        this.maxSize = maxSize;
        items = new ItemInstance[maxSize];
    }


    public bool IsInventoryFull() {
        return Array.IndexOf(items, null) == -1;
        // return items.IndexOf(null) == -1;
    }


    public int AddItemAmount(Item item, int amount, bool playNotify = false) {
        if(!item) {
            Debug.LogWarning($"Tried to add null item to inventory {id}");
            return -1;
        }
        if(maxSize < 1) {
            Debug.LogWarning($"{id} inventory maxSize < 1");
            return -1;
        }
        int initAmount = amount;
        var existing = FindAllExisting(item);
        
        for(int i = 0; i < existing.Length; ++i) {
            if(amount <= 0)
                break;
            int room = existing[i].item.maxAmount - existing[i].amount;

            if(existing[i].amount < existing[i].item.maxAmount) {
                int transfer = Mathf.Clamp(room, 0, amount);

                amount -= transfer;
                existing[i].amount += transfer;
            }

            existing[i].onChange?.Invoke();
        }

        while(amount > 0 && !IsInventoryFull()) {
            int newAmount;

            if(amount > item.maxAmount) {
                newAmount = item.maxAmount;
                amount -= newAmount;
            }
            else {
                newAmount = amount;
                amount = 0;
            }
            int empty = Array.IndexOf(items, null);
            // int empty = items.IndexOf(null);
            if(empty < maxSize)
                items[empty] = new ItemInstance(item, newAmount);
        }
        
        if(playNotify && amount < initAmount) {
            Feedback.I.SendLineQueue(string.Format("Obtained {0} {1}", initAmount - amount, item.displayName));
        }

        AddToTotalItems(item, initAmount - amount);
        onChange?.Invoke();
        return amount;
    }

    public int RemoveItemAmounts(List<ItemInstance> items) {
        int accum = 0;
        foreach(var i in items) {
            accum += RemoveItemAmount(i.item, i.amount);
        }
        return accum;
    }
    

    public int RemoveItemAmount(Item item, int amount, bool playNotify = false) {
        int initAmount = amount;
        var existing = FindAllExisting(item);

        for(int i = existing.Length - 1; i >= 0; --i) {
            if(amount <= 0)
                continue;

            if(existing[i].amount > 0) {
                int transfer = Mathf.Clamp(existing[i].amount, 0, amount);

                amount -= transfer;
                existing[i].amount -= transfer;
            }
            existing[i].onChange?.Invoke();

            if(existing[i].amount < 1 && !keepZeroItems) {
                existing[i] = null;
            }
        }
        
        if(playNotify && amount < initAmount) {
            Feedback.I.SendLineQueue(string.Format("Removed {0} {1}", initAmount - amount, item.displayName));
        }

        RemoveFromTotalItems(item, initAmount - amount);
        onChange?.Invoke();
        return amount;
    }



    public int GetItemTotalAmount(Item item) {
        int total = 0;

        var existing = totalItemsList.Find(x=>x.item == item);
        if(existing != null)
            total = existing.amount;

        return total;
    }

    public int GetMaxAmountItemsFit(Item item) {
        int total = 0;
        var existing = FindAllExisting(item);
        var empty = GetNumberEmptySlots();

        for(int i = 0; i < existing.Length; ++i) {
            total += item.maxAmount - existing[i].amount;
        }
        total += empty * item.maxAmount;

        return total;
    }

    public int GetNumberEmptySlots() {
        int num = 0;
        for(int i = 0; i < items.Length; ++i) {
            if(items[i] == null)
                num++;
        }
        return num;
    }

    public void SortAmountDescend() {
        Array.Sort(items, CompareByItemAmount);
        totalItemsList.Sort(CompareByItemAmount);
        onChange?.Invoke();
    }

    public List<ItemInstance> GetFilteredTotal(Predicate<ItemInstance> predicate) {
        return totalItemsList.FindAll(predicate);
    }

    public ItemInstance[] GetFiltered(Predicate<ItemInstance> predicate) {
        return Array.FindAll(items, predicate);
    }

    public void ModifyCurrency(long val) {
        currency += val;
        onCurrencyChange?.Invoke(val, currency);
    }

    public void AddItemAmounts(List<ItemInstance> itemInstances) {
        foreach(var i in itemInstances) {
            AddItemAmount(i.item, i.amount);
        }
    }

    public void AddItemAmounts(List<Item> its, List<int> amounts = null) {
        for(int i = 0; i < its.Count; ++i) {
            int a = amounts == null ? 0 : amounts[i];
            AddItemAmount(its[i], a);
        }
    }



    public static int Transfer(Inventory from, Inventory to, List<ItemInstance> items, bool monetary = false) {
        int count = 0;
        ItemInstance[] itemsCopy = new ItemInstance[items.Count];
        items.CopyTo(itemsCopy);
        foreach(var i in itemsCopy) {
            count += Transfer(from, to, i.amount, i.item, monetary);
        }
        return count;
    }

    public static int Transfer(Inventory from, Inventory to, int amount, Item item, bool monetary = false) 
    {
        if(amount > from.GetItemTotalAmount(item)) {
            Feedback.I.SendLine("Transfer amount exceeds origin inventory amount");
            return amount;
        }
        else if(amount > to.GetMaxAmountItemsFit(item)) {
            Feedback.I.SendLine("Transfer amount exceeds destination inventory space");
            return amount;
        }
        if(monetary) {
            int val = (int)GetCostValue(item, amount);
            if(val > to.Currency) {
                Feedback.I.SendLine("Not enough currency");
                return amount;
            }
            from.ModifyCurrency(val);
            to.ModifyCurrency(-val);
        }
        from.RemoveItemAmount(item, amount, true);
        to.AddItemAmount(item, amount, true);

        return 0;
    }

    public static float GetCostValue(Item item, int amount) {
        return item.value * amount;
    }
    public static float GetCostValue(List<ItemInstance> itemInstances) {
        float total = 0f;
        foreach(var i in itemInstances) {
            total += i.item.value * i.amount;
        }
        return total;
    }


    int CompareByItemAmount(ItemInstance a, ItemInstance b) {
        if(a.amount > b.amount)
            return 1;
        else if(b.amount > a.amount)
            return -1;
        return 0;
    }
    
    void AddToTotalItems(Item item, int amount) {
        var itemTotalInd = totalItemsList.FindIndex(x=>x.item == item);

        if(itemTotalInd == -1) {
            totalItemsList.Add(new ItemInstance(item, amount));
        }
        else {
            totalItemsList[itemTotalInd].amount += amount;
        }
    }

    void RemoveFromTotalItems(Item item, int amount) {
        var itemTotalInd = totalItemsList.FindIndex(x=>x.item == item);
        if(itemTotalInd == -1) {
            Debug.LogWarning("itemTotal not found");
        }
        else {
            totalItemsList[itemTotalInd].amount -= amount;
            if(totalItemsList[itemTotalInd].amount < 1 && !keepZeroItems)
                totalItemsList.RemoveAt(itemTotalInd);
        }
    }

    ItemInstance[] FindAllExisting(Item item) {
        return Array.FindAll(items, x=>x != null && x.item == item);
        // return items.FindAll(x=>x != null && x.item == item);
    }

    /// <summary>
    /// Remove all items and reset currency to 0
    /// </summary>
    public void Clear() {
        RemoveItemAmounts(totalItemsList);
        currency = 0;
    }

    public bool HasItem(Item item) {
        return totalItemsList.Exists(x=>x.item == item);
    }

    public void OnBeforeSerialize() {
        _items = new List<ItemData>();
        
        if(totalItemsList == null) // workaround for editor errors
            return;
        foreach(var i in totalItemsList) 
            if(i != null)
                _items.Add(new ItemData(i.item.name, i.amount));
        //         i.OnBeforeSerialize();
    }
    public void OnAfterDeserialize() {
        totalItemsList = new List<ItemInstance>();
        items = new ItemInstance[maxSize];
        
        for(int i = 0; i < _items.Count; ++i) {
            var item = AssetRegistry.I.GetItemFromName(_items[i].name);
            AddItemAmount(item, _items[i].amount);
        }
    }
}
}