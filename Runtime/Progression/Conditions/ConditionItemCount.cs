using System;
using UnityEngine;
using m4k.Items;

namespace m4k.Progression {
[Serializable]
public class ConditionItemCount : Condition {
    public Item item;
    public ComparisonType op;
    public int val;
    public bool removeItems;
    
    string _lastCheckStatus = "";

    public override bool CheckConditionMet() {
        if(!item) {
            Debug.LogError("No item in condition");
            return false;
        }
        return Comparisons.Compare(op, InventoryManager.I.mainInventory.GetItemTotalAmount(item), val);
    }

    public override void FinalizeCondition() {
        if(removeItems)
            InventoryManager.I.mainInventory.RemoveItemAmount(item, val, true);
    }

    public override string ToString() {
        if(!item) {
            Debug.LogError("No item in condition");
            return "";
        }
        int itemCt = InventoryManager.I.mainInventory.GetItemTotalAmount(item);
        bool pass = Comparisons.Compare(op, itemCt, val);

        string col = pass ? "green" : "white";
        _lastCheckStatus = $"<color={col}>- {item.displayName}: {itemCt}/{val}</color>";
        return _lastCheckStatus;
    }
}
}