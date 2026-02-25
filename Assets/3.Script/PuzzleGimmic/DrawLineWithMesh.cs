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
    private bool isMeshDirty = false;

    CatmullRomPath catmullRomPath;
    private bool isGamePlaying;

    private void Awake()
    {
        mesh = new Mesh();
        mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void InitGame()
    {
        if (IsServer)
        {
            CatmullRomPath[] catmullRomPaths = FindObjectsByType<CatmullRomPath>(FindObjectsSortMode.None);

            if (IsOwner)
            {
                catmullRomPath = catmullRomPaths[0];
                catmullRomPaths[0].GetComponent<LineChecker>().Initialize(this);
                vertices.OnListChanged += catmullRomPaths[0].GetComponent<LineChecker>().CheckDistance;
            }
            else
            {
                catmullRomPath = catmullRomPaths[1];
                catmullRomPaths[1].GetComponent<LineChecker>().Initialize(this);
                vertices.OnListChanged += catmullRomPaths[1].GetComponent<LineChecker>().CheckDistance;
            }
        }
    }
    
    public void StartGame(MultiMissionType multiMissionType)
    {
        if (multiMissionType == MultiMissionType.DrawLine)
        {
            StartGame_ClientRpc();
        }
    }

    [ClientRpc]
    private void StartGame_ClientRpc()
    {
        if (!IsOwner)
        {
            isMeshDirty = true;
        }
        else
        {
            vertices.Clear();
            uvs.Clear();
            triangles.Clear();
        }
        isNewStroke = true;
        hasFirstPair = false;
        isMeshDirty = false;
        isGamePlaying = true;
    }

    public void EndGame(bool isClear, MultiMissionType multiMissionType)
    {
        if(multiMissionType == MultiMissionType.DrawLine)
        {
            EndGame_ClientRpc();
        }
    }

    [ClientRpc]
    private void EndGame_ClientRpc()
    {
        isGamePlaying = false;
        if(IsOwner)
        {
            vertices.Clear();
            uvs.Clear();
            triangles.Clear();
        }

        mesh.Clear();
        mesh.RecalculateBounds();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            vertices.OnListChanged += OnNetworkListChanged;
            triangles.OnListChanged += OnNetworkListChanged;
            uvs.OnListChanged += OnNetworkListChanged;

            isMeshDirty = true;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
        {
            vertices.OnListChanged -= OnNetworkListChanged;
            triangles.OnListChanged -= OnNetworkListChanged;
            uvs.OnListChanged -= OnNetworkListChanged;
        }

        if (IsServer)
        {
            foreach (var path in FindObjectsByType<CatmullRomPath>(FindObjectsSortMode.None))
            {
                var checker = path.GetComponent<LineChecker>();
                if (checker != null)
                {
                    vertices.OnListChanged -= checker.CheckDistance;
                }
            }

            catmullRomPath.GetComponentInParent<PuzzleMissonListener>().OnEndPuzzle -= EndGame;
            catmullRomPath.GetComponentInParent<PuzzleMissonListener>().OnStartPuzzle -= StartGame;
        }
    }

    private void OnNetworkListChanged<T>(NetworkListEvent<T> changeEvent)
    {
        isMeshDirty = true;
    }

    private void Update()
    {
        if (!isGamePlaying)
            return;

        if (IsOwner)
        {
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
        else
        {
            // 주인이 아닌 경우 메쉬가 변경되었을 때만 그리기
            if (isMeshDirty)
            {
                RefreshMesh();
                isMeshDirty = false;
            }
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

        RefreshMesh();

        lastLocalMousePos = currentLocalPos;
    }

    private void CreateVertexPair(Vector3 pos, Vector3 normal)
    {
        vertices.Add(pos + normal * (brushWidth * 0.5f));
        vertices.Add(pos - normal * (brushWidth * 0.5f));

        uvs.Add(new Vector2(0, vertices.Count / 2f));
        uvs.Add(new Vector2(1, vertices.Count / 2f));
    }

    private void RefreshMesh()
    {
        if (vertices.Count == 0 || triangles.Count == 0) return;
        if (vertices.Count % 2 != 0 || triangles.Count % 6 != 0 || vertices.Count != uvs.Count) return;

        List<Vector3> vectorVertices = new List<Vector3>();
        foreach (var v in vertices) vectorVertices.Add(v);

        List<int> vectorTriangles = new List<int>();
        foreach (var t in triangles) vectorTriangles.Add(t);

        List<Vector2> vectorUV = new List<Vector2>();
        foreach (var uv in uvs) vectorUV.Add(uv);
        mesh.Clear();
        mesh.SetVertices(vectorVertices);
        mesh.SetUVs(0, vectorUV);
        mesh.SetTriangles(vectorTriangles, 0);
        mesh.RecalculateBounds();
    }

    private Vector3 GetMousePositionToWorldPos()
    {
        Vector3 mousePos = InputManager.Singleton.MousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = -0.1f;
        return worldPos;
    }
}