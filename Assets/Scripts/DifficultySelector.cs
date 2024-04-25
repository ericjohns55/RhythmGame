using UnityEngine;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour
{
    public Toggle easyToggle;
    public Toggle mediumToggle;
    public Toggle hardToggle;
    public Toggle ghostToggle;

    private const string DifficultyKey = "SelectedDifficulty";
    private const string GhostKey = "GhostNotes";

    public void Start()
    {
        easyToggle.onValueChanged.AddListener(delegate { OnToggleActivated(easyToggle, Difficulty.Easy); });
        mediumToggle.onValueChanged.AddListener(delegate { OnToggleActivated(mediumToggle, Difficulty.Medium); });
        hardToggle.onValueChanged.AddListener(delegate { OnToggleActivated(hardToggle, Difficulty.Hard); });
        ghostToggle.onValueChanged.AddListener(delegate { ToggleGhostNotes(ghostToggle); });
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

    public void ToggleGhostNotes(Toggle toggle) {
        bool ghostNotesStatus = toggle.isOn;

        PlayerPrefs.SetString(GhostKey, ghostNotesStatus.ToString());
        PlayerPrefs.Save();

        Debug.LogFormat("Ghost notes: {0}", ghostNotesStatus ? "enabled" : "disabled");
    }
}

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}
