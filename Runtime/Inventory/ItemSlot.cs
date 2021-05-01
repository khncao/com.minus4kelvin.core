using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace m4k.InventorySystem {
public class ItemSlot : Selectable, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemInstance item;
    public Image bgImg, itemImgUI, disableItemImg;
    public TMPro.TMP_Text itemNameUI, itemTxtUI;
    public ItemSlotHandler slotManager;
    public bool isInteractable = true;
    public bool canDrag = true;
    [HideInInspector]
    public int slotIndex;
    RectTransform canvasRt, dragRt;

    public void AssignItem(ItemInstance newItem) {
        if(item != null)
            item.onChange -= RefreshUI;

        item = newItem;
        newItem.onChange += RefreshUI;
        RefreshUI();
    }

    public void UnassignItem() {
        if(item != null) {
            item.onChange -= RefreshUI;
        }
        item = null;
        if(itemImgUI) {
            itemImgUI.sprite = null;
            itemImgUI.color = Color.clear;
        }
        if(itemNameUI)
            itemNameUI.text = "";
        itemTxtUI.text = "";
        if(disableItemImg)
            disableItemImg.enabled = false;
    }

    public void ToggleHidden(bool show) {
        if(bgImg)
            bgImg.enabled = show;
    }

    public void RefreshUI() {
        if(item == null || item.item == null) {
            UnassignItem();
        }
        else if(item.amount < 0) {
            Debug.LogError("less than 0 items");
            return;
        }
        else if(item.amount >= 0) {
            if(itemImgUI) {
                itemImgUI.sprite = item.item.itemIcon;
                itemImgUI.color = Color.white;
            }
            if(itemNameUI)
                itemNameUI.text = item.itemName;
            
            itemTxtUI.text = item.item.maxAmount > 1 && item.amount > 0 ? item.amount.ToString() : "";

            if(disableItemImg) {
                disableItemImg.enabled = slotManager.interactableOverride ? 
                !slotManager.isInteractableOverride : 
                !item.item.conditions.CheckCompleteReqs() || 
                (item.amount == 0);
                isInteractable = !disableItemImg.enabled;
            }
        }
    }

	public override void OnPointerUp(PointerEventData eventData) {
        base.OnPointerUp(eventData);

        if(item == null || item.item == null || !isInteractable) 
            return;
        if(isDragging) return;

		if(eventData.button.ToString() == "Left")
		{
			if(eventData.clickCount == 2) {			
                item.item.DoubleClick(this);
			}
            else {
                item.item.SingleClick(this);
                slotManager.selected = this;
                // Debug.Log($"selected: {item.itemName}");
            }
		}
		else if(eventData.button.ToString() == "Right")
		{
            slotManager.inventoryManager.UI.ToggleContextMenu(true);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData) {
		base.OnPointerEnter(eventData);

		if(item != null && item.item != null) {
            slotManager.inventoryManager.UI.UpdateHoverWindow(this);
            slotManager.inventoryManager.UI.ToggleHoverWindow(true);
        }
	}

	public override void OnPointerExit(PointerEventData eventData) {
		base.OnPointerExit(eventData);

        if(!slotManager.inventoryManager.UI.contextMenu.activeInHierarchy) {
            slotManager.inventoryManager.UI.ToggleContextMenu(false);
            slotManager.inventoryManager.UI.ToggleHoverWindow(false);
        }
	}

    bool isDragging = false;
	public void OnBeginDrag(PointerEventData eventData) {
        if(!isInteractable) return;
        isDragging = true;
        if(!canDrag) return;
		if(item != null && item.item != null) {
			slotManager.inventoryManager.UI.dragSlot = this;
            var dragTxt = slotManager.inventoryManager.UI.dragTxt;
            var dragImg = slotManager.inventoryManager.UI.dragImg;

            if(item.item.itemIcon) {
                dragImg.sprite = item.item.itemIcon;
                // image.SetNativeSize();
                slotManager.inventoryManager.UI.dragImg.color = Color.white;
            }
            dragTxt.text = item.item.itemName;
            dragRt = dragTxt.transform as RectTransform;
			canvasRt = slotManager.inventoryManager.UI.inventoryCanvas.transform as RectTransform;

			SetDraggedPosition(eventData);
		}
	}

	public void OnDrag(PointerEventData eventData) {
        if(!isInteractable) return;
        if(!canDrag) return;
        if (slotManager.inventoryManager.UI.dragSlot && item != null && item.item != null)
            SetDraggedPosition(eventData);
	}

    private void SetDraggedPosition(PointerEventData data) {
        Vector3 globalMousePos;
		
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRt, data.position, data.pressEventCamera, out globalMousePos))
        {
            dragRt.position = globalMousePos;
            // imageRt.rotation = canvasRt.rotation;
        }
    }

	public void OnEndDrag(PointerEventData eventData) {
        if(!isInteractable) return;
        isDragging = false;
        if(!canDrag) return;
		if(slotManager.inventoryManager.UI.dragImg)
            slotManager.inventoryManager.UI.dragImg.color = Color.clear;
			// slotManager.inventoryManager.UI.dragImg.gameObject.SetActive(false);
        slotManager.inventoryManager.UI.dragTxt.text = "";
		
		if(slotManager.inventoryManager.UI.dragSlot) {
			slotManager.inventoryManager.UI.dragSlot = null;
		}
	}

	public void OnDrop(PointerEventData eventData) {
        if(!isInteractable) return;
        if(!canDrag) return;

        var dragSlot = slotManager.inventoryManager.UI.dragSlot;
        if(!dragSlot)
            return;
        
        if(dragSlot == this) {}
        else if(dragSlot.slotManager != slotManager) {
            slotManager.inventoryManager.UI.InitiateItemTransfer(dragSlot, this);
        }
        
        slotManager.inventoryManager.UI.dragSlot = null;
	}
}
}