using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the behavior of the buttons in the gamebar. Buttons can be activated with their
/// given activation key to enable collision on note objects.
/// </summary>
public class ButtonControl : MonoBehaviour
{
    [SerializeField] private KeyCode activationKey;         // Key press that activates button
    [SerializeField] private float yOffset;                 // yOffset to the bottom of the screen for the buttons
    [SerializeField] private float buttonLifetime = 0.1f;   // Time between button presses
    public Color pressedColor;                             
    public bool collisionActive = false;                   
    private Color defaultColor;                             
    private bool isPressed = false;                         
    private float timer;                                    // Timer to track when the button is initially pressed
    private Vector3 notePosition;                           // Position of the corresponding note
    private SpriteCreator spriteCreator;
    private float xPosition;
    private ScoreManager scoreManager;

    List<KeyCode> keys = new List<KeyCode>() {
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
        KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon
    };

    // Start is called before the first frame update
    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        defaultColor = GetComponent<SpriteRenderer>().color;
        spriteCreator = FindObjectOfType<SpriteCreator>();
        transform.position = notePosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(activationKey))
        {
            isPressed = true;
            timer = Time.time;
            GetComponent<SpriteRenderer>().color = pressedColor;

            collisionActive = true;
        }

        // Check if the activation key is released or the button lifetime expires
        if (Input.GetKeyUp(activationKey) || (isPressed && Time.time - timer >= buttonLifetime))
        {
            isPressed = false;
            GetComponent<SpriteRenderer>().color = defaultColor;
            collisionActive = false;
        }
    }

    // FixedUpdate is called a fixed number of times per second
    void FixedUpdate()
    {
        FindNotePosition();
    }

    // Finds the position of the corresponding note based on the activation key
    private void FindNotePosition()
    {
        spriteCreator.setScreenUnits();
        float spacerSize = spriteCreator.GetSpacerSize();
        int index = keys.IndexOf(activationKey);
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

    void OnTriggerStay2D(Collider2D other)
    {
        if (collisionActive && other.gameObject.CompareTag("Note"))
        {
            float distance = Vector2.Distance(transform.position, other.transform.position);

            switch (distance)
            {
                case float d when d > 1.5f:
                    break;
                case float d when d > 1.0f:
                    // Awful
                    scoreManager.AddPoints(10);
                    break;
                case float d when d > 0.5f:
                    // Good
                    scoreManager.AddPoints(20);
                    break;
                default:
                    // Excellent
                    scoreManager.AddPoints(30);
                    break;
            }

            Destroy(other.gameObject);
        }
    }
}
