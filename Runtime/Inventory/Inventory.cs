using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace m4k.Items {
public enum InventoryType { Bag, Storage, Characters, Craft }

// [Serializable]
// public class TransactionRecord {
//     public string fromInvId, toInvId;
//     public List<ItemInstance> items;
//     public int currency;
// }
[Serializable]
public class InventoryData {
    public List<Inventory> inventories;
}

[Serializable]
public class Inventory
{
    public string id;
    public TickTimer timer;
    // array vs list
    // slot position retention, serialization compactness, expandability framework
    [NonSerialized]
    public ItemInstance[] items; 
    [NonSerialized]
    public ItemInstance[] aux;

    [SerializeField]
    List<ItemData> _items;
    [SerializeField]
    List<ItemData> _aux;
    
    [SerializeField]
    long currency;
    [SerializeField]
    int maxSize;
    [SerializeField]
    int auxSize;

    [NonSerialized]
    public bool isStatic = false;
    [NonSerialized]
    public bool condHide;
    [NonSerialized]
    public List<ItemInstance> totalItemsList = new List<ItemInstance>();
    [NonSerialized]
    public Action onChange;
    [NonSerialized]
    public Action<long, long> onCurrencyChange;
    [NonSerialized]
    public GameObject owner;

    public long Currency { get { return currency; }}


    public Inventory(int maxSize, int auxSize = 0, bool setStatic = false) {
        this.maxSize = maxSize;
        this.auxSize = auxSize;
        items = new ItemInstance[maxSize];
        if(auxSize > 0)
            aux =  new ItemInstance[auxSize];
        isStatic = setStatic;
        timer = new TickTimer();
    }


    public bool IsInventoryFull() {
        return Array.IndexOf(items, null) == -1;
        // return items.IndexOf(null) == -1;
    }

    public void AssignReserved(int slotInd, Item item, int amount) {
        if(aux == null) return;
        if(aux[slotInd] != null && aux[slotInd].item == item) {
            aux[slotInd].amount += amount;
        }
        else {
            aux[slotInd] = new ItemInstance(item, amount);
        }
        onChange?.Invoke();
    }
    public void UnassignReserved(int slotInd, int amount) {
        if(aux == null) return;
        if(amount == 0) amount = aux[slotInd].amount;
        aux[slotInd].amount -= amount;
        if(aux[slotInd].amount <= 0) {
            aux[slotInd] = null;
        }
        onChange?.Invoke();
    }
    public ItemInstance GetReserved(int slotInd) {
        if(aux == null) return null;
        if(aux[slotInd] == null)
            Debug.LogWarning("null aux slot");
        return aux[slotInd];
    }
    public bool IsReservedIndexEmpty(int ind) {
        return aux[ind] == null;
    }



    public int AddItemAmount(Item item, int amount, bool playNotify = false) {
        if(!item) {
            Debug.LogWarning("no item");
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

            if(existing[i].amount < 1 && !isStatic) {
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

    public List<ItemInstance> GetFiltered(Predicate<ItemInstance> predicate) {
        return totalItemsList.FindAll(predicate);
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



    public static int Transfer(Inventory from, Inventory to, List<ItemInstance> items) {
        int count = 0;
        for(int i = 0; i < items.Count; ++i) {
            count += Transfer(from, to, items[i].amount, items[i].item);
        }
        return count;
    }

    public static int Transfer(Inventory from, Inventory to, int amount, Item item, bool monetary = false) 
    {
        if(amount > from.GetItemTotalAmount(item)) {
            Debug.Log("Transfer amount exceeds owned amount");
            return amount;
        }
        else if(amount > to.GetMaxAmountItemsFit(item)) {
            Debug.Log("Transfer amount exceeds destination inventory space");
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
            if(totalItemsList[itemTotalInd].amount < 1 && !isStatic)
                totalItemsList.RemoveAt(itemTotalInd);
        }
    }

    ItemInstance[] FindAllExisting(Item item) {
        return Array.FindAll(items, x=>x != null && x.item == item);
        // return items.FindAll(x=>x != null && x.item == item);
    }

    public bool HasItem(Item item) {
        return totalItemsList.Exists(x=>x.item == item);
    }

    public void OnBeforeSerialize() {
        _items = new List<ItemData>();
        
        foreach(var i in totalItemsList) 
            if(i != null)
                _items.Add(new ItemData(i.item.name, i.amount));
        //         i.OnBeforeSerialize();
        
        if(aux == null) return;
        _aux = new List<ItemData>();
        foreach(var i in aux)
            if(i != null)
                _aux.Add(new ItemData(i.item.name, i.amount));
        //         i.OnBeforeSerialize();
    }
    public void OnAfterDeserialize() {
        totalItemsList = new List<ItemInstance>();
        items = new ItemInstance[maxSize];
        
        for(int i = 0; i < _items.Count; ++i) {
            var item = AssetRegistry.I.GetItemFromName(_items[i].name);
            AddItemAmount(item, _items[i].amount);
        }
        
        if(auxSize > 0) aux = new ItemInstance[auxSize];
        for(int i = 0; i < _aux.Count; ++i) {
            var item = AssetRegistry.I.GetItemFromName(_aux[i].name);
            AssignReserved(i, item, _aux[i].amount);
        }
        timer.OnLoad();
    }
}
}