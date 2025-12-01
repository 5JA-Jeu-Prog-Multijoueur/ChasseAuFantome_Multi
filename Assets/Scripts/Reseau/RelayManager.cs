using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Http;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using NetworkEvent = Unity.Networking.Transport.NetworkEvent;
using TMPro;

public class RelayManager : MonoBehaviour
{
    public static RelayManager instance;
    const int m_MaxConnections = 10; // Celui qui établit le relais compte d'abord
    public string RelayJoinCode;

    private Allocation allocation; // ajout, diff�rent du tuto
    private JoinAllocation joinAllocation; // ajout, diff�rent du tuto

    [SerializeField] private TextMeshProUGUI joinCodeText;
    [SerializeField] private TMP_InputField joinCodeInputField;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        AuthenticatePlayer();
    }

    async void AuthenticatePlayer()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            var playerID = AuthenticationService.Instance.PlayerId;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    // diff�rent du tuto : task de type string
    public async Task<string> AllocateRelayServerAndGetJoinCode(int maxConnections, string region = null)
    {
        string createJoinCode;
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
        }
        catch (Exception e)
        {
            Debug.LogError($"Relay create allocation request failed {e.Message}");
            throw;
        }

        Debug.Log($"server: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"server: {allocation.AllocationId}");

        try
        {
            createJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            joinCodeText.text = createJoinCode;
        }
        catch
        {
            Debug.LogError("Relay create join code request failed");
            throw;
        }
        return createJoinCode;




    }

    public IEnumerator ConfigureTransportAndStartNgoAsHost()
    {
        var serverRelayUtilityTask = AllocateRelayServerAndGetJoinCode(m_MaxConnections);
        while (!serverRelayUtilityTask.IsCompleted)
        {
            yield return null;
        }

        if (serverRelayUtilityTask.IsFaulted)
        {
            Debug.LogError("Exception thrown when attempting to start relay server. Server not started. Exception: " + serverRelayUtilityTask.Exception.Message);
            yield break;
        }

        var relayServerData = serverRelayUtilityTask.Result;

        //Display the joincode to the user
        // ajout, diff�rent du tuto : SetRelayServerData
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
        NetworkManager.Singleton.StartHost();
        yield return null;
    }

    public async Task<JoinAllocation> JoinRelayServerFromJoinCode(string joincode)
    {

        try
        {
            joinAllocation = await RelayService.Instance.JoinAllocationAsync(joincode);
        }
        catch
        {
            Debug.LogError($"Relay create join code request failed");
            throw;
        }

        Debug.Log($"client: {joinAllocation.ConnectionData[0]} {joinAllocation.ConnectionData[1]}");
        Debug.Log($"host: {joinAllocation.HostConnectionData[0]} {joinAllocation.HostConnectionData[1]}");
        Debug.Log($"client: {joinAllocation.AllocationId}");

        return joinAllocation;
        //return new RelayServerData(allocation, "dtls");
    }

    public IEnumerator ConfigureTransportAndStartNgoAsConnectingPlayer()
    {
        var clientRelayUtilityTask = JoinRelayServerFromJoinCode(joinCodeInputField.text);

        while (!clientRelayUtilityTask.IsCompleted)
        {
            yield return null;
        }

        if (clientRelayUtilityTask.IsFaulted)
        {
            Debug.LogError("Exception thrown when attempting to connect to Relay Server. Exception: " + clientRelayUtilityTask.Exception.Message);
            yield break;
        }

        var relayServerData = clientRelayUtilityTask.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));
        NetworkManager.Singleton.StartClient();
        yield return null;
    }

}
