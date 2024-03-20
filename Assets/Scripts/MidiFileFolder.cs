using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SearchMidiFile : MonoBehaviour
{
    public GameObject ContentHolder;
    public GameObject SearchBar;
    public GameObject[] Element;

    public int totalElements;

    // Start is called before the first frame update
    void Start()
    {
        totalElements = ContentHolder.transform.childCount;

        Element = new GameObject[totalElements];

        for (int i = 0; i < totalElements; i++)
        {
            Element[i] = ContentHolder.transform.GetChild(i).gameObject;
        }
        
    }

    public void Search() 
    {
        string SearchText = SearchBar.GetComponent<TMP_InputField>().text.ToLower();
        
        foreach(GameObject element in Elements)
        {
           string elementText = element.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text.ToLower();
            bool containsSearchText = elementText.Contains(searchText);
            element.SetActive(containsSearchText);
        }
    }
    
}
