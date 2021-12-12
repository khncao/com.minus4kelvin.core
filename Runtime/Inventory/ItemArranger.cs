using UnityEngine;
using System.Collections.Generic;

namespace m4k.Items {
public class ItemArranger : MonoBehaviour { //ITaskInteractable
    public Item[] items;
    public GameObject[] itemInstances;
    public Transform[] objPlaces;
    public bool downSurfacePlacement;
    public bool hasSurface;
    public LayerMask hitLayers;
    // public float radius;
    bool initialized;


    private void Start() {
        hitLayers = LayerMask.NameToLayer("Buildable");
        items = new Item[objPlaces.Length];
        itemInstances = new GameObject[objPlaces.Length];
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

    public void UpdateItems(List<Item> newItems) {
        // if(newItems.Count > objPlaces.Length) {
        //     Debug.LogError("More items than places");
        //     return;
        // }
        if(!initialized) Start();
        GameObject item;
        for(int i = 0; i < objPlaces.Length; ++i) {
            if(i > newItems.Count - 1) 
                continue;
            if(newItems[i] == items[i]) {
                item = itemInstances[i];
                itemInstances[i].SetActive(true);
            }
            else {
                Destroy(itemInstances[i]);
                item = Instantiate(newItems[i].prefab);
                item.transform.SetParent(objPlaces[i], false);
                items[i] = newItems[i];
                itemInstances[i] = item;
            }
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

    public void UpdateItems(Item newItem) {
        if(newItem == items[0]) {
            itemInstances[0].SetActive(true);
        }
        else {
            itemInstances[0] = Instantiate(newItem.prefab);
            itemInstances[0].transform.SetParent(objPlaces[0], false);
            items[0] = newItem;
        }
    }

    public void HideItems() {
        for(int i = 0; i < itemInstances.Length; ++i) {
            if(itemInstances[i])
                itemInstances[i].SetActive(false);
        }
    }

    // public void OnTaskInteract(Task task) {

    // }
}}