using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotManager : MonoBehaviour
{
    private GameStateUpdateMessage latestGameState;

    public float botThinkingInterval = 0.5f; // Time between bot decisions

    private Dictionary<int, Vector3> virtualBotPositions = new Dictionary<int, Vector3>();
    private Dictionary<int, int> botTargetEggs = new Dictionary<int, int>();

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

            foreach (PlayerState p in gameState.Players)
            {
                if (p.PlayerID != 1)
                {
                    virtualBotPositions[p.PlayerID] = p.Position;
                }
            }
        }
    }

    private IEnumerator BotLogicLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(botThinkingInterval);

            if (latestGameState == null || latestGameState.Players == null || latestGameState.Eggs == null)
                continue;

            ClientPlayer[] allPlayersOnScreen = FindObjectsOfType<ClientPlayer>();

            foreach (ClientPlayer bot in allPlayersOnScreen)
            {
                if (bot.isLocalPlayer) continue;

                if (!virtualBotPositions.ContainsKey(bot.PlayerID))
                {
                    virtualBotPositions[bot.PlayerID] = bot.transform.position;
                }

                Vector3 currentVirtualPos = virtualBotPositions[bot.PlayerID];

                Vector3? targetEggPos = GetClosestEgg(bot.PlayerID, currentVirtualPos);

                if (targetEggPos.HasValue)
                {
                    List<Vector3> path = Pathfinding.Instance.FindPath(currentVirtualPos, targetEggPos.Value); //A* pathfinding
                    Vector3 nextStep = currentVirtualPos;
                    if (path != null && path.Count > 0)
                    {
                        nextStep = path[0];
                    }
                    else
                    {
                        nextStep = targetEggPos.Value;
                    }
                    virtualBotPositions[bot.PlayerID] = nextStep;

                    MoveRequestMessage req = new MoveRequestMessage
                    {
                        PlayerID = bot.PlayerID,
                        TargetPosition = nextStep
                    };

                    NetworkSimulator.Instance.SendToServer(req);

                    bot.UpdateServerPosition(nextStep);
                }
            }

        }
    }

    private Vector3? GetClosestEgg(int botID, Vector3 botPosition) //with Stickiness
    {
        float minDistance = float.MaxValue;
        EggState? bestEgg = null;

        int currentTargetID = botTargetEggs.ContainsKey(botID) ? botTargetEggs[botID] : -1;

        foreach (EggState egg in latestGameState.Eggs)
        {
            float dist = Vector3.Distance(botPosition, egg.Position);

            float adjustedDist = (egg.EggID == currentTargetID) ? (dist - 1.5f) : dist;

            if (adjustedDist < minDistance)
            {
                minDistance = adjustedDist;
                bestEgg = egg;
            }
        }

        if (bestEgg.HasValue)
        {
            botTargetEggs[botID] = bestEgg.Value.EggID;
            return bestEgg.Value.Position;
        }

        return null;
    }

}
