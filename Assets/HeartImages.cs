using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartImages : MonoBehaviour
{
    public Player player;
    public Image heartImage;
    private Image[] heartImages;
    void Start()
    {
        heartImages = new Image[100];
        DrawHearts();
    }

    public void DrawHearts()
    {
        foreach (Image img in heartImages)
        {
            if (img != null)
                Destroy(img.gameObject);
        }   
        for (int i = 0; i < player.playerHealth; i++)
        {
            Image instantiatedObj = Instantiate(heartImage, transform);
            instantiatedObj.transform.position = new Vector3(80f + 57.5f * i, 670f);
            heartImages[i] = instantiatedObj;
        }
    }
}
