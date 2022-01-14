using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Items {
[CreateAssetMenu(menuName="Data/Items/Item Recipe")]
public class ItemRecipe : Item
{
    [Header("Recipe")]
    // public CraftType craftType;
    public List<ItemInstance> ingredients;
    public ItemInstance output;
    public int craftTime;
    // public bool enforceOrder;

    public override bool Secondary(ItemSlot slot)
    {
        // Debug.Log("ItemRecipe double click");
        if(InventoryManager.I.craftSlotManager.inventory.totalItemsList.Count > 0) {
            Feedback.I.SendLine("Items still in craft window");
            return false;
        }
        InventoryManager.I.UI.InitiateItemTransfer(slot);
        return true;
    }
}
// [System.Serializable]
// public class RecipeInstance {
//     public ItemRecipe recipe;
//     public int craftableAmount;
// }
}