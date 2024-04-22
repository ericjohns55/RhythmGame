using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the behavior of the buttons in the gamebar. Buttons can be activated with their
/// given activation key to enable collision on note objects. Points are awarded to the player
/// based on how close to the center of the collision box a note is when the button is activated.
/// </summary>

public class ButtonControl : MonoBehaviour
{
    [SerializeField] private KeyCode activationKey;
    [SerializeField] private float yOffset;
    [SerializeField] private float buttonLifetime = 0.1f;
    [SerializeField] private Color pressedColor;
    [SerializeField] private Color defaultColor;

    private bool isPressed = false;
    private bool collisionActive = false;
    private float pressStartTime;

    private Vector3 notePosition;
    private SpriteCreator spriteCreator;
    private float xPosition;

    private ScoreManager scoreManager;
    private GameManager gameManager;

    private int miss = 0;
    private int awful = 0;
    private int good = 0;
    private int excellent = 0;

    private List<KeyCode> activationKeys = new List<KeyCode>() {
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
        KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon
    };

    // Start is called before the first frame update
    void Start()
    {
        miss = 0;
        awful = 0;
        good = 0;
        excellent = 0;
        InitializeComponents();
    }

    // Update is called once per frame
    void Update()
    {
        CheckButtonPress();
    }

    // FixedUpdate is called a fixed number of times per second
    void FixedUpdate()
    {
        FindNotePosition();
    }

    private void InitializeComponents()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        defaultColor = GetComponent<SpriteRenderer>().color;
        spriteCreator = FindObjectOfType<SpriteCreator>();
        gameManager = (GameManager) FindObjectOfType<GameManager>().GetComponent("GameManager");
        transform.position = notePosition;
    }

    private void CheckButtonPress()
    {
        if (Input.GetKeyDown(activationKey))
        {
            isPressed = true;
            pressStartTime = Time.time;
            GetComponent<SpriteRenderer>().color = pressedColor;
            collisionActive = true;
        }

        if (Input.GetKeyUp(activationKey) || (isPressed && Time.time - pressStartTime >= buttonLifetime))
        {
            isPressed = false;
            GetComponent<SpriteRenderer>().color = defaultColor;
            collisionActive = false;
        }
    }

    private float CalculateDistance(Vector3 targetPosition)
    {
        return Vector2.Distance(transform.position, targetPosition);
    }

    // Finds the position of the corresponding note based on the activation key
    private void FindNotePosition()
    {
        spriteCreator.setScreenUnits();
        float spacerSize = spriteCreator.GetSpacerSize();
        int index = activationKeys.IndexOf(activationKey);
        if (index != -1)
        {
            xPosition = (spacerSize * (index + 1) + index + 0.5f);
            xPosition -= spriteCreator.GetUnitWidth();
        }

        float bottomOfScreenWorldY = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        float yOffset = 2.0f;

        Vector3 newPosition = transform.position;
        newPosition.x = xPosition;
        
        newPosition.y = bottomOfScreenWorldY + yOffset;
        transform.position = newPosition;
    }

    /**
    * Detects collision between the buttons/keys a player is activating and the falling notes.
    * The distance between them determines the points awarded when a note is destroyed.
    */
    void OnTriggerStay2D(Collider2D other)
    {
        if (collisionActive && other.gameObject.CompareTag("Note"))
        {
            float distance = CalculateDistance(other.transform.position);

            switch (distance)
            {
                case float d when d > 1.5f: // Miss
                    miss++;
                    scoreManager.ResetCombo();
                    break;
                case float d when d > 1.0f: // Awful
                    awful++;
                    scoreManager.IncrementComboAndScore(10);
                    break;
                case float d when d > 0.5f: // Good
                    good++;
                    Debug.Log("Good");
                    scoreManager.IncrementComboAndScore(20);
                    break;
                default: // Excellent
                    excellent++;
                    Debug.Log("Excellent");
                    scoreManager.IncrementComboAndScore(30);
                    break;
            }

            gameManager.NoteDestroyed();
            Destroy(other.gameObject);
        }
    }
}
