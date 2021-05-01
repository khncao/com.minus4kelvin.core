using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorHash : MonoBehaviour
{
    Dictionary<string, int> paramHashDict = new Dictionary<string, int>();
    // Dictionary<string, int> stateHashDict = new Dictionary<string, int>();
    Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();

        for(int i = 0; i < animator.parameterCount; ++i) {
            paramHashDict.Add(animator.parameters[i].name, animator.parameters[i].nameHash);
        }
    }

    public int Param(string name) {
        int hash;
        if(!paramHashDict.TryGetValue(name, out hash)) {
            Debug.LogWarning($"Param {name} not found on {gameObject}");
        }
        return hash;
    }
}
