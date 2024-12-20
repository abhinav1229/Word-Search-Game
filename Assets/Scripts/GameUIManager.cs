using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameUIManager : MonoBehaviour
{
    [Header("--- Texts ---")]
    [SerializeField] private Text _levelText;
    [SerializeField] private Text _timerText;
    [SerializeField] private Text _starCountsText;

    [SerializeField] RectTransform _wordsContainer, _gamePlayPanel, _bottomPanel;

    public Coroutine _timerCoroutine;

    public static GameUIManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        _wordsContainer.localScale = Vector3.zero;
    }

    private void OnEnable()
    {
        SetUI(true);

        _gamePlayPanel.DOAnchorPos(new Vector3(0, -130f, 0), 0.3f).SetEase(Ease.InFlash);
        _wordsContainer.DOScale(Vector3.one, 0.5f).SetDelay(0.4f);
        _bottomPanel.DOAnchorPos(new Vector3(0, 160f, 0), 0.3f).SetDelay(1.0f);
    }

    public void OnHomeButtonClick(GameObject homeButton)
    {
        GameData.Instance.ObjectScaleAnimation(homeButton);
        AudioManager.Instance.PlayButtonClickSound();

        transform.DORotate(Vector3.zero, 0).SetDelay(0.5f).OnComplete(() =>
        {
            // add some delay without using invoke or coroutine(so that object scale can be done)
            GameObject gameOver = Resources.Load<GameObject>("SettingPopup");
            Instantiate(gameOver, GameData.Instance.MainScreen.transform);

            _isPaused = true;
        });

    }

    public void ResumeCouroutine()
    {
        _isPaused = false;
    }

    public void SetUI(bool isFromEnable = false)
    {
        if (!isFromEnable)
        {
            _wordsContainer.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
            {
                _wordsContainer.DOScale(Vector3.one, 0.3f);
            });
        }

        _timeRemaining = GameData.Instance.GetLevelWiseTimeRemaining(GameData.UnlockedLevel);
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

    public void SetStarsCounts()
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
