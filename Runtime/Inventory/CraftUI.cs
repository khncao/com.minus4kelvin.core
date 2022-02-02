using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace m4k.Items.Crafting {
public class CraftUI : MonoBehaviour
{
    public GameObject craftingWindow;
    public Button craftButton;
    public TMP_Text craftProgressTxt;
    public ScrollRect recipeScroll;

    public void Awake() {
        craftButton.onClick.AddListener(OnClickCraft);
    }

    public void ToggleCraft(bool enabled) {
        if(enabled) {
            InventoryManager.I.UI.ToggleBag(enabled);
        }
        else {
            InventoryManager.I.UI.ToggleBag(false);
            InventoryManager.I.ToggleTransaction(null);
            CraftManager.I.TransferCraftToBag();
        }
        craftingWindow.SetActive(enabled);
        
        recipeScroll.normalizedPosition = Vector2.one;
    }

    TMP_Text craftButtonText;
    public void OnClickCraft() {
        bool crafting = CraftManager.I.TryCraftOrCancel();
        string s = crafting ? "Cancel" : "Craft";
        UpdateCraftButton(s);
        UpdateCraftProgess("");
    }
    public void SetCraftWindowDefault() {
        UpdateCraftProgess("");
        UpdateCraftButton("Craft");
    }
    public void UpdateCraftButton(string s) {
        if(!craftButtonText)
            craftButtonText = craftButton.GetComponentInChildren<TMP_Text>();
        craftButtonText.text = s;
    }
    public void UpdateCraftProgess(string s) {
        craftProgressTxt.text = s;
    }

    ItemSlot fromSlot, toSlot;
    public void InitiateItemTransfer(ItemSlot itemSlot) {
        InitiateItemTransfer(itemSlot, null);
    }
    public void InitiateItemTransfer(ItemSlot from, ItemSlot to) {
        fromSlot = from;
        toSlot = to;

        if(from.item.item.maxAmount == 1) {
            Feedback.I.RegisterConfirmRequest(ConfirmTransaction,
            "Complete transaction?");
        }
        else {
            int ownedValue = from.slotManager.inventory.GetItemTotalAmount(from.item.item);
            Feedback.I.RegisterQuantityRequest(QuantityTransaction, "Amount?", ownedValue, 0);
        }
    }

    public void QuantityTransaction(int value) {
        CraftManager.I.CompleteTranfer(value, fromSlot, toSlot);
    }

    public void ConfirmTransaction() {
        CraftManager.I.CompleteTranfer(1, fromSlot, toSlot);
    }
}
}