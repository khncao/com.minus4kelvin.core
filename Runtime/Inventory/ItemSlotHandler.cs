using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// TODO: handle different display modes
namespace m4k.Items {
// [ExecuteInEditMode]
public class ItemSlotHandler : MonoBehaviour, IDropHandler
{
    public Inventory inventory;
    public GameObject slotsParent;
    [Header("Will auto expand slot if slotPrefab nonnull")]
    public GameObject slotPrefab;
    public Toggle toggleHideLocked;
    [Header("If true, list total items as one entry each")]
    public bool ignoreMaxStack = true; // list style total items
    [Header("If true, will skip populating item if not interactable")]
    public bool hideLockedSlots = false;
    public bool canDrag = true;
	public ItemSlot[] slots;
    
    public InventoryManager inventoryManager;

    public bool interactableOverride { get; set; } = false;
    public bool isInteractableOverride { get; set; }
    public ItemSlot selected { get; set; }

    const int ExpandSlotsBuffer = 4;
    bool initialized;
    ItemInstance[] _items;
    List<ItemInstance> _totalItems;

    public void InitSlots() {
        if(initialized)
            return;

        if(slots.Length < 1)
		    slots = slotsParent.GetComponentsInChildren<ItemSlot>(false);
		for(int i = 0; i < slots.Length; i++) {
			slots[i].slotManager = this;
            slots[i].slotIndex = i;
            slots[i].UnassignItem();
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

        if(!ignoreMaxStack) {
            _items = filter != null ? inventory.GetFiltered(filter) : inventory.items;
            int slotDeficit = _items.Length - slots.Length;
            if(slotDeficit > 0 && slotPrefab)
                ExpandSlots(slotDeficit);
        }
        else {
            _totalItems = filter != null ? inventory.GetFilteredTotal(filter) : inventory.totalItemsList;
            int slotDeficit = _totalItems.Count - slots.Length;
            if(slotDeficit > 0 && slotPrefab)
                ExpandSlots(slotDeficit);
        }

        if(_totalItems == null) {
            Debug.LogError("Null items");
            return;
        }
        UpdateAllSlots();
    }

    void OnInventoryChange() {
        UpdateAllSlots();
    }

    public void UpdateAllSlots() {
        InitSlots();
        if(inventory == null) {
            return;
        }

        if(!ignoreMaxStack) { // should not hide items to maintain slot positions
            for(int i = 0; i < slots.Length; ++i) {
                if(_items.Length > i && _items[i] != null) {
                    slots[i].AssignItem(_items[i]);
                }
                else {
                    slots[i].UnassignItem();
                }
            }
        }
        else {
            int slotInd = 0;
            for(int i = 0; i < _totalItems.Count; ++i) {
                if(_totalItems.Count > i && _totalItems[i] != null) 
                {
                    // assign first to update interactability
                    slots[slotInd].AssignItem(_totalItems[i]);

                    // unassign item to hide if conds not met
                    if((_totalItems[i].item is ItemConditional itemCond
                    && itemCond.hideIfUnmet
                    && !itemCond.CheckConditions())
                    || // or hide locked/nonInteractable slots toggled
                    (hideLockedSlots 
                    && !slots[slotInd].interactable)) 
                    {
                        slots[slotInd].UnassignItem();
                        continue; // do not increment slotInd
                    }
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
    }

    void ExpandSlots(int amount) {
        if(!slotPrefab) return;

        int newSize = slots.Length + amount + ExpandSlotsBuffer;
        var newSlots = new ItemSlot[newSize];
        for(int i = 0; i < slots.Length; ++i) {
            newSlots[i] = slots[i];
        }
        for(int i = slots.Length; i < newSize; ++i) {
            var obj = Instantiate(slotPrefab, slotsParent.transform, false);
            var slot = obj.GetComponent<ItemSlot>();
			slot.slotManager = this;
            slot.slotIndex = i;
            slot.UnassignItem();
            newSlots[i] = slot;
        }
        slots = newSlots;
    }

    /// <summary>
    /// Overrides interactability of all slots; ie block recipes when crafting
    /// </summary>
    /// <param name="overrideEnabled"></param>
    /// <param name="overrideValue"></param>
    public void ToggleInteractableOverride(bool overrideEnabled, bool overrideValue) {
        interactableOverride = overrideEnabled;
        isInteractableOverride = overrideValue;
        RefreshAllSlots();
    }

    public void RefreshAllSlots() {
        foreach(var slot in slots) {
            slot.RefreshUI();
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