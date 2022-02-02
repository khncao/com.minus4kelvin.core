using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace m4k.Items {
public class ItemSlot : Selectable, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemInstance item;
    public Image bgImg, itemImgUI;
    public TMPro.TMP_Text itemNameUI, itemTxtUI;
    public ItemSlotHandler slotManager;
    [HideInInspector]
    public int slotIndex;
    public System.Action onAssign;
    RectTransform canvasRt, dragRt;

    public void AssignItem(ItemInstance newItem) {
        if(item != null)
            item.onChange -= RefreshUI;

        item = newItem;
        newItem.onChange += RefreshUI;
        onAssign?.Invoke();
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
        interactable = true;
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
        else {
            if(itemImgUI) {
                itemImgUI.sprite = item.item.itemIcon;
                itemImgUI.color = Color.white;
            }
            if(itemNameUI)
                itemNameUI.text = item.DisplayName;
            
            itemTxtUI.text = item.item.maxAmount > 1 && item.amount > 0 ? item.amount.ToString() : "";

            if(slotManager.interactableOverride) {
                interactable = slotManager.isInteractableOverride;
            }
            else if(item.item is ItemConditional condItem) {
                interactable = condItem.CheckConditions() && item.amount > 0;
            }
            else {
                interactable = item.amount > 0;
            }
        }
    }

    float lastClickTime;
    int clickCount;
	public override void OnPointerUp(PointerEventData eventData) {
        base.OnPointerUp(eventData);

        if(item == null || item.item == null || !interactable) 
            return;
        if(isDragging) return;
        string buttonName = eventData.button.ToString();
        clickCount = (Time.time - lastClickTime < 0.2f) ? clickCount + 1 : 1;

		if(buttonName == "Left")
		{
			if(clickCount == 2) {			
                item.item.ContextTransfer(this);
			}
            else {
                item.item.Primary(this);
                slotManager.selected = this;
                // Debug.Log($"selected: {item.itemName}");
            }
		}
		else if(buttonName == "Right")
		{
            slotManager.inventoryManager.UI.ToggleContextMenu(true);
		}
        lastClickTime = Time.time;
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

        // if(!slotManager.inventoryManager.UI.contextMenu.activeInHierarchy) {
        //     slotManager.inventoryManager.UI.ToggleContextMenu(false);
            slotManager.inventoryManager.UI.ToggleHoverWindow(false);
        // }
	}

    public override void OnSelect(BaseEventData eventData) {
        base.OnSelect(eventData);
        slotManager.selected = this;
    }
    public override void OnDeselect(BaseEventData eventData) {
        base.OnDeselect(eventData);
    }

    bool isDragging = false;
	public void OnBeginDrag(PointerEventData eventData) {
        if(!interactable) return;
        isDragging = true;
        if(!slotManager.canDrag) return;
		if(item != null && item.item != null) {
			slotManager.inventoryManager.UI.dragSlot = this;
            var dragTxt = slotManager.inventoryManager.UI.dragTxt;
            var dragImg = slotManager.inventoryManager.UI.dragImg;

            if(item.item.itemIcon) {
                dragImg.sprite = item.item.itemIcon;
                // image.SetNativeSize();
                slotManager.inventoryManager.UI.dragImg.color = Color.white;
            }
            dragTxt.text = item.item.displayName;
            dragRt = dragTxt.transform as RectTransform;
			canvasRt = slotManager.inventoryManager.UI.inventoryCanvas.transform as RectTransform;

			SetDraggedPosition(eventData);
		}
	}

	public void OnDrag(PointerEventData eventData) {
        if(!interactable) return;
        if(!slotManager.canDrag) return;
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
        if(!interactable) return;
        isDragging = false;
        if(!slotManager.canDrag) return;
		if(slotManager.inventoryManager.UI.dragImg)
            slotManager.inventoryManager.UI.dragImg.color = Color.clear;
			// slotManager.inventoryManager.UI.dragImg.gameObject.SetActive(false);
        slotManager.inventoryManager.UI.dragTxt.text = "";
		
		if(slotManager.inventoryManager.UI.dragSlot) {
			slotManager.inventoryManager.UI.dragSlot = null;
		}
	}

	public void OnDrop(PointerEventData eventData) {
        if(!interactable) return;
        if(!slotManager.canDrag) return;

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