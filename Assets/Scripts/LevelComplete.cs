using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelComplete : MonoBehaviour
{
    public void OnHomeButtonClick(GameObject homeButton)
    {

        Destroy(gameObject, 0.1f);
    }

    public void OnNextButtonClick(GameObject nextButton)
    {
        LetterDragController.Instance.ClearAllDrawLines();
        GameData.Instance.UnlockNewLevel();
        GameManager.instance.StartGame();
        Destroy(gameObject, 0.1f);
    }
}
