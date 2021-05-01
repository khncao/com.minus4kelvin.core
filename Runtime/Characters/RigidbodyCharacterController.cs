using UnityEngine;
using System;

namespace m4k.Characters {
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class RigidbodyCharacterController : MonoBehaviour
{
	public CharacterControl cc;
	public TMPro.TMP_Text debugText;
	public Rigidbody rb;
	// public AudioSource footstepAudioSource;
	[SerializeField] float m_MovingTurnSpeed = 360;
	[SerializeField] float m_StationaryTurnSpeed = 180;
	[SerializeField] float m_JumpPower = 12f;
	[Range(1f, 4f)][SerializeField] float m_GravityMultiplier = 2f;
	[SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
	[SerializeField] float m_MoveInputMultiplier = 1f;
	[SerializeField] float m_MoveSpeedMultiplier = 1f;
	[SerializeField] float m_AnimSpeedMultiplier = 1f;
	[SerializeField] float m_GroundCheckDistance = 0.1f;
	public Vector3 move;
	public bool isGrounded = true;

	bool jumpInput, crouchInput;
	float m_OrigGroundCheckDistance;
	const float k_Half = 0.5f;
	float m_TurnAmount;
	float m_ForwardAmount;
	Vector3 m_GroundNormal;
	float m_CapsuleHeight;
	Vector3 m_CapsuleCenter;
	CapsuleCollider m_Capsule;
	bool m_Crouching;

	void Start()
	{
		if(!cc) cc = GetComponent<CharacterControl>();
		rb = GetComponent<Rigidbody>();
		m_Capsule = GetComponent<CapsuleCollider>();
		m_CapsuleHeight = m_Capsule.height;
		m_CapsuleCenter = m_Capsule.center;

		m_OrigGroundCheckDistance = m_GroundCheckDistance;
	}
	// void Update() {
	// 	if(debugText && Time.frameCount % 12 == 0) {
	// 		debugText.text = "Velocity: x" + string.Format("{0:0.###}", m_Rigidbody.velocity.x) + " y" + string.Format("{0:0.###}", m_Rigidbody.velocity.y) + " z" + string.Format("{0:0.###}", m_Rigidbody.velocity.z) + '\n';
	// 	}
	// }

	public void Move(Vector3 move, bool crouch, bool jump)
	{
		move *= cc.moveMult;
		if (move.magnitude > 1f) move.Normalize();
		move = transform.InverseTransformDirection(move);
		// CheckGroundStatus();
		// move = Vector3.ProjectOnPlane(move, m_GroundNormal);
		m_TurnAmount = Mathf.Atan2(move.x, move.z);
		m_ForwardAmount = move.z;
		this.move = move;

		ApplyExtraTurnRotation();
		jumpInput = jump;
		crouchInput = crouch;

		HandleAirborneMovement();

		// ScaleCapsuleForCrouching(crouch);
		// PreventStandingInLowHeadroom();
		cc.charAnim.SetMoveParams(m_ForwardAmount, m_TurnAmount, crouchInput);
	}

	// public void SetAnimMultiplier(float mod) {
	// 	m_MoveSpeedMultiplier = mod;
	// }
	public void SetMoveInputMultiplier(float mod) {
		m_MoveInputMultiplier = mod;
	}

	void ScaleCapsuleForCrouching(bool crouch)
	{
		if (isGrounded && crouch)
		{
			if (m_Crouching) return;
			m_Capsule.height = m_Capsule.height / 2f;
			m_Capsule.center = m_Capsule.center / 2f;
			m_Crouching = true;
		}
		else
		{
			Ray crouchRay = new Ray(rb.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
			float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
			if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
			{
				m_Crouching = true;
				return;
			}
			m_Capsule.height = m_CapsuleHeight;
			m_Capsule.center = m_CapsuleCenter;
			m_Crouching = false;
		}
	}

	void PreventStandingInLowHeadroom()
	{
		// prevent standing up in crouch-only zones
		if (!m_Crouching)
		{
			Ray crouchRay = new Ray(rb.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
			float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
			if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
			{
				m_Crouching = true;
			}
		}
	}

	void HandleAirborneMovement()
	{
		// if(curr0State.shortNameHash == onGroundHash) {
		if(isGrounded) {
			if (jumpInput && !crouchInput)
			{
				rb.velocity = new Vector3(rb.velocity.x, m_JumpPower, rb.velocity.z);
				isGrounded = false;
				// m_Animator.applyRootMotion = false;
				m_GroundCheckDistance = 0.1f;
			}
		}
		else {
			Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
			rb.AddForce(extraGravityForce);

			m_GroundCheckDistance = rb.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
		}
	}

	void ApplyExtraTurnRotation()
	{
		float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
		transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
	}

	// public void OnAnimatorMove()
	// {
	// 	if (isGrounded && Time.deltaTime > 0)
	// 	{
	// 		Vector3 v = (charAnim.anim.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;
	// 		v.y = rb.velocity.y;
	// 		rb.velocity = v;
	// 		if(m_ForwardAmount != 0)
	// 			navCharacter.agent.nextPosition = transform.position;
	// 	}
	// }
	
	void CheckGroundStatus()
	{
		RaycastHit hitInfo;
#if UNITY_EDITOR
		// helper to visualise the ground check ray in the scene view
		Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
#endif
		// 0.1f is a small offset to start the ray from inside the character
		// it is also good to note that the transform position in the sample assets is at the base of the character
		if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
		{
			m_GroundNormal = hitInfo.normal;
			isGrounded = true;
			cc.charAnim.anim.applyRootMotion = true;
		}
		else
		{
			isGrounded = false;
			m_GroundNormal = Vector3.up;
			cc.charAnim.anim.applyRootMotion = false;
		}
	}
}
}