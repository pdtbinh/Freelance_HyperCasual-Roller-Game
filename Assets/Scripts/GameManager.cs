using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/* This class is used to control levels counter in UI. */
public class GameManager : MonoBehaviour
{
    // Text of the level counter
    public TextMeshProUGUI level;

    // Start is called before the first frame update
    void Start()
    {
        // Take current level
        int currentLevel = PlayerPrefs.GetInt("Level", 1);

        // Set level counter
        level.text = "Level " + currentLevel.ToString();
    }

    // This is to restart the current level
    public void ResetLevel()
    {
        SceneManager.LoadScene(0);
    }
}
