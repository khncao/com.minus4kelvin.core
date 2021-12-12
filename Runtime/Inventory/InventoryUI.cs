using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace m4k.Items {
public class InventoryUI : MonoBehaviour
{
    public Canvas inventoryCanvas;
    public GameObject bagInventorySlots, characterInventorySlots, shopInventorySlots, storageInventorySlots, craftingWindow;
    public Button craftButton;
    public TMP_Text craftProgressTxt;
    public GameObject dailyReportPanel;
    public TMP_Text dailyReportTxt;
    public string currencyName = "coins";
    public TMP_Text currencyText;
    public TMP_Text currencyAnimText;
    public TMP_Text confirmTransactionText;
    public AudioSource currencyAudio;
    public GameObject confirmTransactionPrompt, quantityPrompt;
    public TMP_InputField quantityPromptInput;
    public GameObject itemContext, contextMenu;
    public TMP_Text hoverNameTxt, hoverInfoTxt;
    public Button contextUseButton, contextBuyButton, contextSellButton, contextTransferButton;
    public Button transferAllToBagButton, transferAllFromBagButton;
    public GameObject objectPreviewPanel;
    public TMP_Text dragTxt;
    public Image dragImg;
    public GameObject itemSlotPrefab;
    public Item quantityPromptItem;
    public ItemSlot hoverSlot, dragSlot;

    int timesToPlay;
    Coroutine addCurrencyAnim;
    InventoryManager inventoryManager;

    public void Init(InventoryManager im) {
        inventoryManager = im;

        quantityPromptInput.onValueChanged.AddListener(OnQuantityPromptChanged);
        craftButton.onClick.AddListener(OnClickCraft);
        if(transferAllToBagButton)
            transferAllToBagButton.onClick.AddListener(delegate {Feedback.I.RegisterConfirmRequest(inventoryManager.TransferAllToBag, "Transfer all to bag?");} );

        if(transferAllFromBagButton)
            transferAllFromBagButton.onClick.AddListener(delegate {Feedback.I.RegisterConfirmRequest(inventoryManager.TransferAllFromBag, "Transfer all from bag?");} );
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
    public void ToggleCraft(bool enabled) {
        craftingWindow.SetActive(enabled);
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
        // var record = inventoryManager.dailyRecords[inventoryManager.dailyRecords.Count - 1];
        // dailyReportTxt.text = string.Format("Day {0}\nCustomers: {1}\nExpenditure: {2}\nProfit: {3}\nNet: {4}", Game.day, record.customers, record.expenditure, record.profit, record.profit - record.expenditure);

        // var record = Game.Progression.GetLatestRecord();
        // dailyReportTxt.text = string.Format("Day {0}\nCustomers: {1}\nExpenditure: {2}\nProfit: {3}\nNet: {4}", Game.day - 1, record.TryGetValue("customers"), record.TryGetValue("expenditure"), record.TryGetValue("profit"), record.TryGetValue("profit") - record.TryGetValue("expenditure"));
        // dailyReportPanel.SetActive(true);
    }
    public void ShowMonthReport() {

    }

    TMP_Text craftButtonText;
    void OnClickCraft() {
        bool crafting = InventoryManager.I.craftManager.CheckCraft();
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

    ItemSlot contextSlot;
    public void UpdateHoverWindow(ItemSlot slot) {
        hoverSlot = slot;
        itemContext.transform.position = hoverSlot.transform.position;
        hoverNameTxt.text = hoverSlot.item.ItemName;
        hoverInfoTxt.text = hoverSlot.item.item.description;
    }
    public void ToggleHoverWindow(bool enabled) {
        if(!enabled) {
            hoverSlot = null;
        }
        itemContext.SetActive(enabled);
    }
    void UpdateContextMenuButtons() {
        if(!hoverSlot) {
            Debug.Log("No hover slot");
            return;
        }
        contextSlot = hoverSlot;
        contextBuyButton.gameObject.SetActive(false);
        contextSellButton.gameObject.SetActive(false);
        contextTransferButton.gameObject.SetActive(false);
        contextUseButton.gameObject.SetActive(false);

        if(inventoryManager.inShop) {
            if(hoverSlot.slotManager == inventoryManager.shopSlotManager)
                contextBuyButton.gameObject.SetActive(true);
            if(hoverSlot.slotManager == inventoryManager.bagSlotManager)
                contextSellButton.gameObject.SetActive(true);
        }
        else if(inventoryManager.inStorage) {
            contextTransferButton.gameObject.SetActive(true);
        }
        else if(hoverSlot.item.item.itemTags.Contains(ItemTag.Consumable)) {
            contextUseButton.gameObject.SetActive(true);
        }
    }
    public void ToggleContextMenu(bool enabled) {
        if(enabled) {
            UpdateContextMenuButtons();
        }
        contextMenu.SetActive(enabled);
    }
    public void ContextBuy() {
        InitiateItemTransfer(contextSlot);
    }
    public void ContextSell() {
        InitiateItemTransfer(contextSlot);
    }
    public void ContextTransfer() {
        InitiateItemTransfer(contextSlot);
    }
    public void ContextUse() {

    }
    public void ContextDestroy() {

    }

    public void InitiateItemTransfer(ItemSlot itemSlot) {
        InitiateItemTransfer(itemSlot, null);
    }
    public void InitiateItemTransfer(ItemSlot from, ItemSlot to) {
        inventoryManager.fromSlot = from;
        inventoryManager.toSlot = to;

        if(from.item.item.maxAmount == 1) {
            quantValue = 1;
            // EnableTransactionConfirmPrompt();
            ConfirmTransaction();
        }
        else {
            quantityPromptItem = from.item.item;
            quantValue = 1;
            ownedValue = from.slotManager.inventory.GetItemTotalAmount(from.item.item);
            quantityPromptInput.text = quantValue.ToString();
            quantityPrompt.SetActive(true);
        }
    }

    int quantValue, ownedValue;
    // used by input quantity prompt input field onvaluechange
    void OnQuantityPromptChanged(string value) {
        quantValue = int.Parse(value);
    }
    // void EnableTransactionConfirmPrompt() {
    //     confirmTransactionText.text = string.Format("{0} {1}. Complete transaction?", Inventory.GetCostValue(inventoryManager.fromSlot.item.item, quantValue), currencyName);
    //     confirmTransactionPrompt.SetActive(true);
    // }

    // called by input quantity prompt confirm button
    public void ModifyQuantityPrompt(int amount) {
        quantValue = amount == 0 ? 0 : Mathf.Clamp(quantValue + amount, 0, ownedValue);
        quantityPromptInput.text = quantValue.ToString();
    }
    public void ConfirmTransaction() {
        Feedback.I.RegisterConfirmRequest(ConfirmQuantityPromptTransaction,
        confirmTransactionText.text = string.Format("{0} {1}. Complete transaction?", Inventory.GetCostValue(inventoryManager.fromSlot.item.item, quantValue), currencyName));
    }
    // called by confirm prompt confirm button
    public int ConfirmQuantityPromptTransaction() {
        inventoryManager.CompleteTransaction(quantValue);
        return quantValue;
    }
}
}