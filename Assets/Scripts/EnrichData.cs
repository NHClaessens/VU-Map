using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class EnrichData : MonoBehaviour
{
    public string[] sampleSetNames;
    public string fileName;
    public EnrichType type;

    private List<GameObject> sampleLocations = new List<GameObject>();

    public void Start() {
        foreach(string sampleSetName in sampleSetNames) {
            GameObject parent = GameObject.Find(sampleSetName);

            if(parent == null) return;

            foreach(Transform child in parent.transform) {
                sampleLocations.Add(child.gameObject);
            }
        }
    }

    public void Enrich() {
        switch(type){
            case EnrichType.YESNO:
                yesno();
            break;
            case EnrichType.COUNT:
                count();
            break;
            case EnrichType.THICKNESS:
                thickness();
            break;
            case EnrichType.ALL:
                yesno();
                count();
                thickness();
            break;
            case EnrichType.NONE:
            break;
        }

        Save();
    }

    private Dictionary<(Vector3, Vector3), bool> yesnoCache = new Dictionary<(Vector3, Vector3), bool>();

    private void yesno() {
        foreach(GameObject loc in sampleLocations){
            PositionData data = loc.GetComponent<PositionData>();
            foreach((string key, string value) in data.rssi) {
                GameObject ap = GameObject.Find(key);

                if(ap == null) continue;

                if(yesnoCache.ContainsKey((loc.transform.position, ap.transform.position))) {
                    data.obstaclePresent.Add(key, yesnoCache.GetValueOrDefault((loc.transform.position, ap.transform.position), false));
                    continue;
                }

                Debug.Log($"Cache miss: {loc.transform.position} to {ap.transform.position}");

                bool obstacle = Utilities.ObstacleOnPath(loc.transform.position, ap.transform.position);
                yesnoCache.Add((loc.transform.position, ap.transform.position), obstacle);

                data.obstaclePresent.Add(key, obstacle);
            }
            break;
        }
    }

    private Dictionary<(Vector3, Vector3), int> countCache = new Dictionary<(Vector3, Vector3), int>();

    private void count() {
        foreach(GameObject loc in sampleLocations){
            PositionData data = loc.GetComponent<PositionData>();
            foreach((string key, string value) in data.rssi) {
                GameObject ap = GameObject.Find(key);

                if(ap == null) continue;

                if(countCache.ContainsKey((loc.transform.position, ap.transform.position))) {
                    data.obstacleCount.Add(key, countCache.GetValueOrDefault((loc.transform.position, ap.transform.position)));
                    continue;
                }

                Debug.Log($"Cache miss: {loc.transform.position} to {ap.transform.position}");

                int obstacles = Utilities.CountObstacles(loc.transform.position, ap.transform.position);
                countCache.Add((loc.transform.position, ap.transform.position), obstacles);

                data.obstacleCount.Add(key, obstacles);
            }
            break;
        }
    }

    private Dictionary<(Vector3, Vector3), float> thicknessCache = new Dictionary<(Vector3, Vector3), float>();

    private void thickness() {
        foreach(GameObject loc in sampleLocations){
            PositionData data = loc.GetComponent<PositionData>();
            foreach((string key, string value) in data.rssi) {
                GameObject ap = GameObject.Find(key);

                if(ap == null) continue;

                if(thicknessCache.ContainsKey((loc.transform.position, ap.transform.position))) {
                    data.obstacleThickness.Add(key, thicknessCache.GetValueOrDefault((loc.transform.position, ap.transform.position)));
                    continue;
                }

                Debug.Log($"Cache miss: {loc.transform.position} to {ap.transform.position}");

                float obstacles = Utilities.CalculateObstacleThickness(loc.transform.position, ap.transform.position);
                thicknessCache.Add((loc.transform.position, ap.transform.position), obstacles);

                data.obstacleThickness.Add(key, obstacles);
            }
            break;
        }
    }

    public SerializableDictionary<string, SerializableList<string>> dict = new SerializableDictionary<string, SerializableList<string>>();
    public void Save() {
        void AddToDict(string key, string value) {
            SerializableList<string> list;

            if(dict.TryGetValue(key, out list)) {
                list.Add(value);
            } else {
                dict.Add(key, new SerializableList<string>(){value});
            }
        }


        foreach(GameObject loc in sampleLocations) {
            PositionData data = loc.GetComponent<PositionData>();

            AddToDict("x", loc.transform.position.x.ToString(System.Globalization.CultureInfo.InvariantCulture));
            AddToDict("y", loc.transform.position.y.ToString(System.Globalization.CultureInfo.InvariantCulture));
            AddToDict("z", loc.transform.position.z.ToString(System.Globalization.CultureInfo.InvariantCulture));

            if(data.rssi.Count > 0) {
                foreach((string key, string value) in data.rssi) {
                    AddToDict(key, value);
                }
            }

            if(data.obstaclePresent.Count > 0) {
                foreach((string key, bool value) in data.obstaclePresent) {
                    AddToDict(key+"_obstacle_present", value ? "1" : "0");
                }
            }

            if(data.obstacleCount.Count > 0) {
                foreach((string key, int value) in data.obstacleCount) {
                    AddToDict(key+"_obstacle_count", value.ToString());
                }
            }

            if(data.obstacleThickness.Count > 0) {
                foreach((string key, float value) in data.obstacleThickness) {
                    AddToDict(key+"_obstacle_thickness", value.ToString());
                }
            }
        }

        string csv = Utilities.ConvertDictionaryToString(dict);
        string formattedDateTime = System.DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");

        Utilities.SaveToFile(csv, $"{fileName}-{type}-{formattedDateTime}.csv");
    }
}

public enum EnrichType {
    YESNO,
    COUNT,
    THICKNESS,
    ALL,
    NONE,
}
