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

    // Start is called before the first frame update
    void Start()
    {
        defaultColor = GetComponent<SpriteRenderer>().color;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(activationKey))
        {
            isPressed = true;
            timer = Time.time;

            // Change button color to pressedColor
            GetComponent<SpriteRenderer>().color = pressedColor;

            // Change collision state
            collisionActive = true;
        }
        
        if(Input.GetKeyUp(activationKey) || (isPressed && Time.time - timer >= buttonLifetime))
        {
            isPressed = false;

            // Change button color to defaultColor
            GetComponent<SpriteRenderer>().color = defaultColor;

            // Change collision state
            collisionActive = false;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // If collision is active delete collider
        if (collisionActive && other.gameObject.tag == "Note")
        {
            Destroy(other.gameObject);
        }
    }
}
