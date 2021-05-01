
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace m4k.UI {
public class SelectableInteraction : MonoBehaviour, ISelectHandler, IPointerEnterHandler, ISubmitHandler {
    public void OnSelect(BaseEventData eventData) {
        Feedback.I.PlayAudio(Feedback.I.highlightAudio);
        // Debug.Log("Selected");
    }
    public void OnPointerEnter(PointerEventData eventData) {
        // Feedback.I.PlayHighlightAudio();
        // Debug.Log("Pointer enter");
    }
    public void OnSubmit(BaseEventData eventData) {
        Feedback.I.PlayAudio(Feedback.I.selectAudio);
    }
    // public void OnPointerUp(PointerEventData eventData) {
    //     Feedback.I.PlaySelectAudio();
    // }
}}