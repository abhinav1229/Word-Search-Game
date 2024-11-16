using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    public void OnHomeButtonClick(GameObject homeButton)
    {
        Destroy(gameObject, 0.1f);
    }

    public void OnRetryButtonClick(GameObject nextButton)
    {
        LetterDragController.Instance.ClearAllDrawLines();
        LetterDragController.Instance.SetCellSize();

        GameUIManager.Instance.SetUI();

        GameManager.Instance.StartGame();
        Destroy(gameObject, 0.1f);
    }
}
