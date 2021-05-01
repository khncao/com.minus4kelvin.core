using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace m4k.Interaction {
public class VisibilityInteraction : MonoBehaviour
{
    [System.Serializable]
    public class UnityEvents {
        public UnityEvent onBecomeVisible, onBecomeInvisible;
    }
    public UnityEvents events;
    public Conditions onVisibleConds, onInvisibleConds;
    public float minTime = 0;

    Coroutine visibleCR, invisibleCR;

    private void OnBecameVisible() {
        if(!onVisibleConds.CheckCompleteReqs())
            return;

        if(invisibleCR != null)
            StopCoroutine(invisibleCR);

        if(minTime == 0f)
            events.onBecomeVisible?.Invoke();
        else
            visibleCR = StartCoroutine(WaitToInvokeVisible(minTime));
    }

    private void OnBecameInvisible() {
        if(!onInvisibleConds.CheckCompleteReqs())
            return;

        if(visibleCR != null)
            StopCoroutine(visibleCR);

        if(minTime == 0f)
            events.onBecomeInvisible?.Invoke();
        else if(gameObject.activeInHierarchy)
            invisibleCR = StartCoroutine(WaitToInvokeInvisible(minTime));
    }

    IEnumerator WaitToInvokeVisible(float time) {
        yield return new WaitForSeconds(time);
        events.onBecomeVisible?.Invoke();
    }

    IEnumerator WaitToInvokeInvisible(float time) {
        yield return new WaitForSeconds(time);
        events.onBecomeInvisible?.Invoke();
    }
}
}