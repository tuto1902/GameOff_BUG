using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugDeath : MonoBehaviour
{
    public AudioClip explosionSound;

    private void Start()
    {
        SoundManager.Instance.PlaySound(explosionSound);
        CameraShake.Instance.ShakeCamera(3f, 0.1f);
    }

    public void SelfDestruct()
    {
        Destroy(gameObject);
    }
}
