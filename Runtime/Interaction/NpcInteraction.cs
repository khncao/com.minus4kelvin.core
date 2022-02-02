using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Progression;
using m4k.Characters;

namespace m4k.Interaction {
[RequireComponent(typeof(Interactable))]
public class NpcInteraction : MonoBehaviour
{
    public CharacterIK iK;
    public INavMovable movable;
    public m4k.Progression.Dialogue dialogue;

    Interactable interactable;

    private void Start() {
        interactable = GetComponentInChildren<Interactable>();
        if(!iK) iK = GetComponentInParent<CharacterIK>();
        if(movable == null) movable = GetComponentInParent<INavMovable>();
    }

    public void OnInteractable() {
        if(!iK)
            return;
        iK.SetLook(CharacterManager.I.Player.Head);
        // Game.I?.SetFocusTarget(cc.iK.headTarget, true);
    }
    public void OnNonInteractable() {
        if(iK)
            iK.SetLook(null);
        // Game.I?.SetFocusTarget(null, false);
        if(DialogueManager.I && dialogue) {
            dialogue.EndDialogue();
        }
    }

    public void StartDialogue() {
        if(dialogue) {
            movable?.Pause();
            dialogue.AssignDialogue();
            DialogueManager.I.onEndDialogue += OnEndDialogue;
        }
    }

    void OnEndDialogue() {
        DialogueManager.I.onEndDialogue -= OnEndDialogue;
        movable?.Resume();
    }
}
}