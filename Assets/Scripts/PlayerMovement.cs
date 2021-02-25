using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/* This class is used to control player's movement and interaction with the game. 
 * 
 * Detailed description on the game logic:
 * - Player can be assigned 3-4 colors (currently 3, to add another color, just adjust the list below)
 * - Each different color has different ID: 1, 2, 3 and this match with the keys' color ID
 * - On every update frame, player constantly checks if there is wall or color flashing or keys ahead.
 * If there is, the class will invoke methods accordingly. 
 * 
 * Since if-else method is taking quite long to implement moving and checking logic of the game
 * I implement a lot of programming with lists to automated the process. */
public class PlayerMovement : MonoBehaviour
{
    /* ID for moving direction:
     * 0 means not moving; 
     * 1 means moving upward; 
     * 2 means moving rightward; 
     * 3 means moving downward;
     * 4 means moving leftward */
    private int moving;

    private bool allowedInput;

    // Speed should always be larger than zero
    [SerializeField]
    [Range(0, 20)]
    float inputSpeed = 20;

    // Used to identify whether player have collided with maze wall or color flashing
    public LayerMask platformMask;

    // List of moving directions and rotations
    private readonly Vector2[] axis = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
    private readonly Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
    private readonly float[] yrotates = { 0f, 90f, 180f, 270f };

    // List of material for flashing
    private int currentColorID;
    private string currentColor;
    
    private List<GameObject> colorFlashings = new List<GameObject>();
    private readonly string[] colorFlashingTags = { "RedCorner", "GreenCorner", "BlueCorner" };
    public Material[] flashingMaterials;

    private List<GameObject> colorPieces = new List<GameObject>();
    private readonly string[] colorPieceTags = { "RedPiece", "GreenPiece", "BluePiece" };

    // Player Editor Components:
    private Animator animator;
    public ParticleSystem trail;
    public Material[] trailColors;

    // Game manager components: If all 3 color pieces are satisfied, finish the game
    private bool[] colorSatisfactions = { false, false, false }; // Red, Green, Blue
    private GameObject warning;
    public GameObject[] platforms;

    // For Android swipe
    private Vector2 startPos;
    private Vector2 endPos;

    // SFX
    public GameObject[] pieceAcquiredBuffs;
    public GameObject[] flashingBuffs;
    public GameObject[] confettiEffects;
    public GameObject[] splatEffects;

    // Start is called before the first frame update
    void Start()
    {
        int currentLevelIndex = PlayerPrefs.GetInt("Level", 1) - 1;
        // Spawn platform accordingly
        GameObject.Instantiate(platforms[currentLevelIndex % platforms.Length], new Vector3(5f, 0f, 5f), Quaternion.identity);

        // Initially not moving
        moving = 0;
        allowedInput = true;
        currentColor = "";

        // Player Editor Components:
        animator = GetComponent<Animator>();

        // Environment components:
        for (int i = 0; i < colorFlashingTags.Length; i++)
        {
            GameObject flashing = GameObject.FindGameObjectWithTag(colorFlashingTags[i]);
            if (flashing == null) Debug.Log("Deo tim dc flashing");
            colorFlashings.Add(flashing) ;

            if (flashing == null) Debug.Log("Deo tim dc piece");
            GameObject piece = GameObject.FindGameObjectWithTag(colorPieceTags[i]);
            colorPieces.Add(piece);
        }
        warning = GameObject.FindGameObjectWithTag("Warning");
    }

    // Update is called once per frame
    void Update()
    {
        // Only recieve input if avatar is not moving
        if (moving == 0 && allowedInput)
        {
            // PC region--------------------------------------------------------
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // Only move if at least one of the two larger than zero
            if (Mathf.Abs(horizontal) + Mathf.Abs(vertical) > Mathf.Epsilon)
            {
                for (int i = 0; i < directions.Length; i++)
                {
                    // Choose to move on the closet direction
                    if (Vector2.Angle(new Vector2(horizontal, vertical), axis[i]) <= 45
                        && !Physics.Raycast(transform.position, directions[i], 0.8f, platformMask))
                    {
                        allowedInput = false;
                        moving = i + 1;
                        transform.rotation = Quaternion.Euler(0f, yrotates[moving - 1], 0f);
                        trail.Play();
                        StartCoroutine("Splat");
                    }
                }
            }

            // Android region---------------------------------------------------
            if (allowedInput && Input.touchCount > 0)
            {
                Touch touch = Input.touches[0];
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        startPos = touch.position;
                        break;
                    case TouchPhase.Ended:
                        endPos = touch.position;
                        Vector2 swipeDirection = endPos - startPos;

                        for (int i = 0; i < directions.Length; i++)
                        {
                            // Choose to move on the closet direction
                            if (Vector2.Angle(swipeDirection, axis[i]) <= 45
                                && !Physics.Raycast(transform.position, directions[i], 0.8f, platformMask)
                                && Vector2.Distance(startPos, endPos) > 5f)
                            {
                                allowedInput = false;
                                moving = i + 1;
                                transform.rotation = Quaternion.Euler(0f, yrotates[moving - 1], 0f);
                                trail.Play();
                                StartCoroutine("Splat");
                            }
                        }

                        break;
                }
            }
        }

