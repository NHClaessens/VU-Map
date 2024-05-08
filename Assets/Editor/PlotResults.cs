using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class DataVisualizerWindow : EditorWindow
{
    private string csvFilePath;
    private TextAsset file;
    private Dictionary<Vector3, List<Vector3>> dataDictionary = new Dictionary<Vector3, List<Vector3>>();

    [MenuItem("Tools/Result Plotter")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(DataVisualizerWindow));
    }

    void OnGUI()
    {
        GUILayout.Label("CSV File Path", EditorStyles.boldLabel);
        if(csvFilePath != "")
            GUILayout.Label(csvFilePath);
        if(GUILayout.Button("Pick file"))
            csvFilePath = EditorUtility.OpenFilePanel("Select CSV", "Assets/Resources/Data", "csv");

        if (GUILayout.Button("Visualize Data"))
        {
            LoadCSVData(csvFilePath);
            VisualizeData();
        }
    }

    void LoadCSVData(string filePath)
    {
        dataDictionary.Clear();

        if (!File.Exists(filePath))
        {
            Debug.LogError("CSV file not found at path: " + filePath);
            return;
        }

        StreamReader reader = new StreamReader(filePath);
        reader.ReadLine(); //Skip headers
        while (!reader.EndOfStream)
        {
            string[] line = reader.ReadLine().Split(',');
            if (line.Length >= 6)
            {
                float x = float.Parse(line[0], System.Globalization.CultureInfo.InvariantCulture);
                float y = float.Parse(line[1], System.Globalization.CultureInfo.InvariantCulture);
                float z = float.Parse(line[2], System.Globalization.CultureInfo.InvariantCulture);
                float predictedX = float.Parse(line[3], System.Globalization.CultureInfo.InvariantCulture);
                float predictedY = float.Parse(line[4], System.Globalization.CultureInfo.InvariantCulture);
                float predictedZ = float.Parse(line[5], System.Globalization.CultureInfo.InvariantCulture);

                Vector3 realLocation = new Vector3(x, y, z);
                Vector3 predictedLocation = new Vector3(predictedX, predictedY, predictedZ);

                if (!dataDictionary.ContainsKey(realLocation))
                {
                    dataDictionary.Add(realLocation, new List<Vector3>());
                }
                dataDictionary[realLocation].Add(predictedLocation);
            }
        }
        reader.Close();
    }

    void VisualizeData()
    {
        GameObject parent = GameObject.Find("Results");

        if(parent) {
            List<Transform> children = new List<Transform>();
            foreach(Transform child in parent.transform)
                children.Add(child);
            
            foreach(Transform child in children)
                DestroyImmediate(child.gameObject);
        } else {
            parent = new GameObject("Results");
        }

        foreach (KeyValuePair<Vector3, List<Vector3>> entry in dataDictionary)
        {
            Vector3 realLocation = entry.Key;
            List<Vector3> predictedLocations = entry.Value;

            Material pointMaterial = new Material(Shader.Find("Standard"));
            pointMaterial.color = GetRandomColor();

            // Create a point for the real location
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.transform.position = realLocation;
            point.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            point.GetComponent<Renderer>().sharedMaterial = pointMaterial;
            point.transform.parent = parent.transform;

            Material material = new Material(Shader.Find("Standard"));

            // Create a larger sphere for the predicted locations
            GameObject largerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            largerSphere.transform.position = point.transform.position;
            largerSphere.transform.localScale = Vector3.one;
            largerSphere.GetComponent<Renderer>().sharedMaterial = material;
            Color parentColor = point.GetComponent<Renderer>().sharedMaterial.color;
            parentColor.a = 0.5f;
            largerSphere.GetComponent<Renderer>().sharedMaterial.color = parentColor;
            largerSphere.GetComponent<Renderer>().sharedMaterial.SetFloat("_Mode", 3); // Set transparency mode
            largerSphere.GetComponent<Renderer>().sharedMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            largerSphere.GetComponent<Renderer>().sharedMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            largerSphere.GetComponent<Renderer>().sharedMaterial.SetInt("_ZWrite", 0);
            largerSphere.GetComponent<Renderer>().sharedMaterial.DisableKeyword("_ALPHATEST_ON");
            largerSphere.GetComponent<Renderer>().sharedMaterial.EnableKeyword("_ALPHABLEND_ON");
            largerSphere.GetComponent<Renderer>().sharedMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            largerSphere.GetComponent<Renderer>().sharedMaterial.renderQueue = 3000;
            largerSphere.transform.parent = point.transform;

            foreach(Vector3 pred in predictedLocations) {
                GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                p.transform.position = pred;
                p.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                p.GetComponent<Renderer>().sharedMaterial.color = parentColor;
                p.transform.parent = point.transform;

                Debug.Log($"Dist {pred} = {Vector3.Distance(realLocation, pred)}");
            }

            // Adjust the size of the larger sphere to contain all predicted points
            float maxDistance = predictedLocations.Max(loc => Vector3.Distance(loc, realLocation)) * 2 * (1 / point.transform.localScale.x);
            Debug.Log("MAX DIST " + maxDistance);
            largerSphere.transform.localScale = new Vector3(maxDistance, 0.01f, maxDistance);

        }
    }

    Vector3 GetCenter(List<Vector3> points)
    {
        Vector3 center = Vector3.zero;
        foreach (Vector3 point in points)
        {
            center += point;
        }
        return center / points.Count;
    }

    Color GetRandomColor()
    {
        float h = Random.Range(0f, 1f);
        float s = Random.Range(0.5f, 1f);
        float v = Random.Range(0.5f, 1f);
        return Color.HSVToRGB(h, s, v);
    }
}
