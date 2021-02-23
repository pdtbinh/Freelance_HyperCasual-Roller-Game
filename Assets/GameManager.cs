using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI level;

    // Start is called before the first frame update
    void Start()
    {
        // Take current level
        int currentLevel = PlayerPrefs.GetInt("Level", 1);

        // Set level counter
        level.text = "Level " + currentLevel.ToString();
    }

    public void ResetLevel()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(0);
    }
}
