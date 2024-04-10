using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class NavigationController : Singleton<NavigationController>
{
    public Vector3 destination;
    public float turnDistanceThreshold = 1.5f;
    public float subCornerThreshold = 0.5f;
    public float maxDistThreshold = 5f;
    public List<Instruction> allInstructions;
    public List<Instruction> instructions;
    public NavUIConfig navUI;
    public UnityEvent<List<Instruction>> navigationUpdated = new UnityEvent<List<Instruction>>();

    private LineRenderer line;
    private int wayPointIndex = 0;
    private bool inWayPointRange = false;
    public Vector3[] waypoints;

    // Start is called before the first frame update
    public NavigationController() : base()
    {
        line = GameObject.Find("Path").GetComponent<LineRenderer>();
        PositionController.Instance.positionUpdated.AddListener(LocationUpdated);

        navUI = GameObject.Find("NavUI").GetComponent<NavUIConfig>();
    }

    // Update is called once per frame
    public void LocationUpdated(Vector3 newLocation)
    {
        if(wayPointIndex > 0 && wayPointIndex < waypoints.Length) checkDistToPath();
        FindClosestPointOnPath(newLocation);
    }

    public void StartNavigation(Vector3 dest) {
        wayPointIndex = 0;
        inWayPointRange = false;
        
        destination = dest;

        NavMeshPath path = new NavMeshPath();
        bool possible = NavMesh.CalculatePath(PositionController.Instance.position, destination, NavMesh.AllAreas, path);

        if(!possible) return;

        waypoints = CleanPath(path.corners).ToArray();
        DrawPath(waypoints);
        List<Instruction> instructions = GenerateInstructionTypes(waypoints);
        navigationUpdated.Invoke(instructions);
    }
    
    void checkDistToPath() {
        Vector3 origin = waypoints[wayPointIndex - 1];
        Vector3 destination = waypoints[wayPointIndex];
        Ray path = new Ray(origin, destination - origin);

        float distance = Vector3.Cross(path.direction, PositionController.Instance.position - path.origin).magnitude;

        if(distance > maxDistThreshold) {
            Debug.Log("Left route, recalculating");
            StartNavigation(destination);
        }
    }

    void DrawPath(Vector3[] path) {
        line.positionCount = path.Length;
        line.SetPositions(path);
    }

    public List<Vector3> CleanPath(Vector3[] path)
    {
        if (path.Length < 3)
            return new List<Vector3>(path);

        List<Vector3> cleanedPath = new List<Vector3> {path[0]};
        Vector3 accumulatedPosition = path[1];
        int accumulatedCount = 1;

        for (int i = 1; i < path.Length-1; i++)
        {            
            Debug.Log(path[i]);
            if (
                Vector3.Distance(path[i], path[i + 1]) <= subCornerThreshold
                && isAngleSameDirection(
                    calculateAngle(path[i-1], path[i], path[i+1]),
                    calculateAngle(path[i], path[i+1], path[i+2])
                    )
            )
            {
                accumulatedPosition += path[i];
                accumulatedCount++;
            }
            else
            {
                Vector3 newPosition = accumulatedPosition / accumulatedCount;
                cleanedPath.Add(newPosition);
                
                accumulatedPosition = path[i+1];
                accumulatedCount = 1;
            }
        }

        cleanedPath.Add(path[path.Count()-1]);

        return cleanedPath;
    }

    private float calculateAngle(Vector3 a, Vector3 b, Vector3 c) {
        Vector3 dir1 = (b - a).normalized;
        Vector3 dir2 = (c - b).normalized;
        return Vector3.SignedAngle(dir1, dir2, Vector3.up);
    }

    private bool isAngleSameDirection(float a, float b) {
        if(a > 0) return b > 0;
        else if(a < 0) return b < 0;
        else return b == 0;
    }

    public List<Instruction> GenerateInstructionTypes(Vector3[] pathCorners)
    {
        List<Instruction> instructions = new List<Instruction>();

        if(pathCorners.Length < 3) {
            instructions.Add(new Instruction(InstructionType.Straight, pathCorners[0], Vector3.Distance(pathCorners[0], pathCorners[1])));
            return instructions;
        }

        for (int i = 1; i < pathCorners.Length - 1; i++)
        {
            // Calculate the angle between the vectors
            float angle = calculateAngle(pathCorners[i-1], pathCorners[i], pathCorners[i+1]);

            InstructionType type;
            if(angle >= -15 && angle <= 15)
                type = InstructionType.Straight;
            else if (angle > 15 && angle <= 45)
                type = InstructionType.SlightRight;
            else if (angle > 45)
                type = InstructionType.TurnRight;
            else if (angle >= -45 && angle < -15)
                type = InstructionType.SlightLeft;
            else if (angle < -45)
                type = InstructionType.TurnLeft;
            else type = InstructionType.Error;


            instructions.Add(new Instruction(type, pathCorners[i], Vector3.Distance(pathCorners[i-1], pathCorners[i])));
        }

        this.instructions = instructions;

        return instructions;
    }

    private Vector3 FindClosestPointOnPath(Vector3 currentPosition)
    {
        float minDistance = float.MaxValue;
        Vector3 closestPoint = Vector3.zero;
        int index = 0;
        
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Vector3 start = waypoints[i];
            Vector3 end = waypoints[i + 1];
            Vector3 closestPointOnSegment = ClosestPointOnLineSegment(start, end, currentPosition);

            float distance = Vector3.Distance(closestPointOnSegment, currentPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                index = i;
                closestPoint = closestPointOnSegment;
            }
        }

        navUI.ScrollToInstruction(index);
        navUI.SetDistance(Vector3.Distance(currentPosition, waypoints[index+1]));

        return closestPoint;
    }

    private Vector3 ClosestPointOnLineSegment(Vector3 start, Vector3 end, Vector3 point)
    {
        Vector3 segment = end - start;
        float t = Vector3.Dot(point - start, segment) / segment.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return start + t * segment;
    }
}

