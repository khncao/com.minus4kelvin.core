/// <summary>
/// Adopted from implementation in Unity's Standard Assets
/// </summary>

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

    Transform faceTarget, prevTarget;
    bool pause;

    private void Start()
    {
        if(!cc) cc = GetComponent<CharacterControl>();
        agent = GetComponentInChildren<NavMeshAgent>();
        // obstacle = GetComponent<NavMeshObstacle>();
        if(pathTarget)
            pathTarget.transform.SetParent(null);
        if(target)
            SetTarget(target);
        agent.updateRotation = false;
        if(isPlayer)
            agent.updatePosition = false;
    }

    Vector3 lastTargetPos;
    float nextRepathThresh;
    private void Update()
    {
        if(!agent.isOnNavMesh) return;

        if (target != null && (isPlayer || Time.time > nextRepathThresh)) {
            if((!agent.hasPath || agent.isPathStale) || target.position != lastTargetPos) {
                agent.SetDestination(target.position);
                if(pathTarget)
                    pathTarget.transform.position = target.position;
                lastTargetPos = target.position;
            }
            nextRepathThresh = repathInterval + Time.time;
        }
        
        agent.isStopped = !cc.charAnim.IsMobile || pause;

        if(agent.hasPath && cc.charAnim.IsMobile && isPathing) {
            if(agent.remainingDistance > agent.stoppingDistance) {
                if(cc.rbChar)
                    cc.rbChar.Move(agent.velocity, false, false);
                else
                    cc.charAnim.SetMoveParams(0.5f, 0f, false);
            }
            else {
                if(target && (target.position - transform.position).sqrMagnitude < Mathf.Pow(agent.stoppingDistance, 2)) {
                    OnArrive();
                }
            }
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
    }

    public void StopAgent() {
        isPathing = false;
        if(target)
            prevTarget = target;
        target = null;
        cc.charAnim.SetMoveParams(0, 0, false);
        cc.iK?.EnableIk();
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
        SetTarget(pathTarget.transform, true);
    }
    public void SetTarget(Transform t, bool overrideReset = false) {
        if(cc.charAnim && cc.charAnim.IsSitting) {
            cc.charAnim.Unsit();
        }
        if(isPathing && !overrideReset) {
            StopAgent();
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
    public void ToggleAgentPause(bool b) {
        pause = b;
    }
    public void ResumeLastTarget() {
        if(!prevTarget) return;
        SetTarget(prevTarget);
    }
}
}