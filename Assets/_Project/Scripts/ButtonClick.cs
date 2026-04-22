using UnityEngine;
using TMPro;

public class ButtonClick : MonoBehaviour
{
    public AudioSource clickSound;
    public AudioClip sound;

    public void buttonSound()
    {
        clickSound.PlayOneShot(sound);
    }
}
