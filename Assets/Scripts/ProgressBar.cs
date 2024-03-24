using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    // Remove SerializeField when done
    [SerializeField] private Image progressBar;
    [SerializeField] private float currentValue = 0f;
    [SerializeField] private float maxValue = 100f;
    [SerializeField] private float barWidth = 200f; // Size of the progress bar when full in Unity units

    private RectTransform transform;

    void Start() {
        transform = progressBar.rectTransform;
    }

    // Temporary to demonstrate functionality
    void Update() {
        UpdateProgressBar(currentValue);
        currentValue += Time.deltaTime * 10; // Example: increment by 10 per second
        currentValue = Mathf.Clamp(currentValue, 0f, maxValue);
    }

    public void UpdateProgressBar(float newValue) {
        // Adjusting the width of the progress bar
        float fillAmount = newValue / maxValue;
        transform.sizeDelta = new Vector2(barWidth * fillAmount, transform.sizeDelta.y);
    
        // Adjusting the position of the progress bar so it only extends to the right
        float newPos = (-barWidth + transform.sizeDelta.x) / 2;
        transform.anchoredPosition = new Vector2(newPos, transform.anchoredPosition.y);
    }
}
