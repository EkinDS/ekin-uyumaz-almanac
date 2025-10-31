using System.Collections.Generic;
using _Assets.Games;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Assets.PipesGame
{
    public class PipesGameManager : MonoBehaviour, IGameManager
    {
        [SerializeField] private Transform pipesBackgroundTransform;
        [SerializeField] private Transform instructionsBackgroundTransform;
        [SerializeField] private CanvasGroup instructionCanvasGroup;
        [SerializeField] private List<Pipe> pipePrefabs;

        private int gridWidth = 5;
        private int gridHeight = 5;
        private float cellSize = 150F;
        private Pipe[,] pipes;
        private Vector2Int startPipeCoordinates;


        public void ContinueButton()
        {
            ShowPipes();
        }


        public void BackButton()
        {
            GamesEventHandler.OnGameExited();
        }
        
        
        public void OnGameStarted()
        {
            SetUiPositions();
            ShowInstructions();
            InstantiateGridViews();
            ChooseStartPipe();
            ValidateAndHighlight();
        }


        public void OnPipeRotated()
        {
            ValidateAndHighlight();
        }


        private void SetUiPositions()
        {
            instructionsBackgroundTransform.transform.localPosition = new Vector3(0F, -2000F, 0F);
            pipesBackgroundTransform.transform.localPosition = new Vector3(0F, -2000F, 0F);
            instructionCanvasGroup.alpha = 0F;
        }
        
        
        private void ShowInstructions()
        {
            instructionsBackgroundTransform.transform.DOLocalMoveY(0F, 0.5F);
            instructionCanvasGroup.DOFade(1F, 0.5F);
        }


        private void ShowPipes()
        {
            pipesBackgroundTransform.transform.DOLocalMoveY(0F, 0.5F);
        }
        

        private void InstantiateGridViews()
        {
            pipes = new Pipe[gridWidth, gridHeight];

            Vector2 offset = 0.5F * cellSize * new Vector2(1 - gridWidth, 1 - gridHeight);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Pipe pipe = Instantiate(pipePrefabs[Random.Range(0, 4)], pipesBackgroundTransform);
                    pipe.name = $"Pipe({x},{y})";

                    ((RectTransform)pipe.transform).anchoredPosition = offset + new Vector2(x * cellSize, y * cellSize);

                    pipe.Initialize(x, y, 90 * Random.Range(0, 4), this);
                    pipes[x, y] = pipe;
                }
            }
        }


        private void ChooseStartPipe()
        {
            startPipeCoordinates = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));

            pipes[startPipeCoordinates.x, startPipeCoordinates.y].BecomeStartPipe();
        }


        private void ValidateAndHighlight()
        {
            PipeValidationResult result = ValidateGrid();

            foreach (var view in pipes)
            {
                view.BecomeConnected();
            }

            if (!result.isFullyConnected)
            {
                foreach (var view in pipes)
                {
                    Vector2Int p = new Vector2Int(view.GetX(), view.GetY());
                    if (!result.connectedTiles.Contains(p) && p != startPipeCoordinates)
                    {
                        view.BecomeDisconnected();
                    }
                }
            }

            pipes[startPipeCoordinates.x, startPipeCoordinates.y].BecomeStartPipe();

            Debug.Log(result.isFullyConnected ? "All pipes connected!" : "Connection errors found.");
        }


        private PipeValidationResult ValidateGrid()
        {
            int width = pipes.GetLength(0);
            int height = pipes.GetLength(1);

            var result = new PipeValidationResult
            {
                connectedTiles = new HashSet<Vector2Int>()
            };

            var queue = new Queue<Vector2Int>();
            queue.Enqueue(startPipeCoordinates);
            result.connectedTiles.Add(startPipeCoordinates);

            while (queue.Count > 0)
            {
                var tileToDequeue = queue.Dequeue();

                foreach (int direction in pipes[tileToDequeue.x, tileToDequeue.y].GetRotatedConnections())
                {
                    var tileToAnalyze = tileToDequeue + GetOffset(direction);
                    if (!(tileToAnalyze.x >= 0 && tileToAnalyze.y >= 0 && tileToAnalyze.x < width &&
                          tileToAnalyze.y < height))
                    {
                        continue;
                    }

                    int oppositeDirection = GetOppositeDirection(direction);
                    if (!pipes[tileToAnalyze.x, tileToAnalyze.y].GetRotatedConnections().Contains(oppositeDirection))
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


        private int GetOppositeDirection(int direction)
        {
            return (direction + 2) % 4;
        }


        private Vector2Int GetOffset(int direction)
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


        private struct PipeValidationResult
        {
            public bool isFullyConnected;
            public HashSet<Vector2Int> connectedTiles;
        }
    }
}