using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Assets.Games;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Assets.PipesGame
{
    public class PipesGameManager : MonoBehaviour, IGameManager
    {
        [SerializeField] private Transform pipesBackgroundTransform;
        [SerializeField] private Transform instructionsBackgroundTransform;
        [SerializeField] private Transform levelCompleteBackgroundTransform;
        [SerializeField] private CanvasGroup pipesCanvasGroup;
        [SerializeField] private CanvasGroup instructionCanvasGroup;
        [SerializeField] private Button levelFinishButton;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private List<Pipe> pipePrefabs;

        private int gridWidth = 5;
        private int gridHeight = 5;
        private float cellSize = 150F;
        private float gameStartTime;
        private Pipe[,] pipes;
        private Vector2Int startPipeCoordinates;
        private List<int>[,] gridConnections;
        private bool levelFinished;


        private void Update()
        {
            UpdateTimer();
        }


        public void ContinueButton()
        {
            InstantiatePipes();
            ChooseStartPipe();
            ValidateAndHighlight();
            ShowPipes();
            StartTimer();
        }


        public void LevelFinishButton()
        {
            ShowLevelFinish();
        }


        public void BackButton()
        {
            GamesEventHandler.OnGameExited();
        }


        public void OnGameStarted()
        {
            SetUiPositions();
            ShowInstructions();
        }


        public void OnPipeRotated()
        {
            ValidateAndHighlight();
        }


        private void UpdateTimer()
        {
            if (levelFinished)
            {
                return;
            }

            float elapsedTime = Time.time - gameStartTime;
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);

            timerText.text = $"{minutes:00}:{seconds:00}";
        }


        private void StartTimer()
        {
            gameStartTime = Time.time;
        }


        private void SetUiPositions()
        {
            instructionsBackgroundTransform.localPosition = new Vector3(0F, -2000F, 0F);
            pipesBackgroundTransform.localPosition = new Vector3(0F, -2000F, 0F);
            levelCompleteBackgroundTransform.localPosition = new Vector3(0F, -2000F, 0F);
            instructionCanvasGroup.alpha = 0F;
        }


        private void ShowInstructions()
        {
            instructionsBackgroundTransform.transform.DOLocalMoveY(0F, 0.5F);
            instructionCanvasGroup.DOFade(1F, 0.5F);
        }


        private void ShowPipes()
        {
            pipesBackgroundTransform.transform.DOLocalMoveY(0F, 0.5F).OnComplete((() =>
            {
                pipesCanvasGroup.DOFade(1F, 0.5F);
            }));
        }


        private void ShowLevelFinish()
        {
            levelCompleteBackgroundTransform.transform.DOLocalMoveY(0F, 0.5F).OnComplete((() =>
            {
                pipesCanvasGroup.DOFade(1F, 0.5F);
            }));
        }


        private void InstantiatePipes()
        {
            pipes = new Pipe[gridWidth, gridHeight];

            startPipeCoordinates = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));

            gridConnections = GenerateConnections(startPipeCoordinates);

            Vector2 offset = 0.5f * cellSize * new Vector2(1 - gridWidth, 1 - gridHeight);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    DirectionsToTypeRotation(gridConnections[x, y], out PipeType type);

                    Pipe prefab = GetPipePrefab(type);

                    Pipe pipe = Instantiate(prefab, pipesCanvasGroup.transform);
                    pipe.name = $"Pipe({x},{y})";
                    ((RectTransform)pipe.transform).anchoredPosition = offset + new Vector2(x * cellSize, y * cellSize);

                    pipe.Initialize(x, y, 90 * Random.Range(0, 4), this);
                    pipes[x, y] = pipe;
                }
            }
        }


        private Pipe GetPipePrefab(PipeType pipeType)
        {
            switch (pipeType)
            {
                case PipeType.Straight: return pipePrefabs[1];
                case PipeType.Elbow: return pipePrefabs[2];
                case PipeType.Tee: return pipePrefabs[3];
                default: return pipePrefabs[0];
            }
        }


        private void ChooseStartPipe()
        {
            startPipeCoordinates = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));

            pipes[startPipeCoordinates.x, startPipeCoordinates.y].BecomeStartPipe();
        }


        private void ValidateAndHighlight()
        {
            PipeValidationResult result = ValidateConnections();

            foreach (var pipe in pipes)
            {
                pipe.IsConnected = true;
            }

            if (!result.isFullyConnected)
            {
                foreach (var pipe in pipes)
                {
                    Vector2Int p = new Vector2Int(pipe.GetX(), pipe.GetY());
                    if (!result.connectedTiles.Contains(p) && p != startPipeCoordinates)
                    {
                        pipe.IsConnected = false;
                    }
                }
            }

            foreach (var pipe in pipes)
            {
                pipe.SetColor();
            }

            pipes[startPipeCoordinates.x, startPipeCoordinates.y].BecomeStartPipe();

            if (result.isFullyConnected)
            {
                FinishLevel();
            }
        }


        private PipeValidationResult ValidateConnections()
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


        private enum PipeType
        {
            End,
            Straight,
            Elbow,
            Tee
        }


        private bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < gridWidth && y < gridHeight;


        private int Opp(int d) => (d + 2) % 4;


        private static Vector2Int DirOffset(int dir)
        {
            switch (dir)
            {
                case 0: return new Vector2Int(0, 1); // Up
                case 1: return new Vector2Int(1, 0); // Right
                case 2: return new Vector2Int(0, -1); // Down
                case 3: return new Vector2Int(-1, 0); // Left
                default: return Vector2Int.zero;
            }
        }


        private List<int>[,] GenerateConnections(Vector2Int start)
        {
            var connections = new List<int>[gridWidth, gridHeight];
            var visited = new bool[gridWidth, gridHeight];

            for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                connections[x, y] = new List<int>();

            var rng = new System.Random();
            var stack = new Stack<Vector2Int>();

            stack.Push(start);

            visited[start.x, start.y] = true;

            while (stack.Count > 0)
            {
                Vector2Int current = stack.Pop();

                int[] directions = { 0, 1, 2, 3 };
                directions = directions.OrderBy(_ => rng.Next()).ToArray(); // easy shuffle

                foreach (int dir in directions)
                {
                    Vector2Int next = current + DirOffset(dir);

                    if (!InBounds(next.x, next.y) || visited[next.x, next.y])
                    {
                        continue;
                    }

                    connections[current.x, current.y].Add(dir);
                    connections[next.x, next.y].Add(Opp(dir));

                    visited[next.x, next.y] = true;

                    stack.Push(current);
                    stack.Push(next);

                    break;
                }
            }

            return connections;
        }


        private static void DirectionsToTypeRotation(List<int> directions, out PipeType type)
        {
            var list = new List<int>(directions);

            switch (list.Count)
            {
                case 1:
                    type = PipeType.End;
                    break;
                case 2:
                    bool isStraight = (list.Contains(0) && list.Contains(2)) || (list.Contains(1) && list.Contains(3));
                    type = isStraight ? PipeType.Straight : PipeType.Elbow;
                    break;
                case 3:
                    type = PipeType.Tee;
                    break;
                default:
                    Debug.Log("There is a problem here.");
                    type = PipeType.End;
                    break;
            }
        }


        private void FinishLevel()
        {
            levelFinished = true;
            
            foreach (var pipe in pipes)
            {
                pipe.DisableInteraction();
            }

            StartCoroutine(AnimatePipesInConnectionOrder());
        }


        private IEnumerator AnimatePipesInConnectionOrder()
        {
            yield return new WaitForSeconds(0.25F);

            levelFinishButton.gameObject.SetActive(true);
            
            var visited = new bool[gridWidth][];
            for (int index = 0; index < gridWidth; index++)
            {
                visited[index] = new bool[gridHeight];
            }

            visited[startPipeCoordinates.x][startPipeCoordinates.y] = true;

            var queue = new Queue<Vector2Int>();
            queue.Enqueue(startPipeCoordinates);

            var currentWave = new List<Vector2Int>();

            while (queue.Count > 0)
            {
                currentWave.Clear();
                int waveCount = queue.Count;

                for (int i = 0; i < waveCount; i++)
                {
                    Vector2Int current = queue.Dequeue();
                    currentWave.Add(current);

                    foreach (int dir in pipes[current.x, current.y].GetRotatedConnections())
                    {
                        Vector2Int next = current + GetOffset(dir);

                        if (!InBounds(next.x, next.y) || visited[next.x][next.y])
                        {
                            continue;
                        }

                        int oppositeDir = GetOppositeDirection(dir);

                        if (!pipes[next.x, next.y].GetRotatedConnections().Contains(oppositeDir))
                        {
                            continue;
                        }

                        visited[next.x][next.y] = true;
                        queue.Enqueue(next);
                    }
                }

                foreach (Vector2Int pos in currentWave)
                {
                    Pipe pipe = pipes[pos.x, pos.y];
                    if (pipe != null)
                    {
                        pipe.ShowWinAnimation();
                    }
                }

                yield return new WaitForSeconds(0.1F);
            }
        }
    }
}