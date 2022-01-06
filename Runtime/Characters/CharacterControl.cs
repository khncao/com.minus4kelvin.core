using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Characters {
public class CharacterControl : MonoBehaviour
{
    public Character character;
    public Animator animator;
    public INavMovable movable;
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
        if(movable == null) movable = GetComponent<INavMovable>();
        if(animator && !head) {
            head = animator.GetBoneTransform(HumanBodyBones.Head);
        }
    }

    private void Start() {
        OnEnable();
    }

    private void OnEnable() {
        if(CharacterManager.I)
            CharacterManager.I.RegisterCharacter(this);
    }
    
    private void OnDisable() {
        if(CharacterManager.I)
            CharacterManager.I.RemoveCharacter(this);
    }
}
}