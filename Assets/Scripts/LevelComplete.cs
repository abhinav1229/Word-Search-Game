using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelComplete : MonoBehaviour
{

    [SerializeField] private Text _starCountText;
    [SerializeField] private Text _bonusCountText;

    private void OnEnable() 
    {
        int bonusCount = GameManager.Instance.CurrentLevelWords.Count * 5;

        GameData.Instance.UpdateStarsCount(bonusCount, true);
        _starCountText.text = GameData.StarsCount.ToString();
        _bonusCountText.text = "+" + bonusCount;
    }

    public void OnHomeButtonClick(GameObject homeButton)
    {
        GameManager.Instance.DestroyThisWindow();
        HomeScreen.Instance.gameObject.SetActive(true);
        Destroy(gameObject, 0.1f);
    }

    public void OnNextButtonClick(GameObject nextButton)
    {
        GameData.Instance.UnlockNewLevel();

        LetterDragController.Instance.ClearAllDrawLines();
        LetterDragController.Instance.SetCellSize();

        GameUIManager.Instance.SetUI();

        GameManager.Instance.StartGame();
        Destroy(gameObject, 0.1f);
    }
}
