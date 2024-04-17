using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class Sampler : MonoBehaviour
{
    public WifiManager wifiManager;
    public SamplingMethod samplingMethod;
    public int randomSampleAmount;
    public int wifiSampleAmount;
    public float gridSize = 1f;
    public float[] floorHeights;
    public Sample[] results;
    public GameObject sampleUI;
    public CameraController cameraController;


    private Bounds bounds;
    private Vector3 currentPosition;


    public List<Vector3> samplePoints = new List<Vector3>();
    public List<Vector3> missedPoints = new List<Vector3>();

    private GameObject cannot;
    private GameObject start;
    public GameObject indicator;
    public TMP_Text location;
    public TMP_Text progress;
    private int coveredLocations = 0;
    private int totalLocations = 0;

    public void Start() {
        start = sampleUI.transform.Find("Panel/start").gameObject;
        start.GetComponent<Button>().onClick.AddListener(takeMeasurement);

        cannot = sampleUI.transform.Find("Panel/cannot").gameObject;
        cannot.GetComponent<Button>().onClick.AddListener(cannotReach);

        location = sampleUI.transform.Find("Panel/location").GetComponent<TMP_Text>();
        progress = sampleUI.transform.Find("Panel/progress").GetComponent<TMP_Text>();

    }

    private void print(string s) {
        Debug.Log("Sampler: " + s);
    }
    public void StartSampling() {
        print("Start sampling");
        wifiManager.scanComplete.AddListener(onScanComplete);

        bounds = new Bounds(transform.position, Vector3.zero);
        Renderer[] renderers = GameObject.Find("3D models").GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        generateSampleLocations();
        totalLocations = samplePoints.Count;

        preprareMeasurement(samplePoints[0]);
    }

    private void preprareMeasurement(Vector3 pos) {
        cameraController.moveTo(pos, 10, 0.5f);
        sampleUI.SetActive(true);
        indicator.SetActive(true);
        indicator.transform.position = new Vector3(pos.x, 990, pos.z);
        currentPosition = pos;

        coveredLocations++;

        location.text = "Location" + coveredLocations + "/" + totalLocations;
        progress.text = "Sample 0/" + wifiSampleAmount;
    }

    private void cannotReach() {
        samplePoints.RemoveAt(0);
        preprareMeasurement(samplePoints[0]);
    }

    public void OnDrawGizmos() {



        foreach(Vector3 pos in samplePoints) {
            Gizmos.DrawSphere(pos, 1);
        }
        
        Gizmos.color = Color.red;

        foreach(Vector3 pos in missedPoints) {
            Gizmos.DrawSphere(pos, 1);
        }

        Gizmos.color = Color.green; // Set the color of the Gizmos
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }

    private void generateSampleLocations(){
        switch(samplingMethod){
            case SamplingMethod.Grid:
                sampleGrid();
                break;
            case SamplingMethod.Random:
                sampleRandom();
                break;
        }
    }

    public List<string> intermediate = new List<string>();

    public void takeMeasurement() {

        wifiManager.startScan();
    }

    public void onScanComplete(string result) {
        print("Scan complete: " + result);

        intermediate.Add(result);

        sampleUI.transform.Find("Panel/progress").GetComponent<TMP_Text>().text = "Sample " + intermediate.Count + "/" + wifiSampleAmount;

        if(intermediate.Count >= wifiSampleAmount) {
            processMeasurements();
        } else {
            wifiManager.startScan();
        }

        // Measurement m = new Measurement("AP1")
    }

    private void processMeasurements() {
        foreach(string res in intermediate) {
            print("Result: " + res);
        }

        intermediate.Clear();

        if(samplePoints.Count > 2) {
            samplePoints.RemoveAt(0);
            preprareMeasurement(samplePoints[0]);
        } else {
            samplePoints.Clear();
            start.GetComponent<Button>().enabled = false;
        }
    }

    private void sampleGrid() {
        for (float x = bounds.min.x; x < bounds.max.x; x += gridSize)
        {
            for (float z = bounds.min.z; z < bounds.max.z; z += gridSize)
            {
                // Check if the grid point is inside the floor shape
                Vector3 gridPoint = new Vector3(x, 0f, z);

                if (isPointOnNavMesh(gridPoint))
                {
                    // Create a square or perform some action at this grid point
                    samplePoints.Add(gridPoint);
                    print(gridPoint + " on Mesh");
                } else {
                    missedPoints.Add(gridPoint);
                }
                print(gridPoint + " NOT ON MESH");
            }
        }
    }

    private void sampleRandom() {
        for(int i = 0; i < randomSampleAmount; i++) {
            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                0,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            bool onMesh = isPointOnNavMesh(randomPoint);
            if(onMesh) {
                samplePoints.Add(randomPoint);
            } else {
                i--;
            }
        }
    }

    private bool isPointOnNavMesh(Vector3 point) {
        NavMeshHit hit;
        return NavMesh.SamplePosition(point, out hit,0.1f, NavMesh.AllAreas);
    }
}

public enum SamplingMethod {
    Random,
    Grid
}

[System.Serializable]
public class Sample {
    public Vector3 location;
    public Measurement[] measurements;
}

[System.Serializable]
public class Measurement {

    public Measurement(string MAC, float signalStrength) {
        this.MAC = MAC;
        this.signalStrength = signalStrength;
    }
    public Measurement(string MAC, float signalStrength, Dictionary<string, float> obstacles) {
        this.MAC = MAC;
        this.signalStrength = signalStrength;
        this.obstacles = obstacles;
    }

    public string MAC;
    public float signalStrength;
    public Dictionary<string, float> obstacles;
}
