using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class NetworkRelayManager : MonoBehaviour
{
    private const int MaxConnections = 2;
    private const string JoinCodeKey = "j"; // 로비 데이터 키
    [SerializeField]private bool isHost = false;
    private Lobby _currentLobby;

    // TMP
    public string StageKey = "StageKey";

    private async void Start()
    {
           // 1. 유니티 게이밍 서비스 초기화
        await UnityServices.InitializeAsync();

        // 2. 익명 로그인 (Relay/Lobby 사용 필수)
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Signed in: {AuthenticationService.Instance.PlayerId}");
        }
    }

    /// <summary>
    /// [HOST] 릴레이를 생성하고, 해당 코드를 가진 로비를 만듭니다.
    /// </summary>
    public async Task CreateGame(int myMaxStage)
    {
        try
        {
            // 1. Relay 할당
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // 2. Transport 설정
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // 3. Lobby 옵션 설정 (여기가 핵심!)
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.Data = new Dictionary<string, DataObject>
            {
                {
                    JoinCodeKey, new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: joinCode
                    )
                },
                {
                    // 스테이지 정보를 검색 가능하도록 N1 (Number 1) 인덱스에 저장
                    StageKey, new DataObject(
                        visibility: DataObject.VisibilityOptions.Public,
                        value: myMaxStage.ToString(),
                        index: DataObject.IndexOptions.N1 // 중요: 검색을 위해 인덱싱 설정
                    )
                }
            };

            // 로비 생성
            _currentLobby = await LobbyService.Instance.CreateLobbyAsync($"Stage {myMaxStage} Room", MaxConnections, options);

            NetworkManager.Singleton.StartHost();
            Debug.Log($"Host Started. Lobby Stage: {myMaxStage}");

            StartCoroutine(HandleLobbyHeartbeat());
        }
        catch (Exception e)
        {
            Debug.LogError($"Create Game Failed: {e}");
        }
    }

    /// <summary>
    /// [CLIENT] 빠른 입장(Quick Join)을 시도하고, 성공 시 Relay에 접속합니다.
    /// </summary>
    public async Task QuickJoinGame(int myMaxStage)
    {
        try
        {
            // 1. 필터 옵션 설정 (여기가 핵심!)
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();
            options.Filter = new List<QueryFilter>
            {
                // 조건: N1(방의 스테이지) 값이 내 스테이지(myMaxStage)와 같은(EQ) 방만 검색
                new QueryFilter(
                    field: QueryFilter.FieldOptions.N1,
                    op: QueryFilter.OpOptions.EQ,
                    value: myMaxStage.ToString()
                )
            };

            Debug.Log($"Looking for lobby with Stage {myMaxStage}...");

            // 2. 필터가 적용된 QuickJoin 실행
            _currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);

            // 3. 이후 로직 동일 (Relay 접속)
            string joinCode = _currentLobby.Data[JoinCodeKey].Value;
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
            Debug.Log($"Joined Stage {myMaxStage} Room!");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning($"No suitable lobby found for Stage {myMaxStage}. (Make sure the host is set to this stage)");
            _ = CreateGame(myMaxStage);
        }
        catch (Exception e)
        {
            Debug.LogError($"Join Game Failed: {e}");
        }
    }

    // 로비는 30초 이상 신호가 없으면 삭제되므로 주기적으로 신호를 보냄
    private System.Collections.IEnumerator HandleLobbyHeartbeat()
    {
        var wait = new WaitForSeconds(15);
        while (_currentLobby != null)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
            yield return wait;
        }
    }

    // 게임 종료 시 로비 정리
    private async void OnDestroy()
    {
        if (_currentLobby != null && isHost)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
                Debug.Log($"Delete lobby");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error shutting down lobby: {e}");
            }
        }
    }
}
