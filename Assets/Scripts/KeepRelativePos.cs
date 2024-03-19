using UnityEngine;

public class KeepRelativePos : MonoBehaviour
{
    private Vector3 initialOffset;
    private Vector2 initialResolution;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 cameraPos = Camera.main.transform.position;
        initialOffset = transform.position - cameraPos;
        initialResolution = new Vector2(Screen.width, Screen.height);
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the update position based on the camera position
        Vector3 cameraPos = Camera.main.transform.position;
        
        // Calculate the aspect ratio of the screen relative to the initial resolution
        float screenAspect = (float)Screen.width / Screen.height;
        float referenceAspect = initialResolution.x / initialResolution.y;
        float aspectRatioMultiplier = referenceAspect / screenAspect;

        // Apply the aspect ratio multiplier to the initial offset
        Vector3 scaledOffset = initialOffset * aspectRatioMultiplier;

        // Calculate the new position based on the scaled offset
        Vector3 newPos = cameraPos + scaledOffset;

        // Update the position
        transform.position = newPos;
    }
}
