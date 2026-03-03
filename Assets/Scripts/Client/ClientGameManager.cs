using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientGameManager : MonoBehaviour
{
    public LayerMask obstacleMask;

    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject eggPrefab;

    private Dictionary<int, ClientPlayer> activePlayers = new Dictionary<int, ClientPlayer>();
    private Dictionary<int, GameObject> activeEggs = new Dictionary<int, GameObject>();

    private int localPlayerID = 1; //1 is local player ID
    private Vector3 currentInputTarget; //local target pos
    private float inputCooldown = 0f;
    private float inputRate = 0.15f;

    private void Start()
    {
        NetworkSimulator.Instance.OnClientReceivedMessage += HandleMessageFromServer;
    }
    private void Update()
    {
        HandleKeyboardInput();
    }

    private void HandleKeyboardInput()
    {
        inputCooldown -= Time.deltaTime;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 moveDir = new Vector3(horizontal, 0f, vertical);
        if (moveDir.magnitude > 1f)
        {
            moveDir.Normalize();
        }

        if (moveDir != Vector3.zero && activePlayers.ContainsKey(localPlayerID))
        {
            if (inputCooldown <= 0f)
            {
                Vector3 currentPos = activePlayers[localPlayerID].transform.position;
                currentInputTarget = currentPos + moveDir * 1f;

                if (!Physics.SphereCast(currentPos, 0.4f, moveDir, out RaycastHit hit, 1f, obstacleMask))
                {
                    MoveRequestMessage req = new MoveRequestMessage
                    {
                        PlayerID = localPlayerID,
                        TargetPosition = currentInputTarget
                    };
                    NetworkSimulator.Instance.SendToServer(req);

                    activePlayers[localPlayerID].UpdateServerPosition(currentInputTarget);

                    inputCooldown = inputRate;
                }
                else
                {
                    currentInputTarget = currentPos;
                }

            }
        }
        else if (moveDir == Vector3.zero && inputCooldown <= 0f) //player dung lai khi nha phim
        {
            if (activePlayers.ContainsKey(localPlayerID))
            {
                currentInputTarget = activePlayers[localPlayerID].transform.position;
            }
        }
    }

    private void HandleMessageFromServer(BaseMessage msg)
    {
        if (msg is GameStateUpdateMessage gameState)
        {
            UpdatePlayersVisuals(gameState.Players);
            UpdateEggsVisuals(gameState.Eggs);
            Debug.Log("Client: Received from the Server!");
        }
    }

    private void UpdatePlayersVisuals(PlayerState[] playersData)
    {
        foreach (PlayerState pData in playersData)
        {
            if (!activePlayers.ContainsKey(pData.PlayerID))
            {
                GameObject newPlayerObj = Instantiate(playerPrefab, pData.Position, Quaternion.identity);
                ClientPlayer clientPlayer = newPlayerObj.AddComponent<ClientPlayer>();
                clientPlayer.Initialize(pData.PlayerID, pData.Position, pData.PlayerID == localPlayerID);

                activePlayers.Add(pData.PlayerID, clientPlayer);

                if (pData.PlayerID == localPlayerID) currentInputTarget = pData.Position;
            }
            //else
            //{
            //    if (pData.PlayerID != localPlayerID)
            //    {
            //        activePlayers[pData.PlayerID].UpdateServerPosition(pData.Position);
            //    }
            //}
        }
    }

    private void UpdateEggsVisuals(EggState[] eggsData)
    {
        HashSet<int> serverEggIDs = new HashSet<int>();

        foreach (EggState eData in eggsData)
        {
            serverEggIDs.Add(eData.EggID);

            if (!activeEggs.ContainsKey(eData.EggID))
            {
                GameObject newEgg = Instantiate(eggPrefab, eData.Position, Quaternion.identity);
                newEgg.GetComponent<Renderer>().material.color = eData.EggColor;
                activeEggs.Add(eData.EggID, newEgg);
            }

            List<int> eggsToRemove = new List<int>();
            foreach (int clientEggID in activeEggs.Keys)
            {
                if (!serverEggIDs.Contains(clientEggID))
                {
                    Destroy(activeEggs[clientEggID]);
                    eggsToRemove.Add(clientEggID);
                }
            }

            foreach (int id in eggsToRemove)
            {
                activeEggs.Remove(id);
            }
        }
    }
}
