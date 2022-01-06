using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Items {
[RequireComponent(typeof(InventoryComponent))]
public class InteractableCraftInventory : MonoBehaviour
{
    public ItemTag craftType;

    [System.NonSerialized]
    Inventory inventory;

    private void Start() {
        inventory = GetComponent<InventoryComponent>().inventory;
        if(inventory.aux.Length < 1) {
            Debug.LogError("Craft inventory no aux slots");
            return;
        }
        InventoryManager.I.craftManager.OnLoadStation(inventory);
    }

    public void Interact() {
        InventoryManager.I.ToggleCraft(inventory, craftType);
    }
}
}