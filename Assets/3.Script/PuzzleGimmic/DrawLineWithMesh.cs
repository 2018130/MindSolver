using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DrawLineWithMesh : NetworkBehaviour
{
    [SerializeField] private float brushWidth = 0.1f;
    [SerializeField] private float minDistance = 0.05f;
    // 최적화 추가: 이전 진행 방향과 새 방향의 각도 차이(도 단위)가 이 값보다 작으면 버텍스를 새로 파지 않고 마지막 버텍스를 연장합니다.
    [SerializeField] private float simplifyAngleThreshold = 2.0f;

    private Mesh mesh;
    private NetworkList<Vector3> vertices = new NetworkList<Vector3>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkList<Vector3> Vertices => vertices;

    private NetworkList<int> triangles = new NetworkList<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkList<Vector2> uvs = new NetworkList<Vector2>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Vector3 lastLocalMousePos;
    private Vector3 lastDirection; // 최적화 추가: 직전 진행 방향 저장
    private bool isNewStroke = true;
    private bool hasFirstPair = false;
    private bool isMeshDirty = false;

    CatmullRomPath catmullRomPath;
    private bool isGamePlaying;

    // GC 배제를 위한 캐싱 리스트 (메시 갱신용)
    private readonly List<Vector3> cachedVertices = new List<Vector3>();
    private readonly List<int> cachedTriangles = new List<int>();
    private readonly List<Vector2> cachedUVs = new List<Vector2>();

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
            ClearMeshData();
        }
        isNewStroke = true;
        hasFirstPair = false;
        isMeshDirty = false;
        isGamePlaying = true;
    }

    public void EndGame(bool isClear, MultiMissionType multiMissionType)
    {
        if (multiMissionType == MultiMissionType.DrawLine)
        {
            EndGame_ClientRpc();
        }
    }

    [ClientRpc]
    private void EndGame_ClientRpc()
    {
        isGamePlaying = false;
        if (IsOwner)
        {
            ClearMeshData();
        }

        mesh.Clear();
        mesh.RecalculateBounds();
    }

    private void ClearMeshData()
    {
        vertices.Clear();
        uvs.Clear();
        triangles.Clear();
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

            if (catmullRomPath != null)
            {
                var listener = catmullRomPath.GetComponentInParent<PuzzleMissonListener>();
                if (listener != null)
                {
                    listener.OnEndPuzzle -= EndGame;
                    listener.OnStartPuzzle -= StartGame;
                }
            }
        }
    }

    private void OnNetworkListChanged<T>(NetworkListEvent<T> changeEvent)
    {
        isMeshDirty = true;
    }

    private void Update()
    {
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
            lastDirection = Vector3.zero;
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

            // 첫 세그먼트 생성
            int vIndex = vertices.Count;
            CreateVertexPair(currentLocalPos, normal);

            triangles.Add(vIndex - 2);
            triangles.Add(vIndex + 0);
            triangles.Add(vIndex - 1);

            triangles.Add(vIndex - 1);
            triangles.Add(vIndex + 0);
            triangles.Add(vIndex + 1);

            lastDirection = direction;
            lastLocalMousePos = currentLocalPos;
            RefreshMesh();
            return;
        }

        // --- [최적화 핵심 핵심 로직] 직선 구간 판정 ---
        // 이전 방향과 현재 방향의 각도 차이를 계산합니다.
        float angleDiff = Vector3.Angle(lastDirection, direction);

        if (angleDiff < simplifyAngleThreshold)
        {
            // 방향 전환이 거의 없다면(직선 구간), 새로운 버텍스를 쌓지 않고 
            // 마지막에 생성되었던 버텍스 2개의 위치를 현재 마우스 위치 기준으로 "연장"시킵니다.
            int lastIdx = vertices.Count - 2;

            vertices[lastIdx] = currentLocalPos + normal * (brushWidth * 0.5f);
            vertices[lastIdx + 1] = currentLocalPos - normal * (brushWidth * 0.5f);

            // 트라이앵글은 추가할 필요가 없고, 마우스 기준 좌표만 갱신합니다.
            lastLocalMousePos = currentLocalPos;
        }
        else
        {
            // 방향이 일정 수치 이상 꺾였을 때만 새로운 사각형(버텍스 2개, 트라이앵글 6개)을 추가합니다.
            int vIndex = vertices.Count;
            CreateVertexPair(currentLocalPos, normal);

            triangles.Add(vIndex - 2);
            triangles.Add(vIndex + 0);
            triangles.Add(vIndex - 1);

            triangles.Add(vIndex - 1);
            triangles.Add(vIndex + 0);
            triangles.Add(vIndex + 1);

            lastDirection = direction;
            lastLocalMousePos = currentLocalPos;
        }

        RefreshMesh();
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
        int vCount = vertices.Count;
        int tCount = triangles.Count;

        if (vCount == 0 || tCount == 0) return;
        if (vCount % 2 != 0 || tCount % 6 != 0 || vCount != uvs.Count) return;

        // 최적화: foreach 대신 미리 할당된 캐시 리스트를 Clear 후 재사용하여 GC 대폭 감소
        cachedVertices.Clear();
        cachedTriangles.Clear();
        cachedUVs.Clear();

        for (int i = 0; i < vCount; i++) cachedVertices.Add(vertices[i]);
        for (int i = 0; i < tCount; i++) cachedTriangles.Add(triangles[i]);
        for (int i = 0; i < vCount; i++) cachedUVs.Add(uvs[i]);

        mesh.Clear();
        mesh.SetVertices(cachedVertices);
        mesh.SetUVs(0, cachedUVs);
        mesh.SetTriangles(cachedTriangles, 0);
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