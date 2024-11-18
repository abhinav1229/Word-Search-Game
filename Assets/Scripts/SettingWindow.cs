using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SettingWindow : MonoBehaviour
{

    private void OnEnable()
    {
        transform.Find("BackgroundImage").GetComponent<RectTransform>().DOAnchorPos3D(Vector3.zero, 0.2f).SetEase(Ease.Flash);
    }

    public void OnPlayButtonClick(GameObject button)
    {
        GameData.Instance.ObjectScaleAnimation(button);
        AudioManager.Instance.PlayButtonClickSound();

        transform.DORotate(Vector3.zero, 0).SetDelay(0.5f).OnComplete(() =>
        {
            GameUIManager.Instance.ResumeCouroutine();
            transform.Find("BackgroundImage").GetComponent<RectTransform>().DOAnchorPos3D(new Vector3(1100, 0, 0), 0.2f).SetEase(Ease.Flash).OnComplete(() =>
            {
                Destroy(gameObject);
            });
        });
    }
    public void OnHomeButtonClick(GameObject button)
    {
        GameData.Instance.ObjectScaleAnimation(button);
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
    public void OnSoundButtonClick(GameObject button)
    {
        GameData.Instance.ObjectScaleAnimation(button);
        AudioManager.Instance.PlayButtonClickSound();
        GameData.Instance.SetSoundStatus();
    }
}
