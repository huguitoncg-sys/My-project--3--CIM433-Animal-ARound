using UnityEngine;

public class HomeVisualSelector : MonoBehaviour
{
    public HomeTargetHandler owner;

    private void OnMouseDown()
    {
        if (owner != null)
            owner.SelectHome();
    }
}
