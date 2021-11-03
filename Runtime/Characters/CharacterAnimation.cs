using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace m4k.Characters {
public class CharacterAnimation : MonoBehaviour
{
	// public AnimationClip testClip1;
	// PlayableGraph playableGraph;
	public CharacterControl cc;
	public Transform headHold, rHandHold;
	public RandomAudioPlayer footstepAudioPlayer;
	public AudioSource seatAudioSource;
	public AnimatorOverrideController drunkOverride;
	public readonly int forwardHash = Animator.StringToHash("Forward");
	public readonly int turnHash = Animator.StringToHash("Turn");
	public readonly int jumpHash = Animator.StringToHash("Jump");
	public readonly int sittingHash = Animator.StringToHash("Sitting");
	public readonly int footfallHash = Animator.StringToHash("Footfall");
	public readonly int onGroundHash = Animator.StringToHash("OnGround");
	public readonly int crouchHash = Animator.StringToHash("Crouch");
	public readonly int upperbodyGestureHash = Animator.StringToHash("UpperbodyGesture");
	public readonly int fullbodyGestureHash = Animator.StringToHash("FullbodyGesture");
	public readonly int rHandHoldHash = Animator.StringToHash("RHHold");
	public readonly int posingHash = Animator.StringToHash("Posing");

	public readonly int groundedStateHash = Animator.StringToHash("Grounded");
	public readonly int neutralStateHash = Animator.StringToHash("neutral1");
	public readonly int drinkStateHash = Animator.StringToHash("drink");

	public AnimatorStateInfo curr0State, curr1State, prev0State, prev1State;
    public Animator anim;
	public System.Action onceOnState0End, onceOnState1End;
	public AnimationClip[] fullbodyGestures, upperbodyGestures;
	public System.Action onSit, onUnsit;

	public bool IsDrinking { get { return curr1State.shortNameHash == drinkStateHash; }}
	public bool IsMobile { get { return curr0State.shortNameHash == groundedStateHash; }}
	public bool IsNeutral { get { return curr1State.shortNameHash == neutralStateHash; }}
	public bool IsSitting { get { return sitting;} }

	bool sitting = false;
	float turn, forward, forwardMult = 1f;
	bool crouch;
	RuntimeAnimatorController origAnimController;
	AnimatorOverrideController overrideController;
	AnimationClipOverrides clipOverrides;
	Rigidbody rb;
	CapsuleCollider col;
	bool posing;
	GameObject prop;

    void Awake() {
		rb = GetComponent<Rigidbody>();
		col = GetComponent<CapsuleCollider>();
        anim = GetComponentInChildren<Animator>();

		if(headHold)
			headHold.SetParent(anim.GetBoneTransform(HumanBodyBones.Head), false);
		if(rHandHold)
			rHandHold.SetParent(anim.GetBoneTransform(HumanBodyBones.RightHand), false);
    }

	private void Start() {
		// if(testClip1)
		// 	AnimationPlayableUtilities.PlayClip(anim, testClip1, out playableGraph);
		overrideController = new AnimatorOverrideController(anim.runtimeAnimatorController);
		anim.runtimeAnimatorController = overrideController;
		clipOverrides = new AnimationClipOverrides(overrideController.overridesCount);
		overrideController.GetOverrides(clipOverrides);

		origAnimController = anim.runtimeAnimatorController;
		RandomizeGestures();
	}

    void Update() {
		HandleFootsteps();
		UpdateAnimator();
        UpdateStates();
    }
	public void RandomizeGestures() {
		if(fullbodyGestures.Length > 0) {
			int randFullbody = Random.Range(0, fullbodyGestures.Length - 1);
			clipOverrides["fullbodyGesture"] = fullbodyGestures[randFullbody];
		}
		if(upperbodyGestures.Length > 0) {
			int randUpperbody = Random.Range(0, upperbodyGestures.Length - 1);
			clipOverrides["upperbodyGesture"] = upperbodyGestures[randUpperbody];
		}

		overrideController.ApplyOverrides(clipOverrides);
	}
	public void TogglePoseMode(bool b) {
		posing = b;
		anim.SetBool(posingHash, b);
	}

	public void Drink(bool hideItemAfterAnim) {
		anim.Play(drinkStateHash);
		if(hideItemAfterAnim)
			onceOnState1End += DisableRightHandHold;
	}
	public void SetDrunk(int dur) {
		anim.runtimeAnimatorController = drunkOverride;
		Invoke("SetOrigAnims", dur);
	}
	public void SetOrigAnims() {
		anim.runtimeAnimatorController = origAnimController;
	}
	// TODO: action key:anim in editor or playables for oneoff blending
	public void DoAction(string key) {
		anim.SetTrigger(key);
	}
	public void UpperbodyGesture() {
		anim.SetTrigger(upperbodyGestureHash);
		RandomizeGestures();
	}
	public void FullbodyGesture() {
		anim.SetTrigger(fullbodyGestureHash);
		RandomizeGestures();
	}
	public void EnableRightHandHold(GameObject p) {
		prop = p;
		prop?.SetActive(true);
		anim.SetBool(rHandHoldHash, true);
	}
	public void DisableRightHandHold() {
		if(anim)
			anim.SetBool(rHandHoldHash, false);
		prop?.SetActive(false);
	}
	public void SetMoveMult(float f) {
		forwardMult = f;
	}
	public void SetMoveParams(float f, float t, bool c) {
		forward = f; // * forwardMult
		turn = t;
		crouch = c;
	}
	void UpdateAnimator()
	{
		anim.SetFloat(forwardHash, forward, 0.1f, Time.deltaTime);
		anim.SetFloat(turnHash, turn, 0.1f, Time.deltaTime);
		anim.SetBool(crouchHash, crouch);
		// m_Animator.SetBool(onGroundHash, m_IsGrounded);
		// if (!m_IsGrounded) {
		// 	m_Animator.SetFloat(jumpHash, m_Rigidbody.velocity.y);
		// }
	}

	private void OnAnimatorMove() {
		if(!cc) return;
		if(cc.rbChar) {
			if (cc.rbChar.isGrounded && Time.deltaTime > 0)
			{
				Vector3 v = (anim.deltaPosition) / Time.deltaTime;

				v.y = rb.velocity.y;
				rb.velocity = v;

				if(!Mathf.Approximately(cc.rbChar.move.sqrMagnitude, 0))
					cc.navChar.agent.nextPosition = transform.position;
			}
		}
		else if(cc.navChar) {
			cc.navChar.agent.speed = (anim.deltaPosition / Time.deltaTime).magnitude;
			transform.position = cc.navChar.agent.nextPosition;
			if(cc.navChar.agent.velocity.sqrMagnitude > Mathf.Epsilon)
				transform.rotation = Quaternion.LookRotation(cc.navChar.agent.velocity.normalized);
		}
	}

	void UpdateStates() {
		prev0State = curr0State;
		prev1State = curr1State;

		curr0State = anim.GetCurrentAnimatorStateInfo(0);
		curr1State = anim.GetCurrentAnimatorStateInfo(1);
		if(onceOnState0End != null && (curr0State.normalizedTime > 0.95f)) {
			onceOnState0End?.Invoke();
			onceOnState0End = null;
		}
		if(onceOnState1End != null && (curr1State.normalizedTime > 0.95f)) {
			onceOnState1End?.Invoke();
			onceOnState1End = null;
		}
	}
	Vector3 preSitPosition, origColCenter;
	bool origAgentUpdating;
	SeatController currSeat;
	public void Sit(SeatController seat) {
		currSeat = seat;
		currSeat.AssignSeat(this);
		currSeat.occupied = true;
		anim.SetBool(sittingHash, true);
		
		preSitPosition = transform.position;
		transform.position = seat.transform.position;
		transform.rotation = seat.transform.rotation;
		sitting = true;
		if(seatAudioSource) {
			seatAudioSource.PlayOneShot(seatAudioSource.clip);
		}
		onSit?.Invoke();
	}
	public void Unsit() {
		anim.SetBool(sittingHash, false);
		transform.position = preSitPosition;
		sitting = false;
		if(seatAudioSource) {
			seatAudioSource.PlayOneShot(seatAudioSource.clip);
		}
		onUnsit?.Invoke();
		if(!currSeat) return;
		currSeat.occupied = false;
		currSeat.UnassignSeat();
		currSeat = null;
	}

	float footstepCD = 0.1f, lastFootstepTime;
	void HandleFootsteps() {
		// if(footstepAudioSource) {
			if(footstepAudioPlayer) {
			float footfall = anim.GetFloat(footfallHash);
			if(Time.time - lastFootstepTime > footstepCD && footfall > 0.1f) {
				// footstepAudioSource.PlayOneShot(footstepAudioSource.clip);
				footstepAudioPlayer.PlayRandomClip();
				lastFootstepTime = Time.time;
			}
		}
	}

	public void PlayAnimation(string name) {
		anim.Play(name);
	}
}
}