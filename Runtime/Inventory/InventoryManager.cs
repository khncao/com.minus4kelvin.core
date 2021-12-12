using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Items {
[System.Serializable]
public class InventoryManager : Singleton<InventoryManager>//, IStateSerializable
{
    public InventoryUI UI;
    public CraftManager craftManager;
    public Inventory mainInventory;
    public Inventory characterInventory;
    public ItemSlotHandler bagSlotManager, characterSlotManager, shopSlotManager, storageSlotManager, craftSlotManager, recipeSlotManager;
    public List<Item> testItems1;

    [System.NonSerialized]
    public ItemSlot fromSlot, toSlot;

    public bool inShop { get { return UI.shopInventorySlots.activeInHierarchy; }}
    public bool inStorage { get { return UI.storageInventorySlots.activeInHierarchy; }}
    public bool inCraft { get { return UI.craftingWindow.activeInHierarchy; }}
    public bool inCharacter { get { return UI.characterInventorySlots.activeInHierarchy; }}
    public bool InTransaction { get { return inShop || inStorage || inCraft; } }

    ItemSlotHandler currTransferSlots;
    Dictionary<string, Inventory> inventoryDict = new Dictionary<string, Inventory>();

    public void TestAddBagItems() {
        for(int i = 0; i < testItems1.Count; ++i) {
            mainInventory.AddItemAmount(testItems1[i], 1, true);
        }
    }
    protected override void Awake() {
        base.Awake();
        if(m_ShuttingDown) return; 

        ResetInventories();
        UI.Init(this);
    }

    void ResetInventories() {
        inventoryDict = new Dictionary<string, Inventory>();

        mainInventory = GetOrRegisterInventory("main", 16);
        characterInventory = GetOrRegisterInventory("character", 16);

        bagSlotManager.AssignInventory(mainInventory);
        characterSlotManager.AssignInventory(characterInventory);

        mainInventory.onCurrencyChange -= OnCurrencyChange;
        mainInventory.onCurrencyChange += OnCurrencyChange;

        UI.UpdateCurrency(0, 0, false);
    }
    public void ToggleBag() {
        if(InTransaction) return;
        UI.ToggleBag(!UI.bagInventorySlots.activeInHierarchy);
    }
    public void ToggleCharInventory() {
        if(InTransaction) return;
        UI.ToggleCharacters(!UI.characterInventorySlots.activeInHierarchy);
    }
    public void ToggleCraft(Inventory inventory, ItemTag craftType) {
        craftSlotManager.AssignInventory(inventory);
        recipeSlotManager.AssignInventory(craftManager.GetRecipeInventory(craftType));
        craftManager.OnToggleCraftWindow(inventory);

        UI.ToggleBag(true);
        UI.ToggleCraft(true);
        currTransferSlots = craftSlotManager;
    }
    public void ToggleCharShop(Inventory inventory) {
        shopSlotManager.AssignInventory(inventory);

        UI.ToggleCharacters(true);
        UI.ToggleShop(true);
        currTransferSlots = characterSlotManager;
    }
    public void ToggleShop(Inventory inventory) {
        shopSlotManager.AssignInventory(inventory);
        
        UI.ToggleBag(true);
        UI.ToggleShop(true);
        currTransferSlots = shopSlotManager;
    }
    public void ToggleStorage(Inventory inventory) {
        storageSlotManager.AssignInventory(inventory);
        
        UI.ToggleBag(true);
        UI.ToggleStorage(true);
        currTransferSlots = storageSlotManager;
    }
    public void ExitTransactions() {
        UI.ToggleStorage(false);
        UI.ToggleShop(false);
        UI.ToggleBag(false);
        UI.ToggleCharacters(false);
        UI.ToggleCraft(false);
    }

    public void CompleteTransaction(int amount) {
        ItemSlotHandler from = fromSlot.slotManager;
        ItemSlotHandler to;
        bool monetary = false;

        if(toSlot)
            to = toSlot.slotManager;
        else if(from == bagSlotManager || from == characterSlotManager)
            to = currTransferSlots;
        else if(from == shopSlotManager && inCharacter)
            to = characterSlotManager;
        else
            to = bagSlotManager;

        if(inShop) monetary = true;

        if(inCraft && fromSlot.item.item is ItemRecipe) 
        {
            if(craftSlotManager.inventory.totalItemsList.Count > 0) 
            {
                Inventory.Transfer(craftSlotManager.inventory, mainInventory, craftSlotManager.inventory.totalItemsList);
            }

            ItemRecipe recipe = fromSlot.item.item as ItemRecipe;
            for(int i = 0; i < recipe.ingredients.Count; ++i) 
            {
                Inventory.Transfer(bagSlotManager.inventory, craftSlotManager.inventory, recipe.ingredients[i].amount * amount, recipe.ingredients[i].item);
            }
            recipeSlotManager.inventory.RemoveItemAmount(fromSlot.item.item, amount, true);
        }
        else {
            Inventory.Transfer(from.inventory, to.inventory, amount, fromSlot.item.item, monetary);
        }
        fromSlot = null;
        toSlot = null;
    }

    public int TransferAllFromBag() {
        if(!inStorage) return 0;
        return Inventory.Transfer(mainInventory, currTransferSlots.inventory, mainInventory.totalItemsList);
    }
    public int TransferAllToBag() {
        if(!InTransaction || inShop) return 0;
        return Inventory.Transfer(currTransferSlots.inventory, mainInventory, currTransferSlots.inventory.totalItemsList);
    }

    void OnCurrencyChange(long change, long final) {
        UI.UpdateCurrency(final, change, true);
    }

    public void AddCurrency(Inventory inventory, long amount) {
        inventory.ModifyCurrency(amount);
    }

    public Inventory GetOrRegisterInventory(string key, int maxSize, int auxSize = 0, bool isStatic = false, GameObject owner = null) {
        Inventory inv;
        inventoryDict.TryGetValue(key, out inv);
        if(inv != null) {
            if(!inv.owner) {
                inv.owner = owner;
            }
            return inv;
        }
        if(inventoryDict.ContainsKey(key)) {
            Debug.LogError("Already contains key");
            return null;
        }
        inv = new Inventory(maxSize, auxSize, isStatic);
        inv.id = key;
        inv.owner = owner;
        inventoryDict.Add(key, inv);
        // Debug.Log($"Registered invInst {key}");

        return inv;
    }

    public void Serialize(ref InventoryData data) {
        var _inventories = new List<Inventory>(inventoryDict.Values);
        foreach(var inv in _inventories) {
            if(inv.owner)
                inv.id = inv.owner.scene.name + inv.owner.transform.parent.position.ToString();
            inv.OnBeforeSerialize();
        }
        data.inventories = _inventories;
    }

    public void Deserialize(InventoryData data) {
        inventoryDict = new Dictionary<string, Inventory>();
        var _inventories = data.inventories;
        for(int i = 0; i < _inventories.Count; ++i) {
            _inventories[i].OnAfterDeserialize();
            inventoryDict.Add(_inventories[i].id, _inventories[i]);
        }
        mainInventory = inventoryDict["main"];
        characterInventory = inventoryDict["character"];
        bagSlotManager.AssignInventory(mainInventory);
        characterSlotManager.AssignInventory(characterInventory);
    }

    // public void Serialize(ref GameDataWriter writer) {
    //     writer.Write(mainInventory.currency);
    //     writer.Write(amountGenericStaff);

    //     writer.Write(mainInventory.items.Length);
    //     for(int i = 0; i < mainInventory.items.Length; ++i) {
    //         writer.Write(mainInventory.items[i].guid);
    //         writer.Write(mainInventory.items[i].data.slotIndex);
    //         writer.Write(mainInventory.items[i].data.amount);
    //     }
    // }
    // public void Deserialize(ref GameDataReader reader) {
    //     mainInventory.currency = reader.ReadInt();
    //     amountGenericStaff = reader.ReadInt();
    //     UI.UpdateCurrency(0, false);

    //     int len = reader.ReadInt();
    //     for(int i = 0; i < len; ++i) {
    //         string guid = reader.ReadString();

    //         Item item = AssetRegistry.I.GetItemFromGuid(guid);
    //         Item instance = Instantiate(item);

    //         instance.data.slotIndex = reader.ReadInt();
    //         instance.data.amount = reader.ReadInt();

    //         mainInventory.items[i] = instance;
    //     }
    // }
}
}