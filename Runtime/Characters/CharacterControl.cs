using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Characters {
public class CharacterControl : MonoBehaviour
{
    public Character character;
    public Animator animator;
    public float moveMult = 1f;

    [SerializeField]
    Transform head;

    public Transform Head { 
        get {
            if(!head) head = animator.GetBoneTransform(HumanBodyBones.Head);
            return head;
        }
    }

    private void Awake() {
        if(!animator) animator = GetComponent<Animator>();
        if(animator && !head) {
            head = animator.GetBoneTransform(HumanBodyBones.Head);
        }
    }

    private void Start() {
        if(!character) {
            // Debug.Log("Character has no character item");
            return;
        }
        OnEnable();
    }
    private void OnEnable() {
        if(character && CharacterManager.I)
            CharacterManager.I.RegisterCharacter(character, this);
    }
    private void OnDisable() {
        if(character && CharacterManager.I)
            CharacterManager.I.RemoveCharacter(character);
    }
}
}