using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingWindow : MonoBehaviour
{
    public void OnPlayButtonClick(GameObject button)
    {
        Destroy(gameObject, 0.1f);
    }
    public void OnHomeButtonClick(GameObject button)
    {
        GameManager.Instance.DestroyThisWindow();
        HomeScreen.Instance.gameObject.SetActive(true);
        Destroy(gameObject);
    }
    public void OnSoundButtonClick(GameObject button)
    {
        //add code...
    }
}
