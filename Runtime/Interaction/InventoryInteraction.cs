using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.InventorySystem;
using m4k.Progression;

namespace m4k.Interaction {
public class InventoryInteraction : MonoBehaviour
{
    // [NaughtyAttributes.Expandable]
    public ItemTierTable itemSpawnTable;
    public InventoryType inventoryType;
    public ItemTag craftType;
    public Dialogue dialogue;
    public string choiceText;
    public int shopItemsTier = 0;
    Dialogue.Choice choice;
    KeyAction choiceAction;
    Inventory inventory = null;
    InventoryManager inventoryManager;

    private void Start() {
        inventoryManager = InventoryManager.I;
        
        if(inventoryType == InventoryType.Craft) {
            inventory = inventoryManager.GetOrRegisterInventory(gameObject.scene.name + transform.parent.position.ToString(), 4, 1, false, gameObject);
            InventoryManager.I.craftManager.OnLoadStation(inventory);
        }
        if(inventoryType == InventoryType.Storage) {
            inventory = inventoryManager.GetOrRegisterInventory(gameObject.scene.name + transform.parent.position.ToString(), 16, 0, false, gameObject);
        }
        if(!dialogue)
            dialogue = GetComponentInParent<Dialogue>();
        if(!dialogue) 
            return;
        choice = new Dialogue.Choice();
        choice.text = choiceText;
        choice.key = choiceText;
        choiceAction = new KeyAction();
        choiceAction.key = choiceText;
        choiceAction.action = new UnityEngine.Events.UnityEvent();
        choiceAction.action.AddListener(Interact);
        dialogue.RegisterInteractionChoice(choice, choiceAction);
    }
    public void Interact() {
        if(inventoryType == InventoryType.CharacterShop || inventoryType == InventoryType.ItemShop) 
            OpenShop();
        else if(inventoryType == InventoryType.Craft) 
            OpenCraft();
        else if(inventoryType == InventoryType.Storage)
            OpenStorage();
    }

    public void OpenShop() {
        if(itemSpawnTable && inventory == null)
            inventory = itemSpawnTable.GetItemsUpToTier(shopItemsTier);
        if(inventoryType == InventoryType.ItemShop)
            inventoryManager.ToggleShop(inventory);
        else
            inventoryManager.ToggleCharShop(inventory);
    }

    public void OpenStorage() {
        inventoryManager.ToggleStorage(inventory);
    }

    public void OpenCraft() {
        inventoryManager.ToggleCraft(inventory, craftType);
    }
}
}