using System;
using System.Collections.Generic;
using UnityEngine;


public static class PipeGridValidator
{
    public static PipeValidationResult ValidateGrid(PipeTile[,] grid, Vector2Int startTile)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        var result = new PipeValidationResult
        {
            connectedTiles = new HashSet<Vector2Int>()
        };

        var queue = new Queue<Vector2Int>();
        queue.Enqueue(startTile);
        result.connectedTiles.Add(startTile);
        
        while (queue.Count > 0)
        {
            var tileToDequeue = queue.Dequeue();

            foreach (int direction in grid[tileToDequeue.x, tileToDequeue.y].GetRotatedConnections())
            {
                var tileToAnalyze = tileToDequeue + PipeTile.GetOffset(direction);
                if (!( tileToAnalyze.x >= 0 && tileToAnalyze.y >= 0 && tileToAnalyze.x < width && tileToAnalyze.y < height))
                {
                    continue;
                }

                int oppositeDirection = PipeTile.GetOppositeDirection(direction);
                if (!grid[tileToAnalyze.x, tileToAnalyze.y].GetRotatedConnections().Contains(oppositeDirection))
                {
                    continue;
                }

                if (result.connectedTiles.Add(tileToAnalyze))
                {
                    queue.Enqueue(tileToAnalyze);
                }
            }
        }

        result.isFullyConnected = result.connectedTiles.Count == width * height;
        return result;
    }
}


public struct PipeValidationResult
{
    public bool isFullyConnected;
    public HashSet<Vector2Int> connectedTiles;
}