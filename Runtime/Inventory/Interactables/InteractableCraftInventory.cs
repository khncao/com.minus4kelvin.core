using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Items.Crafting {
[RequireComponent(typeof(GuidComponent))]
public class InteractableCraftInventory : MonoBehaviour
{
    public ItemTag craftType;
    public ItemTierTable recipeItemTable;
    public int recipeTierIndex = 0;
    public bool hideRecipeIfNoIngredients;
    // public InventoryComponent inputInventory, outputInventory;

    // public List<string> recipeTierFlags; // addons

    public string id { get; set; }

    [System.NonSerialized]
    Inventory _recipes;
    [System.NonSerialized]
    System.Predicate<Item> predicate;

    private void Start() {
        if(TryGetComponent<GuidComponent>(out GuidComponent guidComponent)) {
            id = guidComponent.GetGuid().ToString();
        }
        // inputInventory.inventory = CraftManager.I.GetStationInputInventory(id);
        // outputInventory.inventory = CraftManager.I.GetStationOutputInventory(id);

        if(hideRecipeIfNoIngredients)
            predicate = x=>x is ItemRecipe recipe 
                && recipe.CheckHasAtLeastOneIngredient(InventoryManager.I.mainInventory);

        CraftManager.I.OnLoadStation(id);
    }

    public void Interact() {
        if(recipeItemTable) {
            if(_recipes == null)
                _recipes = new Inventory(16);
            _recipes.Clear();
            _recipes = recipeItemTable.GetItemsUpToTier(_recipes, recipeTierIndex, predicate);
            _recipes.keepZeroItems = true;
        }
        CraftManager.I.OpenCraftStation(id, craftType, _recipes);
    }
}
}