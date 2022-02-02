using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace m4k.Items {
public class InventoryUI : MonoBehaviour
{
    public GameObject itemSlotPrefab;
    public Canvas inventoryCanvas;

    [Header("Windows")]
    public GameObject bagInventorySlots;
    public GameObject characterInventorySlots, shopInventorySlots, storageInventorySlots;
    public List<GameObject> otherWindows;

    public GameObject dailyReportPanel;
    public TMP_Text dailyReportTxt;

    [Header("Currency")]
    public string currencyName = "coins";
    public TMP_Text currencyText;
    public TMP_Text currencyAnimText;
    public AudioSource currencyAudio;

    [Header("Context")]
    public GameObject itemContext;
    public TMP_Text hoverNameTxt, hoverInfoTxt;

    // public Button transferAllToBagButton, transferAllFromBagButton;
    // public GameObject objectPreviewPanel;
    [HideInInspector]
    public TMP_Text dragTxt;
    [HideInInspector]
    public Image dragImg;
    [HideInInspector]
    public ItemSlot dragSlot;
    [HideInInspector]
    public ItemSlot hoverSlot;

    int timesToPlay;
    Coroutine addCurrencyAnim;
    InventoryManager inventoryManager;

    public void Init(InventoryManager im) {
        inventoryManager = im;
    }

    public void ToggleBag(bool enabled) {
        bagInventorySlots.SetActive(enabled);
    }
    public void ToggleCharacters(bool enabled) {
        characterInventorySlots.SetActive(enabled);
    }
    public void ToggleShop(bool enabled) {
        shopInventorySlots.SetActive(enabled);
    }
    public void ToggleStorage(bool enabled) {
        storageInventorySlots.SetActive(enabled);
    }
    public void ToggleOtherWindows(bool enabled) {
        foreach(var w in otherWindows)
            w.SetActive(enabled);
    }
    public void ExitTransaction() {
        inventoryManager.ExitTransactions();
    }

    public void SetCurrency(int amount) {
        currencyText.text = amount.ToString();
    }

    public void UpdateCurrency(long totalAmount, long amount, bool anim) {
        currencyText.text = $"{totalAmount.ToString()} {currencyName}";

        if(!anim || amount == 0)
            return;
        if(addCurrencyAnim != null) StopCoroutine(addCurrencyAnim);
        addCurrencyAnim = StartCoroutine(AddCurrencyAnimation(amount));
    }

    public void PlayCurrencyAudio(AudioClip clip = null) {
        if(currencyAudio) {
            if(clip == null)
                clip = currencyAudio.clip;
            currencyAudio.PlayOneShot(clip);
        }
    }

    IEnumerator AddCurrencyAnimation(long amount) {
        currencyAnimText.enabled = true;
        currencyAnimText.alpha = 1f;
        currencyAnimText.color = amount > 0 ? Color.yellow : Color.red;
        currencyAnimText.text = amount.ToString();
        // timesToPlay += amount > 0 ? amount : 1;
        timesToPlay = 1;

        while(timesToPlay > 0) {
            timesToPlay--;
            PlayCurrencyAudio();
            yield return new WaitForSeconds(0.2f);
        }

        float fadeTimer = 1f;

        while(fadeTimer > 0) {
            fadeTimer -= Time.deltaTime;
            currencyAnimText.alpha = fadeTimer;
            yield return null;
        }

        currencyAnimText.enabled = false;
    }

    public void ShowDayReport() {

    }
    public void ShowMonthReport() {

    }
    public void TransferAllFromBag() {
        Feedback.I.RegisterConfirmRequest(AllFromBag, "Transfer all from bag?");
    }
    public void TransferAllToBag() {
        Feedback.I.RegisterConfirmRequest(AllToBag, "Transfer all to bag?");
    }
    void AllFromBag() {
        inventoryManager.TransferAllFromBag();
    }
    void AllToBag() {
        inventoryManager.TransferAllToBag();
    }


    ItemSlot contextSlot;
    public void UpdateHoverWindow(ItemSlot slot) {
        hoverSlot = slot;
        itemContext.transform.position = hoverSlot.transform.position;
        hoverNameTxt.text = hoverSlot.item.DisplayName;
        hoverInfoTxt.text = hoverSlot.item.item.description;
    }
    public void ToggleHoverWindow(bool enabled) {
        if(!enabled) {
            hoverSlot = null;
        }
        itemContext.SetActive(enabled);
    }
    public void ToggleContextMenu(bool enabled) {
        if(enabled) {
            UpdateContextMenuButtons();
        }
    }
    void UpdateContextMenuButtons() {
        if(!hoverSlot) {
            Debug.Log("No hover slot");
            return;
        }
        contextSlot = hoverSlot;
        Feedback.I.StartContextMenu(contextSlot.transform.position);

        if(inventoryManager.inShop) {
            if(hoverSlot.slotManager == inventoryManager.shopSlotManager)
                Feedback.I.RegisterContextMenuItem("Buy", ContextBuy);
            if(hoverSlot.slotManager == inventoryManager.bagSlotManager)
                Feedback.I.RegisterContextMenuItem("Sell", ContextSell);
        }
        else if(inventoryManager.inStorage) {
            Feedback.I.RegisterContextMenuItem("Transfer", ContextTransfer);
        }
        else if(hoverSlot.item.item.itemTags.Contains(ItemTag.Consumable)) {
            Feedback.I.RegisterContextMenuItem("Use", ContextUse);
        }
    }
    public void ContextBuy() => InitiateItemTransfer(contextSlot);
    public void ContextSell() => InitiateItemTransfer(contextSlot);
    public void ContextTransfer() => InitiateItemTransfer(contextSlot);
    public void ContextUse() {}
    public void ContextDestroy() {}


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
        inventoryManager.CompleteTransaction(value, fromSlot, toSlot);
    }

    public void ConfirmTransaction() {
        inventoryManager.CompleteTransaction(1, fromSlot, toSlot);
    }
}
}