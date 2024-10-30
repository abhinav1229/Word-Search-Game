using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("--- GameObjects ---")]
    [SerializeField] private GameObject _wordsContainer;

    // Private Fields
    private List<string> _currentLevelWords = new();

    private void OnEnable() {
        // Initialize the list values for the current gameplay level
        UpdateListOfCurrentLevelWords();
    }

    private void Start() {
        AddWordsOnWordsContainer();
    }

    private void UpdateListOfCurrentLevelWords()
    {
        _currentLevelWords = new(){"hello", "prime"};
    }

    private void AddWordsOnWordsContainer()
    {
        // load the prefab from the resource to instatiate
        GameObject wordObjectMain = Resources.Load("WordText") as GameObject;

        // Instantiate the wordObjects 
        foreach (string wordText in _currentLevelWords)
        {
            // Instantiate and set up it correctly
            GameObject wordObject = Instantiate(wordObjectMain);
            wordObject.transform.SetParent(_wordsContainer.transform);
            wordObject.transform.localScale = Vector3.one;
            Vector3 wordObjectAnchors = wordObject.transform.GetComponent<RectTransform>().anchoredPosition3D;
            wordObjectAnchors.z = 0;
            wordObject.transform.GetComponent<RectTransform>().anchoredPosition3D = wordObjectAnchors;

            // set the text
            wordObject.GetComponent<Text>().text = wordText;
        }
    }
}
