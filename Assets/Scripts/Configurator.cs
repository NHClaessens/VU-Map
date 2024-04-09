using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class Configurator : MonoBehaviour
{
    public SamplingMethod samplingMethod;
    public int sampleAmount;
    public float gridSize = 1f;

    private NavMeshTriangulation navMeshTriangulation;
    private Vector3[] vertices;
    private int[] indices;
    private List<Vector3> samplePoints = new List<Vector3>();
    public void StartConfiguration() {
        navMeshTriangulation = NavMesh.CalculateTriangulation();
        vertices = navMeshTriangulation.vertices;
        indices = navMeshTriangulation.indices;
        generateSampleLocations();
    }

    public void OnDrawGizmos() {
        if(vertices != null) {
            foreach(Vector3 pos in vertices) {
                // Gizmos.DrawSphere(pos, 0.5f);
            }
        }

        foreach(Vector3 pos in samplePoints) {
            Gizmos.DrawSphere(pos, 1);
        }
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

    private void sampleGrid() {
        NavMeshSurface surf = GameObject.Find("NavMesh Surface").GetComponent<NavMeshSurface>();
        Bounds bounds = surf.navMeshData.sourceBounds;

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
                }
            }
        }
    }

    private void sampleRandom() {
        NavMeshSurface surf = GameObject.Find("NavMesh Surface").GetComponent<NavMeshSurface>();
        Bounds bounds = surf.navMeshData.sourceBounds;

        for(int i = 0; i < sampleAmount; i++) {
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
            // navMeshTriangulation.
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
