using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.InventorySystem {
[CreateAssetMenu(menuName="ScriptableObjects/Item Recipe")]
public class ItemRecipe : Item
{
    [Header("Recipe")]
    // public CraftType craftType;
    public List<ItemInstance> ingredients;
    public ItemInstance output;
    public int craftTime;
    // public bool enforceOrder;

    public override void DoubleClick(ItemSlot slot)
    {
        base.DoubleClick(slot);
        // Debug.Log("ItemRecipe double click");
        if(InventoryManager.I.craftSlotManager.inventory.totalItemsList.Count > 0) {
            Feedback.I.SendLine("Items still in craft window");
            return;
        }
        InventoryManager.I.UI.InitiateItemTransfer(slot);
    }
}
// [System.Serializable]
// public class RecipeInstance {
//     public ItemRecipe recipe;
//     public int craftableAmount;
// }
}