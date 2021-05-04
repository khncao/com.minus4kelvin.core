// using System;
using UnityEngine;
using UnityEngine.AI;

namespace m4k.Characters {
[RequireComponent(typeof (NavMeshAgent))]
public class NavCharacterControl : MonoBehaviour
{
    public CharacterControl cc;
    public float repathInterval = 1f;
    public bool isPlayer;
    public GameObject pathTarget;
    public bool isPathing;
    public Transform target;
    public System.Action<Transform> onArrive, onNewTarget;
    // public TMPro.TMP_Text debugText;
    public NavMeshAgent agent;
    // public NavMeshObstacle obstacle { get; private set; }

    Camera mainCam;
    Transform faceTarget;

    private void Start()
    {
        if(!cc) cc = GetComponent<CharacterControl>();
        agent = GetComponentInChildren<NavMeshAgent>();
        // obstacle = GetComponent<NavMeshObstacle>();
        if(pathTarget)
            pathTarget.transform.SetParent(null);
        agent.updateRotation = false;
        if(isPlayer)
            agent.updatePosition = false;
        mainCam = Camera.main;
    }

    Vector3 lastTargetPos;
    float nextRepathThresh;
    private void Update()
    {
        if (target != null && (Time.time > nextRepathThresh || isPlayer)) {
            if((!agent.hasPath || agent.isPathStale) || target.position != lastTargetPos) {
                agent.SetDestination(target.position);
                lastTargetPos = target.position;
            }
            nextRepathThresh = repathInterval + Time.time;
        }
        
        agent.isStopped = !cc.charAnim.IsMobile;
        if(agent.hasPath && cc.charAnim.IsMobile) {
            if(agent.remainingDistance > agent.stoppingDistance * Time.timeScale) {
                if(cc.rbChar)
                    cc.rbChar.Move(agent.velocity, false, false);
            }
            else {
                if(target && isPathing) {
                    OnArrive();
                }
                if(cc.rbChar)
                    cc.rbChar.Move(Vector3.zero, false, false);
            }
            if(!cc.rbChar)
                cc.charAnim.SetMoveParams(0.5f, 0f, false);
        }
        else {
            cc.charAnim.SetMoveParams(0, 0, false);
        }
            

        if(!isPathing) {
            if(agent.hasPath) {
                if(!isPlayer)
                    agent.updatePosition = true;
                isPathing = true;
            }

            if(faceTarget) {
                Vector3 dir = faceTarget.position - transform.position;
                    
                var rot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * 2f);
            }
        }
    }

    private void OnDestroy() {
        Destroy(pathTarget);
    }

    void OnArrive() {
        onArrive?.Invoke(target);
        StopAgent();
        cc.iK?.EnableIk();
    }

    public void StopAgent() {
        isPathing = false;
        target = null;
        if(agent.isOnNavMesh && agent.hasPath)
            agent.ResetPath();
        // if(cc.rbChar)
        //     cc.rbChar.rb.isKinematic = false;

        if(!pathTarget) return;
        pathTarget.SetActive(false);
        // pathTarget.transform.SetParent(null);
    }

    void SetPathIndicator(Vector3 pos) {
        if(!pathTarget) return;
        pathTarget.SetActive(true);
        pathTarget.transform.position = pos;
    }
    public void SetGroundTarget(Vector3 pos) {
        SetPathIndicator(pos);
        SetTarget(pathTarget.transform);
    }
    public void SetTarget(Transform t) {
        if(cc.charAnim && cc.charAnim.IsSitting) {
            cc.charAnim.Unsit();
        }
        isPathing = true;
        cc.iK?.DisableIk();

        SetPathIndicator(t.position);
        target = t;

        SetFaceTarget(null);
        onNewTarget?.Invoke(target);

        // Debug.Log($"Set target: {target.gameObject.name}");
    }

    public void SetFaceTarget(Transform target) {
        faceTarget = target;
    }
    public void AgentIsStopped(bool b) {
        agent.isStopped = b;
    }
}
}