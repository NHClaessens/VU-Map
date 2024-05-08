using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Transform))]
public class WorldPos : Editor
{
    public override void OnInspectorGUI()
    {
        Transform transform = (Transform) target;

        DrawDefaultInspector();
        
        EditorGUILayout.BeginHorizontal ();
        transform.position = EditorGUILayout.Vector3Field("World Pos", transform.position);
        //this will display the target's world pos.
        EditorGUILayout.EndHorizontal ();

    }
}
