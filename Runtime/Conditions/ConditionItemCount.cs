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
    public bool removeItemsOnFinalize;
    
    [System.NonSerialized]
    string _lastCheckStatus = "";
    [System.NonSerialized]
    Inventory _inventory;

    public void TryGetInventory() {
        if(string.IsNullOrEmpty(inventoryId)) {
            _inventory = InventoryManager.I.mainInventory;
            return;
        }
        _inventory = InventoryManager.I.TryGetInventory(inventoryId);
        if(_inventory == null) {
            Debug.LogError($"Inventory with id {inventoryId} not found");
        }
    }

    public override bool CheckConditionMet() {
        if(!item) {
            Debug.LogError("No item in condition");
            return false;
        }
        if(_inventory == null) {
            TryGetInventory();
        }
        return Comparisons.Compare<int>(op, _inventory.GetItemTotalAmount(item), val);
    }

    public override void AfterComplete() {
        if(removeItemsOnFinalize)
            _inventory.RemoveItemAmount(item, val, true);
    }

    public override string ToString() {
        if(!item) {
            Debug.LogError("No item in condition");
            return "";
        }
        if(_inventory == null) {
            TryGetInventory();
        }
        int itemCt = _inventory.GetItemTotalAmount(item);
        bool pass = Comparisons.Compare(op, itemCt, val);

        string col = pass ? "green" : "white";
        _lastCheckStatus = $"<color={col}>- {item.displayName}: {itemCt}/{val}</color>";
        return _lastCheckStatus;
    }

    public override void RegisterListener(Conditions conditions) {
        if(_inventory == null) {
            TryGetInventory();
        }
        _inventory.onChange -= conditions.OnChange;
        _inventory.onChange += conditions.OnChange;
    }

    public override void UnregisterListener(Conditions conditions) {
        _inventory.onChange -= conditions.OnChange;
    }
}
}