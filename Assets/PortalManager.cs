using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance;
    GameObject[] portals;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        portals = GameObject.FindGameObjectsWithTag("Portal");
    }
    
    public void Teleport(Transform player, int destinationId)
    {
        if (destinationId == -1)
            return;

        foreach (GameObject obj in portals)
        {
            Portal objPortal;

            if (obj.GetComponent<Portal>() != null)
                objPortal = obj.GetComponent<Portal>();
            else
                continue;

            if (objPortal.id == destinationId)
                player.position = new Vector3(obj.transform.position.x + objPortal.xOffset, obj.transform.position.y, player.position.z);
        }
    }
}
