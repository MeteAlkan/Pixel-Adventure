using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public CharacterController2D controller;

    public static bool isFalling = false;
    public float moveSpeed;
    public int playerHealth = 10;
    [SerializeField] float fallThreshold = -0.5f;
    [SerializeField] GameObject canvas;

    float horizontalMove;

    bool jump = false;
    bool canTakeDamage = true;
    Rigidbody2D rb;
    Animator animator;
    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        rb = playerObj.GetComponent<Rigidbody2D>();
        animator = playerObj.GetComponent<Animator>();
    }
    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * moveSpeed;

        if (Input.GetButtonDown("Jump"))
            jump = true;

        bool shouldFall = rb.velocity.y < fallThreshold && !controller.m_Grounded;

        if (controller.m_Grounded)
            shouldFall = false;

        if (isFalling != shouldFall) // Only update if state changed
        {
            isFalling = shouldFall;
            animator.SetBool("IsFalling", isFalling);
        }
    }

    public void TakeDamage()
    {
        playerHealth--;
        canvas.GetComponent<HeartImages>().DrawHearts();
        animator.SetTrigger("Hurt");

        if (playerHealth == 0)
            Die();
    }

    public void IncreaseHealth()
    {
        playerHealth++;
        canvas.GetComponent<HeartImages>().DrawHearts();
    }

    void Die()
    {

    }

    private void FixedUpdate()
    {
        controller.Move(horizontalMove * Time.fixedDeltaTime, jump, animator);
        jump = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Trap"))
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * 3.8f, ForceMode2D.Impulse);
            if (canTakeDamage)
            {
                canTakeDamage = false;
                TakeDamage();
                StartCoroutine(ResetTakeDamage());
            }
        }

        if (collision.CompareTag("Destroyer"))
        {
            Die();
        }        
    }
    
    IEnumerator ResetTakeDamage()
    {
        yield return new WaitForSeconds(1f);
        canTakeDamage = true;
    }
}