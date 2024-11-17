using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreen : MonoBehaviour
{
    [SerializeField] private Text _starCountText, _levelText;

    private void OnEnable()
    {
        _levelText.text = "Level " + GameData.UnlockedLevel.ToString();
        _starCountText.text = GameData.StarsCount.ToString();
    }

    public void OnPlayButtonClick(GameObject playButton)
    {
        GameObject gameScreen = Resources.Load<GameObject>("GameScreen");
        Instantiate(gameScreen, GameData.Instance.MainCanvas.transform);

        Destroy(gameObject);
    }
}
