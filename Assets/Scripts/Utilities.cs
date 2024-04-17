using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities : MonoBehaviour
{
    public static float CalculateObstacleThickness(Vector3 start, Vector3 end, string layers = "")
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        direction.Normalize();

        // Setup the layer mask
                int layerMask = 0;
        if (!string.IsNullOrEmpty(layers))
        {
            string[] layerNames = layers.Split(',');
            foreach (string layerName in layerNames)
            {
                int layer = LayerMask.NameToLayer(layerName.Trim());
                if (layer != -1)  // Layer exists
                {
                    layerMask |= (1 << layer);
                }
                else
                {
                    Debug.LogWarning($"Layer '{layerName}' does not exist.");
                }
            }
        }
        else
        {
            layerMask = Physics.DefaultRaycastLayers;
        }

        // Perform raycast in the forward direction
        RaycastHit[] forwardHits = Physics.RaycastAll(start, direction, distance, layerMask);
        // Perform raycast in the reverse direction
        RaycastHit[] reverseHits = Physics.RaycastAll(end, -direction, distance, layerMask);

        // Combine and sort hits by distance
        List<(RaycastHit, string)> allHits = new List<(RaycastHit, string)>();
        // forwardHits.CopyTo(allHits, 0);
        // reverseHits.CopyTo(allHits, forwardHits.Length);
        foreach (var hit in forwardHits)
        {
            allHits.Add((hit, "forward"));
        }
        foreach (var hit in reverseHits)
        {
            allHits.Add((hit, "reverse"));
        }

        (RaycastHit, string)[] arr = allHits.ToArray();

        System.Array.Sort(arr, (hit1, hit2) => Vector3.Distance(start, hit1.Item1.point).CompareTo(Vector3.Distance(start, hit2.Item1.point)));

        float totalThickness = 0f;
        int enterCount = 0;
        Vector3 firstEnter = Vector3.zero;

        // Calculate cumulative thickness based on entry and exit points
        for(int i = 0; i < arr.Length; i++) {
            var item = arr[i];
            RaycastHit hit = item.Item1;
            Debug.Log(hit.point + " " + item.Item2 + " " + enterCount);
            if(item.Item2 == "forward") {
                if(enterCount == 0) {
                    firstEnter = hit.point;
                }
                enterCount++;
            } else {
                enterCount--;

                if(enterCount == 0) {
                    totalThickness += Vector3.Distance(firstEnter, hit.point);
                }
            }

        }

        return totalThickness;
    }

    public static void SelectFloor(string floor) {
        GameObject models = GameObject.Find("3D models");

        foreach(Transform child in models.transform) {
            if(child.name == floor) child.gameObject.SetActive(true);
            else child.gameObject.SetActive(false);
        }
    }

    public static void SelectFloor(int floor) {
        SelectFloor("F"+floor);
    }

    public static void print(int any) {
        Debug.Log(any);
    }
}
