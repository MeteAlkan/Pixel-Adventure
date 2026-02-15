using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{

	[SerializeField] private float m_JumpForce;                          // Amount of force added when the player jumps.
	[SerializeField] private float m_DoubleJumpForce;                          // Amount of force added when the player jumps.
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;  // How much to smooth out the movement
	[SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
	[SerializeField] private LayerMask enemyLayer;                          // A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.

	const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	 public bool m_Grounded;            // Whether or not the player is grounded.
	private Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private Vector3 m_Velocity = Vector3.zero;
	private bool attacked = false;

	[Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

	}

    private void Update()
    {
		if (m_Grounded)
			canDoubleJump = true;
    }
    private void FixedUpdate()
	{
		bool wasGrounded = m_Grounded;
		m_Grounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
			{
				m_Grounded = true;
		
				if (!wasGrounded)
					OnLandEvent.Invoke();
			}
		}

		RaycastHit2D hitEnemy = Physics2D.BoxCast(m_GroundCheck.position, new Vector2(1f,0.1f), 0f, Vector2.down, 0.1f, enemyLayer);

		// visualize ray in Scene view
		Debug.DrawRay(m_GroundCheck.position,  Vector2.down * 0.1f, Color.red);

		if (hitEnemy.collider != null)
        {
			if (!attacked)
            {
				print("attacked");
				Enemy enemyScript = hitEnemy.collider.gameObject.GetComponent<Enemy>();
				if (enemyScript.type != EnemyType.Shelled)
                {
					enemyScript.TakeDamage();
                }
				else
                {
					if (!enemyScript.shellsOff)
						GetComponent<Player>().TakeDamage();
					else
						enemyScript.TakeDamage();
                }
				enemyScript.Flip();

				m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0f);
				m_Rigidbody2D.AddForce(Vector2.up * (m_JumpForce -2f), ForceMode2D.Impulse);

				attacked = true;
				StartCoroutine(ResetAttack());

				IEnumerator ResetAttack()
				{
					yield return new WaitForSeconds(0.8f);
					attacked = false;
				}
            }
        }
	}
	

	bool canDoubleJump = true;
	public void Move(float move, bool jump, Animator animator)
	{
		
		//only control the player if grounded or airControl is turned on

		// Move the character by finding the target velocity
		Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
		// And then smoothing it out and applying it to the character
		m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

		if (move == 0)
		{
			animator.SetInteger("AnimState", 0);
		}
		
		// If the input is moving the player right and the player is facing left...
		if (move > 0 && !m_FacingRight)
		{
			// ... flip the player.
			Flip();
			animator.SetInteger("AnimState", 1);
		}
		// Otherwise if the input is moving the player left and the player is facing right...
		else if (move < 0 && m_FacingRight)
		{
			// ... flip the player.
			Flip();
			animator.SetInteger("AnimState", 1);
		}

		// If the player should jump...
		if (m_Grounded && jump)
		{
			// Add a vertical force to the player.
			Player.isFalling = false;
			m_Grounded = false;
			m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0f);

			// Apply upward force
			m_Rigidbody2D.AddForce(Vector2.up * m_JumpForce, ForceMode2D.Impulse); 
			animator.SetTrigger("Jump");
		}
		else if (jump && canDoubleJump)
        {
			Player.isFalling = false;
			m_Grounded = false;
			m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0f);

			// Apply upward force
			m_Rigidbody2D.AddForce(Vector2.up * m_DoubleJumpForce, ForceMode2D.Impulse);
			canDoubleJump = false;
			animator.SetTrigger("DoubleJump");
		}
	}

	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
}