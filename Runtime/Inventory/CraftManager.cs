using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Items {
// public enum CraftType { None, Bar, Kitchen, Brew, Foundry, Stonemason, Sawmill }
// public class CraftType {
//     public static readonly string Bar = "Bar";
//     public static readonly string Kitchen = "Kitchen";
//     public static readonly string Brew = "Brew";
//     public static readonly string Foundry = "Foundry";
//     public static readonly string Stonemason = "Stonemason";
//     public static readonly string Sawmill = "Sawmill";
// }

// [System.Serializable]
// public class CraftBatchData {
//     public string inventoryKey;
//     public string recipeName;
//     public int amount;
//     public TickTimer timer;

//     [System.NonSerialized]
//     public ItemRecipe recipe;

//     public CraftBatchData(ItemRecipe item, int a) {
//         recipeName = item.name;
//         amount = a;
//         timer = new TickTimer(recipe.craftTime);
//     }
// }

[System.Serializable]
public class RecipeList {
    public ItemTag tag;
    [HideInInspector]
    public Inventory inv;
}
public class CraftManager : MonoBehaviour
{
    // public ItemSlotHandler craftSlotManager, recipeSlotManager;
    public List<RecipeList> recipeLists;

    Inventory currCraftInv, currRecipeInv;
    ItemSlotHandler craftSlots, recipeSlots;
    InventoryManager inventoryManager;

    void Start() {
        inventoryManager = InventoryManager.I;
        craftSlots = inventoryManager.craftSlotManager;
        recipeSlots = inventoryManager.recipeSlotManager;

        // recipe lists should be static with locked items, and vary with station
        // ie. base station recipes, base addon/better station adds/unhides recipes
        for(int i = 0; i < recipeLists.Count; ++i) {
            recipeLists[i].inv = new Inventory(16, 0, true);
            recipeLists[i].inv.condHide = true;
            var list = AssetRegistry.I.GetItemListByTag(recipeLists[i].tag);
            if(list == null)
                continue;
            for(int j = 0; j < list.Count; ++j) {
                recipeLists[i].inv.AddItemAmount(list[j], 1, false);
            }
        }
    }
    public void OnLoadStation(Inventory inventory) {
        if(inventory.timer.Running) {
            Inventory inv = inventory;
            inventory.timer.onComplete.AddListener(delegate { CompleteCraft(inventory); });
        }
    }
    public void CheckCraftableAmount() {
        CheckCraftableAmount(currRecipeInv);
    }
    public void CheckCraftableAmount(Inventory recipeInv) {
        for(int i = 0; i < recipeInv.items.Length; ++i) {
            if(recipeInv.items[i] == null)
                continue;
            ItemRecipe recipe = recipeInv.items[i].item as ItemRecipe;
            int craftableCt = 999;

            for(int j = 0; j < recipe.ingredients.Count; ++j) {
                int amount = inventoryManager.mainInventory.GetItemTotalAmount(recipe.ingredients[j].item);

                if(recipe.ingredients[j].amount < 1) {
                    Debug.LogError("Ingred req less than 1");
                    continue;
                }
                int temp = amount / recipe.ingredients[j].amount;
                if(temp < craftableCt)
                    craftableCt = temp;
            }
            recipeInv.RemoveItemAmount(recipeInv.items[i].item, recipeInv.items[i].amount);
            recipeInv.AddItemAmount(recipeInv.items[i].item, craftableCt, false);
        }
    }

    int CheckRecipeCraftable(ItemRecipe recipe) {
        int count = 999;

        for(int j = 0; j < recipe.ingredients.Count; ++j) {
            // int amount = inventoryManager.mainInventory.GetItemTotalAmount(recipe.ingredients[j].item);
            int amount = currCraftInv.GetItemTotalAmount(recipe.ingredients[j].item);

            if(recipe.ingredients[j].amount < 1) {
                Debug.LogError("Ingred req less than 1");
                continue;
            }
            int temp = amount / recipe.ingredients[j].amount;
            if(temp < count)
                count = temp;
        }

        return count;
    }

    public Inventory GetRecipeInventory(ItemTag craftType) {
        var inv = recipeLists.Find(x=>x.tag == craftType).inv;
        CheckCraftableAmount(inv);
        currRecipeInv = inv;

        return inv;
    }

    public void OnToggleCraftWindow(Inventory craftInv) {
        if(currCraftInv != null) {
            currCraftInv.timer.onChange -= TickCraft;
        }
        currCraftInv = craftInv;
        currCraftInv.timer.onChange += TickCraft;

        // if currently crafting
        if(currCraftInv.timer.Running) {
            craftSlots.ToggleInteractableOverride(true, false);
            recipeSlots.ToggleInteractableOverride(true, false);
            inventoryManager.UI.UpdateCraftButton("Cancel");
            inventoryManager.UI.UpdateCraftProgess(currCraftInv.timer.time.ToString());
        }
        else {
            inventoryManager.UI.SetCraftWindowDefault();
            craftSlots.ToggleInteractableOverride(false, false);
            recipeSlots.ToggleInteractableOverride(false, false);
        }
    }

    
    public bool CheckCraft() {
        if(!recipeSlots.selected) {
            return false;
        }
        Debug.Log($"recipe: {recipeSlots.selected.item.DisplayName}");
        var recipe = recipeSlots.selected.item.item as ItemRecipe;
        int craftable = CheckRecipeCraftable(recipe);

        bool canCraft = craftable > 0 && !currCraftInv.timer.Running;

        if(!canCraft) { // CancelCraft
            Debug.Log("cancel craft");
            if(!currCraftInv.timer.Running)
                return canCraft;
            currCraftInv.timer.EndTimer();
            Inventory.Transfer(currCraftInv, inventoryManager.mainInventory, currCraftInv.totalItemsList);
            CheckCraftableAmount();
            currCraftInv.UnassignReserved(0, 0);

            craftSlots.ToggleInteractableOverride(false, false);
            recipeSlots.ToggleInteractableOverride(false, false);
        }
        else { // StartCraft
            Debug.Log("start craft");
            currCraftInv.timer.SetTimer(10);
            Inventory inv = currCraftInv;
            currCraftInv.timer.onComplete.AddListener(delegate { CompleteCraft(inv); });
            currCraftInv.AssignReserved(0, recipe, craftable);
            craftSlots.ToggleInteractableOverride(true, false);
            recipeSlots.ToggleInteractableOverride(true, false);

        }
        return canCraft;
    }

    void TickCraft(int time) {
        inventoryManager.UI.UpdateCraftProgess(time.ToString());
    }

    public void CompleteCraft(Inventory inv) {
        Debug.Log("craft complete");
        var recipeItemInst = inv.GetReserved(0);
        var recipe = recipeItemInst.item as ItemRecipe;
        if(!recipe) Debug.LogError("No item");
        // remove ingreds
        foreach(var ingred in recipe.ingredients) {
            inv.RemoveItemAmount(ingred.item, ingred.amount * recipeItemInst.amount, true);
        }
        // add output
        inv.AddItemAmount(recipe.output.item, recipeItemInst.amount);
        // inv.AssignReserved(0, recipe.output.item, recipeItemInst.amount);

        // cleanup
        inv.UnassignReserved(0, recipeItemInst.amount);
        inv.timer.onComplete.RemoveAllListeners();

        if(inv == currCraftInv) {
            CheckCraftableAmount();
            inventoryManager.UI.SetCraftWindowDefault();
            craftSlots.ToggleInteractableOverride(false, false);
            recipeSlots.ToggleInteractableOverride(false, false);
        }
    }
}
}