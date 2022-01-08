using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    AudioSource src;
    [SerializeField] AudioClip[] clips;

    void Awake()
    {
        src = this.gameObject.GetComponent<AudioSource>();
    }

    public void PlaySound(int i, float vol)
    {
        src.volume = vol;
        src.clip = clips[i];
        src.Play();
    }
}
