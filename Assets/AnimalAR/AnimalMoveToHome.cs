using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalMoveToHome : MonoBehaviour
{
    public Animator animator;
    public float moveSpeed = 0.12f;
    public float turnSpeed = 540f;
    public float modelForwardOffset = 0f;

    private Coroutine moveRoutine;

    private void Awake()
    {
        animator = FindUsableAnimator();
        PlayIdle();
    }

    public void FollowPath(IReadOnlyList<Vector3> worldPathPoints, Action onArrived)
    {
        if (worldPathPoints == null || worldPathPoints.Count == 0) return;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(FollowPathRoutine(worldPathPoints, onArrived));
    }

    private IEnumerator FollowPathRoutine(IReadOnlyList<Vector3> worldPathPoints, Action onArrived)
    {
        PlayWalk();

        for (int i = 0; i < worldPathPoints.Count; i++)
        {
            Vector3 target = worldPathPoints[i];

            while (Vector3.Distance(transform.position, target) > 0.005f)
            {
                TurnToward(target);

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    moveSpeed * Time.deltaTime
                );

                yield return null;
            }

            transform.position = target;
        }

        PlayIdle();
        moveRoutine = null;
        onArrived?.Invoke();
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

    private void PlayIdle()
    {
        SetAnimalAnimation(0f, 0f);
    }

    private void PlayWalk()
    {
        SetAnimalAnimation(1f, 0f);
    }

    private void TurnToward(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        targetRotation *= Quaternion.Euler(0f, modelForwardOffset, 0f);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
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
