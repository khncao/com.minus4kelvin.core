using UnityEngine;
using System.Collections.Generic;

namespace m4k.Items {
public class ItemArranger : MonoBehaviour { //ITaskInteractable
    [Tooltip("Concrete points for gameObject instance placement; should be children of this component")]
    public Transform[] objPlaces;
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


    private void Start() {
        hitLayers = LayerMask.NameToLayer("Buildable");
        _items = new Item[objPlaces.Length];
        _spawnedItems = new GameObject[objPlaces.Length];
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

    /// <summary>
    /// If gameobject instance matches new, recycle; otherwise destroy & replace. 
    /// Only up to amount of placement transforms will have item arranged. 
    /// </summary>
    /// <param name="newItems"></param>
    public void UpdateItems(List<ItemInstance> newItems) {
        if(!initialized) Start();
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
                    DestroyImmediate(_spawnedItems[objPlaceIdx]);
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

    // public void OnTaskInteract(Task task) {

    // }
}}