using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("--- Texts ---")]
    [SerializeField] private Text _levelText;
    [SerializeField] private Text _timerText;
    [SerializeField] private Text _starCountsText;

    private void OnEnable()
    {
        SetCurrentLevel();
        SetStarsCounts();
        StartTimer();
    }

    private void SetCurrentLevel()
    {
        _levelText.text = "Level " + GameData.UnlockedLevel.ToString();
    }

    private void SetStarsCounts()
    {
        _starCountsText.text = GameData.StarsCount.ToString();
    }

    private void StartTimer()
    {
        _timerText.text = "0s";
    }
}