        else if (moving > 0)
        {
            // Keep moving until detect maze wall ahead
            Move(moving, inputSpeed);
            CheckForPlatforms();
            CheckForColorFlashings();
            CheckForColorPieces();
            CheckForWarningSign();
            Splat();
        }
    }

    // Detect wall maze ahead
    private void CheckForPlatforms()
    {
        if (Physics.Raycast(transform.position, directions[moving - 1], 0.8f, platformMask))
        {
            // Stop moving and squat
            moving = 0;
            animator.SetTrigger("squat");
            trail.Stop();

            // Tune player's current position to pretty numbers
            Vector3 pos = transform.position;
            transform.position = new Vector3(TunePosition(pos.x), 0.5f, TunePosition(pos.z));

            // Allow continue moving after squatting animation completed
            Invoke("StopMoving", 0.12f);
        }
    }

    // Check for colors flashing
    private void CheckForColorFlashings()
    {
        for (int i = 0; i < colorFlashings.Count; i++)
        {
            // Find the satisfying closest color flashing
            if (colorFlashings[i] != null
                && Vector3.Distance(colorFlashings[i].transform.position, transform.position) < 0.3f)
            {
                Handheld.Vibrate();

                currentColorID = i + 1;

                flashingBuffs[i].SetActive(true);

                // Change current color to the flashing's color
                transform.GetChild(0).GetComponent<MeshRenderer>().material
                  = flashingMaterials[i];

                trail.GetComponent<Renderer>().material
                  = trailColors[i];

                // Change color ID to check if it hits the right trigger point
                currentColor = colorPieceTags[i];
            }

            else
            {
                flashingBuffs[i].SetActive(false);
            }
        }
    }

    // Check for color pieces, which should be place at maze corners (not 4 outsidest corners) to avoid errors
    private void CheckForColorPieces()
    {
        for (int i = 0; i < colorPieces.Count; i++)
        {
            // Find the satisfying closest color piece
            if (colorPieces[i] != null
                && Vector3.Distance(colorPieces[i].transform.position, transform.position) < 0.6f
                && !colorSatisfactions[i])
            {
                if (currentColor == colorPieces[i].tag)
                {
                    Handheld.Vibrate();

                    pieceAcquiredBuffs[i].SetActive(true);

                    colorPieces[i].GetComponent<Coloring>().InsertSprite(i);
                    colorSatisfactions[i] = true;

                    if (colorSatisfactions[0] && colorSatisfactions[1] && colorSatisfactions[2])
                    {
                        FinishGame();
                    }
                }
                return;
            }
        }
    }

    // Check for warning sign
    private void CheckForWarningSign()
    {
        if (warning != null
            && Vector3.Distance(warning.transform.position, transform.position) < 0.6f)
        {
            Handheld.Vibrate();

            warning.SetActive(false);
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            transform.GetChild(2).gameObject.SetActive(true);

            RestartGame();
        }

    }

    // Move in the "direction" with "speed"
    private void Move(int direction, float speed)
    {
        transform.position = transform.position + (speed * Time.deltaTime * directions[direction - 1]);
    }

    // Stop moving animation and allow input (movement was stopped already when maze wall was detected)
    private void StopMoving()
    {
        allowedInput = true;
        StopAllCoroutines();
    }

    // Tune the player's position to pretty numbers
    private float TunePosition(float currentCoordinate)
    {
        for (int i = 0; i < 15; i++)
        {
            if (Mathf.Abs(currentCoordinate - (float)(i + 0.5)) < 0.5)
            {
                return (float)(i + 0.5);
            }
        }
        // This should never happens as the loop has to guarantee a solution
        return currentCoordinate;
    }

    // Restart game is called whenever a level is completed or restart
    private void RestartGame()
    {
        // If level is completed, play animation on the maze
        if (colorSatisfactions[0] && colorSatisfactions[1] && colorSatisfactions[2])
        {
            GameObject maze = GameObject.FindGameObjectWithTag("Maze");
            maze.GetComponent<Animator>().SetTrigger("disappear");
            warning.SetActive(false);
            Invoke("ReloadScene", 4.5f);
        }

        // Else, simply reload the scene
        else
        {
            Invoke("ReloadScene", 1.5f);
        }
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(0);
    }

    // This is called whenever player completes a level
    private void FinishGame()
    {
        // Player the confetti effects
        for (int i = 0; i < confettiEffects.Length; i++)
        {
            confettiEffects[i].SetActive(true);
        }

        // Increase level by 1
        int currentLevel = PlayerPrefs.GetInt("Level", 1);
        PlayerPrefs.SetInt("Level", currentLevel + 1);

        // Play animation to show player the colored picture
        RestartGame();
    }

    // This is used to spawn the spat effects on the player trails
    private IEnumerator Splat()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.018f);

            GameObject splat = Instantiate(splatEffects[currentColorID],
                transform.position,
                Quaternion.Euler(-90f, 0f, 0f));
        }
    }
}