[System.Serializable]
public class Instruction {
    public Instruction(InstructionType instructionType, Vector3 location, float distance) {
        this.instructionType = instructionType;
        this.location = location;
        this.distance = distance;
    }
    public InstructionType instructionType;
    public Vector3 location;
    public float distance;
    // public Sprite icon;

    public string toString() {
        switch(instructionType) {
            case InstructionType.Straight:
                return "Go straight";
            case InstructionType.TurnLeft:
                return "Turn left";
            case InstructionType.TurnRight:
                return "Turn right";
            case InstructionType.SlightLeft:
                return "Slight left";
            case InstructionType.SlightRight:
                return "Slight right";
            case InstructionType.TurnAround:
                return "Turn around";
            case InstructionType.GoUpStairs:
                return "Go up the stairs";
            case InstructionType.GoDownStairs:
                return "Go down the stairs";
            case InstructionType.Error:
            default:
                return "Something went wrong...";
        }
    }

    public Texture2D getIcon() {
        switch(instructionType) {
            case InstructionType.Straight:
                return Resources.Load<Texture2D>("Icons/goStraight");
            case InstructionType.TurnLeft:
                return Resources.Load<Texture2D>("Icons/turnLeft");
            case InstructionType.TurnRight:
                return Resources.Load<Texture2D>("Icons/turnRight");
            case InstructionType.SlightLeft:
                return Resources.Load<Texture2D>("Icons/slightLeft");
            case InstructionType.SlightRight:
                return Resources.Load<Texture2D>("Icons/slightRight");
            case InstructionType.TurnAround:
                return Resources.Load<Texture2D>("Icons/turnAround");
            case InstructionType.GoUpStairs:
                return Resources.Load<Texture2D>("Icons/slightRight");
            case InstructionType.GoDownStairs:
                return Resources.Load<Texture2D>("Icons/turnAround");
            case InstructionType.Error:
            default:
                return null;
        }
    }
}

public enum InstructionType
{
    Straight,
    TurnLeft,
    TurnRight,
    SlightLeft,
    SlightRight,
    TurnAround,
    GoUpStairs,
    GoDownStairs,
    Error
}