using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonControl : MonoBehaviour
{
    [SerializeField] private KeyCode activationKey;     // Key press that activates button
    public Color pressedColor;                          // Button color on press
    public bool collisionActive = false;                // Flag for collision state
    
    private Color defaultColor;                         // Default button color

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
            // Change button color to pressedColor
            GetComponent<SpriteRenderer>().color = pressedColor;

            // Change collision state
            collisionActive = !collisionActive;
        }
        else if (Input.GetKeyUp(activationKey))
        {
            // Change button color to defaultColor
            GetComponent<SpriteRenderer>().color = defaultColor;

            // Change collision state
            collisionActive = !collisionActive;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // If collision is active delete collider
        if (collisionActive && other.gameObject.tag == "Note")
        {
            Destroy(other.gameObject);
        }
    }
}
