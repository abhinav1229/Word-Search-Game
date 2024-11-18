using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelComplete : MonoBehaviour
{

    [SerializeField] private Text _starCountText;
    [SerializeField] private Text _bonusCountText;

    [SerializeField] private AudioClip _levelCompleteClip;

    private void OnEnable()
    {
        StartCoroutine(AudioManager.Instance.PlaySound(_levelCompleteClip));

        transform.Find("BackgroundImage").GetComponent<RectTransform>().DOAnchorPos3D(Vector3.zero, 0.2f).SetEase(Ease.Flash);

        int bonusCount = GameManager.Instance.CurrentLevelWords.Count * 5;

        GameData.Instance.UpdateStarsCount(bonusCount, true);
        _starCountText.text = GameData.StarsCount.ToString();
        _bonusCountText.text = "+" + bonusCount;

        GameData.Instance.UnlockNewLevel();
    }

    public void OnHomeButtonClick(GameObject homeButton)
    {
        GameData.Instance.ObjectScaleAnimation(homeButton);
        AudioManager.Instance.PlayButtonClickSound();

        transform.DORotate(Vector3.zero, 0).SetDelay(0.5f).OnComplete(() =>
        {
            GameManager.Instance.DestroyThisWindow();
            HomeScreen.Instance.gameObject.SetActive(true);
            transform.Find("BackgroundImage").GetComponent<RectTransform>().DOAnchorPos3D(new Vector3(1100, 0, 0), 0.2f).SetEase(Ease.Flash).OnComplete(() =>
            {
                Destroy(gameObject);
            });
        });
    }

    public void OnNextButtonClick(GameObject nextButton)
    {
        GameData.Instance.ObjectScaleAnimation(nextButton);
        AudioManager.Instance.PlayButtonClickSound();

        LetterDragController.Instance.ClearAllDrawLines();
        LetterDragController.Instance.SetCellSize();

        transform.DORotate(Vector3.zero, 0).SetDelay(0.5f).OnComplete(() =>
        {
            GameUIManager.Instance.SetUI();
            GameManager.Instance.StartGame();
            transform.Find("BackgroundImage").GetComponent<RectTransform>().DOAnchorPos3D(new Vector3(1100, 0, 0), 0.2f).SetEase(Ease.Flash).OnComplete(() =>
            {
                Destroy(gameObject);
            });
        });
    }
}
