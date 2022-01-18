using System;
using UnityEngine;
using m4k.Items;

namespace m4k {
[Serializable]
public class ConditionItemCount : Condition {
    [Header("Leaving empty defaults to main inventory")]
    public string inventoryId;
    public Item item;
    public ComparisonType op;
    public int val;
    public bool removeItems;
    
    string _lastCheckStatus = "";
    Inventory _inventory;

    public override void BeforeCheck(Conditions conditions) {
        if(string.IsNullOrEmpty(inventoryId)) {
            _inventory = InventoryManager.I.mainInventory;
            return;
        }
        _inventory = InventoryManager.I.TryGetInventory(inventoryId);
        if(_inventory == null) {
            Debug.LogWarning($"Inventory with id {inventoryId} not found");
        }
    }

    public override bool CheckConditionMet() {
        if(!item) {
            Debug.LogError("No item in condition");
            return false;
        }
        return Comparisons.Compare<int>(op, _inventory.GetItemTotalAmount(item), val);
    }

    public override void AfterComplete() {
        if(removeItems)
            _inventory.RemoveItemAmount(item, val, true);
    }

    public override string ToString() {
        if(!item) {
            Debug.LogError("No item in condition");
            return "";
        }
        int itemCt = _inventory.GetItemTotalAmount(item);
        bool pass = Comparisons.Compare(op, itemCt, val);

        string col = pass ? "green" : "white";
        _lastCheckStatus = $"<color={col}>- {item.displayName}: {itemCt}/{val}</color>";
        return _lastCheckStatus;
    }

    public override void RegisterListener(Conditions conditions) {
        _inventory.onChange -= conditions.OnChange;
        _inventory.onChange += conditions.OnChange;
    }

    public override void UnregisterListener(Conditions conditions) {
        _inventory.onChange -= conditions.OnChange;
    }
}
}