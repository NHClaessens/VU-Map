using UnityEditor;

[CustomEditor(typeof(Configurator))]
public class ConfiguratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();

        Configurator tool = (Configurator) target;

        tool.samplingMethod = (SamplingMethod) EditorGUILayout.EnumPopup("Sampling Method", tool.samplingMethod);

        switch(tool.samplingMethod) {
            case SamplingMethod.Random:
                tool.sampleAmount = EditorGUILayout.IntField("Sample amount", tool.sampleAmount);
                break;
            case SamplingMethod.Grid:
                tool.gridSize = EditorGUILayout.FloatField("Grid size", tool.gridSize);
                break;
        }
    }
}
