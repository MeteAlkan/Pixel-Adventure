using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [HideInInspector] public bool shellsOff = false;
    public Transform wallSensor;
    public Transform groundSensor;
    public Transform attackPoint;
    public GameObject bulletPrefab; 

    public float groundSensorDistance;

    public LayerMask wallLayer;
    public LayerMask groundLayer;
    public LayerMask playerLayer;
    public LayerMask enemyLayer;
    public EnemyType type;

    [SerializeField] float attackRange = 0.5f;
    [SerializeField] float rangedAttackRange = 0f;
    [SerializeField] float idleRange = 10f;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float timeToCreateBullet = 0.7f;
    [SerializeField] float attackRadius = 0.5f;
    [SerializeField] bool hasMeleeAttack = true;
    [SerializeField] bool canTurn = true;
    [SerializeField] int health = 4;
    [SerializeField] Direction direction;

    private Animator animator;
    private Rigidbody2D rb;
    private GameObject player;
    private Transform playerTransform;
    private float spriteWidth;
    private bool hasAttacked = false;
    private bool dead = false;
    private bool canShelledEnemyMove = true;
    private float shellTime;
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerTransform = player.GetComponent<Transform>();
        spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        shellTime += Time.deltaTime;
        if (dead)
            return;

        float distance = transform.position.x - playerTransform.position.x;
        float yDistance = transform.position.y - playerTransform.position.y;

        if (type == EnemyType.Shelled)
            HandleShelled(distance);
        else if (hasMeleeAttack && Mathf.Abs(distance) <= attackRange && CanAttack(distance) && Mathf.Abs(yDistance) < 1.1f)
        {
            if (!hasAttacked)
                Attack();
        }
        else if (type == EnemyType.Ranged && Mathf.Abs(distance) <= rangedAttackRange && CanAttack(distance) && Mathf.Abs(yDistance) < 1.1f)
        {
            if (!hasAttacked)
                RangedAttack();
        }
        else if (Mathf.Abs(distance) <= idleRange)
        {
            animator.SetInteger("AnimState", 1);
            float dir = direction == Direction.Left ? -1f : 1f;
            rb.velocity = new Vector2(dir * moveSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            animator.SetInteger("AnimState", 0);
        }   
    }

    private void FixedUpdate()
    {
        if (dead)
            return;
        // Check wall ahead
        RaycastHit2D wallHit = Physics2D.BoxCast(
            wallSensor.position,
            new Vector2(0.2f, 1.2f),  // width x height of cast box
            0f,
            transform.right,
            0.5f,
            wallLayer
        );
        // Check enemy ahead
        RaycastHit2D enemyHit = Physics2D.Raycast(wallSensor.position, transform.right, 0.5f, enemyLayer);

        // Check ground below
        RaycastHit2D groundHit = Physics2D.Raycast(groundSensor.position, Vector2.down, groundSensorDistance, groundLayer);

        // Debug
        Debug.DrawRay(wallSensor.position, transform.right * 0.5f, Color.red);
        Debug.DrawRay(wallSensor.position, transform.right * 0.5f, Color.yellow);
        Debug.DrawRay(groundSensor.position, Vector2.down * groundSensorDistance, Color.blue);

        if (wallHit.collider != null || groundHit.collider == null || enemyHit.collider != null)
        {
            Flip();
        }
    }

    void HandleShelled(float distance)
    {
        if (Mathf.Abs(distance) <= idleRange && canShelledEnemyMove)
        {
            float dir = direction == Direction.Left ? -1f : 1f;
            rb.velocity = new Vector2(dir * moveSpeed, rb.velocity.y);
            if (shellTime >= Random.Range(3f, 6f))
            { 
                MoveShell();
                shellTime = 0;
            }
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void MoveShell()
    {
        canShelledEnemyMove = false;
        if (shellsOff) 
        {
            shellsOff = false;
            rb.velocity = new Vector2(0, rb.velocity.y);
            animator.SetTrigger("SpikesOut");
            StartCoroutine(ShellInteger(0));
        }
        else
        {
            shellsOff = true;
            float dir = direction == Direction.Left ? -1f : 1f;
            rb.velocity = new Vector2(dir * moveSpeed, rb.velocity.y);
            animator.SetTrigger("SpikesIn");
            StartCoroutine(ShellInteger(1));
        }    
    }

    IEnumerator ShellInteger(int shellNum)
    {
        yield return new WaitForSeconds(0.37f);
        animator.SetInteger("AnimState", shellNum);
        canShelledEnemyMove = true;
    }

    bool CanAttack(float distance)
    {
        if (direction == Direction.Left && distance > 0)
            return true;
        else if (direction == Direction.Right && distance < 0)
            return true;
        else
            return false;
    }

    public void TakeDamage()
    {
        health--;
        animator.SetTrigger("Hit");

        if (health == 0)
        {
            Die();
        }
    }

    void Die()
    {
        dead = true;
        animator.SetTrigger("Death");
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        GetComponent<BoxCollider2D>().enabled = false;

        StartCoroutine(DestroyObject());

        IEnumerator DestroyObject()
        {
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
        hasAttacked = true;

        StartCoroutine(GiveDamage());
        StartCoroutine(AfterAttack());
    }

    void RangedAttack()
    {
        Debug.Log("RangedAttack");
        animator.SetTrigger("RangedAttack");
        hasAttacked = true;

        StartCoroutine(CreateBullet());
        StartCoroutine(AfterAttack());
    }
    IEnumerator CreateBullet()
    {
        yield return new WaitForSeconds(timeToCreateBullet);
        GameObject bullet = Instantiate(bulletPrefab, attackPoint.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().Instantiate(direction);
    }

    IEnumerator GiveDamage()
    {
        yield return new WaitForSeconds(0.5f);
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackRadius, playerLayer);
        if (hit != null)
        {
            Player player = hit.GetComponent<Player>();
            if (player != null)
                player.TakeDamage();
        }
    }
    IEnumerator AfterAttack()
    {
        yield return new WaitForSeconds(1f);

        Flip();
        hasAttacked = false;
    }
    public void Flip()
    {
        if (!canTurn)
            return;

        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;

        float offset = spriteWidth * 0.4f; // Adjust this multiplier as needed
        if (direction == Direction.Left)
        {
            direction = Direction.Right;
            transform.position += new Vector3(offset, 0, 0);
        }
        else
        {
            direction = Direction.Left;
            transform.position += new Vector3(-offset, 0, 0);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Destroyer"))
        {
            Die();
        }
    }

}
    public enum EnemyType
    {
        Melee,
        Shelled,
        Ranged
    }
    public enum Direction 
    { 
        Left, 
        Right 
    }
