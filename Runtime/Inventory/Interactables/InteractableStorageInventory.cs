using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Items {
[RequireComponent(typeof(InventoryComponent))]
public class InteractableStorageInventory : MonoBehaviour
{
    [System.NonSerialized]
    Inventory inventory;

    private void Start() {
        inventory = GetComponent<InventoryComponent>().inventory;
    }

    public void Interact() {
        InventoryManager.I.ToggleStorage(inventory);
    }
}
}