using System;
using m4k.InventorySystem;

namespace m4k.Progression {
[Serializable]
public class ConditionItemCount : Condition {
    public Item item;
    public ComparisonType op;
    public int val;
    public bool removeItems;
    
    string _lastCheckStatus = "";

    public override bool CheckConditionMet() {
        return Comparisons.Compare(op, InventoryManager.I.mainInventory.GetItemTotalAmount(item), val);
    }

    public override void FinalizeCondition() {
        if(removeItems)
            InventoryManager.I.mainInventory.RemoveItemAmount(item, val, true);
    }

    public override string ToString() {
        int itemCt = InventoryManager.I.mainInventory.GetItemTotalAmount(item);
        bool pass = Comparisons.Compare(op, itemCt, val);

        string col = pass ? "green" : "white";
        _lastCheckStatus = $"<color={col}>- {item.itemName}: {itemCt}/{val}</color>";
        return _lastCheckStatus;
    }
}
}