using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;

public class PathManager : MonoBehaviour //used to manage the pathfinding. 
{
    static PathManager instance;
    Pathfinding pathfinding;
    Queue<PathResult> results = new Queue<PathResult>();

    void Awake()
    {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();
    }

    private void Update()
    {
        if (results.Count > 0)
        {
            int itemsInQueue = results.Count;

            lock (results)
            {
                for (int i = 0; i < itemsInQueue; ++i)
                {
                    PathResult result = results.Dequeue();
                    result.callback(result.path, result.success);
                }
            }
        }
    }

    public static void RequestPath(PathRequest request)
    {
        ThreadStart threadStart = delegate //used to queue pathfinding requests
        {
            instance.pathfinding.FindPath(request, instance.FinishedProcessingPath);
        };

        threadStart.Invoke();
    }

    public void FinishedProcessingPath(PathResult result)
    {
        lock (results) //when path is found, enqueue the result to be withdrawn
        {
            results.Enqueue(result);
        }
    }


}

public struct PathRequest
{
    public Vector3 pathStart;
    public Vector3 pathEnd;
    public Action<Vector3[], bool> callback;

    public PathRequest(Vector3 start, Vector3 end, Action<Vector3[], bool> back)
    {
        pathStart = start;
        pathEnd = end;
        callback = back;
    }
}

public struct PathResult
{
    public Vector3[] path;
    public bool success;
    public Action<Vector3[], bool> callback;

    public PathResult(Vector3[] path, bool success, Action<Vector3[], bool> callback)
    {
        this.path = path;
        this.success = success;
        this.callback = callback;
    }
}
