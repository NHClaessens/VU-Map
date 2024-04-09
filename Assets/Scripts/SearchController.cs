using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SearchController : MonoBehaviour
{
    public bool ignoreCapitals;
    public bool ignoreWhiteSpace;
    public bool sortByDistance;
    public bool moveCamera;
    public CameraController cameraController;
    public List<GameObject> search(string query, Transform poiController) {
        query = clean(query);

        //TODO: only show results from current floor on map, show on floor selector how many results for each floor

        

        List<GameObject> results = new List<GameObject>();

        foreach(Transform child in poiController) {
            POI poi = child.GetComponent<POI>();
            if(clean(poi.title).Contains(clean(query))) {
                results.Add(child.gameObject);
                continue;
            }
            foreach(string tag in poi.tags) {
                if(clean(tag).Contains(clean(query))) {
                    results.Add(poi.gameObject);
                    break;
                }
            }
        }

        if(sortByDistance) {
            results.Sort();
        }

        return results;
    }

    private string clean(string s) {
        s = s.Trim();
        if(ignoreCapitals) {
            s = s.ToLower();
        }

        if(ignoreWhiteSpace) {
            s = string.Concat(s.Where(c => !char.IsWhiteSpace(c)));
        }

        return s;
    }
}
