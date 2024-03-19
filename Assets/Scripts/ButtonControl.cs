using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonControl : MonoBehaviour
{
    [SerializeField] private KeyCode activationKey;         // Key press that activates button
    public Color pressedColor;                              // Button color on press
    public bool collisionActive = false;                    // Flag for collision state

    private Color defaultColor;                             // Default button color
    private bool isPressed = false;                         // Flag to track whether the button is being pressed
    private float timer;                                    // Timer to track when the button is initially pressed
    [SerializeField] private float buttonLifetime = 0.1f;   // Time between button presses

    private Vector3 notePosition;                           // Position of the corresponding note

    private SpriteCreator spriteCreator;

    private float xPosition;

    List<KeyCode> keys = new List<KeyCode>() {
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
        KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon
    };

    // Start is called before the first frame update
    void Start()
    {
        defaultColor = GetComponent<SpriteRenderer>().color;

        spriteCreator = FindObjectOfType<SpriteCreator>();

        // Set the button's position based on the note's position
        transform.position = notePosition;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the activation key is pressed
        if (Input.GetKeyDown(activationKey))
        {
            isPressed = true;
            timer = Time.time;

            // Change button color to pressedColor
            GetComponent<SpriteRenderer>().color = pressedColor;

            // Change collision state
            collisionActive = true;
        }

        // Check if the activation key is released or the button lifetime expires
        if (Input.GetKeyUp(activationKey) || (isPressed && Time.time - timer >= buttonLifetime))
        {
            isPressed = false;

            // Change button color to defaultColor
            GetComponent<SpriteRenderer>().color = defaultColor;

            // Change collision state
            collisionActive = false;
        }
    }

    // FixedUpdate is called a fixed number of times per second
    void FixedUpdate()
    {
        FindNotePosition();
    }

    // Find the position of the corresponding note based on the activation key
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

        Vector3 newPosition = transform.position;
        newPosition.x = xPosition;
        
        // Temp magic number
        newPosition.y = -3.6f;
        transform.position = newPosition;
    }

    // OnTriggerStay2D is called when another Collider2D stays in the trigger
    void OnTriggerStay2D(Collider2D other)
    {
        // If collision is active and colliding with a note, delete the note
        if (collisionActive && other.gameObject.tag == "Note")
        {
            Destroy(other.gameObject);
        }
    }
}
