using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HomeScreen : MonoBehaviour
{
    [SerializeField] private Text _starCountText, _levelText;
    [SerializeField] RectTransform _resetWindow;

    public static HomeScreen Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        _levelText.text = "Level " + GameData.UnlockedLevel.ToString();
        _starCountText.text = GameData.StarsCount.ToString();
    }

    public void OnPlayButtonClick(GameObject playButton)
    {
        GameObject gameScreen = Resources.Load<GameObject>("GameScreen");
        Instantiate(gameScreen, GameData.Instance.MainCanvas.transform);

        gameObject.SetActive(false);
    }

    public void OnResetGameButtonClick(GameObject resetButton)
    {
        _resetWindow.DOAnchorPos(new Vector3(0, 300f, 0), 0.3f).SetEase(Ease.InFlash);
    }

    public void OnYesButtonClick(GameObject yesButton)
    {
        PlayerPrefs.DeleteAll();
        _levelText.text = "Level " + GameData.UnlockedLevel.ToString();
        _starCountText.text = GameData.StarsCount.ToString();

        _resetWindow.DOAnchorPos(new Vector3(0, -700f, 0), 0.5f).SetEase(Ease.InFlash);
    }

    public void OnNoButtonClick(GameObject noButton)
    {
        _resetWindow.DOAnchorPos(new Vector3(0, -700f, 0), 0.5f).SetEase(Ease.InFlash);
    }
}
