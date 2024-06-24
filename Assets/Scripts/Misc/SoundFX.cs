using UnityEngine;

public class SoundFX : MonoBehaviour
{

    [SerializeField]
    private AudioClip buttonSound;
    private AudioSource audioSrc;


    private void Start()
    {
        audioSrc = GetComponent<AudioSource>();
    }
    public void SoundOnClick()
    {
        audioSrc.PlayOneShot(buttonSound);
    }
}

   