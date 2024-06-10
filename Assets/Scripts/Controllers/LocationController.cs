using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class LocationController : MonoBehaviour
{

    public static string selectedEndpoint = "predict/default";
    public static UnityEvent<Vector3> locationChanged = new UnityEvent<Vector3>();
    public static Vector3 location;

    private Coroutine scanning;

    void Start()
    {
        // if(!WifiManager.available) return;
        WifiManager.scanComplete.AddListener(onScanComplete);
        location = new Vector3(30, 1, 30);
        scanning = StartCoroutine(WifiScanning());
    }

    void OnDestroy() {
        StopCoroutine(scanning);
    }


    private async void onScanComplete(JToken result) {
        JToken res = await API.Post(selectedEndpoint, result);

        Vector3 position = new Vector3(float.Parse(res["x"].ToString()), float.Parse(res["y"].ToString()), float.Parse(res["z"].ToString()));

        NavMeshHit adjusted;
        NavMesh.SamplePosition(position, out adjusted, 30, NavMesh.AllAreas);
        position = adjusted.position;

        transform.position = position;
        location = position;
        locationChanged.Invoke(position);
        transform.position = position;
    }

    private IEnumerator WifiScanning() {
        while(true) {
            WifiManager.startScan();

            yield return new WaitForSeconds(3);
        }
    }
}
