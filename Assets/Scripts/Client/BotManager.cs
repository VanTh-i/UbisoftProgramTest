using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotManager : MonoBehaviour
{
    private GameStateUpdateMessage latestGameState;

    public float botThinkingInterval = 0.5f; // Time between bot decisions

    private void Start()
    {
        NetworkSimulator.Instance.OnClientReceivedMessage += HandleMessageFromServer;

        StartCoroutine(BotLogicLoop()); //Think
    }

    private void HandleMessageFromServer(BaseMessage msg)
    {
        if (msg is GameStateUpdateMessage gameState)
        {
            latestGameState = gameState;
        }
    }

    private IEnumerator BotLogicLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(botThinkingInterval);

            if (latestGameState == null || latestGameState.Players == null || latestGameState.Eggs == null)
                continue;

            foreach (PlayerState player in latestGameState.Players)
            {
                if (player.PlayerID == 1) continue; //bo qua local player

                Vector3? targetEggPos = GetClosestEgg(player.Position);
                if (targetEggPos.HasValue)
                {
                    List<Vector3> path = Pathfinding.Instance.FindPath(player.Position, targetEggPos.Value); //A* pathfinding

                    if (path != null && path.Count > 0)
                    {
                        Vector3 nextStep = path[0];

                        MoveRequestMessage req = new MoveRequestMessage
                        {
                            PlayerID = player.PlayerID,
                            TargetPosition = nextStep
                        };

                        NetworkSimulator.Instance.SendToServer(req);
                    }
                }
            }
        }
    }

    private Vector3? GetClosestEgg(Vector3 botPosition)
    {
        float minDistance = float.MaxValue;
        Vector3? closestPos = null;

        foreach (EggState egg in latestGameState.Eggs)
        {
            float dist = Vector3.Distance(botPosition, egg.Position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestPos = egg.Position;
            }
        }

        return closestPos;
    }

}
