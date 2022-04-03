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
    Inventory _inventory;

    public void UpdateInventory() {
        if(string.IsNullOrEmpty(inventoryId)) {
            _inventory = InventoryManager.I.mainInventory;
            return;
        }
        _inventory = InventoryManager.I.TryGetInventory(inventoryId);
        if(_inventory == null) {
            Debug.LogError($"Inventory with id {inventoryId} not found");
        }
    }

    public override void Init() {
        UpdateInventory();
    }

    public override bool CheckConditionMet() {
        if(!item) {
            Debug.LogError("No item in condition");
            return false;
        }
        UpdateInventory();
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
        UpdateInventory();
        int itemCt = _inventory.GetItemTotalAmount(item);
        bool pass = Comparisons.Compare(op, itemCt, val);

        string col = pass ? "green" : "white";

        return $"<color={col}>- {item.displayName}: {itemCt}/{val}</color>";
    }

    public override void RegisterListener(Conditions conditions) {
        UpdateInventory();
        _inventory.onChange -= conditions.OnChange;
        _inventory.onChange += conditions.OnChange;
    }

    public override void UnregisterListener(Conditions conditions) {
        _inventory.onChange -= conditions.OnChange;
    }
}
}