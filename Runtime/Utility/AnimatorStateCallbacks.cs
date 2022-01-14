using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class AnimatorStateCallbacks : StateMachineBehaviour
{
    public string stateName;

    public AnimatorStateInfo stateInfo { get; private set; }
    public int layerIndex { get; private set; }

    public System.Action<Animator, int, AnimatorControllerPlayable> onStateMachineEnter, onStateMachineExit;
    public System.Action<Animator, AnimatorStateInfo, int> onStateEnter, onStateUpdate, onStateExit, onStateMove;
    public System.Action<Animator, AnimatorStateInfo, int, AnimatorControllerPlayable> onStateIK;

    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash, AnimatorControllerPlayable controller) {
        base.OnStateMachineEnter(animator, stateMachinePathHash, controller);
        onStateMachineEnter?.Invoke(animator, stateMachinePathHash, controller);
    }
    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash, AnimatorControllerPlayable controller) {
        base.OnStateMachineExit(animator, stateMachinePathHash, controller);
        onStateMachineExit?.Invoke(animator, stateMachinePathHash, controller);
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        base.OnStateEnter(animator, animatorStateInfo, layerIndex);
        this.stateInfo = animatorStateInfo;
        this.layerIndex = layerIndex;
        onStateEnter?.Invoke(animator, animatorStateInfo, layerIndex);
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        base.OnStateUpdate(animator, animatorStateInfo, layerIndex);
        onStateUpdate?.Invoke(animator, animatorStateInfo, layerIndex);
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        base.OnStateEnter(animator, animatorStateInfo, layerIndex);
        onStateExit?.Invoke(animator, animatorStateInfo, layerIndex);
    }
    public override void OnStateMove(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        base.OnStateMove(animator, animatorStateInfo, layerIndex);
        onStateMove?.Invoke(animator, animatorStateInfo, layerIndex);
    }
    public override void OnStateIK(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex, AnimatorControllerPlayable controller) {
        base.OnStateIK(animator, animatorStateInfo, layerIndex, controller);
        onStateIK?.Invoke(animator, animatorStateInfo, layerIndex, controller);
    }
}
