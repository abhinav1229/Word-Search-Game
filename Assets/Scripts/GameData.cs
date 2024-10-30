using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }


    public static int UnlockedLevel
    {
        get
        {
            return PlayerPrefs.GetInt("UnlockedLevel", 1);
        }
    }

    public static int StarsCount
    {
        get
        {
            return PlayerPrefs.GetInt("StarsCount", 0);
        }
    }

    public void UnlockNewLevel()
    {
        PlayerPrefs.SetInt("UnlockedLevel", UnlockedLevel + 1);
    }

    public int UpdateStarsCount(int numberOfStars, bool shouldCountAdd)
    {
        if (shouldCountAdd)
        {
            PlayerPrefs.SetInt("StarsCount", StarsCount + numberOfStars);
            return 1;
        }

        if (StarsCount < numberOfStars)
        {
            return -1;
        }

        PlayerPrefs.SetInt("StarsCount", StarsCount - numberOfStars);
        return 1;
    }

    public List<LevelData> levels;
    public List<string> GetLevelData(int level)
    {
        if(level <= 0 || level > UnlockedLevel)
        {
            Debug.LogError($"Trying to get invalid level({level}) data.");
            return new List<string>(){};
        }

        string path = Application.streamingAssetsPath + "/levels.json";
        string json = System.IO.File.ReadAllText(path);

        // Parse JSON into a LevelDataList object
        LevelDataList levelDataList = JsonUtility.FromJson<LevelDataList>(json);
        
        // Access levels
        levels = levelDataList.levels;

        foreach (LevelData levelData in levels)
        {
            if(levelData.levelName == level)
            {
                Debug.Log($"Level({level}) Data is successfully loaded!");
                return levelData.words;
            }
        }

        Debug.LogError($"Something went wrong to return the level({level}) data.");
        return new List<string>(){};
    }
}


[System.Serializable]
public class LevelData
{
    public int levelName;
    public List<string> words;
}

[System.Serializable]
public class LevelDataList
{
    public List<LevelData> levels;
}