using UnityEditor;
using UnityEngine;
using Vuforia;

public class FixARScene : EditorWindow
{
    [MenuItem("Tools/Fix AR Scene Components")]
    public static void FixComponents()
    {
        // 1. Ensure ARCamera has VuforiaBehaviour
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            if (mainCam.GetComponent<VuforiaBehaviour>() == null)
            {
                mainCam.gameObject.AddComponent<VuforiaBehaviour>();
                Debug.Log("Added VuforiaBehaviour to Main Camera");
            }
        }
        else
        {
            Debug.LogWarning("No Main Camera found in scene!");
        }

        // 2. Fix Penguin Target
        GameObject penguinTarget = GameObject.Find("ImageTarget_Penguin");
        if (penguinTarget != null)
        {
            if (penguinTarget.GetComponent<ImageTargetBehaviour>() == null)
            {
                penguinTarget.AddComponent<ImageTargetBehaviour>();
                Debug.Log("Added ImageTargetBehaviour to ImageTarget_Penguin");
            }
        }
        else
        {
            Debug.LogWarning("Could not find ImageTarget_Penguin object!");
        }

        // 3. Fix Arctic Target
        GameObject arcticTarget = GameObject.Find("ImageTarget_Arctic");
        if (arcticTarget != null)
        {
            if (arcticTarget.GetComponent<ImageTargetBehaviour>() == null)
            {
                arcticTarget.AddComponent<ImageTargetBehaviour>();
                Debug.Log("Added ImageTargetBehaviour to ImageTarget_Arctic");
            }
        }

        // 4. Fix Desert Target
        GameObject desertTarget = GameObject.Find("ImageTarget_Desert");
        if (desertTarget != null)
        {
            if (desertTarget.GetComponent<ImageTargetBehaviour>() == null)
            {
                desertTarget.AddComponent<ImageTargetBehaviour>();
                Debug.Log("Added ImageTargetBehaviour to ImageTarget_Desert");
            }
        }

        EditorUtility.DisplayDialog("Fix Complete", "Vuforia Components have been checked and auto-repaired!\n\nPlease check Inspector for each Target and assign their Images now.", "OK");
    }
}
