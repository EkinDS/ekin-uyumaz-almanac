using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PipeTile
{
    public PipeType type;
    public int rotation; // 0, 90, 180, 270 degrees


    public PipeTile(PipeType type, int rotation)
    {
        this.type = type;
        this.rotation = rotation;
    }
    
    
    private static readonly Dictionary<PipeType, int[]> BaseConnections = new Dictionary<PipeType, int[]>
    {
        { PipeType.End,      new[] { 0 } },          // Up
        { PipeType.Straight, new[] { 0, 2 } },       // Up & Down
        { PipeType.Elbow,    new[] { 0, 1 } },       // Up & Right
        { PipeType.Tee,      new[] { 0, 1, 3 } }     // Up, Right & Left
    };
    
    
    public List<int> GetRotatedConnections()
    {
        var directions = new List<int>();

        if (BaseConnections.TryGetValue(type, out int[] baseDirs))
        {
            int rotationSteps = (rotation % 360 + 360) % 360 / 90;

            foreach (int dir in baseDirs)
            {
                directions.Add((dir + rotationSteps) % 4);
            }
        }

        return directions;
    }


    public static int GetOppositeDirection(int direction)
    {
        return (direction + 2) % 4;
    }

    
    public static Vector2Int GetOffset(int direction)
    {
        switch (direction)
        {
            case (int)PipeConnectionDirection.Up: return new Vector2Int(0, 1);
            case (int)PipeConnectionDirection.Right: return new Vector2Int(1, 0);
            case (int)PipeConnectionDirection.Down: return new Vector2Int(0, -1);
            case (int)PipeConnectionDirection.Left: return new Vector2Int(-1, 0);
            default: return Vector2Int.zero;
        }
    }
}