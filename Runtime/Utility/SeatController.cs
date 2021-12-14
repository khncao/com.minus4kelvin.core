// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace m4k {
public interface ISittable {
    void Sit(SeatController seat);
    void Unsit();
}

public class SeatController : MonoBehaviour, IInteractable
{
    public int seatIndex;
    public Renderer visualRend;
    public Transform UITargetTrans;
    public Renderer pointRenderer;
    public ISittable sitter;
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
        origPointerMat = pointRenderer.material;
    }

    private void OnDisable() {
        Feedback.I?.worldToScreenUIFollow.UnregisterFollowUI(UITargetTrans);
    }

    public bool Interact(GameObject go) {
        ISittable sittable;
        go.TryGetComponent<ISittable>(out sittable);
        if(sittable != null) {
            sittable.Sit(this);
            return true;
        }
        return false;
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

    public void AssignSeat(ISittable s) {
        sitter = s;
        reserved = true;
    }

    public void UnassignSeat() {
        reserved = false;
        sitter = null;
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
            sitter?.Unsit();
        }
    }
    void OnChangeOccupied() {
        if(seatLabel)
            seatLabel.color = reserved ? Color.red : Color.green;
    }
}
}