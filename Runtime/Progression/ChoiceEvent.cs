using UnityEngine;
using UnityEngine.Events;

namespace m4k.Progression {
public class ChoiceEvent : MonoBehaviour {
    public Dialogue dialogue;
    public string choiceText;

    [Tooltip("Conditions for interaction choice to auto register as a choice on dialogue")]
    public Conditions choiceRegisterConditions;
    public UnityEvent onChoice;

    Choice _choice;
    KeyAction _choiceAction;

    private void Start() {
        if(!dialogue)
            dialogue = GetComponent<Dialogue>();
        if(!dialogue)
            dialogue = GetComponentInParent<Dialogue>();
        if(!dialogue) {
            Debug.LogWarning("Choice event could not find dialogue");
            return;
        }
        dialogue.RegisterChoiceEvent(this);

        if(string.IsNullOrEmpty(choiceText))
            choiceText = gameObject.name;

        _choice = new Choice();
        _choice.text = choiceText;
        _choice.key = choiceText;

        _choiceAction = new KeyAction();
        _choiceAction.key = choiceText;
        _choiceAction.action = onChoice;
    }

    private void OnDisable() {
        RemoveChoice();
    }

    public void RegisterChoice() {
        if(!choiceRegisterConditions.CheckCompleteReqs())
            return;
        dialogue?.RegisterChoice(_choice, _choiceAction);
    }

    public void RemoveChoice() {
        dialogue?.RemoveChoice(_choice);
    }
}
}