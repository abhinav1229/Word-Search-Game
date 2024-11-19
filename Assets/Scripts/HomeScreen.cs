using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Networking;

public class HomeScreen : MonoBehaviour
{
    [SerializeField] private Text _starCountText, _levelText;
    [SerializeField] RectTransform _resetWindow, _developerWindow;

    public static HomeScreen Instance;

    private void Awake()
    {
        if (Instance == null)
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
        GameData.Instance.ObjectScaleAnimation(playButton);
        _resetWindow.DOAnchorPos(new Vector3(0, -700f, 0), 0.5f).SetEase(Ease.InFlash);
        _developerWindow.DOAnchorPos(new Vector3(0, -1000f, 0), 0.5f).SetEase(Ease.InFlash);
        AudioManager.Instance.PlayButtonClickSound();

        transform.DOScale(Vector3.one, 0f).SetDelay(0.5f).OnComplete(() =>
        {
            GameObject gameScreen = Resources.Load<GameObject>("GameScreen");
            Instantiate(gameScreen, GameData.Instance.MainScreen.transform);

            gameObject.SetActive(false);
        });

    }

    public void OnResetGameButtonClick(GameObject resetButton)
    {
        GameData.Instance.ObjectScaleAnimation(resetButton);
        AudioManager.Instance.PlayButtonClickSound();

        transform.DORotate(Vector3.zero, 0).SetDelay(0.5f).OnComplete(() =>
        {
            _resetWindow.DOAnchorPos(new Vector3(0, 300f, 0), 0.3f).SetEase(Ease.InFlash);
        });

    }

    public void OnYesButtonClick(GameObject yesButton)
    {
        GameData.Instance.ObjectScaleAnimation(yesButton);
        AudioManager.Instance.PlayButtonClickSound();
        PlayerPrefs.DeleteAll();

        transform.DORotate(Vector3.zero, 0).SetDelay(0.5f).OnComplete(() =>
        {
            _levelText.text = "Level " + GameData.UnlockedLevel.ToString();
            _starCountText.text = GameData.StarsCount.ToString();

            _resetWindow.DOAnchorPos(new Vector3(0, -700f, 0), 0.5f).SetEase(Ease.InFlash);
        });
    }

    public void OnNoButtonClick(GameObject noButton)
    {
        GameData.Instance.ObjectScaleAnimation(noButton);
        AudioManager.Instance.PlayButtonClickSound();
        transform.DORotate(Vector3.zero, 0).SetDelay(0.5f).OnComplete(() =>
        {
            _resetWindow.DOAnchorPos(new Vector3(0, -700f, 0), 0.5f).SetEase(Ease.InFlash);
        });
    }

    public void OnDeveloperButtonClick(GameObject devButton)
    {
        GameData.Instance.ObjectScaleAnimation(devButton);
        AudioManager.Instance.PlayButtonClickSound();
        transform.DORotate(Vector3.zero, 0).SetDelay(0.5f).OnComplete(() =>
        {
            _developerWindow.DOAnchorPos(new Vector3(0, 420f, 0), 0.5f).SetEase(Ease.InFlash);
        });
    }

    public void OnDeveloperCloseButtonClick(GameObject devCloseButton)
    {
        GameData.Instance.ObjectScaleAnimation(devCloseButton);
        AudioManager.Instance.PlayButtonClickSound();
        transform.DORotate(Vector3.zero, 0).SetDelay(0.5f).OnComplete(() =>
        {
            _developerWindow.DOAnchorPos(new Vector3(0, -1000f, 0), 0.5f).SetEase(Ease.InFlash);
        });
    }


    public void OnMailButtonClick(GameObject mailButton)
    {
        GameData.Instance.ObjectScaleAnimation(mailButton);
        AudioManager.Instance.PlayButtonClickSound();
        transform.DORotate(Vector3.zero, 0).SetDelay(0.5f).OnComplete(() =>
        {
            // Replace with your desired email address
            string email = "abhinavrajat1229@gmail.com";
            string subject = "Hi Abhinav, Happy to connect with you.";
            string body = "";


            // Construct the mailto URL
            string mailto = "mailto:" + email + "?subject=" + UnityWebRequest.EscapeURL(subject) + "&body=" + UnityWebRequest.EscapeURL(body);

            Application.OpenURL(mailto);
        });
    }
}
