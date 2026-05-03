using UnityEngine;
using Vuforia;

public class AnimalTargetHandler : MonoBehaviour
{
    public string animalId;

    private ObserverBehaviour observerBehaviour;

    private void Start()
    {
        observerBehaviour = GetComponent<ObserverBehaviour>();

        if (observerBehaviour != null)
            observerBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
    }

    private void OnDestroy()
    {
        if (observerBehaviour != null)
            observerBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        bool isTracked =
            status.Status == Status.TRACKED ||
            status.Status == Status.EXTENDED_TRACKED;

        if (isTracked)
        {
            if (AnimalARManager.Instance != null)
                AnimalARManager.Instance.OnAnimalTargetFound(animalId, transform);
        }
    }
}
