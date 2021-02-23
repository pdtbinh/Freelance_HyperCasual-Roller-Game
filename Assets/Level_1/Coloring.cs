using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coloring : MonoBehaviour
{
    public Sprite[] coloredSprites;
    public Transform[] photos;

    public void InsertSprite(int index)
    {
        photos[index].GetComponent<SpriteRenderer>().sprite
            = coloredSprites[index];

        gameObject.SetActive(false);
    }
}
