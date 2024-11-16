using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("--- GameObjects ---")]
    [SerializeField] private GameObject _wordsContainer;
    [SerializeField] private GameObject _boardLetterContainer;
    [SerializeField] private Canvas _canvas;

    [HideInInspector] public List<string> CurrentLevelWords = new();  // Words for the current level
    private GameObject _boardLetterPrefab;
    private GameObject _wordObjectPrefab;
    private string _defaultBoardLetter = "A"; // Default text for board letters

    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }
    }


    private void OnEnable()
    {
        // Initialize word list for the current gameplay level

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


        StartGame();
    }

    public void StartGame()
    {
        StartCoroutine(UpdateListOfCurrentLevelWords());
    }

    // Coroutine to load current level words from GameData asynchronously
    private IEnumerator UpdateListOfCurrentLevelWords()
    {
        yield return GameData.Instance.LoadLevelData(GameData.UnlockedLevel, words =>
        {
            CurrentLevelWords = words;
            FillBoardWithDefaults();
        });
    }

    // Populates the words container with the words for the current level
    private void PopulateWordsContainer()
    {
        DestroyAllChildren(_wordsContainer);

        Debug.Log($"Level {GameData.UnlockedLevel} has {CurrentLevelWords.Count} words to add to the container.");

        foreach (string wordText in CurrentLevelWords)
        {
            GameObject wordObject = InstantiateWithParent(_wordObjectPrefab, _wordsContainer, false, ObjectNaming.Incremental);
            wordObject.GetComponent<Text>().text = wordText;
        }

        UpdateGridUI();
    }

    private List<GameObject> letterObjects = new List<GameObject>();
    // Fills the board with default letters based on the board size
    private void FillBoardWithDefaults()
    {
        DestroyAllChildren(_boardLetterContainer);

        letterObjects.Clear();

        Vector2Int boardSize = GameData.Instance.GetLevelBoardMatrixSize(GameData.UnlockedLevel);
        Vector2Int cellSize = GameData.Instance.GetLevelBoardCellSize(GameData.UnlockedLevel);
        gridSize = boardSize.x * boardSize.y;

        Debug.Log($"Board size for Level {GameData.UnlockedLevel} is {boardSize.x} X {boardSize.y} and Cell size is {cellSize.x} X {cellSize.y}.");

        //set board cell size
        _boardLetterContainer.GetComponent<GridLayoutGroup>().cellSize = cellSize;

        if (_boardLetterPrefab == null)
        {
            Debug.LogError("BoardLetter prefab is missing.");
            return;
        }

        grid = new char[boardSize.x, boardSize.y];

        for (int i = 0; i < boardSize.x; i++)
        {
            for (int j = 0; j < boardSize.y; j++)
            {
                GameObject boardLetterObject = InstantiateWithParent(_boardLetterPrefab, _boardLetterContainer, false, ObjectNaming.Repeat);
                boardLetterObject.name += "_" + i + "_" + j;
                boardLetterObject.GetComponent<Text>().text = _defaultBoardLetter;
                letterObjects.Add(boardLetterObject);
                grid[i, j] = '\0';
            }
        }
        PlaceWordsInGrid();
    }

    void UpdateGridUI()
    {
        Vector2Int boardSize = GameData.Instance.GetLevelBoardMatrixSize(GameData.UnlockedLevel);
        int k = 0;

        Debug.Log("UpdateGridUI: boardSize: " + boardSize);
        for (int i = 0; i < boardSize.x; i++)
        {
            for (int j = 0; j < boardSize.y; j++)
            {
                if (grid[i, j] == '\0')
                {
                    letterObjects[k].GetComponentInChildren<Text>().text = GetRandomLetter();
                }
                else
                {
                    letterObjects[k].GetComponentInChildren<Text>().text = grid[i, j].ToString();
                    // letterObjects[k].GetComponentInChildren<Text>().color = Color.red;
                }
                k++;
            }
        }
    }



    private void PlaceWordsInGrid()
    {
        Vector2Int boardSize = GameData.Instance.GetLevelBoardMatrixSize(GameData.UnlockedLevel);

        List<(int x, int y)> positions = new List<(int, int)>();

        // Populate and shuffle list of all grid positions
        for (int x = 0; x < boardSize.x; x++)
        {
            for (int y = 0; y < boardSize.y; y++)
            {
                positions.Add((x, y));
            }
        }

        positions = positions
            .OrderBy(_ => System.Guid.NewGuid()) // Randomize the order
            .ToList(); // Convert back to a list

        List<string> lstOfNonPlacedWords = new List<string>();
        CurrentLevelWords = CurrentLevelWords
                    .OrderBy(_ => System.Guid.NewGuid()) // Randomize the order
                    .ToList();

        foreach (string word in CurrentLevelWords)
        {
            bool placed = false;

            foreach (var pos in positions)
            {
                int x = pos.x;
                int y = pos.y;

                // Create and shuffle directions array (0-7 includes diagonals)
                List<int> directions = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
                directions = directions
                    .OrderBy(_ => System.Guid.NewGuid()) // Randomize the order
                    .ToList();

                foreach (int direction in directions)
                {
                    if (CanPlaceWord(word, x, y, direction, boardSize))
                    {
                        PlaceWord(word, x, y, direction);
                        placed = true;
                        break; // Exit direction loop
                    }
                }

                if (placed) break; // Exit position loop if word is placed
            }

            // If not placed, log a warning
            if (!placed)
            {
                Debug.LogError($"Could not place word: {word}. Grid may be too small or crowded.");
                lstOfNonPlacedWords.Add(word);

            }
        }

        foreach (string word in lstOfNonPlacedWords)
        {
            CurrentLevelWords.Remove(word);
        }

        PopulateWordsContainer();
    }


    public void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n - 1; i++)
        {
            // Determine a new index to swap with, based on a pattern
            int j = (i + 3) % n; // This is just an arbitrary pattern for swapping

            // Swap elements at indices i and j
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private char[,] grid;
    int gridSize = 0;
    bool CanPlaceWord(string word, int startX, int startY, int direction, Vector2Int boardSize)
    {
        int dx = 0, dy = 0;

        switch (direction)
        {
            case 0: dx = 1; dy = 0; break;   // Left to Right
            case 1: dx = -1; dy = 0; break;  // Right to Left
            case 2: dx = 0; dy = 1; break;   // Top to Bottom
            case 3: dx = 0; dy = -1; break;  // Bottom to Top
            case 4: dx = 1; dy = 1; break;   // Top-left to Bottom-right (↘️)
            case 5: dx = -1; dy = -1; break; // Bottom-right to Top-left (↖️)
            case 6: dx = -1; dy = 1; break;  // Bottom-left to Top-right (↗️)
            case 7: dx = 1; dy = -1; break;  // Top-right to Bottom-left (↙️)
        }

        int x = startX;
        int y = startY;

        for (int i = 0; i < word.Length; i++)
        {
            if (x < 0 || x >= boardSize.x || y < 0 || y >= boardSize.y ||
                (grid[x, y] != '\0' && grid[x, y] != word[i]))
            {
                return false;
            }

            x += dx;
            y += dy;
        }

        return true;
    }


    void PlaceWord(string word, int x, int y, int direction)
    {
        int dx = 0, dy = 0;

        switch (direction)
        {
            case 0: dx = 1; dy = 0; break;   // Left to Right
            case 1: dx = -1; dy = 0; break;  // Right to Left
            case 2: dx = 0; dy = 1; break;   // Top to Bottom
            case 3: dx = 0; dy = -1; break;  // Bottom to Top
            case 4: dx = 1; dy = 1; break;   // Top-left to Bottom-right (↘️)
            case 5: dx = -1; dy = -1; break; // Bottom-right to Top-left (↖️)
            case 6: dx = -1; dy = 1; break;  // Bottom-left to Top-right (↗️)
            case 7: dx = 1; dy = -1; break;  // Top-right to Bottom-left (↙️)
        }

        for (int i = 0; i < word.Length; i++)
        {
            grid[x + dx * i, y + dy * i] = word[i];
        }
    }



    private string GetRandomLetter()
    {
        // Generate a random number between 65 ('A') and 90 ('Z')
        int randomAscii = Random.Range(65, 91);

        // Convert the ASCII value to a character and then to a string
        return ((char)randomAscii).ToString();
    }


    private int _countMarksWords = 0;
    public void MarkWordAsFound(string matchedWord)
    {
        foreach (Transform word in _wordsContainer.transform)
        {
            if (word.GetComponent<Text>().text.Equals(matchedWord))
            {
                _countMarksWords += 1;
                word.GetChild(0).gameObject.SetActive(true);
            }
        }

        if (_countMarksWords == CurrentLevelWords.Count)
        {
            Invoke(nameof(ShowLevelCompletePopup), 0.8f);
        }
    }

    private void ShowLevelCompletePopup()
    {
        GameObject levelComplete = Resources.Load<GameObject>("LevelCompletePopup");
        Instantiate(levelComplete, _canvas.transform);
        _countMarksWords = 0;

        GameUIManager.Instance.StopCoroutine(GameUIManager.Instance._timerCoroutine);
    }

    public void ShowGameOverPopup()
    {
        CancelInvoke(nameof(ShowLevelCompletePopup));

        GameObject levelComplete = Resources.Load<GameObject>("GameOverPopup");
        Instantiate(levelComplete, _canvas.transform);
        _countMarksWords = 0;
    }

    public bool IsMarkedTheWord(string matchedWord)
    {
        bool isActive = false;
        foreach (Transform word in _wordsContainer.transform)
        {
            if (word.GetComponent<Text>().text.Equals(matchedWord))
            {
                isActive = word.GetChild(0).gameObject.activeInHierarchy;
                break;
            }
        }

        return isActive;
    }
    public void OnHintButtonClick(GameObject hintButton)
    {
        foreach (Transform word in _wordsContainer.transform)
        {
            // Find the first word that hasn't been completed
            if (!word.GetChild(0).gameObject.activeInHierarchy)
            {
                string targetWord = word.GetComponent<Text>().text;
                Debug.Log("Hint for word: " + targetWord);

                // Find the starting position and direction of the word in the grid
                (Vector2Int startPosition, Vector2Int direction) = FindWordStartAndDirection(targetWord);

                if (startPosition.x != -1 && startPosition.y != -1)
                {
                    Debug.Log($"Word {targetWord} starts at: {startPosition} in direction {direction}");

                    // Highlight the first two letters in the UI
                    HighlightLetters(startPosition, direction, 2);
                }
                else
                {
                    Debug.Log($"Word {targetWord} not found in the grid!");
                }

                break; // Stop after finding the first incomplete word
            }
        }
    }

    /// <summary>
    /// Finds the starting position and direction of a word in the grid.
    /// </summary>
    private (Vector2Int, Vector2Int) FindWordStartAndDirection(string targetWord)
    {
        Vector2Int boardSize = GameData.Instance.GetLevelBoardMatrixSize(GameData.UnlockedLevel);

        // Define all 8 possible directions
        Vector2Int[] directions = {
        Vector2Int.right,                // Left to right
        Vector2Int.left,                 // Right to left
        Vector2Int.down,                 // Top to bottom
        Vector2Int.up,                   // Bottom to top
        new Vector2Int(1, 1),            // Top-left to bottom-right
        new Vector2Int(-1, -1),          // Bottom-right to top-left
        new Vector2Int(-1, 1),           // Top-right to bottom-left
        new Vector2Int(1, -1)            // Bottom-left to top-right
    };

        for (int i = 0; i < boardSize.x; i++)
        {
            for (int j = 0; j < boardSize.y; j++)
            {
                // Check if the grid matches the first letter of the word
                if (char.ToLower(grid[i, j]) == char.ToLower(targetWord[0]))
                {
                    Debug.Log($"First letter match at {i},{j}");

                    // Check all possible directions for the word
                    foreach (var direction in directions)
                    {
                        if (DoesWordFitInDirection(i, j, targetWord, direction))
                        {
                            Debug.Log($"Word {targetWord} fits starting at {i},{j} in direction {direction}");
                            return (new Vector2Int(i, j), direction);
                        }
                    }
                }
            }
        }

        // Return an invalid position if not found
        return (new Vector2Int(-1, -1), Vector2Int.zero);
    }

    /// <summary>
    /// Checks if a word fits starting from a position in a specific direction.
    /// </summary>
    private bool DoesWordFitInDirection(int startX, int startY, string word, Vector2Int direction)
    {
        int wordLength = word.Length;
        Vector2Int boardSize = GameData.Instance.GetLevelBoardMatrixSize(GameData.UnlockedLevel);

        for (int k = 0; k < wordLength; k++)
        {
            int x = startX + k * direction.x;
            int y = startY + k * direction.y;

            // Check boundaries
            if (x < 0 || x >= boardSize.x || y < 0 || y >= boardSize.y)
                return false;

            // Check if grid letter matches the word letter (case insensitive)
            if (char.ToLower(grid[x, y]) != char.ToLower(word[k]))
                return false;
        }

        return true;
    }



    /// <summary>
    /// Highlights the specified number of letters starting from a position in a direction.
    /// </summary>
    private void HighlightLetters(Vector2Int startPosition, Vector2Int direction, int count)
    {
        Vector2Int boardSize = GameData.Instance.GetLevelBoardMatrixSize(GameData.UnlockedLevel);

        for (int i = 0; i < count; i++)
        {
            Vector2Int currentPosition = startPosition + i * direction;

            // Check boundaries
            if (currentPosition.x < 0 || currentPosition.x >= boardSize.x || currentPosition.y < 0 || currentPosition.y >= boardSize.y)
            {
                Debug.LogWarning($"Position {currentPosition} is out of bounds, stopping highlight.");
                break;
            }

            int k = currentPosition.x * boardSize.y + currentPosition.y;

            // Assuming letterObjects contains the UI elements corresponding to the grid
            if (k >= 0 && k < letterObjects.Count)
            {
                Text letterText = letterObjects[k].GetComponentInChildren<Text>();
                if (letterText != null)
                {
                    letterText.color = Color.red; // Highlight with red
                }
                else
                {
                    Debug.LogWarning($"Text component not found for letter object at index {k}");
                }
            }
            else
            {
                Debug.LogError($"Invalid index {k} for letterObjects array.");
            }
        }
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
