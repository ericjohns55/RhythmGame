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
                              Color.magenta, Color.gray, Color.black, Color.white};

    // Start is called before the first frame update
    void Start()
    {
        setScreenUnits();
    }

    private void generateObject(float xPosition, int colorIndex, bool isGhostNote) {
        setScreenUnits();
        xPosition -= unitWidth; 

        GameObject note = Instantiate(notePrefab, new Vector2(xPosition, 4), Quaternion.identity);

        if (isGhostNote) {
            note.tag = "GhostNote";
            note.GetComponent<Renderer>().material.color = new Color(0.0f, 0.0f, 0.0f, 0.75f); 
            note.GetComponent<Renderer>().material.SetColor("_OutlineColor", Color.black); 
            note.GetComponent<Renderer>().material.SetFloat("_OutlineWidth", 0.1f); 
            note.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -10);
        } else {
            note.tag = "Note";
            note.GetComponent<Renderer>().material.SetColor("_Color", colors[colorIndex]);
        }
        
        note.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -downwardsForce);
    }

    // finds units width of screen and sets spacerSize
    public void setScreenUnits() {        
        unitWidth = (float)Math.Round((Camera.main.orthographicSize * Camera.main.aspect),0);
        spacerSize = (unitWidth * 2 - 8) / 9f;
    }

    public void generateNote(int index, bool ghostNote) {
        float xPosition = (spacerSize * (index + 1) + index + 0.5f);
        generateObject(xPosition, index, ghostNote);
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

