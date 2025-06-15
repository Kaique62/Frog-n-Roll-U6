#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class PositionCalculator : EditorWindow
{
    // Configuration
    private Transform movingObject;    // Object that moves (e.g. player)
    private Transform targetObject;    // Object to position (e.g. obstacle)
    private float objectSpeed = 10f;  // Speed of moving object (units/sec)
    private float triggerTime = 1f;   // Time when objects should meet (seconds)
    private float xOffset = 2.4f;     // Manual position adjustment

    [MenuItem("Tools/Position Calculator")]
    public static void ShowWindow()
    {
        GetWindow<PositionCalculator>("Position Sync Tool");
    }

    void OnGUI()
    {
        GUILayout.Label("Object Synchronization", EditorStyles.boldLabel);
        
        movingObject = (Transform)EditorGUILayout.ObjectField("Moving Object", movingObject, typeof(Transform), true);
        targetObject = (Transform)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(Transform), true);
        objectSpeed = EditorGUILayout.FloatField("Moving Object Speed", objectSpeed);
        triggerTime = EditorGUILayout.FloatField("Synchronization Time", triggerTime);
        xOffset = EditorGUILayout.FloatField("X Position Offset", xOffset);

        if (GUILayout.Button("Calculate and Position"))
        {
            CalculatePosition();
        }
    }

    private void CalculatePosition()
    {
        if (movingObject == null || targetObject == null)
        {
            Debug.LogError("Assign both objects first!");
            return;
        }

        // Core calculation: Position = (Current X) + (Speed × Time) + Offset
        float newX = movingObject.position.x + (objectSpeed * triggerTime) + xOffset;
        
        // Apply position in editor (undo-able)
        Undo.RecordObject(targetObject, "Position Target Object");
        targetObject.position = new Vector3(
            newX,
            targetObject.position.y,
            targetObject.position.z
        );
        EditorUtility.SetDirty(targetObject);

        Debug.Log($"Positioned {targetObject.name} at X = {newX} " +
                 $"(Will sync at t = {triggerTime}s with speed {objectSpeed})");
    }
}
#endif