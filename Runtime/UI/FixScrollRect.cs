using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using m4k.Items;

namespace m4k.UI {
public class FixScrollRect: MonoBehaviour, IBeginDragHandler,  IDragHandler, IEndDragHandler, IScrollHandler
{
    public UnityEngine.UI.ScrollRect MainScroll;

    private void Start() {
        if(!MainScroll) MainScroll = GetComponentInParent<UnityEngine.UI.ScrollRect>();
    }
 
 
    public void OnBeginDrag(PointerEventData eventData)
    {
        MainScroll.OnBeginDrag(eventData);
    }
 
 
    public void OnDrag(PointerEventData eventData)
    {
        if(InventoryManager.I.UI.dragSlot) return;
        MainScroll.OnDrag(eventData);
    }
 
    public void OnEndDrag(PointerEventData eventData)
    {
        MainScroll.OnEndDrag(eventData);
    }
 
 
    public void OnScroll(PointerEventData data)
    {
        MainScroll.OnScroll(data);
    }
 
 
}
}