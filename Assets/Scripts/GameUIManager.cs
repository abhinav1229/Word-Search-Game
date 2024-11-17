using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("--- Texts ---")]
    [SerializeField] private Text _levelText;
    [SerializeField] private Text _timerText;
    [SerializeField] private Text _starCountsText;

    public Coroutine _timerCoroutine;

    public static GameUIManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        SetUI();
    }

    public void OnHomeButtonClick(GameObject homeButton)
    {
        AudioManager.Instance.PlayButtonClickSound();
        GameObject gameOver = Resources.Load<GameObject>("SettingPopup");
        Instantiate(gameOver, GameData.Instance.MainScreen.transform);

        _isPaused = true;
    }

    public void ResumeCouroutine()
    {
        _isPaused = false;
    }

    public void SetUI()
    {
        _timeRemaining = 20f;
        _isRunning = true;

        SetCurrentLevel();
        SetStarsCounts();

        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
        }

        _timerCoroutine = StartCoroutine(StartTimer());
    }

    private void SetCurrentLevel()
    {
        _levelText.text = "Level " + GameData.UnlockedLevel.ToString();
    }

    private void SetStarsCounts()
    {
        _starCountsText.text = GameData.StarsCount.ToString();
    }


    private float _timeRemaining = 160f; // Timer starts from 160 seconds
    private bool _isRunning = true, _isPaused = false;
    public IEnumerator StartTimer()
    {
        while (_timeRemaining > 0 && _isRunning)
        {
            if (!_isPaused)
            {
                _timeRemaining -= Time.deltaTime; // Decrease time by deltaTime
            }

            int seconds = Mathf.CeilToInt(_timeRemaining); // Convert to integer
            _timerText.text = $"{seconds}s"; // Update the UI text
            yield return null; // Wait for the next frame
        }

        _timeRemaining = 0; // Ensure time is exactly 0 when done
        _timerText.text = "0s";

        yield return new WaitForSeconds(1f);
        TimerEnded(); // Call any functionality for when the timer ends;
    }

    private void TimerEnded()
    {
        Debug.Log("Timer has finished!");
        _isRunning = false; // Stop the timer

        GameManager.Instance.ShowGameOverPopup();
    }
}
