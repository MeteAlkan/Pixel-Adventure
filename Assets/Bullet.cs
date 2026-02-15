using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float velocity;
    public Rigidbody2D rb;
    public GameObject bulletPiece1;
    public GameObject bulletPiece2;

    public void Instantiate(Direction direction)
    {
        if (direction == Direction.Left)
            rb.velocity = new Vector2(-velocity, rb.velocity.y);
        else
        {
            rb.velocity = new Vector2(velocity, rb.velocity.y);
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }

        StartCoroutine(DestroyByTIme(6f, gameObject));
    }

    IEnumerator DestroyByTIme(float time, GameObject obj)
    {
        yield return new WaitForSeconds(time);
        Destroy(obj);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CreateParticles();
            collision.gameObject.GetComponent<Player>().TakeDamage();
            a();
        }
        if (collision.CompareTag("MainGround"))
        {
            CreateParticles();
            a();
        }
    }

    private void a()
    {
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }

    void CreateParticles()
    {
        float offset;
        if (rb.velocity.x > 0)
            offset = 0.26f;
        else
            offset = -0.26f;

        GameObject particle1 = GameObject.Instantiate(bulletPiece1, new Vector3(transform.position.x + offset, transform.position.y - 0.1f, transform.position.z), Quaternion.identity);
        GameObject particle2 = GameObject.Instantiate(bulletPiece2, transform.position, Quaternion.identity);

        if (offset >= 0)
        {
            particle1.transform.localScale = new Vector3(-particle1.transform.localScale.x, particle1.transform.localScale.y, particle1.transform.localScale.z);
            particle2.transform.localScale = new Vector3(-particle2.transform.localScale.x, particle2.transform.localScale.y, particle2.transform.localScale.z);
        }

        StartCoroutine(DestroyByTIme(1f, particle1));
        StartCoroutine(DestroyByTIme(1f, particle2));
        StartCoroutine(DestroyByTIme(1f, gameObject));
    }
}
