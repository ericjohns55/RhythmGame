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

    private float unitWidth;

    private Color[] colors = {Color.red, Color.yellow, Color.green, Color.blue,
                              Color.magenta, Color.white, Color.gray, Color.black};

    List<KeyCode> keys = new List<KeyCode>() {
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
        KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon
    };

    private float timestamp = 0f;
    private float lastRender = 0f;

    private OutputDevice outputDevice;
    private Playback playback;

    // Start is called before the first frame update
    void Start()
    {
        setScreenUnits();

        MidiFile testMidi = MidiFile.Read("Assets/MIDIs/BasicRhythms.mid");
        outputDevice = OutputDevice.GetByName("Microsoft GS Wavetable Synth");
        playback = testMidi.GetPlayback(outputDevice);
        playback.NotesPlaybackStarted += OnNotesPlaybackStarted;
        playback.NotesPlaybackFinished += OnNotesPlaybackFinished;
    }

    void OnApplicationQuit() {
        if (playback != null) {
            playback.Dispose();
        }

        if (outputDevice != null) {
            outputDevice.Dispose();
        }
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

            if (Input.GetKey(KeyCode.Space)) {
                timestamp = Time.time;

                if (playback.IsRunning) {
                    playback.Stop();
                } else {
                    playback.MoveToStart();
                    playback.Start();
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

    private void generateObject(float xPosition, int colorIndex) {
        
        setScreenUnits();
        xPosition -= unitWidth; 

        GameObject newNote = Instantiate(notePrefab, new Vector2(xPosition, 4), Quaternion.identity);
        newNote.tag = "Note";

        newNote.GetComponent<Renderer>().material.SetColor("_Color", colors[colorIndex]);
        newNote.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, -downwardsForce));

        lastRender = Time.time;
    }

    // finds units width of screen and sets spacerSize
    private void setScreenUnits() {        
        unitWidth = (float)Math.Round((Camera.main.orthographicSize * Camera.main.aspect),0);
        spacerSize = (unitWidth * 2 - 8) / 9f;
    }

    private void OnNotesPlaybackStarted(object sender, NotesEventArgs e)
    {
        LogNotes("Note played: ", e);
    }

    private void OnNotesPlaybackFinished(object sender, NotesEventArgs e)
    {
        LogNotes("Notes finished:", e);
    }

    private void LogNotes(string title, NotesEventArgs e)
    {
        var message = new StringBuilder()
            .AppendLine(title)
            .AppendLine(string.Join(Environment.NewLine, e.Notes.Select(n => $"  {n}")))
            .ToString();
        Debug.Log(message.Trim());
    }
}
