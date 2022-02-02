using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Items.Crafting {
[CreateAssetMenu(menuName="Data/Items/Item Recipe")]
public class ItemRecipe : Item
{
    [Header("Recipe")]
    public SerializableDictionary<Item, int> ingredients;
    public SerializableDictionary<Item, int> output;
    public int craftTime;

    public override void ContextTransfer(ItemSlot slot) {
        if(CraftManager.I.inputSlotManager.inventory.totalItemsList.Count > 0) {
            Feedback.I.SendLine("Items still in craft window");
            return;
        }
        CraftManager.I.UI.InitiateItemTransfer(slot);
    }

    public bool CheckHasAtLeastOneIngredient(Inventory sourceInv) {
        foreach(var i in ingredients) {
            if(sourceInv.GetItemTotalAmount(i.Key) > 0)
                return true;
        }
        return false;
    }
}
}