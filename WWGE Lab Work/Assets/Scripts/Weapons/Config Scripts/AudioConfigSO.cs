using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Audio Config", menuName = "Weapons/Guns/Audio Config", order = 5)]
public class AudioConfigSO : ScriptableObject
{
    [Range(0, 1f)]
    public float Volume = 1f;
    public AudioClip[] FireClips; // Different audio clips for when shooting.
    public AudioClip LastBulletClip; // The last bullet in a clip.
    public AudioClip EmptyClip; // When you try to fire but have no bullets.

    public AudioClip ReloadClip; // Reloading audio.
    public AudioClip IntermediateReloadClip; // Intermediate reloading audio (Shotguns, etc).

    public void PlayerShootingClip(AudioSource audioSource, bool isLastBullet = false)
    {
        if (isLastBullet && LastBulletClip != null)
        {
            audioSource.PlayOneShot(LastBulletClip, Volume);
        } else {
            audioSource.PlayOneShot(FireClips[Random.Range(0, FireClips.Length)], Volume);
        }
    }

    public void PlayOutOfAmmoClip(AudioSource audioSource)
    {
        if (EmptyClip != null)
        {
            audioSource.PlayOneShot(EmptyClip, Volume);
        }
    }

    public void PlayIntermediateReload(AudioSource audioSource)
    {
        if (IntermediateReloadClip != null)
        {
            audioSource.PlayOneShot(IntermediateReloadClip, Volume);
        }
    }

    public void PlayReloadClip(AudioSource audioSource)
    {
        if (ReloadClip != null)
        {
            audioSource.PlayOneShot(ReloadClip, Volume);
        }
    }
}
