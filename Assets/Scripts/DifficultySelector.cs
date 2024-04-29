using UnityEngine;
using UnityEngine.UI;
using MapGeneration;

public class DifficultySelector : MonoBehaviour
{
    public Toggle easyToggle;
    public Toggle mediumToggle;
    public Toggle hardToggle;
    public Toggle ghostToggle;

    public GameObject midiList;
    private MidiList midiListScript;


    public static string DifficultyKey = "SelectedDifficulty";
    public static string GhostKey = "GhostNotesKey";

    public void Start()
    {
        easyToggle.onValueChanged.AddListener(delegate { OnToggleActivated(easyToggle, MapDifficulty.Easy); });
        mediumToggle.onValueChanged.AddListener(delegate { OnToggleActivated(mediumToggle, MapDifficulty.Medium); });
        hardToggle.onValueChanged.AddListener(delegate { OnToggleActivated(hardToggle, MapDifficulty.Hard); });
        ghostToggle.onValueChanged.AddListener(delegate { ToggleGhostNotes(ghostToggle); });

        midiListScript = (MidiList) midiList.GetComponent("MidiList");

        easyToggle.isOn = true;
        PlayerPrefs.SetString(DifficultyKey, MapDifficulty.Easy.ToString());
    }

    public void OnToggleActivated(Toggle toggle, MapDifficulty difficulty)
    {
        if (toggle.isOn)
        {
            PlayerPrefs.SetString(DifficultyKey, difficulty.ToString());
            PlayerPrefs.Save();

            Debug.Log("Selected difficulty: " + difficulty.ToString());

            midiListScript.UpdateText();
        }
    }

    public void ToggleGhostNotes(Toggle toggle) {
        int ghostNotesStatus = toggle.isOn ? 1 : 0;

        PlayerPrefs.SetInt(GhostKey, ghostNotesStatus);
        PlayerPrefs.Save();

        Debug.LogFormat("Ghost notes: {0}", ghostNotesStatus == 1 ? "enabled" : "disabled");
    }
}
