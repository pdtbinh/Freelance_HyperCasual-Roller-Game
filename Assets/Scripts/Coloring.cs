using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Coloring class assigned to all keys game object.
 * They are used to assign colors to sprites every time
 * player hits the keys. */
public class Coloring : MonoBehaviour
{
    // This list stores all the colored sprites to replace black-white sprites every time player hits the keys 
    public Sprite[] coloredSprites;

    // This list stores all the sprites holder placed underneath the maze
    public Transform[] photos;

    /* Insert the color with the corresponding color index 
     * General rules: Red is 1, Green is 2, Blue is 3. */
    public void InsertSprite(int index)
    {
        photos[index].GetComponent<SpriteRenderer>().sprite
            = coloredSprites[index];

        // Hide the keys after being hit by player
        gameObject.SetActive(false);
    }
}
