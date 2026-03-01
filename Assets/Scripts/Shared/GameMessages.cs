using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseMessage { }

public class MoveRequestMessage : BaseMessage
{
    public int PlayerID;
    public Vector3 TargetPosition;
}

public class GameStateUpdateMessage : BaseMessage
{
    public PlayerState[] Players;
    public EggState[] Eggs;
}

[System.Serializable]
public struct PlayerState
{
    public int PlayerID;
    public Vector3 Position;
    public int Score;
}

[System.Serializable]
public struct EggState
{
    public int EggID;
    public Vector3 Position;
    public Color EggColor;
}
