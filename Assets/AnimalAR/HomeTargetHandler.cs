using UnityEngine;
using Vuforia;

public class HomeTargetHandler : MonoBehaviour
{
    public string homeId;
    public GameObject homeVisual;
    public float maxCardVisualScale = 0.4f;
    public float fallbackCardVisualScale = 0.14f;

    private ObserverBehaviour observerBehaviour;

    private void Awake()
    {
        PrepareHomeVisualForCard();
        HideHomeVisual();
    }

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
                AnimalARManager.Instance.OnHomeTargetFound(homeId);
        }
    }

    public void ShowHomeVisual()
    {
        if (homeVisual != null)
        {
            PrepareHomeVisualForCard();
            homeVisual.SetActive(true);
        }
    }

    public void HideHomeVisual()
    {
        if (homeVisual != null)
            homeVisual.SetActive(false);
    }

    public void PrepareHomeVisualForCard()
    {
        if (homeVisual == null) return;

        homeVisual.transform.SetParent(transform, false);
        homeVisual.transform.localPosition = Vector3.zero;
        homeVisual.transform.localRotation = Quaternion.identity;
        EnsureHomeCanBeTapped();

        Vector3 scale = homeVisual.transform.localScale;
        float largestAxis = Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));

        if (largestAxis > maxCardVisualScale)
            homeVisual.transform.localScale = Vector3.one * fallbackCardVisualScale;
    }

    public void SelectHome()
    {
        if (AnimalARManager.Instance != null)
            AnimalARManager.Instance.SelectHome(homeId);
    }

    private void EnsureHomeCanBeTapped()
    {
        HomeVisualSelector selector = homeVisual.GetComponent<HomeVisualSelector>();
        if (selector == null)
            selector = homeVisual.AddComponent<HomeVisualSelector>();

        selector.owner = this;

        if (homeVisual.GetComponent<Collider>() == null)
            homeVisual.AddComponent<BoxCollider>();
    }
}
