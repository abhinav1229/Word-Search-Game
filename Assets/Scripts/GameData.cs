using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class GameData : MonoBehaviour
{
    public static GameData Instance; // Singleton instance

    private LevelDataList _cachedLevelDataList; // Cached level data to avoid repeated file reads

    private void Awake()
    {
        // Set up singleton pattern, ensuring a single instance persists across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Property to get the currently unlocked level from PlayerPrefs
    public static int UnlockedLevel
    {
        get
        {
#if UNITY_EDITOR
            return 1;
#else
            return PlayerPrefs.GetInt("UnlockedLevel", 1);
#endif
        }
    }

    // Property to get the current star count from PlayerPrefs
    public static int StarsCount
    {
        get { return PlayerPrefs.GetInt("StarsCount", 0); }
    }

    // Unlocks the next level and saves it in PlayerPrefs
    public void UnlockNewLevel()
    {
        PlayerPrefs.SetInt("UnlockedLevel", UnlockedLevel + 1);
        PlayerPrefs.Save();
    }

    // Updates star count in PlayerPrefs, returning a result based on the success of the operation
    public StarsUpdateResult UpdateStarsCount(int numberOfStars, bool shouldCountAdd)
    {
        if (shouldCountAdd)
        {
            PlayerPrefs.SetInt("StarsCount", StarsCount + numberOfStars);
            PlayerPrefs.Save();
            return StarsUpdateResult.Success;
        }

        // Check for sufficient stars before deduction
        if (StarsCount < numberOfStars)
        {
            return StarsUpdateResult.NotEnoughStars;
        }

        PlayerPrefs.SetInt("StarsCount", StarsCount - numberOfStars);
        PlayerPrefs.Save();
        return StarsUpdateResult.Success;
    }

    // Loads level data asynchronously; caches it after the first load
    public IEnumerator LoadLevelData(int level, System.Action<List<string>> onLevelDataLoaded)
    {
        if (_cachedLevelDataList == null)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "levels.json");
            using (UnityWebRequest request = UnityWebRequest.Get(path))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    // Cache the data to avoid reloading
                    _cachedLevelDataList = JsonUtility.FromJson<LevelDataList>(request.downloadHandler.text);
                }
                else
                {
                    Debug.LogError("Failed to load level data: " + request.error);
                    onLevelDataLoaded?.Invoke(new List<string>());
                    yield break;
                }
            }
        }

        // Find and return words for the specified level
        LevelData levelData = _cachedLevelDataList.levels.Find(l => l.levelName == level);
        if (levelData != null)
        {
            Debug.Log($"Level({level}) Data successfully loaded!");
            onLevelDataLoaded?.Invoke(levelData.words);
        }
        else
        {
            Debug.LogError($"Level({level}) data not found.");
            onLevelDataLoaded?.Invoke(new List<string>());
        }
    }

    // Determines the board size based on level; returns default size if level is 20+
    public Vector2Int GetLevelBoardMatrixSize(int level)
    {
        if (level < 5)
        {
            return new Vector2Int(5, 4);
        }
        else if (level < 20)
        {
            return new Vector2Int(8, 6);
        }
        return new Vector2Int(9, 8);
    }

    public Vector2Int GetLevelBoardCellSize(int level)
    {
        if (level < 5)
        {
            return new Vector2Int(210, 180);
        }
        else if (level < 20)
        {
            return new Vector2Int(135, 110);
        }
        return new Vector2Int(100, 100);
    }
}

// Enum to indicate results of updating star count
public enum StarsUpdateResult
{
    Success,
    NotEnoughStars
}

[System.Serializable]
public class LevelData
{
    public int levelName;           // Level identifier
    public List<string> words;       // Words associated with the level
}

[System.Serializable]
public class LevelDataList
{
    public List<LevelData> levels;   // List of all level data
}
