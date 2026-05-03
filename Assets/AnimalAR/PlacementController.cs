using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

public class PlacementController : MonoBehaviour
{
    public float markerSize = 0.018f;

    private readonly List<Vector3> pathPoints = new();
    private readonly List<GameObject> waypointMarkers = new();

    private Camera arCamera;
    private GameObject currentAnimal;
    private Transform currentHome;
    private Plane pathPlane;
    private bool canBuildPath;

    private void Awake()
    {
        arCamera = Camera.main;
    }

    private void Update()
    {
        if (!canBuildPath) return;
        if (IsPointerOverUi()) return;

        if (TryGetTapPosition(out Vector2 screenPosition))
            TryAddWaypoint(screenPosition);
    }

    public void BeginPathBuilding(GameObject animal, Transform home)
    {
        ClearPath();

        currentAnimal = animal;
        currentHome = home;
        canBuildPath = currentAnimal != null;

        if (canBuildPath)
            pathPlane = new Plane(Vector3.up, currentAnimal.transform.position);
    }

    public void StopPathBuilding()
    {
        canBuildPath = false;
    }

    public void ClearPath()
    {
        pathPoints.Clear();

        foreach (GameObject marker in waypointMarkers)
        {
            if (marker != null)
                Destroy(marker);
        }

        waypointMarkers.Clear();
    }

    public void SendAnimalHome(AnimalMoveToHome mover)
    {
        if (mover == null) return;

        if (currentHome != null)
        {
            Vector3 homePoint = currentHome.position;
            homePoint.y = currentAnimal.transform.position.y;

            if (pathPoints.Count == 0 || Vector3.Distance(pathPoints[pathPoints.Count - 1], homePoint) > 0.01f)
                pathPoints.Add(homePoint);
        }

        if (pathPoints.Count == 0) return;

        StopPathBuilding();
        mover.FollowPath(pathPoints, OnAnimalArrived);
    }

    private void OnAnimalArrived()
    {
        if (AnimalARManager.Instance != null)
            AnimalARManager.Instance.OnAnimalArrivedHome();
    }

    private bool TryGetTapPosition(out Vector2 screenPosition)
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPosition = Mouse.current.position.ReadValue();
            return true;
        }

        if (Touchscreen.current != null)
        {
            foreach (TouchControl touch in Touchscreen.current.touches)
            {
                if (touch.press.wasPressedThisFrame)
                {
                    screenPosition = touch.position.ReadValue();
                    return true;
                }
            }
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetMouseButtonDown(0))
        {
            screenPosition = Input.mousePosition;
            return true;
        }
#endif

        screenPosition = Vector2.zero;
        return false;
    }

    private bool IsPointerOverUi()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private void TryAddWaypoint(Vector2 screenPosition)
    {
        if (arCamera == null)
            arCamera = Camera.main;

        if (arCamera == null || currentAnimal == null) return;

        Ray ray = arCamera.ScreenPointToRay(screenPosition);

        if (!pathPlane.Raycast(ray, out float distance)) return;

        Vector3 worldPoint = ray.GetPoint(distance);
        worldPoint.y = currentAnimal.transform.position.y;
        pathPoints.Add(worldPoint);

        CreateMarker(worldPoint);
    }

    private void CreateMarker(Vector3 worldPoint)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = "WaypointMarker";
        marker.transform.position = worldPoint;
        marker.transform.localScale = Vector3.one * markerSize;

        Collider markerCollider = marker.GetComponent<Collider>();
        if (markerCollider != null)
            Destroy(markerCollider);

        waypointMarkers.Add(marker);
    }
}
