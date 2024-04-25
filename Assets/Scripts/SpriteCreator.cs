using UnityEngine;
using TMPro;
using System;

public class SpriteCreator : MonoBehaviour
{
    public GameObject notePrefab;
    public GameObject ghostNotePrefab;
    public TMP_Text textElement;

    public float downwardsForce = 10f;

    private float spacerSize;

    private float unitWidth;

    private Color[] colors = {Color.red, Color.yellow, Color.green, Color.blue,
                              Color.magenta, new Color(0.647f, 0.647f, 0.647f), Color.gray, Color.white};


    // Start is called before the first frame update
    void Start()
    {
        setScreenUnits();
    }

    private void generateObject(float xPosition, int colorIndex, bool isGhostNote = false) {
        setScreenUnits();
        xPosition -= unitWidth; 

        if (isGhostNote)
        {
            // do ghost note stuff
        }
        else
        {
            // Instantiate a regular note
            GameObject regularNote = Instantiate(notePrefab, new Vector2(xPosition, 4), Quaternion.identity);
            regularNote.tag = "Note";
            regularNote.GetComponent<Renderer>().material.SetColor("_Color", colors[colorIndex]);
            regularNote.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -downwardsForce);
        }
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
}
