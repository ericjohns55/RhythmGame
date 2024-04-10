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

    public float downwardsForce = 100f;

    private float spacerSize;

    private float unitWidth;

    private Color[] colors = {Color.red, Color.yellow, Color.green, Color.blue,
                              Color.magenta, Color.gray, Color.black, Color.white};

    List<KeyCode> keys = new List<KeyCode>() {
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
        KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon
    };

    private float timestamp = 0f;
    private float lastRender = 0f;

    // Start is called before the first frame update
    void Start()
    {
        setScreenUnits();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Time.time > timestamp + 0.15f) {
            String keysPressed = "";

            foreach (KeyCode key in keys) {
                if (Input.GetKey(key)) {
                    Debug.Log(key.ToString() + ": " + keys.IndexOf(key).ToString());

                    int noteIndex = keys.IndexOf(key);
                    
                    setScreenUnits();
                    float xPosition = (spacerSize * (noteIndex + 1)) + noteIndex + 0.5f;
                    generateObject(xPosition, noteIndex);

                    keysPressed += key.ToString() + " ";

                    timestamp = Time.time;
                }
            }

        //spawn ghost notes randomly
        if (UnityEngine.Random.value < 0.2f)
        {
            int randomNoteIndex = UnityEngine.Random.Range(0, keys.Count);
            float randomXPosition = (spacerSize * (randomNoteIndex + 1)) + randomNoteIndex + 0.5f;
            generateObject(randomXPosition, randomNoteIndex, true);
        }

            if (keysPressed.Length != 0) {
                textElement.text = "Keys Pressed: " + keysPressed.Replace("Semicolon", ";").Trim();
            } else {
                if (Time.time > lastRender + 1.0f) {
                    textElement.text = "Waiting for input...";
                }
            }
        }
    }

    private void generateObject(float xPosition, int colorIndex, bool isGhostNote = false) {
        setScreenUnits();
        xPosition -= unitWidth; 

        GameObject newNote;

        if (isGhostNote)
        {
            newNote = Instantiate(ghostNotePrefab, new Vector2(xPosition, 4), Quaternion.identity);
            newNote.tag = "GhostNote";
            //ghost note color gray
            newNote.GetComponent<Renderer>().material.SetColor("_Color", Color.gray);
            //ghost note outline color black
            newNote.GetComponent<Renderer>().material.SetColor("_OutlineColor", Color.black);
        }
        else
        {
            newNote = Instantiate(notePrefab, new Vector2(xPosition, 4), Quaternion.identity);
            newNote.tag = "Note";
            //regular note color
            newNote.GetComponent<Renderer>().material.SetColor("_Color", colors[colorIndex]);
        }

        newNote.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -10);
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
