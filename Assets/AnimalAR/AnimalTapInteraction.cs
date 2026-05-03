using System.Collections;
using UnityEngine;

public class AnimalTapInteraction : MonoBehaviour
{
    public Animator animator;
    public float moveDistance = 0.12f;
    public float moveDuration = 0.8f;

    private bool interactionEnabled;
    private bool isMoving;

    private void Awake()
    {
        animator = FindUsableAnimator();
    }

    public void SetInteractionEnabled(bool enabled)
    {
        interactionEnabled = enabled;
    }

    private void OnMouseDown()
    {
        if (!interactionEnabled || isMoving) return;

        StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        isMoving = true;
        SetAnimalAnimation(1f, 1f);

        Vector3 startPos = transform.localPosition;
        Vector3 endPos = startPos + new Vector3(0f, 0f, moveDistance);

        float time = 0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = time / moveDuration;
            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        SetAnimalAnimation(0f, 0f);
        isMoving = false;
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
