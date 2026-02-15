using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruit : MonoBehaviour
{
    Animator animator;
    bool animationTime = false;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && !animationTime)
        {
            animator.SetTrigger("Collected");
            animationTime = true;
            StartCoroutine(DestroyObj(collision));
        }
    }

    IEnumerator DestroyObj(Collider2D collision)
    {
        yield return new WaitForSeconds(0.34f);
        collision.gameObject.GetComponent<Player>().IncreaseHealth();
        Destroy(gameObject);
    }
}
