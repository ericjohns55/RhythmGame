using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SearchMidiFile : MonoBehaviour
{
    public GameObject contentHolder;
    public GameObject searchBar;
    public GameObject[] element;

    public int totalElements;

    // Start is called before the first frame update
    void Start()
    {
        totalElements = contentHolder.transform.childCount;

        element = new GameObject[totalElements];

        for (int i = 0; i < totalElements; i++)
        {
            element[i] = contentHolder.transform.GetChild(i).gameObject;
        }
        
    }

    public void Search() 
    {
        string searchText = searchBar.GetComponent<TMP_InputField>().text.ToLower();
        
        foreach(GameObject element in Element)
        {
            string elementText = element.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text.ToLower();
            bool containsSearchText = elementText.Contains(searchText);
            element.SetActive(containsSearchText);
        }
    }
    
}
