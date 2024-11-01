using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("--- GameObjects ---")]
    [SerializeField] private GameObject _wordsContainer;
    [SerializeField] private GameObject _boardLetterContainer;

    private List<string> _currentLevelWords = new();  // Words for the current level
    private GameObject _boardLetterPrefab;
    private GameObject _wordObjectPrefab;
    private string _defaultBoardLetter = "A"; // Default text for board letters

    private void OnEnable()
    {
        // Initialize word list for the current gameplay level
        StartCoroutine(UpdateListOfCurrentLevelWords());
    }

    private void Start()
    {
        // Load prefabs to reduce repeated resource loading
        _boardLetterPrefab = Resources.Load<GameObject>("BoardLetter");
        _wordObjectPrefab = Resources.Load<GameObject>("WordText");

        if (_boardLetterPrefab == null || _wordObjectPrefab == null)
        {
            Debug.LogError("One or more prefabs could not be loaded from Resources.");
            return;
        }


        PopulateWordsContainer();
        FillBoardWithDefaults();
    }

    // Coroutine to load current level words from GameData asynchronously
    private IEnumerator UpdateListOfCurrentLevelWords()
    {
        yield return GameData.Instance.LoadLevelData(GameData.UnlockedLevel, words =>
        {
            _currentLevelWords = words;
        });
    }

    // Populates the words container with the words for the current level
    private void PopulateWordsContainer()
    {
        DestroyAllChildren(_wordsContainer);

        Debug.Log($"Level {GameData.UnlockedLevel} has {_currentLevelWords.Count} words to add to the container.");

        foreach (string wordText in _currentLevelWords)
        {
            GameObject wordObject = InstantiateWithParent(_wordObjectPrefab, _wordsContainer, false, ObjectNaming.Incremental);
            wordObject.GetComponent<Text>().text = wordText;
        }
    }

    // Fills the board with default letters based on the board size
    private void FillBoardWithDefaults()
    {
        DestroyAllChildren(_boardLetterContainer);

        Vector2Int boardSize = GameData.Instance.GetLevelBoardMatrixSize(GameData.UnlockedLevel);
        Vector2Int cellSize = GameData.Instance.GetLevelBoardCellSize(GameData.UnlockedLevel);

        Debug.Log($"Board size for Level {GameData.UnlockedLevel} is {boardSize.x} X {boardSize.y} and Cell size is {cellSize.x} X {cellSize.y}.");

        //set board cell size
        _boardLetterContainer.GetComponent<GridLayoutGroup>().cellSize = cellSize;

        if (_boardLetterPrefab == null)
        {
            Debug.LogError("BoardLetter prefab is missing.");
            return;
        }

        for (int i = 0; i < boardSize.x; i++)
        {
            for (int j = 0; j < boardSize.y; j++)
            {
                GameObject boardLetterObject = InstantiateWithParent(_boardLetterPrefab, _boardLetterContainer, false, ObjectNaming.Repeat);
                boardLetterObject.name += "_" + i + "_" + j;
                boardLetterObject.GetComponent<Text>().text = GetRandomLetter();
            }
        }
    }

    private string GetRandomLetter()
    {
        // Generate a random number between 65 ('A') and 90 ('Z')
        int randomAscii = Random.Range(65, 91);
        // Convert the ASCII value to a character and then to a string
        return ((char)randomAscii).ToString();
    }


    // Instantiates an object, sets its parent, and applies additional options
    private GameObject InstantiateWithParent(GameObject prefab, GameObject parent, bool setAsFirstChild = false, ObjectNaming namingOption = ObjectNaming.Default)
    {
        GameObject instantiatedObject = Instantiate(prefab, parent.transform);
        instantiatedObject.transform.localScale = Vector3.one;

        RectTransform rectTransform = instantiatedObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, 0);

        if (setAsFirstChild)
        {
            instantiatedObject.transform.SetAsFirstSibling();
        }

        ApplyNaming(instantiatedObject, prefab.name, namingOption);

        return instantiatedObject;
    }

    // Sets naming conventions based on naming option
    private void ApplyNaming(GameObject obj, string baseName, ObjectNaming namingOption)
    {
        switch (namingOption)
        {
            case ObjectNaming.Incremental:
                // Generate a unique incremental name based on the base name
                int incrementalIndex = GetNextIncrementalIndex(baseName);
                obj.name = $"{baseName}_{incrementalIndex}"; // Append index to base name
                break;

            case ObjectNaming.Decremental:
                // Generate a unique decremental name based on the base name
                int decrementalIndex = GetNextDecrementalIndex(baseName);
                obj.name = $"{baseName}_{decrementalIndex}"; // Append index to base name
                break;

            case ObjectNaming.Repeat:
            default:
                obj.name = baseName; // Default naming
                break;
        }
    }

    // Keeps track of the count of instantiated objects with the same base name for incremental naming
    private Dictionary<string, int> _incrementalCounters = new Dictionary<string, int>();

    // Get the next incremental index for the specified base name
    private int GetNextIncrementalIndex(string baseName)
    {
        if (!_incrementalCounters.ContainsKey(baseName))
        {
            _incrementalCounters[baseName] = 0; // Initialize the counter
        }

        return _incrementalCounters[baseName]++; // Return the current counter and increment it
    }

    // Keeps track of the count of instantiated objects with the same base name for decremental naming
    private Dictionary<string, int> _decrementalCounters = new Dictionary<string, int>();

    // Get the next decremental index for the specified base name
    private int GetNextDecrementalIndex(string baseName)
    {
        if (!_decrementalCounters.ContainsKey(baseName))
        {
            _decrementalCounters[baseName] = 1; // Initialize the counter to 1 for decremental naming
        }

        return _decrementalCounters[baseName]--; // Return the current counter and decrement it
    }

    // Destroys all children of a given parent GameObject
    private void DestroyAllChildren(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }
    }
}

// Enum to define naming options
public enum ObjectNaming
{
    Default,
    Incremental,
    Decremental,
    Repeat,
}
