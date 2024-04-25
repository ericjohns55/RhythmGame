using UnityEngine;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour
{
    public Toggle easyToggle;
    public Toggle mediumToggle;
    public Toggle hardToggle;

    private const string DifficultyKey = "SelectedDifficulty";

    public void Start()
    {
        easyToggle.onValueChanged.AddListener(delegate { OnToggleActivated(easyToggle, Difficulty.Easy); });
        mediumToggle.onValueChanged.AddListener(delegate { OnToggleActivated(mediumToggle, Difficulty.Medium); });
        hardToggle.onValueChanged.AddListener(delegate { OnToggleActivated(hardToggle, Difficulty.Hard); });
    }

    public void OnToggleActivated(Toggle toggle, Difficulty difficulty)
    {
        if (toggle.isOn)
        {
            PlayerPrefs.SetString(DifficultyKey, difficulty.ToString());
            PlayerPrefs.Save();

            Debug.Log("Selected difficulty: " + difficulty.ToString());
        }
    }
}

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}
