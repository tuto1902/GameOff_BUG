using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    public InputManager inputManager;

    [Header("References")]
	private Rigidbody2D rb;
    public GameObject characterHolder;
    public ParticleSystem dust;
    private Animator animator;
    public PlayerStats stats;
	public PlayerHealthbar healthbar;
	public AudioClip attackSound;
	public AudioClip hurtSound;
    private Vector2 moveInput;
	private Vector2 lastMoveInput;
    private bool isFacingRight = true;
	private bool isAttacking = false;
	private float attackCooldown = 0f;
	private float hitCooldown = 2f;
	private float hitCooldownTimer;
	private bool isHurt;
	private bool isInvincible = false;
	private Vector2 knockbackDirection;
	private float health;


	void Awake()
    {
        Application.targetFrameRate = 60;
    }

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponentInChildren<Animator>();
		health = stats.maxHealth;
		healthbar.SetMaxHealth(stats.maxHealth);

		#region Inputs
		inputManager.moveEvent += OnMove;
		inputManager.attackEvent += OnAttack;
        #endregion
		dust.Play();
	}

    private void OnDestroy()
    {
		inputManager.moveEvent -= OnMove;
		inputManager.attackEvent -= OnAttack;
    }

    private void Update()
	{
		#region Run
		if (moveInput.x != 0){
			lastMoveInput.x = moveInput.x;
        }
		if (moveInput.y != 0) {
			lastMoveInput.y = moveInput.y;
        }

        if ((moveInput.x > 0 && !isFacingRight) || (moveInput.x < 0 && isFacingRight)) {
			if (isAttacking == false) {
				Turn();
				isFacingRight = !isFacingRight;
			}
		}
		#endregion

		if (isHurt)
		{
			moveInput = Vector2.zero;
		}

		#region Animation
		if (moveInput != Vector2.zero) {
            animator.SetBool("isWalking", true);
			animator.SetFloat("directionX", moveInput.x);
			animator.SetFloat("directionY", moveInput.y);
			if (!dust.isPlaying && !isAttacking) {
				dust.Play();
			}
        } else {
            animator.SetBool("isWalking", false);
			if (dust.isPlaying) {
				dust.Stop();
			}
        }
        #endregion
		if (attackCooldown > 0) {
			attackCooldown -= Time.deltaTime;
		} else {
			isAttacking = false;
		}

		if (hitCooldownTimer > 0)
        {
			hitCooldownTimer -= Time.deltaTime;
        }
	}

	private void FixedUpdate()
	{
		if (isHurt)
        {
			return;
        }
		#region Run
		if (isAttacking) {
			rb.velocity = Vector2.zero;
			return;
		}
		if (stats.canMove) {
			// Calculate the direction we want to move in and our desired velocity
			Vector2 targetSpeed = new Vector2(moveInput.x * stats.moveSpeed, moveInput.y * stats.moveSpeed);
			// Calculate difference between current velocity and desired velocity
			float speedDif_x = targetSpeed.x - rb.velocity.x;
			float speedDif_y = targetSpeed.y - rb.velocity.y;

			// Applies acceleration to speed difference, then raises to a set power so acceleration increases with higher speeds
			// finally multiplies by sign to reapply direction
			float movement_x = Mathf.Pow(Mathf.Abs(speedDif_x) * stats.acceleration, stats.velPower) * Mathf.Sign(speedDif_x);
			float movement_y = Mathf.Pow(Mathf.Abs(speedDif_y) * stats.acceleration, stats.velPower) * Mathf.Sign(speedDif_y);

			Vector2 movement = new Vector2(movement_x, movement_y);

			// Applies force force to rigidbody, multiplying by Vector2.right so that it only affects X axis 
			rb.AddForce(movement);
		}
		#endregion

		#region Friction
		// Check if we're grounded and we are trying to stop (not pressing forwards or backwards)
		if (Mathf.Abs(moveInput.x) < 0.01f && Mathf.Abs(moveInput.y) < 0.01f) {
			// Use either the friction amount (~ 0.2) or our velocity
			float amount_x = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(stats.frictionAmount));
			float amount_y = Mathf.Min(Mathf.Abs(rb.velocity.y), Mathf.Abs(stats.frictionAmount));
			// Sets to movement direction
			amount_x *= Mathf.Sign(rb.velocity.x);
			amount_y *= Mathf.Sign(rb.velocity.y);
			Vector2 amount = new Vector2(amount_x, amount_y);
			// Applies force against movement direction
			rb.AddForce(Vector2.one * -amount, ForceMode2D.Impulse);
		}
		#endregion
	}

    #region Move
    public void OnMove(Vector2 input)
    {
		if (stats.canMove) {
        	moveInput.x = input.x;
			moveInput.y = input.y;
		}
    }
    #endregion

	#region Attack
	public void OnAttack()
	{
		if (attackCooldown > 0 || isHurt) {
			return;
		}
		isAttacking = true;
		animator.SetBool("isAttacking", true);
		SoundManager.Instance.PlaySound(attackSound);
		if (dust.isPlaying) {
			dust.Stop();
		}
		attackCooldown = 0.4f;
	}

	
	#endregion

	private IEnumerator StopMovement(float duration)
	{
		stats.canMove = false;
		yield return new WaitForSeconds(duration);
		stats.canMove = true;
	}

    private IEnumerator Squeeze(float xSqueeze, float ySqueeze, float seconds)
    {
        Vector3 originalSize = Vector3.one;
        Vector3 newSize = new Vector3(xSqueeze, ySqueeze, originalSize.z);
        float t = 0f;
        while (t <= 1f) {
            t += Time.deltaTime / seconds;
            characterHolder.transform.localScale = Vector3.Lerp(originalSize, newSize, t);
            yield return null;
        }
        t = 0f;
        while (t <= 1f) {
            t += Time.deltaTime / seconds;
            characterHolder.transform.localScale = Vector3.Lerp(newSize, originalSize, t);
            yield return null;
        }
    }

    private void Turn()
	{
		//stores scale and flips x axis, flipping the entire gameObject (could also rotate the player)
		Vector3 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;
	}

	private IEnumerator KnockbackCoroutine()
	{
		animator.SetBool("isHurt", true);
		isHurt = true;
		stats.canMove = false;
		isInvincible = true;
		// Pause the game for a split second
		Time.timeScale = 0;
		yield return new WaitForSecondsRealtime(0.1f);
		Time.timeScale = 1;
		CameraShake.Instance.ShakeCamera(2.5f, 0.1f);
		rb.AddForce(knockbackDirection, ForceMode2D.Impulse);
		StartCoroutine(Squeeze(1.2f, 0.85f, 0.1f));
		yield return new WaitForSeconds(0.3f);
		StartCoroutine(HitCooldownCoroutine());
		knockbackDirection = Vector2.zero;
		stats.canMove = true;
		isHurt = false;
		
	}

	private IEnumerator DeathCoroutine()
	{
		// Pause the game for a split second
		Time.timeScale = 0;
		yield return new WaitForSecondsRealtime(0.1f);
		Time.timeScale = 1;
		CameraShake.Instance.ShakeCamera(3f, 0.1f);
		SoundManager.Instance.PlaySound(hurtSound);
		//Instantiate(deathEffect, transform.position, Quaternion.identity);
		GameManager.Instance.GameOver();
		Destroy(gameObject);
	}

	private IEnumerator HitCooldownCoroutine()
    {
		
		SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();
		hitCooldownTimer = hitCooldown;
		Color transparent = new Color(1, 1, 1, 0.5f);
		while (hitCooldownTimer > 0)
        {
			sprite.color = transparent;
			yield return new WaitForSeconds(0.09f);
			sprite.color = Color.white;
			yield return new WaitForSeconds(0.09f);
		}
		sprite.color = Color.white;
		isInvincible = false;
		// Toggle the box collider off and on to detect new collisions
		BoxCollider2D collider = GetComponent<BoxCollider2D>();
		collider.enabled = false;
		collider.enabled = true;
	}

	public void TakeDamage(float damageAmount, Vector2 knockbackDirection)
	{
		if (isInvincible)
        {
			return;
        }
		if ((health - damageAmount) > 0)
		{
			health -= damageAmount;
			healthbar.SetHealth(health);
			this.knockbackDirection = knockbackDirection;
			SoundManager.Instance.PlaySound(hurtSound);
			StartCoroutine(KnockbackCoroutine());
		}
		else
		{
			healthbar.SetHealth(0);
			StartCoroutine(DeathCoroutine());
		}

	}

	// private void OnDrawGizmos()
	// {
	// 	Gizmos.color = Color.green;
	// 	Gizmos.DrawWireCube(groundCheckPoint.position, stats.groundCheckSize);
	// 	Gizmos.color = Color.blue;
	// 	Gizmos.DrawWireCube(frontWallCheckPoint.position, stats.wallCheckSize);
	// 	Gizmos.DrawWireCube(backWallCheckPoint.position, stats.wallCheckSize);
	// }
}
