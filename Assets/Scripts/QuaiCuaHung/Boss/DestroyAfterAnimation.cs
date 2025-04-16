using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    public float destroyDelay = 0.5f;
    public AudioClip explosionSound;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim != null)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            destroyDelay = stateInfo.length;
        }

        AudioSource audioSource = GetComponent<AudioSource>();
        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        Destroy(gameObject, destroyDelay);
    }
}