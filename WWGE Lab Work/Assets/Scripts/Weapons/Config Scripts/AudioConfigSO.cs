using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Configuration Information & Logic for a Weapon's Audio.</summary>
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


    // Play the firing AudioClip.
    public void PlayerShootingClip(AudioSource audioSource, bool isLastBullet = false)
    {
        // if this is the last bullet (And we have a special clip for that), play the last bullet clip.
        if (isLastBullet && LastBulletClip != null)
        {
            audioSource.PlayOneShot(LastBulletClip, Volume);
        }
        // Otherwise, play a random fire clip.
        else
        {
            audioSource.PlayOneShot(FireClips[Random.Range(0, FireClips.Length)], Volume);
        }
    }

    // If we have an AudioClip to play when we try to fire with no ammo, play it.
    public void PlayOutOfAmmoClip(AudioSource audioSource)
    {
        if (EmptyClip != null)
        {
            audioSource.PlayOneShot(EmptyClip, Volume);
        }
    }

    // Play intermediate reload clips (E.g. For loading individual bullets).
    public void PlayIntermediateReload(AudioSource audioSource)
    {
        if (IntermediateReloadClip != null)
        {
            audioSource.PlayOneShot(IntermediateReloadClip, Volume);
        }
    }

    // If we have a reload clip, play it.
    public void PlayReloadClip(AudioSource audioSource)
    {
        if (ReloadClip != null)
        {
            audioSource.PlayOneShot(ReloadClip, Volume);
        }
    }
}
