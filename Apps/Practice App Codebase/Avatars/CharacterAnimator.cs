using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public Animator animator;
    public AudioSource audioSource;

    private void Update()
    {
        if (audioSource.isPlaying)
        {
            animator.SetBool("isSpeaking", true);
        }
        else
        {
            animator.SetBool("isSpeaking", false);
        }
    }
}
