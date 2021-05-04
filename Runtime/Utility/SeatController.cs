// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
// using m4k.BuildSystem;
using m4k.InventorySystem;
using m4k.Characters;

namespace m4k {
public class SeatController : MonoBehaviour//, IBuildable
{
    public int seatIndex;
    public Renderer visualRend;
    public Transform UITargetTrans;
    public ItemArranger tableTop;
    public Renderer pointRenderer;
    public CharacterAnimation charAnim;
    public bool hasZone, occupied, available = true;
    public System.Action<SeatController> onUpdateSeat;

    public bool reserved { get { return m_Reserved; } 
                            set { m_Reserved = value; OnChangeOccupied(); }}

    TMPro.TMP_Text seatLabel;
    Material origPointerMat;
    bool m_Reserved = false;

    void Start() {
        if(!visualRend)
            visualRend = transform.parent.GetComponentInChildren<Renderer>();
        if(!tableTop)
            tableTop = GetComponentInChildren<ItemArranger>();
        origPointerMat = pointRenderer.material;
    }
    private void OnDisable() {
        Feedback.I?.worldToScreenUIFollow.UnregisterFollowUI(UITargetTrans);
    }
    public void RegisterLabels() {
        seatLabel = Feedback.I.worldToScreenUIFollow.RegisterDefaultTxtUI(visualRend, UITargetTrans).GetComponentInChildren<TMPro.TMP_Text>();
        
        OnChangeOccupied();
    }
    public void SetLabel(string text) {
        if(seatLabel) 
            seatLabel.text = seatIndex.ToString() + " " + text;
    }
    public void UpdateSeat() {
        onUpdateSeat?.Invoke(this);
    }
    public void SetPointerMat(Material material) {
        pointRenderer.material = material;
    }
    public void RevertPointerMat() {
        pointRenderer.material = origPointerMat;
    }

    public void AssignSeat(CharacterAnimation ca) {
        charAnim = ca;
        reserved = true;
    }

    public void UnassignSeat() {
        reserved = false;
        tableTop?.HideItems();
        charAnim = null;
    }
    public void OnToggleBuildableVisual(bool b) {
        pointRenderer.enabled = b;
    }
    public void OnToggleBuildableEdit(bool b) {
        ToggleUsable(b);
    }
    public void ToggleUsable(bool b) {
        available = !b;
        if(available) {
            UpdateSeat();
        }
        else {
            charAnim?.Unsit();
        }
    }
    void OnChangeOccupied() {
        if(seatLabel)
            seatLabel.color = reserved ? Color.red : Color.green;
    }
}
}