using UnityEngine;
using UnityEngine.UI;

public class ProgressBarOutline : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    public RawImage outlineImage;
    public float outlineSize = 5f;

    private Texture2D outlineTexture;
    private RectTransform outlineTransform;
    private RectTransform barTransform;

    public GameObject GameManager;

    private ProgressBar ProgressBarScript;

    void Start() {
        outlineTransform = outlineImage.rectTransform;
        barTransform = progressBar.rectTransform;
        ProgressBarScript = (ProgressBar) GameManager.GetComponent("ProgressBar");
        GenerateOutlineTexture();
    }

    /**
     * Updates the outline every frame.
     */
    void Update() {
        GenerateOutlineTexture();
    }

    /**
     * Generates the outline texture and applies it to the ProgressBarOutline.
     */
    void GenerateOutlineTexture() {
        int width = (int)(ProgressBarScript.barWidth + outlineSize * 2);
        int height = (int)(barTransform.sizeDelta.y + outlineSize); // not times two so the progress bar doesnt glitch

        if (outlineTexture != null)
            Destroy(outlineTexture);

        outlineTransform.sizeDelta = new Vector2(width, height);
        outlineTexture = new Texture2D(width, height);

        // Create new texture pixel by pixel
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (x < outlineSize || x >= width - outlineSize || y < outlineSize || y >= height - outlineSize)
                    outlineTexture.SetPixel(x, y, Color.black); // Outline color
                else
                    outlineTexture.SetPixel(x, y, Color.clear);
            }
        }

        // Apply the new texture
        outlineTexture.Apply();
        outlineImage.texture = outlineTexture;
        outlineTransform.sizeDelta = new Vector2(width, height);

        // Calculate the position of the outline based on the progress bar's position
        float outlineX = ProgressBarScript.xOffset;
        float outlineY = barTransform.anchoredPosition.y;

        // Set the position of the outline
        outlineTransform.anchoredPosition = new Vector2(outlineX, outlineY);
    }
}
