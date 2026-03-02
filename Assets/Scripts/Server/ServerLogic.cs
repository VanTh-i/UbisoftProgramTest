using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServerLogic : MonoBehaviour
{
    [Header("Settings")]
    public int totalPlayers = 4;
    public int maxEggsOnMap = 5;
    public float playerMoveSpeed = 5f;

    private Dictionary<int, PlayerState> players = new Dictionary<int, PlayerState>();
    private Dictionary<int, Vector3> playerTargets = new Dictionary<int, Vector3>();
    private Dictionary<int, EggState> eggs = new Dictionary<int, EggState>();

    private int eggIdCounter = 0;
    private float updateTimer = 0f;
    private float nextUpdateTime = 2f; //random 1s den 5s

    private void Start()
    {
        NetworkSimulator.Instance.OnServerReceivedMessage += HandleMessageFromClient;

        InitializeGame();
        SetNextUpdateTime();
    }

    private void InitializeGame()
    {
        //spawn player
        for (int i = 1; i <= totalPlayers; i++)
        {
            Vector3 startPos = GetRandomWalkablePosition();
            players[i] = new PlayerState { PlayerID = i, Position = startPos, Score = 0 };
            playerTargets[i] = startPos;
        }

        // egg spawn
        for (int i = 0; i < maxEggsOnMap; i++)
        {
            SpawnNewEgg();
        }

        Debug.Log($"Server: Spawn {totalPlayers} player and {maxEggsOnMap} eggs.");
    }

    private void Update()
    {
        //server moving logic
        SimulateServerLogic();

        //gui game state cho client random 1s -> 5s
        updateTimer += Time.deltaTime;
        if (updateTimer >= nextUpdateTime)
        {
            SendGameStateToClients();
            updateTimer = 0f;
            SetNextUpdateTime();
        }

    }

    private void SimulateServerLogic()
    {
        //Authoritative Movement
        List<int> playerIDs = players.Keys.ToList();
        foreach (int id in playerIDs)
        {
            PlayerState p = players[id];
            Vector3 target = playerTargets[id];

            p.Position = Vector3.MoveTowards(p.Position, target, playerMoveSpeed * Time.deltaTime);
            players[id] = p;

            CheckEggCollection(id, p.Position);
        }
    }

    private void CheckEggCollection(int playerID, Vector3 playerPos)
    {
        List<int> eggIDs = eggs.Keys.ToList();
        foreach (var eID in eggIDs)
        {
            if (Vector3.Distance(playerPos, eggs[eID].Position) < 1f)
            {
                PlayerState p = players[playerID];
                p.Score += 1;
                players[playerID] = p;
                Debug.Log($"Server: Player {playerID} collect egg {eID}. Score: {p.Score}");

                //remove eggs khoi Dictionary va spawn egg moi
                eggs.Remove(eID);
                SpawnNewEgg();
                break;
            }
        }
    }

    private void SpawnNewEgg()
    {
        eggIdCounter++;
        EggState newEgg = new EggState
        {
            EggID = eggIdCounter,
            Position = GetRandomWalkablePosition(),
            EggColor = new Color(Random.value, Random.value, Random.value)
        };

        eggs.Add(newEgg.EggID, newEgg);
    }

    private Vector3 GetRandomWalkablePosition()
    {
        if(GridManager.Instance == null) return Vector3.zero;

        Vector2 size = GridManager.Instance.gridWorldSize;
        int maxAttempts = 100;
        for (int i = 0; i < maxAttempts; i++)
        {
            float randomX = Random.Range(-size.x / 2f, size.x / 2f);
            float randomZ = Random.Range(-size.y / 2f, size.y / 2f);
            Vector3 randomPos = new Vector3(randomX, 0, randomZ);

            Node node = GridManager.Instance.NodeFromWorldPoint(randomPos);
            if (node != null && node.walkable)
            {
                return node.worldPosition;
            }
        }

        return Vector3.zero;
    }

    private void HandleMessageFromClient(BaseMessage msg)
    {
        if (msg is MoveRequestMessage moveMsg)
        {
            Node targetNode = GridManager.Instance.NodeFromWorldPoint(moveMsg.TargetPosition);
            if (targetNode != null && targetNode.walkable)
            {
                if (playerTargets.ContainsKey(moveMsg.PlayerID))
                {
                    playerTargets[moveMsg.PlayerID] = moveMsg.TargetPosition;
                }
            }
            else
            {
                //can not move through wall
            }
        }
    }
    private void SendGameStateToClients()
    {
        GameStateUpdateMessage updateMsg = new GameStateUpdateMessage
        {
            Players = players.Values.ToArray(),
            Eggs = eggs.Values.ToArray()
        };

        NetworkSimulator.Instance.SendToClient(updateMsg);
        Debug.Log("Server: Sent to all Clients!");

    }

    private void SetNextUpdateTime()
    {
        nextUpdateTime = Random.Range(1f, 5f); 
        //nextUpdateTime = 2f;
    }
}
