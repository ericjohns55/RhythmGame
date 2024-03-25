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

    void Update() {
        GenerateOutlineTexture();
    }

    void GenerateOutlineTexture() {
        int width = (int)(ProgressBarScript.barWidth + outlineSize * 2);
        int height = (int)(barTransform.sizeDelta.y + outlineSize * 2);

        if (outlineTexture != null)
            Destroy(outlineTexture);

        outlineTransform.sizeDelta = new Vector2(width, height);
        outlineTexture = new Texture2D(width, height);

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (x < outlineSize || x >= width - outlineSize || y < outlineSize || y >= height - outlineSize)
                    outlineTexture.SetPixel(x, y, Color.black); // Outline color
                else
                    outlineTexture.SetPixel(x, y, Color.clear);
            }
        }

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
