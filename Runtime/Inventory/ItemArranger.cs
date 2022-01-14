using UnityEngine;
using System.Collections.Generic;

namespace m4k.Items {
public class ItemArranger : MonoBehaviour { //ITaskInteractable
    [Tooltip("Concrete points for gameObject instance placement; should be children of this component")]
    public Transform[] objPlaces;
    public Renderer containerRenderer;
    [Tooltip("Cast ItemArranger down to first ray hit in hitLayers")]
    public bool downSurfacePlacement;
    [Tooltip("Layers hit for downSurfacePlacement ray cast")]
    public LayerMask hitLayers;
    // public float radius;

    public bool hasSurface { get; private set; }
    public bool isActive { get { return gameObject.activeInHierarchy; }}
    public List<ItemInstance> itemInstances { get; private set; }

    bool initialized;
    
    Item[] _items;
    GameObject[] _spawnedItems;
    Inventory _inventory;

    private void Awake() {
        _items = new Item[objPlaces.Length];
        _spawnedItems = new GameObject[objPlaces.Length];
        if(!TryGetComponent<InventoryComponent>(out InventoryComponent ic)) {
            ic = gameObject.AddComponent<InventoryComponent>();
            ic.inventorySlots = objPlaces.Length;
        }
        _inventory = ic.inventory;
        _inventory.onChange -= UpdateItems;
        _inventory.onChange += UpdateItems;

        if(downSurfacePlacement) {
            RaycastHit hit;
            if(Physics.Raycast(transform.position, -transform.up, out hit, 5, hitLayers, QueryTriggerInteraction.Ignore)) {
                transform.position = hit.point;
                hasSurface = true;
            }
            else hasSurface = false;
        }
        initialized = true;
    }

    public void ToggleContainer(bool on) {
        if(!containerRenderer) return;
        containerRenderer.enabled = on;
    }

    void UpdateItems() {
        UpdateItems(_inventory.totalItemsList);
    }

    /// <summary>
    /// If gameobject instance matches new, recycle; otherwise destroy & replace. 
    /// Only up to amount of placement transforms will have item arranged. 
    /// </summary>
    /// <param name="newItems"></param>
    public void UpdateItems(List<ItemInstance> newItems) {
        GameObject item;
        int objPlaceIdx = 0;
        itemInstances = newItems;

        for(int i = 0; i < newItems.Count; ++i) 
        {
            for(int j = 0; j < newItems[i].amount; ++j) 
            {
                if(newItems[i].item == _items[objPlaceIdx]) {
                    item = _spawnedItems[objPlaceIdx];
                    item.SetActive(true);
                }
                else {
                    Destroy(_spawnedItems[objPlaceIdx]);
                    item = Instantiate(newItems[i].item.prefab);
                    item.transform.SetParent(objPlaces[objPlaceIdx], false);
                }
                _items[objPlaceIdx] = newItems[i].item;
                _spawnedItems[objPlaceIdx] = item;

                objPlaceIdx++;
                if(objPlaceIdx >= objPlaces.Length)
                    return;
            }
        }

        while(objPlaceIdx < objPlaces.Length) {
            _spawnedItems[objPlaceIdx]?.SetActive(false);
            objPlaceIdx++;
        }
        
        ToggleContainer(_inventory.totalItemsList.Count > 0);
    }

    // void ArrangeItems() {
    //     var euler = targetPos.eulerAngles;
    //     targetPos.eulerAngles = new Vector3(euler.x, Random.Range(0, 360), euler.z);
    //     float rad;
    //     float angleStep = 360 / groundTargets.Length;

    //     for(int i = 0; i < groundTargets.Length; ++i) {
    //         rad = i * angleStep * Mathf.Deg2Rad; 
    //         groundTargets[i].localPosition = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * groupZone.col.radius;
    //     }        
    // }

    // Single item
    public void UpdateItems(Item newItem) {
        if(newItem == _items[0]) {
            _spawnedItems[0].SetActive(true);
        }
        else {
            _spawnedItems[0] = Instantiate(newItem.prefab);
            _spawnedItems[0].transform.SetParent(objPlaces[0], false);
            _items[0] = newItem;
        }
    }

    public void HideItems() {
        for(int i = 0; i < _spawnedItems.Length; ++i) {
            if(_spawnedItems[i])
                _spawnedItems[i].SetActive(false);
        }
    }

    public void GetItems(List<ItemInstance> items) {
        _inventory.AddItemAmounts(items);
        // UpdateItems();
    }

    public void RemoveItems(List<ItemInstance> items) {
        foreach(var i in items) 
            _inventory.RemoveItemAmount(i.item, i.amount);
        // UpdateItems();
    }

    public void GetInventory(Inventory from, List<ItemInstance> items = null) {
        if(items == null) items = from.totalItemsList;
        Inventory.Transfer(from, _inventory, items);
        // UpdateItems();
    }

    public void GiveInventory(Inventory to, List<ItemInstance> items = null) {
        if(items == null) items = _inventory.totalItemsList;
        Inventory.Transfer(_inventory, to, items);
        // UpdateItems();
    }

    // public void OnTaskInteract(Task task) {

    // }
}}