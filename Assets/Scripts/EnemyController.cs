using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyController : MonoBehaviour
{
    [HideInInspector]
    private Vector2[] moveDirections = new Vector2[] {
        Vector2.up,
        new Vector2(-0.7f, 0.7f),
        new Vector2(0.7f, 0.7f),
        Vector2.down,
        new Vector2(-0.7f, -0.7f),
        new Vector2(0.7f, -0.7f),
        Vector2.left,
        Vector2.right
    };
    
	private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private RaycastHit2D hit;
	private Vector2 lastMoveInput;
    private Vector2 moveInput;
    private Vector2 knockbackDirection;
    private bool isFacingRight = true;
    private bool isPatrolling = true;
    private bool isChasing = false;
    private bool isHurt = false;
    private bool isSpawning;
    private float health;
    
    [Header("References")]
    public GameObject characterHolder;
    public Animator animator;
    public PlayerStats stats;
    public float detectionRange = 2f;
    public LayerMask targetLayer;
    public Healthbar healthbar;
    public GameObject deathEffect;
    public AudioClip hitSound;
    public AudioClip spawnSound;


    private Vector3 targetPosition;
    private UnityAction<Transform> OnTargetDetected;
    private UnityAction OnTargetOutOfRange;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        OnTargetDetected += TargetDetected;
        OnTargetOutOfRange += TargetOutOfRange;

        health = stats.maxHealth;
        healthbar.SetHealth(health, stats.maxHealth);

        SoundManager.Instance.PlaySound(spawnSound);

        StartCoroutine(RandomDirectionCoroutine());
        StartCoroutine(TargetDetectionCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        isSpawning = animator.GetBool("isSpawning");

        if (isSpawning)
        {
            return;
        }
        #region Move
		if (moveInput.x != 0){
			lastMoveInput.x = moveInput.x;
        }
		if (moveInput.y != 0) {
			lastMoveInput.y = moveInput.y;
        }

        if ((moveInput.x > 0 && !isFacingRight) || (moveInput.x < 0 && isFacingRight)) {
            Turn();
            isFacingRight = !isFacingRight;
		}
		#endregion

        #region Animation
        if (moveInput != Vector2.zero) {
            animator.SetBool("isWalking", true);
			animator.SetFloat("directionX", moveInput.x);
			animator.SetFloat("directionY", moveInput.y);
        } else {
            animator.SetBool("isWalking", false);
        }
        #endregion
    }

    void FixedUpdate()
    {
        if (isSpawning)
        {
            return;
        }
        #region Move
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
        if (!isHurt)
        {
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
        }
		#endregion
    }

    private IEnumerator RandomDirectionCoroutine()
    {
        while(true){
            if (isPatrolling && !isSpawning)
            {
                // Choose a random direction. The state's physics update will take care of moving the enemy
                List<Vector2> validDirections = new List<Vector2>();
                int index = 0;
                foreach (Vector2 direction in moveDirections)
                {
                    hit = Physics2D.BoxCast(transform.position, boxCollider.size, 0, direction, 1);
                    if (hit.collider == null) {
                        validDirections.Add(direction);
                    }
                }
                if (validDirections.Count > 0)
                {
                    index = Random.Range(0, validDirections.Count);
                    moveInput = validDirections[index];
                    // Keep moving for a random amount of time
                    yield return new WaitForSeconds(Random.Range(0.5f, 1f));
                }
                // Stop moving
                moveInput = Vector2.zero;
            }
            yield return new WaitForSeconds(Random.Range(1f, 1.5f));
        }
    }

    private IEnumerator ChaseCoroutine()
    {
        while (isChasing)
        {
            // Move in the direction of the target
            List<Vector2> validDirections = new List<Vector2>();
            Vector3 targetDirection = targetPosition - transform.position;
            float directionX = targetDirection.x;
            float directionY = targetDirection.y;

            moveInput = new Vector2(directionX, directionY).normalized;
            
            yield return null;
        }
    }

    private IEnumerator TargetDetectionCoroutine()
    {
        while (true)
        {
            Collider2D target = Physics2D.OverlapCircle(transform.position, detectionRange, targetLayer);
            if (target != null && OnTargetDetected != null)
            {
                OnTargetDetected.Invoke(target.gameObject.transform);
            }
            else if (OnTargetOutOfRange != null)
            {
                OnTargetOutOfRange.Invoke();
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    private IEnumerator KnockbackCoroutine()
    {
        animator.SetBool("isHurt", true);
        isHurt = true;
        stats.canMove = false;
        // Pause the game for a split second
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1;
        CameraShake.Instance.ShakeCamera(1.5f, 0.1f);
        rb.AddForce(knockbackDirection, ForceMode2D.Impulse);
        StartCoroutine(Squeeze(1.2f, 0.85f, 0.1f));
        yield return new WaitForSeconds(0.3f);
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
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private IEnumerator Squeeze(float xSqueeze, float ySqueeze, float seconds)
    {
        Vector3 originalSize = Vector3.one;
        Vector3 newSize = new Vector3(xSqueeze, ySqueeze, originalSize.z);
        float t = 0f;
        while (t <= 1f)
        {
            t += Time.deltaTime / seconds;
            characterHolder.transform.localScale = Vector3.Lerp(originalSize, newSize, t);
            yield return null;
        }
        t = 0f;
        while (t <= 1f)
        {
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

    private void TargetDetected(Transform target)
    {
        isChasing = true;
        isPatrolling = false;
        targetPosition = target.position;
        StartCoroutine(ChaseCoroutine());
    }

    private void TargetOutOfRange()
    {
        isChasing = false;
        isPatrolling = true;
        targetPosition = Vector2.zero;
    }

    public void TakeDamage(float damageAmount, Vector2 knockbackDirection)
    {
        if ((health - damageAmount) > 0)
        {
            health -= damageAmount;
            healthbar.SetHealth(health, stats.maxHealth);
            this.knockbackDirection = knockbackDirection;
            SoundManager.Instance.PlaySound(hitSound);
            StartCoroutine(KnockbackCoroutine());
        }
        else
        {
            healthbar.SetHealth(0, stats.maxHealth);
            StartCoroutine(DeathCoroutine());
        }
        
    }

    private void OnDrawGizmos()
    {
        if (isChasing)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
        }
        else
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f);
        }

        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
