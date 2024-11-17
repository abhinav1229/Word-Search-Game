using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource _audioSource;

    [SerializeField] private AudioClip _buttonClick;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        _audioSource = gameObject.GetComponent<AudioSource>();
    }

    public void PlayButtonClickSound()
    {
        StartCoroutine(PlaySound(_buttonClick));
    }

    public IEnumerator PlaySound(AudioClip clip, float delay = 0)
    {
        yield return new WaitForSeconds(delay);

        if (GameData.IsSoundEnabled == 1)
        {
            if (clip != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }
    }
}
