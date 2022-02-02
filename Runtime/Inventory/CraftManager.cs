using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Items.Crafting {
// public enum CraftType { None, Bar, Kitchen, Brew, Foundry, Stonemason, Sawmill }
// public class CraftType {
//     public static readonly string Bar = "Bar";
//     public static readonly string Kitchen = "Kitchen";
//     public static readonly string Brew = "Brew";
//     public static readonly string Foundry = "Foundry";
//     public static readonly string Stonemason = "Stonemason";
//     public static readonly string Sawmill = "Sawmill";
// }

[System.Serializable]
public class CraftProgress {
    public string id;
    public string recipeName;
    public int amount;
    public TickTimer timer;

    [System.NonSerialized]
    public ItemRecipe recipe;

    public CraftProgress(string id, ItemRecipe item, int a) {
        this.id = id;
        recipe = item;
        recipeName = item.name;
        amount = a;
        timer = new TickTimer(recipe.craftTime);
    }
}

[System.Serializable]
public class CraftData {
    public SerializableDictionary<string, CraftProgress> inprogressCrafts;
}

[System.Serializable]
public class RecipeList {
    public ItemTag tag;
    [HideInInspector]
    public Inventory inv;
}
public class CraftManager : Singleton<CraftManager>
{
    public CraftUI UI;
    public ItemSlotHandler inputSlotManager;
    public ItemSlotHandler outputSlotManager;
    public ItemSlotHandler recipeSlotManager;

    public bool inCraft { get { return UI.craftingWindow.activeInHierarchy; }}

    SerializableDictionary<string, CraftProgress> _inprogressCrafts = new SerializableDictionary<string, CraftProgress>();

    Inventory _currentStationRecipeInv;
    CraftProgress _currentStationCraft;
    string _currentStationId;

    void Start() {
        recipeSlotManager.canDrag = false;
    }

    /// <summary>
    /// Craft station loaded, register relevant events for inprogress crafts
    /// </summary>
    /// <param name="stationId"></param>
    public void OnLoadStation(string stationId) {
        if(_inprogressCrafts.TryGetValue(stationId, out CraftProgress craft)) {
            if(craft.timer.Running) {
                craft.timer.onComplete.AddListener(()=>CompleteCraft(craft));
            }
        }
    }

    /// <summary>
    /// Update recipe item list with craftable amounts
    /// </summary>
    void UpdateRecipeCraftableAmounts(Inventory recipeInv = null) {
        if(recipeInv == null) 
            recipeInv = _currentStationRecipeInv;

        for(int i = 0; i < recipeInv.totalItemsList.Count; ++i) {
            if(recipeInv.totalItemsList[i] == null || recipeInv.totalItemsList[i].item == null)
                continue;
            ItemRecipe recipe = recipeInv.totalItemsList[i].item as ItemRecipe;

            int craftableCt = RecipeCraftableAmount(InventoryManager.I.mainInventory, recipe);

            recipeInv.RemoveItemAmount(recipeInv.totalItemsList[i].item, recipeInv.totalItemsList[i].amount);
            recipeInv.AddItemAmount(recipeInv.totalItemsList[i].item, craftableCt, false);
        }
    }

    /// <summary>
    /// Amount of times a recipe can be crafted using input from source inventory
    /// </summary>
    /// <param name="sourceInv"></param>
    /// <param name="recipe"></param>
    /// <returns></returns>
    public int RecipeCraftableAmount(Inventory sourceInv, ItemRecipe recipe) {
        int count = recipe.ingredients.Count < 1 || recipe.output.Count < 1 ? 0 : 999;

        foreach(var ingredient in recipe.ingredients) {
            int amount = sourceInv.GetItemTotalAmount(ingredient.Key);

            if(ingredient.Value < 1) {
                Debug.LogError("Ingred req less than 1");
                continue;
            }
            int temp = amount / ingredient.Value;
            if(temp < count)
                count = temp;
        }

        return count;
    }

    /// <summary>
    /// Toggle and update craft UI based on station id and craftType
    /// </summary>
    /// <param name="stationId"></param>
    /// <param name="craftType"></param>
    public void OpenCraftStation(string stationId, ItemTag craftType, Inventory recipeInv) {
        _inprogressCrafts.TryGetValue(stationId, out CraftProgress craft);
        Inventory inputInv = GetStationInputInventory(stationId);
        Inventory outputInv = GetStationOutputInventory(stationId);

        _currentStationRecipeInv = recipeInv;
        recipeSlotManager.AssignInventory(recipeInv);
        inputSlotManager.AssignInventory(inputInv);
        outputSlotManager.AssignInventory(outputInv);
        UpdateRecipeCraftableAmounts();

        // if previous station inprogress craft UI tracked, untrack
        if(_currentStationCraft != null) {
            _currentStationCraft.timer.onChange -= UpdateCraftProgress;
        }
        _currentStationCraft = craft;
        _currentStationId = stationId;

        // if current station crafting, update UI
        if(craft != null) {
            UpdateCraftProgress(craft.timer.time);
            craft.timer.onChange += UpdateCraftProgress;

            inputSlotManager.ToggleInteractableOverride(true, false);
            recipeSlotManager.ToggleInteractableOverride(true, false);
            UI.UpdateCraftButton("Cancel");
            UI.UpdateCraftProgess(_currentStationCraft.timer.time.ToString());
        }
        else {
            UI.SetCraftWindowDefault();
            inputSlotManager.ToggleInteractableOverride(false, false);
            recipeSlotManager.ToggleInteractableOverride(false, false);
        }

        InventoryManager.I.onExitTransactions -= OnLeaveCraftStation;
        InventoryManager.I.onExitTransactions += OnLeaveCraftStation;
        InventoryManager.I.ToggleTransaction(inputSlotManager);
        InventoryManager.I.UI.ToggleBag(true);
        UI.ToggleCraft(true);
    }

    void OnLeaveCraftStation() {
        InventoryManager.I.onExitTransactions -= OnLeaveCraftStation;
        // remove items from input slots if craft not initiated
        TransferCraftToBag();
        UI.ToggleCraft(false);
        _currentStationRecipeInv = null;
        _currentStationId = null;
        _currentStationCraft = null;
    }

    /// <summary>
    /// Craft window is open. Behavior on clicking craft/cancel button.
    /// </summary>
    /// <returns></returns>
    public bool TryCraftOrCancel() {
        // enforce recipe selected; TODO: blind input slot craft
        if(!recipeSlotManager.selected) {
            return false;
        }
        Inventory inputInv = GetStationInputInventory(_currentStationId);
        Inventory outputInv = GetStationOutputInventory(_currentStationId);
        
        // Debug.Log($"recipe: {recipeSlotManager.selected.item.DisplayName}");
        var recipe = recipeSlotManager.selected.item.item as ItemRecipe;
        // amount craftable based on items in inputSlots
        int craftableAmount = RecipeCraftableAmount(inputInv, recipe);

        bool canCraft = craftableAmount > 0 && _currentStationCraft == null;

        if(!canCraft) { // CancelCraft
            // Debug.Log("cancel craft");
            if(_currentStationCraft == null)
                return false;
            _currentStationCraft.timer.CancelTimer();
            Inventory.Transfer(inputInv, InventoryManager.I.mainInventory, inputInv.totalItemsList);
            UpdateRecipeCraftableAmounts();
            // completely unassign recipe and amount
            _inprogressCrafts.Remove(_currentStationId);
            _currentStationCraft = null;

            inputSlotManager.ToggleInteractableOverride(false, false);
            recipeSlotManager.ToggleInteractableOverride(false, false);
        }
        else { // StartCraft
            // Debug.Log("start craft");
            CraftProgress craft = new CraftProgress(_currentStationId, recipe, craftableAmount);
            _currentStationCraft = craft;

            craft.timer.SetTimer(recipe.craftTime);
            craft.timer.onChange += UpdateCraftProgress;
            Inventory inv = inputInv;
            craft.timer.onComplete.AddListener(()=>CompleteCraft(craft));
            _inprogressCrafts.Add(craft.id, craft);

            inputSlotManager.ToggleInteractableOverride(true, false);
            recipeSlotManager.ToggleInteractableOverride(true, false);
        }
        return canCraft;
    }

    void UpdateCraftProgress(int time) {
        UI.UpdateCraftProgess(time.ToString());
    }

    public void TransferCraftToBag() {
        if(_currentStationCraft != null)
            return;
        Inventory.Transfer(inputSlotManager.inventory, InventoryManager.I.mainInventory, inputSlotManager.inventory.totalItemsList);
        // Inventory.Transfer(outputSlotManager.inventory, InventoryManager.I.mainInventory, outputSlotManager.inventory.totalItemsList);
    }

    public void CompleteCraft(CraftProgress craft) {
        // Debug.Log("craft complete");
        Inventory inputInv = GetStationInputInventory(craft.id);
        Inventory outputInv = GetStationOutputInventory(craft.id);

        // remove ingreds from input 
        foreach(var ingredient in craft.recipe.ingredients) {
            inputInv.RemoveItemAmount(ingredient.Key, ingredient.Value * craft.amount);
        }
        // add to output
        foreach(var result in craft.recipe.output) {
            outputInv.AddItemAmount(result.Key, result.Value * craft.amount);
        }
        
        // cleanup; remove amount of recipe
        craft.timer.onComplete.RemoveAllListeners();
        _inprogressCrafts.Remove(craft.id);

        // handle if completed craft currently active UI
        if(craft == _currentStationCraft) {
            UpdateRecipeCraftableAmounts();
            UI.SetCraftWindowDefault();
            inputSlotManager.ToggleInteractableOverride(false, false);
            recipeSlotManager.ToggleInteractableOverride(false, false);
        }
    }

    /// <summary>
    /// Contextual transfers between player inventory, input inventory, output inventory; if recipeItem, transfers ingredient ratios to from bag to inputs
    /// </summary>
    /// <param name="fromSlot"></param>
    /// <param name="toSlot"></param>
    /// <param name="amount"></param>
    public void CompleteTranfer(int amount, ItemSlot fromSlot, ItemSlot toSlot) {
        if(!inCraft) return;

        if(inputSlotManager.inventory.totalItemsList.Count > 0) 
        {
            Inventory.Transfer(inputSlotManager.inventory, InventoryManager.I.mainInventory, inputSlotManager.inventory.totalItemsList);
        }

        ItemRecipe recipe = fromSlot.item.item as ItemRecipe;
        foreach(var ingredient in recipe.ingredients)
        {
            Inventory.Transfer(InventoryManager.I.bagSlotManager.inventory, inputSlotManager.inventory, ingredient.Value * amount, ingredient.Key);
        }
        recipeSlotManager.inventory.RemoveItemAmount(fromSlot.item.item, amount, true);
        UpdateRecipeCraftableAmounts();
    }

    public InventoryCollection GetOrRegisterCraftStationInventories(string id) {
        var invs = InventoryManager.I.GetOrRegisterSavedInventoryCollection(id);
        // initialize if newly created collection
        if(invs.inventories.Count < 1) {
            invs.TryAddInventory("input", new Inventory(16));
            invs.TryAddInventory("output", new Inventory(16));
        }
        return invs;
    }

    public Inventory GetStationInputInventory(string stationId) {
        var invs = GetOrRegisterCraftStationInventories(stationId);
        invs.TryGetInventory("input", out Inventory inv);
        return inv;
    }
    public Inventory GetStationOutputInventory(string stationId) {
        var invs = GetOrRegisterCraftStationInventories(stationId);
        invs.TryGetInventory("output", out Inventory inv);
        return inv;
    }
    
    public void Serialize(ref CraftData craftData) {
        craftData.inprogressCrafts = _inprogressCrafts;
    }

    /// <summary>
    /// Should be called after InventoryManager deserialize
    /// </summary>
    /// <param name="craftData"></param>
    public void Deserialize(ref CraftData craftData) {
        _inprogressCrafts = craftData.inprogressCrafts;
        foreach(var craft in _inprogressCrafts) {
            craft.Value.recipe = AssetRegistry.I.GetItemFromName(craft.Value.recipeName) as ItemRecipe;
        }
    }
}
}