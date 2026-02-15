using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public int id;
    public int destinationId;
    public float xOffset;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Input.GetKey(KeyCode.E))
        {
            StartCoroutine(TP(collision));
        }
    }

    IEnumerator TP(Collider2D collision)
    {
        yield return new WaitForSeconds(0.5f);
        PortalManager.Instance.Teleport(collision.gameObject.transform, destinationId);
    }
}
