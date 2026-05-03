using System.Collections;
using UnityEngine;

public class AnimalReaction : MonoBehaviour
{
    public Animator animator;
    public float runFeedbackSeconds = 1f;

    private Coroutine reactionRoutine;

    private void Awake()
    {
        animator = FindUsableAnimator();
    }

    public void PlayWrongReaction()
    {
        if (reactionRoutine != null)
            StopCoroutine(reactionRoutine);

        reactionRoutine = StartCoroutine(RunThenIdle());
    }

    private IEnumerator RunThenIdle()
    {
        SetAnimalAnimation(1f, 1f);
        yield return new WaitForSeconds(runFeedbackSeconds);
        SetAnimalAnimation(0f, 0f);
        reactionRoutine = null;
    }

    private Animator FindUsableAnimator()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
            return animator;

        Animator[] animators = GetComponentsInChildren<Animator>(true);

        foreach (Animator childAnimator in animators)
        {
            if (childAnimator.runtimeAnimatorController != null)
                return childAnimator;
        }

        return animator != null ? animator : GetComponentInChildren<Animator>(true);
    }

    private void SetAnimalAnimation(float vert, float state)
    {
        if (animator == null) return;

        if (HasParameter("Vert"))
            animator.SetFloat("Vert", vert);

        if (HasParameter("State"))
            animator.SetFloat("State", state);
    }

    private bool HasParameter(string parameterName)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName)
                return true;
        }

        return false;
    }
}
