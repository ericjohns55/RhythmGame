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
    public TMP_Text textElement;

    public float downwardsForce = 100f;

    private float spacerSize;

    private Color[] colors = {Color.red, Color.yellow, Color.green, Color.blue,
                              Color.magenta, Color.white, Color.gray, Color.black};

    List<KeyCode> keys = new List<KeyCode>() {
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
        KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon
    };

    private float timestamp = 0f;
    private float lastRender = 0f;

    // Start is called before the first frame update
    void Start()
    {
        spacerSize = (Camera.main.orthographicSize * 2 - 8) / 9f;
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
                    float xPosition = (spacerSize * (noteIndex + 1)) + noteIndex + 0.5f;
                    generateObject(xPosition, noteIndex);

                    keysPressed += key.ToString() + " ";

                    timestamp = Time.time;
                }
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

    public void generateObject(float xPosition, int colorIndex) {
        xPosition -= 5f; // acount for camera starting at -5 and going to +5

        GameObject newNote = Instantiate(notePrefab, new Vector2(xPosition, 4), Quaternion.identity);
        newNote.tag = "Note";

        newNote.GetComponent<Renderer>().material.SetColor("_Color", colors[colorIndex]);
        newNote.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, -downwardsForce));

        lastRender = Time.time;
    }

    public void generateNote(int index) {
        float xPosition = (spacerSize * (index + 1) + index + 0.5f);
        generateObject(xPosition, index);
    }
}
