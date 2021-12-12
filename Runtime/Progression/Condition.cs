using System;
using m4k.InventorySystem;

namespace m4k.Progression {
[Serializable]
public abstract class Condition {
    public abstract bool CheckConditionMet();
}

[Serializable]
public class ConditionItemCount : Condition {
    public Item itemForCount;
    public string op;
    public int compareVal;

    public override bool CheckConditionMet() {
        return InventoryManager.I.mainInventory.GetItemTotalAmount(itemForCount) < compareVal;
    }

    public override string ToString() {
        return "";
    }
}

[Serializable]
public class ConditionRecordTotal : Condition {
    public override bool CheckConditionMet() {
        return true;
    }
}
}