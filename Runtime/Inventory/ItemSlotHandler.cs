using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace m4k.InventorySystem {
// [ExecuteInEditMode]
public class ItemSlotHandler : MonoBehaviour, IDropHandler
{
    public Inventory inventory;
    public GameObject slotsParent, auxSlotsParent;
    public Toggle toggleHideLocked;
    public bool listView, gridView;
    public bool ignoreMaxStack = true;
    public bool hideLockedSlots = false;
	public ItemSlot[] slots;
    public ItemSlot[] auxSlots;
    public ItemSlot selected;
    public bool interactableOverride = false, isInteractableOverride;
    public InventoryManager inventoryManager;
    
    bool initialized;
    System.Predicate<ItemInstance> filter;
    

    public void InitSlots() {
        if(initialized)
            return;

		slots = slotsParent.GetComponentsInChildren<ItemSlot>(false);
		for(int i = 0; i < slots.Length; i++) {
			slots[i].slotManager = this;
            slots[i].slotIndex = i;
            slots[i].UnassignItem();
		}
        if(!auxSlotsParent)
            return;
        auxSlots = auxSlotsParent.GetComponentsInChildren<ItemSlot>(false);
        for(int i = 0; i < auxSlots.Length; ++i) {
            auxSlots[i].slotManager = this;
            auxSlots[i].slotIndex = i;
            auxSlots[i].UnassignItem();
        }
        initialized = true;
    }
    
    private void Awake() {
        InitSlots();
        if(toggleHideLocked) {
            hideLockedSlots = toggleHideLocked.isOn;
            toggleHideLocked.onValueChanged.AddListener(ToggleHideLocked);
        }
    }
    private void Start() {
        inventoryManager = InventoryManager.I;
    }

    private void OnEnable() {
        UpdateAllSlots();
    }

    private void OnDisable() {
        inventoryManager?.UI?.ToggleHoverWindow(false);
        selected = null;
    }

    void ToggleHideLocked(bool b) {
        hideLockedSlots = b;
        UpdateAllSlots();
    }

    public void AssignInventory(Inventory newInventory, System.Predicate<ItemInstance> filter = null) {
        InitSlots();
        if(inventory != null) {
            inventory.onChange -= OnInventoryChange;
        }
        inventory = newInventory;
        newInventory.onChange += OnInventoryChange;
        this.filter = filter;
        UpdateAllSlots();
    }

    void OnInventoryChange() {
        UpdateAllSlots();
    }

    public void UpdateAllSlots() {
        InitSlots();
        if(inventory == null) {
            Debug.Log(gameObject.name + " no inventory");
            return;
        }

        if(inventory.items == null) {
            Debug.LogError("Inventory items null");
            return;
        }
        
        if(inventory.items.Length > slots.Length) {
            Debug.Log("Differ inventory and slots size " + inventory.items.Length + " " + slots.Length);
        }
        List<ItemInstance> items;
        items = filter != null ? inventory.GetFiltered(filter) : inventory.totalItemsList;

        if(!ignoreMaxStack) {
            for(int i = 0; i < slots.Length; ++i) {
                if(inventory.items.Length > i && inventory.items[i] != null) {
                    slots[i].AssignItem(inventory.items[i]);
                }
                else {
                    slots[i].UnassignItem();
                }
            }
        }
        else if(items != null) {
            int slotInd = 0;
            for(int i = 0; i < items.Count; ++i) {
                if(items.Count > i && items[i] != null) {
                    if((inventory.condHide && !items[i].item.conditions.CheckCompleteReqs()) || (hideLockedSlots && !slots[slotInd].isInteractable)) {
                        slots[slotInd].UnassignItem();
                        continue;
                    }
                    else
                        slots[slotInd].AssignItem(items[i]);
                }
                else {
                    slots[slotInd].UnassignItem();
                }
                slotInd++;
            }
            
            while(slotInd < slots.Length) {
                slots[slotInd].UnassignItem();
                slotInd++;
            }
        }
        if(!auxSlotsParent || inventory.aux == null)
            return;
        for(int i = 0; i < inventory.aux.Length; ++i) {
            if(i >= auxSlots.Length)
                continue;
            if(inventory.aux[i] != null) {
                auxSlots[i].AssignItem(inventory.aux[i]);
            }
            else {
                auxSlots[i].UnassignItem();
            }
        }
    }

    public void ToggleInteractableOverride(bool or, bool b) {
        interactableOverride = or;
        isInteractableOverride = b;
        RefreshAllSlots();
    }

    public void RefreshAllSlots() {
        foreach(var slot in slots) {
            slot.RefreshUI();
        }
        foreach(var aux in auxSlots) {
            aux.RefreshUI();
        }
    }

    public void OnDrop(PointerEventData eventData) {
        var dragSlot = inventoryManager.UI.dragSlot;
        if(!dragSlot)
            return;
        if(dragSlot.slotManager != this) {
            inventoryManager.UI.InitiateItemTransfer(dragSlot);
        }
    }
}
}