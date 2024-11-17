using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GameOver : MonoBehaviour
{

    private void OnEnable()
    {
        transform.Find("BackgroundImage").GetComponent<RectTransform>().DOAnchorPos3D(Vector3.zero, 0.2f).SetEase(Ease.Flash);
    }

    public void OnHomeButtonClick(GameObject homeButton)
    {
        GameManager.Instance.DestroyThisWindow();
        HomeScreen.Instance.gameObject.SetActive(true);
        transform.Find("BackgroundImage").GetComponent<RectTransform>().DOAnchorPos3D(new Vector3(1100, 0, 0), 0.2f).SetEase(Ease.Flash).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    public void OnRetryButtonClick(GameObject nextButton)
    {
        LetterDragController.Instance.ClearAllDrawLines();
        LetterDragController.Instance.SetCellSize();

        GameUIManager.Instance.SetUI();

        GameManager.Instance.StartGame();
        transform.Find("BackgroundImage").GetComponent<RectTransform>().DOAnchorPos3D(new Vector3(1100, 0, 0), 0.1f).SetEase(Ease.Flash).OnComplete(() =>
        {
            Destroy(gameObject, 0.1f);
        });
    }
}
