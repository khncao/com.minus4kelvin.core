using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChokeHandler : MonoBehaviour
{
    public List<GameObject> objectsUsing = new List<GameObject>();
    public List<GameObject> objectsWaiting = new List<GameObject>();
    public int maxUsing = 4;
    //
    private void OnTriggerEnter(Collider other) {
        var agent = other.GetComponent<NavMeshAgent>();
        if(agent) {
            if(objectsUsing.Count >= maxUsing) {
                if(!objectsWaiting.Contains(other.gameObject))
                    objectsWaiting.Add(other.gameObject);

                agent.isStopped = true;
            }
            else {
                if(!objectsUsing.Contains(other.gameObject))
                    objectsUsing.Add(other.gameObject);

                agent.isStopped = false;
            }
        }
        for(int i = 0; i < objectsUsing.Count; i++) {
            if(objectsUsing[i] == null) {
                objectsUsing.RemoveAt(i);
            }
        }
    }
    private void OnTriggerExit(Collider other) {
        var usingIndex = objectsUsing.FindIndex(x=>x == other.gameObject);
        if(usingIndex != -1) {
            
            objectsUsing.RemoveAt(usingIndex);

            if(objectsWaiting.Count > 0) {
                var agent = objectsWaiting[0].GetComponent<NavMeshAgent>();
                agent.isStopped = false;

                if(!objectsUsing.Contains(other.gameObject))
                    objectsUsing.Add(objectsWaiting[0]);

                objectsWaiting.RemoveAt(0);
            }
            return;
        }

        var waitingIndex = objectsWaiting.FindIndex(x=>x == other.gameObject);
        if(waitingIndex != -1) {
            var agent = objectsWaiting[waitingIndex].GetComponent<NavMeshAgent>();
            agent.isStopped = false;
            objectsWaiting.RemoveAt(waitingIndex);
        }
    }

}
