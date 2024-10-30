using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance;

    private void Awake() {
        if(Instance == null)
        {
            Instance = this;
        }
    }


    public static int UnlockedLevel
    {
        get 
        {
            return PlayerPrefs.GetInt("UnlockedLevel", 0);
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
        if(shouldCountAdd)
        {
            PlayerPrefs.SetInt("StarsCount", StarsCount + numberOfStars);
            return 1;
        }

        if(StarsCount < numberOfStars)
        {
            return -1;
        }

        PlayerPrefs.SetInt("StarsCount", StarsCount - numberOfStars);
        return 1;
    }
}
