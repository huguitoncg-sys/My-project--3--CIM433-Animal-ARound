using UnityEngine;

public class ResetButtonUI : MonoBehaviour
{
    public void ResetExperience()
    {
        if (AnimalARManager.Instance != null)
            AnimalARManager.Instance.ResetSession();
    }
}
