using UnityEngine;
using UnityEditor;

public class TeleportToCamera
{
    [MenuItem("Tools/Teleport Object to Camera %t")] // Ctrl+T or Cmd+T
    static void TeleportObjectToCamera()
    {
        // Get the currently selected GameObject
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            Debug.LogWarning("No GameObject selected. Please select an object to teleport.");
            return;
        }

        // Get the Scene View camera
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null)
        {
            Debug.LogWarning("No active Scene View found.");
            return;
        }

        // Get the camera's position
        Vector3 cameraPosition = sceneView.camera.transform.position;

        // Optional: Offset the object slightly in front of the camera
        Vector3 offset = sceneView.camera.transform.forward * 2f; // 2 units in front
        Vector3 targetPosition = cameraPosition + offset;

        // Teleport the selected object to the target position
        Undo.RecordObject(selectedObject.transform, "Teleport Object to Camera"); // Support undo
        selectedObject.transform.position = targetPosition;

        Debug.Log($"Teleported {selectedObject.name} to {targetPosition}");
    }
}