using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using System.Text;
using System.Linq;

public class SpriteCreator : MonoBehaviour
{
    public GameObject notePrefab;
    public GameObject ghostNotePrefab;
    public TMP_Text textElement;

    private bool regularEventRemoved = false;

    public float downwardsForce = 100f;

    private float spacerSize;

    private float unitWidth;

    private Color[] colors = {Color.red, Color.yellow, Color.green, Color.blue,
                              Color.magenta, new Color(0.647f, 0.647f, 0.647f), Color.gray, Color.white};

    List<KeyCode> keys = new List<KeyCode>() {
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
        KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon
    };

    private float timestamp = 0f;
    private float lastRender = 0f;

    private void RemoveEvent(bool isRegularNote)
    {
    // If the removed event was a regular note, trigger chance for ghost note
    if (isRegularNote && UnityEngine.Random.value < 0.4f)
    {
        Debug.Log("Random value chosen");
        SpawnGhostNote();
    }
    }

    // Start is called before the first frame update
    void Start()
    {
        setScreenUnits();
    }

    // Update is called once per frame
    void FixedUpdate()
{
    if (Time.time > timestamp + 0.15f)
    {
        String keysPressed = "";

        foreach (KeyCode key in keys)
        {
            if (Input.GetKey(key))
            {
                Debug.Log(key.ToString() + ": " + keys.IndexOf(key).ToString());

                int noteIndex = keys.IndexOf(key);

                setScreenUnits();
                float xPosition = (spacerSize * (noteIndex + 1)) + noteIndex + 0.5f;
                generateObject(xPosition, noteIndex); 

                RemoveEvent(true); 

                keysPressed += key.ToString() + " ";

                timestamp = Time.time;
            }
        }

        if (keysPressed.Length != 0)
        {
            textElement.text = "Keys Pressed: " + keysPressed.Replace("Semicolon", ";").Trim();
        }
        else
        {
            if (Time.time > lastRender + 1.0f)
            {
                textElement.text = "Waiting for input...";
            }
        }
    }
}


   private void SpawnGhostNote() 
{
    Debug.Log("Attempting to spawn ghost note...");
    GameObject[] regularNotes = GameObject.FindGameObjectsWithTag("Note");

    if (regularNotes.Length > 0)
    {
        Debug.Log("Regular notes found. Spawning ghost note...");
        // Pick a random regular note to be replaced
        int randomIndex = UnityEngine.Random.Range(0, regularNotes.Length);
        GameObject noteToReplace = regularNotes[randomIndex];

        // Instantiate ghost note
        GameObject ghostNote = Instantiate(ghostNotePrefab, noteToReplace.transform.position, Quaternion.identity);
        ghostNote.tag = "GhostNote";

        // Set ghost note color to opaque gray
        ghostNote.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        // Set ghost note outline color to black
        ghostNote.GetComponent<Renderer>().material.SetColor("_OutlineColor", Color.black);
        ghostNote.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -10);

        // Destroy the regular note that was replaced
        Destroy(noteToReplace);
    } 
    else 
    {
        Debug.Log("No regular notes found. Skipping ghost note spawning.");
    }
}




    private void generateObject(float xPosition, int colorIndex, bool isGhostNote = false) {
        setScreenUnits();
        xPosition -= unitWidth; 

        if (isGhostNote)
        {
            // Spawn ghost note
        GameObject ghostNote = Instantiate(ghostNotePrefab, new Vector2(xPosition, 4), Quaternion.identity);
        ghostNote.tag = "GhostNote";
        ghostNote.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        ghostNote.GetComponent<Renderer>().material.SetColor("_OutlineColor", Color.black);
        ghostNote.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -10);
        }
        else
        {
            // Instantiate a regular note
            GameObject regularNote = Instantiate(notePrefab, new Vector2(xPosition, 4), Quaternion.identity);
            regularNote.tag = "Note";
            regularNote.GetComponent<Renderer>().material.SetColor("_Color", colors[colorIndex]);
            regularNote.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -10);
        }

        lastRender = Time.time;
    }

    // finds units width of screen and sets spacerSize
    public void setScreenUnits() {        
        unitWidth = (float)Math.Round((Camera.main.orthographicSize * Camera.main.aspect),0);
        spacerSize = (unitWidth * 2 - 8) / 9f;
    }

    public void generateNote(int index) {
        float xPosition = (spacerSize * (index + 1) + index + 0.5f);
        generateObject(xPosition, index);
    }

    public float GetSpacerSize()
    {
        return spacerSize;
    }

    public float GetUnitWidth()
    {
        return unitWidth;
    }

    public List<KeyCode> Keys
    {
        get { return keys; }
    }
}