using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DrawLineWithMesh : NetworkBehaviour
{
    [SerializeField] private float brushWidth = 0.1f;
    [SerializeField] private float minDistance = 0.05f;

    private Mesh mesh;
    private NetworkList<Vector3> vertices = new NetworkList<Vector3>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkList<Vector3> Vertices => vertices;
    
    private NetworkList<int> triangles = new NetworkList<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkList<Vector2> uvs = new NetworkList<Vector2>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Vector3 lastLocalMousePos;
    private bool isNewStroke = true;
    private bool hasFirstPair = false;

    private void Awake()
    {
        mesh = new Mesh();
        mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void Start()
    {
        if (IsOwner)
            return;

        vertices.OnListChanged += OnVerticesChanged;
        triangles.OnListChanged += OnTrianglesChanged;
        uvs.OnListChanged += OnUVChanged;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(IsServer)
        {
            foreach(var path in FindObjectsByType<CatmullRomPath>(FindObjectsSortMode.None))
            {
                if (path.IsServerPath.Value)
                {
                    if(IsOwner)
                    {
                        path.GetComponent<LineChecker>().Initialize(this);
                        vertices.OnListChanged += path.GetComponent<LineChecker>().CheckDistance;
                    }
                }
                else
                {
                    if(!IsOwner)
                    {
                        path.GetComponent<LineChecker>().Initialize(this);
                        vertices.OnListChanged += path.GetComponent<LineChecker>().CheckDistance;
                    }
                }
            }
        }
    }

    private void OnVerticesChanged(NetworkListEvent<Vector3> networkListEvent)
    {
        List<Vector3> vectorVertices = new List<Vector3>();
        foreach (var vertex in vertices)
        {
            vectorVertices.Add(vertex);
        }

        if (vectorVertices.Count % 2 == 0)
        {
            mesh.SetVertices(vectorVertices);
        }
    }

    private void OnTrianglesChanged(NetworkListEvent<int> netowrkListEvent)
    {
        List<int> vectorTriangles = new List<int>();
        foreach (var triangles in triangles)
        {
            vectorTriangles.Add(triangles);
        }

        if(vectorTriangles.Count % 6 == 0)
            mesh.SetTriangles(vectorTriangles, 0);
    }

    private void OnUVChanged(NetworkListEvent<Vector2> networkListEvent)
    {
        List<Vector2> vectorUV = new List<Vector2>();
        foreach (var uv in uvs)
        {
            vectorUV.Add(uv);
        }

        if(vectorUV.Count % 2 == 0 && vertices.Count == uvs.Count)
            mesh.SetUVs(0, vectorUV);
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (InputManager.Singleton.LeftButtonClicked)
        {
            AddBrushStep();
        }
        else
        {
            isNewStroke = true;
            hasFirstPair = false;
        }
    }

    private void AddBrushStep()
    {
        Vector3 worldPos = GetMousePositionToWorldPos();
        Vector3 currentLocalPos = transform.InverseTransformPoint(worldPos);

        if (isNewStroke)
        {
            lastLocalMousePos = currentLocalPos;
            isNewStroke = false;
            return;
        }

        float distance = Vector3.Distance(currentLocalPos, lastLocalMousePos);
        if (distance < minDistance) return;

        Vector3 direction = (currentLocalPos - lastLocalMousePos).normalized;
        Vector3 normal = new Vector3(-direction.y, direction.x, 0f);

        if (!hasFirstPair)
        {
            CreateVertexPair(lastLocalMousePos, normal);
            hasFirstPair = true;
        }

        int vIndex = vertices.Count;
        CreateVertexPair(currentLocalPos, normal);

        triangles.Add(vIndex - 2);
        triangles.Add(vIndex + 0);
        triangles.Add(vIndex - 1);

        triangles.Add(vIndex - 1);
        triangles.Add(vIndex + 0);
        triangles.Add(vIndex + 1);

        // parsing vertieces 
        List<Vector3> vectorVertices = new List<Vector3>();
        foreach (var vertex in vertices)
        {
            vectorVertices.Add(vertex);
        }
        mesh.SetVertices(vectorVertices);

        // parsing triagles
        List<int> vectorTriangles = new List<int>();
        foreach (var triangles in triangles)
        {
            vectorTriangles.Add(triangles);
        }
        mesh.SetTriangles(vectorTriangles, 0);


        // parsing vertieces 
        List<Vector2> vectorUV = new List<Vector2>();
        foreach (var uv in vectorUV)
        {
            vectorUV.Add(uv);
        }
        mesh.SetUVs(0, vectorUV);
        mesh.RecalculateBounds();

        lastLocalMousePos = currentLocalPos;
    }

    private void CreateVertexPair(Vector3 pos, Vector3 normal)
    {
        vertices.Add(pos + normal * (brushWidth * 0.5f)); // 좌측 정점
        vertices.Add(pos - normal * (brushWidth * 0.5f)); // 우측 정점

        uvs.Add(new Vector2(0, vertices.Count / 2));
        uvs.Add(new Vector2(1, vertices.Count / 2));
    }

    private Vector3 GetMousePositionToWorldPos()
    {
        Vector3 mousePos = InputManager.Singleton.MousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;

        return worldPos;
    }
}
