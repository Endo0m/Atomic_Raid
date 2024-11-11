using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonFX : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip hoverFX;
    public AudioClip pressedFX;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void HoverSound()
    {
        audioSource.PlayOneShot(hoverFX);
    }

    public void PressedFX()
    {
        audioSource.PlayOneShot(pressedFX);
    }
}
