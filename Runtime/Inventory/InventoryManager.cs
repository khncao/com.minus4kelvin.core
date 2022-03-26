using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Items {
[System.Serializable]
public class InventoryData {
    public SerializableDictionary<string, Inventory> inventories;
    public SerializableDictionary<string, InventoryCollection> inventoryCollections;
}

[System.Serializable]
public class InventoryManager : Singleton<InventoryManager>//, IStateSerializable
{
    public InventoryUI UI;
    public Inventory mainInventory;
    public Inventory characterInventory; // character roster
    public ItemSlotHandler bagSlotManager, characterSlotManager, shopSlotManager, storageSlotManager;
    public GameObject itemDropPrefab, inventoryDropPrefab;

    public System.Action onExitTransactions;

    public bool inTransaction { get { return currTransferSlots != null; }}
    public bool inShop { get { return UI.shopInventorySlots.activeInHierarchy; }}
    public bool inStorage { get { return UI.storageInventorySlots.activeInHierarchy; }}
    public bool inCharacter { get { return UI.characterInventorySlots.activeInHierarchy; }}
    

    ItemSlotHandler currTransferSlots;
    SerializableDictionary<string, Inventory> inventoryDict = new SerializableDictionary<string, Inventory>();
    SerializableDictionary<string, InventoryCollection> inventoryCollections = new SerializableDictionary<string, InventoryCollection>();


    protected override void Awake() {
        base.Awake();
        if(m_ShuttingDown) return; 

        ResetInventories();
        UI.Init(this);
    }

    void ResetInventories() {
        inventoryDict = new SerializableDictionary<string, Inventory>();

        mainInventory = GetOrRegisterSavedInventory("main", 16);
        characterInventory = GetOrRegisterSavedInventory("character", 16);

        bagSlotManager.AssignInventory(mainInventory);
        characterSlotManager.AssignInventory(characterInventory);

        mainInventory.onCurrencyChange -= OnCurrencyChange;
        mainInventory.onCurrencyChange += OnCurrencyChange;

        UI.UpdateCurrency(0, 0, false);
    }

    public GameObject SpawnItemDrop(ItemItem item, Vector3 pos) {
        GameObject go = Instantiate(itemDropPrefab, pos, Quaternion.identity);
        go.name = "Item";
        var itemInteract = go.GetComponent<ItemInteraction>();
        itemInteract.item = item;
        return go;
    }

    public GameObject SpawnInventoryDrop(Inventory inventory, Vector3 pos) {
        GameObject go = Instantiate(inventoryDropPrefab, pos, Quaternion.identity);
        go.name = "Storage";
        var invComponent = go.GetComponent<InventoryComponent>();
        Inventory invClone = new Inventory(inventory.MaxSize);
        invClone.AddItemAmounts(inventory.totalItemsList);
        invComponent.inventory = invClone;
        return go;
    }

    public void ToggleBag() {
        if(inTransaction) return;
        UI.ToggleBag(!UI.bagInventorySlots.activeInHierarchy);
    }
    public void ToggleCharInventory() {
        if(inTransaction) return;
        UI.ToggleCharacters(!UI.characterInventorySlots.activeInHierarchy);
    }

    // context transfers for externally handled toggles(craft)
    public void ToggleTransaction(ItemSlotHandler transferSlots) {
        currTransferSlots = transferSlots;
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
        UI.ToggleOtherWindows(false);
        currTransferSlots = null;
        onExitTransactions?.Invoke();
    }

    public void CompleteTransaction(int amount, ItemSlot fromSlot, ItemSlot toSlot, bool monetary = false) {
        ItemSlotHandler from = fromSlot.slotManager;
        ItemSlotHandler to;

        // slot to slot
        if(toSlot)
            to = toSlot.slotManager;
        // transfer from player inventory to context
        else if(from == bagSlotManager || from == characterSlotManager)
            to = currTransferSlots;
        // character shop
        else if(from == shopSlotManager && inCharacter)
            to = characterSlotManager;
        else
            to = bagSlotManager;

        if(inShop) monetary = true;

        Inventory.Transfer(from.inventory, to.inventory, amount, fromSlot.item.item, monetary);
        currTransferSlots = null;
    }

    public int TransferAllFromBag() {
        if(!inStorage) return 0;
        return Inventory.Transfer(mainInventory, currTransferSlots.inventory, mainInventory.totalItemsList);
    }
    public int TransferAllToBag() {
        if(!inStorage) return 0;
        return Inventory.Transfer(currTransferSlots.inventory, mainInventory, currTransferSlots.inventory.totalItemsList);
    }

    void OnCurrencyChange(long change, long final) {
        UI.UpdateCurrency(final, change, true);
    }

    public void AddCurrency(Inventory inventory, long amount) {
        inventory.ModifyCurrency(amount);
    }

    public Inventory GetOrRegisterSavedInventory(string key, int maxSize, GameObject owner = null) {
        Inventory inv;
        inventoryDict.TryGetValue(key, out inv);
        if(inv != null) {
            if(!inv.owner) {
                inv.owner = owner;
            }
            Debug.Log($"Retrieved inventory; key: {key}");
            return inv;
        }
        if(inventoryDict.ContainsKey(key)) {
            Debug.LogError("Already contains key");
            return null;
        }
        inv = new Inventory(maxSize);
        inv.id = key;
        inv.owner = owner;
        inventoryDict.Add(key, inv);
        // Debug.Log($"Registered invInst {key}");

        return inv;
    }

    public Inventory TryGetInventory(string key) {
        inventoryDict.TryGetValue(key, out Inventory inventory);
        return inventory;
    }

    public InventoryCollection GetOrRegisterSavedInventoryCollection(string key) {
        if(!inventoryCollections.TryGetValue(key, out var inventoryCollection)) {
            inventoryCollection = new InventoryCollection(key);
            inventoryCollections.Add(key, inventoryCollection);
        }
        return inventoryCollection;
    }

    public InventoryCollection TryGetInventoryCollection(string key) {
        inventoryCollections.TryGetValue(key, out var inventoryCollection);
        return inventoryCollection;
    }

    public void Serialize(ref InventoryData data) {
        data.inventories = inventoryDict;
        data.inventoryCollections = inventoryCollections;
    }

    public void Deserialize(InventoryData data) {
        inventoryDict = data.inventories;
        inventoryCollections = data.inventoryCollections;

        mainInventory = inventoryDict["main"];
        characterInventory = inventoryDict["character"];
        bagSlotManager.AssignInventory(mainInventory);
        characterSlotManager.AssignInventory(characterInventory);
    }
}
}