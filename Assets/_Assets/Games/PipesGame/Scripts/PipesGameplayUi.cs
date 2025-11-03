using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _Assets.Games;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Assets.PipesGame
{
    public class PipesGameplayUi : MonoBehaviour
    {
        [SerializeField] private CanvasGroup pipesCanvasGroup;
        [SerializeField] private Button continueButton;
        [SerializeField] private Transform pipeContainerTransform;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private List<Pipe> pipePrefabs;
        [SerializeField] private List<ParticleSystem> confettiParticles;

        private int gridWidth = 5;
        private int gridHeight = 5;
        private float cellSize = 150F;
        private float gameStartTime;
        private Pipe[,] pipes;
        private Vector2Int startPipeCoordinates;
        private List<int>[,] gridConnections;
        private bool levelFinished;
        private Sequence levelFinishSequence;


        private void Update()
        {
            UpdateTimer();
        }


        public void Activate()
        {
            gameObject.SetActive(true);

            if (!TryLoadGame())
            {
                InstantiatePipes();
                ChooseStartPipe();
                StartTimer();
            }

            ValidateAndHighlight();
            ShowPipes();
        }


        private void ShowPipes()
        {
            transform.DOLocalMoveY(0F, 0.5F).OnComplete((() => { pipesCanvasGroup.DOFade(1F, 0.5F); }));
        }


        public void OnPipeRotated()
        {
            ValidateAndHighlight();

            SaveGame();
        }


        public void ContinueButton()
        {
            GamesEventHandler.OnGameplayCompleted(timerText.text);

            DeleteSave();
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

                    Pipe pipe = Instantiate(prefab, pipeContainerTransform);
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
                    Vector2Int p = new Vector2Int(pipe.X, pipe.Y);
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

            if (result.isFullyConnected)
            {
                OnAllPipesConnected();
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
                case 0: return new Vector2Int(0, 1);
                case 1: return new Vector2Int(1, 0);
                case 2: return new Vector2Int(0, -1);
                case 3: return new Vector2Int(-1, 0);
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


        private void OnAllPipesConnected()
        {
            levelFinished = true;

            foreach (var pipe in pipes)
            {
                pipe.DisableInteraction();
            }

            DeleteSave();

            StartCoroutine(AnimatePipesInConnectionOrder());
        }


        private IEnumerator AnimatePipesInConnectionOrder()
        {
            yield return new WaitForSeconds(0.25F);

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

                yield return new WaitForSeconds(0.075F);
            }

            foreach (var confettiParticle in confettiParticles)
            {
                confettiParticle.Play();
            }

            pipeContainerTransform.DOScale(1.1F, 0.4F).SetEase(Ease.OutExpo).OnComplete((() =>
            {
                pipeContainerTransform.DOScale(1F, 0.4F).SetEase(Ease.InExpo);
            }));

            levelFinishSequence = DOTween.Sequence().Append(pipeContainerTransform
                .DOLocalRotate(new Vector3(0, 0, -5), 0.1F)
                .SetEase(Ease.InOutSine));

            for (int i = 0; i < 3; i++)
            {
                levelFinishSequence.Append(pipeContainerTransform.DOLocalRotate(new Vector3(0, 0, 5), 0.1F)
                    .SetEase(Ease.InOutSine));
                levelFinishSequence.Append(pipeContainerTransform.DOLocalRotate(new Vector3(0, 0, -5), 0.1F)
                    .SetEase(Ease.InOutSine));
            }

            levelFinishSequence.Append(pipeContainerTransform.DOLocalRotate(Vector3.zero, 0.1F)
                .SetEase(Ease.InOutSine));
            levelFinishSequence
                .Append(pipeContainerTransform.DOScale(1.05F, 1F).SetDelay(0.15F).SetLoops(10, LoopType.Yoyo)).Play();

            continueButton.gameObject.SetActive(true);
        }


        private void OnDestroy()
        {
            levelFinishSequence.Kill();
        }


        //save load

        [System.Serializable]
        public class PipeData
        {
            public int x;
            public int y;
            public int rotation;
            public List<int> connections;
        }

        [System.Serializable]
        public class BoardData
        {
            public int width;
            public int height;
            public int startX;
            public int startY;
            public float elapsedSeconds;
            public List<PipeData> cells;
        }


        private const string SaveKey = "PipesGameSaveKey";

        private bool CanSave => gameObject.activeInHierarchy && !levelFinished;


        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                SaveGame();
            }
        }


        private void OnApplicationQuit()
        {
            SaveGame();
        }


        private void SaveGame()
        {
            if (!CanSave || pipes == null)
            {
                return;
            }

            BoardData boardData = new BoardData
            {
                width = gridWidth,
                height = gridHeight,
                startX = startPipeCoordinates.x,
                startY = startPipeCoordinates.y,
                elapsedSeconds = Time.time - gameStartTime,
                cells = new List<PipeData>(gridWidth * gridHeight)
            };

            for (int a = 0; a < gridWidth; a++)
            {
                for (int b = 0; b < gridHeight; b++)
                {
                    boardData.cells.Add(new PipeData
                    {
                        x = a,
                        y = b,
                        rotation = pipes[a, b].Rotation,
                        connections = new List<int>(gridConnections[a, b])
                    });
                }
            }

            string json = JsonUtility.ToJson(boardData, false);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }


        private bool TryLoadGame()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                return false;
            }

            string json = PlayerPrefs.GetString(SaveKey, string.Empty);

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            BoardData boardData = JsonUtility.FromJson<BoardData>(json);

            if (boardData == null || boardData.cells == null || boardData.cells.Count == 0)
            {
                return false;
            }

            gridWidth = boardData.width;
            gridHeight = boardData.height;
            startPipeCoordinates = new Vector2Int(boardData.startX, boardData.startY);
            pipes = new Pipe[gridWidth, gridHeight];
            gridConnections = new List<int>[gridWidth, gridHeight];

            Vector2 offset = 0.5f * cellSize * new Vector2(1 - gridWidth, 1 - gridHeight);

            foreach (var cell in boardData.cells)
            {
                gridConnections[cell.x, cell.y] = new List<int>(cell.connections);

                DirectionsToTypeRotation(gridConnections[cell.x, cell.y], out PipeType type);
             
                Pipe pipe = Instantiate(GetPipePrefab(type), pipeContainerTransform);
                pipe.name = $"Pipe({cell.x},{cell.y})";
                ((RectTransform)pipe.transform).anchoredPosition = offset + new Vector2(cell.x * cellSize, cell.y * cellSize);
                pipe.Initialize(cell.x, cell.y, cell.rotation, this);
                pipes[cell.x, cell.y] = pipe;
            }

            pipes[startPipeCoordinates.x, startPipeCoordinates.y].BecomeStartPipe();
            
            gameStartTime = Time.time - Mathf.Max(0f, boardData.elapsedSeconds);

            return true;
        }


        private void DeleteSave()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                PlayerPrefs.DeleteKey(SaveKey);
                PlayerPrefs.Save();
            }
        }
    }
}