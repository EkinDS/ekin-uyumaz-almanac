using System;
using System.Collections.Generic;
using _Assets.Games;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Assets.PipesGame
{
    public class PipesGameManager : MonoBehaviour, IGameManager
    {
        [SerializeField] private Transform pipesBackgroundTransform;
        [SerializeField] private Transform instructionsBackgroundTransform;
        [SerializeField] private CanvasGroup pipesCanvasGroup;
        [SerializeField] private CanvasGroup instructionCanvasGroup;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private List<Pipe> pipePrefabs;

        private int gridWidth = 5;
        private int gridHeight = 5;
        private float cellSize = 150F;
        private float gameStartTime;
        private Pipe[,] pipes;
        private Vector2Int startPipeCoordinates;


        private void Update()
        {
            float elapsed = Time.time - gameStartTime;
            int minutes = Mathf.FloorToInt(elapsed / 60f);
            int seconds = Mathf.FloorToInt(elapsed % 60f);

            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        public void ContinueButton()
        {
            InstantiatePipesSolved();
            ChooseStartPipe();
            ValidateAndHighlight();
            ShowPipes();
            StartTimer();
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


        public void StartTimer()
        {
            gameStartTime = Time.time;
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
            pipesBackgroundTransform.transform.DOLocalMoveY(0F, 0.5F).OnComplete((() =>
            {
                pipesCanvasGroup.DOFade(1F, 0.5F);
            }));
        }

/*
        private void InstantiatePipes()
        {
            pipes = new Pipe[gridWidth, gridHeight];

            Vector2 offset = 0.5F * cellSize * new Vector2(1 - gridWidth, 1 - gridHeight);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Pipe pipe = Instantiate(pipePrefabs[Random.Range(0, 4)], pipesCanvasGroup.transform);
                    pipe.name = $"Pipe({x},{y})";

                    ((RectTransform)pipe.transform).anchoredPosition = offset + new Vector2(x * cellSize, y * cellSize);

                    pipe.Initialize(x, y, 90 * Random.Range(0, 4), this);
                    pipes[x, y] = pipe;
                }
            }
        }
*/

/*
        private void InstantiatePipes()
        {
            pipes = new Pipe[gridWidth, gridHeight];

            // pick a start and build a non-leaking, fully-connected network
            var start = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));
            int[,] mask = GenerateConnectionGrid(gridWidth, gridHeight, start);

            Vector2 offset = 0.5f * cellSize * new Vector2(1 - gridWidth, 1 - gridHeight);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    // decide which prefab + rotation this cell needs
                    MaskToTypeRotation(mask[x, y], out PipeType type, out int rotation);

                    // pick the correct prefab for this type (set these via Inspector)
                    Pipe prefab = GetPrefabFor(type); // implement as a switch/dictionary

                    Pipe pipe = Instantiate(prefab, pipesCanvasGroup.transform);
                    pipe.name = $"Pipe({x},{y})";
                    ((RectTransform)pipe.transform).anchoredPosition = offset + new Vector2(x * cellSize, y * cellSize);

                    // Optionally scramble initial rotation to create the puzzle state:
                    int scrambled = (rotation + 90 * Random.Range(0, 4)) % 360;

                    pipe.Initialize(x, y, scrambled, this);
                    pipes[x, y] = pipe;
                }
            }

            // store start for validation/highlight if you use one
            startPipeCoordinates = start;
        }
*/


        private void InstantiatePipesSolved()
        {
            pipes = new Pipe[gridWidth, gridHeight];

            // choose a start for validation/highlight if you use one
            startPipeCoordinates = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));

            int[,] mask = GenerateConnectionMask(startPipeCoordinates); // fully connected, no leaks

            Vector2 offset = 0.5f * cellSize * new Vector2(1 - gridWidth, 1 - gridHeight);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    // pick correct piece + rotation to MATCH the mask
                    MaskToTypeRotation(mask[x, y], out PipeType type, out int rotation);

                    // choose prefab for this type (assign in Inspector)
                    Pipe prefab = GetPrefabFor(type);

                    Pipe pipe = Instantiate(prefab, pipesCanvasGroup.transform);
                    pipe.name = $"Pipe({x},{y})";
                    ((RectTransform)pipe.transform).anchoredPosition = offset + new Vector2(x * cellSize, y * cellSize);

                    // IMPORTANT: use the exact rotation (do NOT scramble) → solved
                    pipe.Initialize(x, y, rotation, this);
                    pipes[x, y] = pipe;
                }
            }
        }


        private Pipe GetPrefabFor(PipeType t)
        {
            switch (t)
            {
                case PipeType.End: return pipePrefabs[0];
                case PipeType.Straight: return pipePrefabs[1];
                case PipeType.Elbow: return pipePrefabs[2];
                case PipeType.Tee: return pipePrefabs[3];
            }

            return pipePrefabs[0];
        }


        private void ChooseStartPipe()
        {
            startPipeCoordinates = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));

            pipes[startPipeCoordinates.x, startPipeCoordinates.y].BecomeStartPipe();
        }


        private void ValidateAndHighlight()
        {
            PipeValidationResult result = ValidateConnections();

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


        //new stuff
        


        bool InBounds(int x, int y, int w, int h) => x >= 0 && y >= 0 && x < w && y < h;


        int[,] GenerateConnectionGrid(int w, int h, Vector2Int start, int extraEdges = -1)
        {
            if (extraEdges < 0) extraEdges = (w * h) / 6; // small loops

            int[,] mask = new int[w, h];
            bool[,] vis = new bool[w, h];

            // randomized DFS spanning tree
            var stack = new Stack<Vector2Int>();
            stack.Push(start);
            vis[start.x, start.y] = true;

            var rnd = new System.Random();

            while (stack.Count > 0)
            {
                var cur = stack.Pop();

                // random order of directions
                int[] order = { 0, 1, 2, 3 };
                for (int i = 0; i < 4; i++)
                {
                    int j = rnd.Next(i, 4);
                    (order[i], order[j]) = (order[j], order[i]);
                }

                foreach (int dir in order)
                {
                    var n = cur + Offsets[dir];
                    if (!InBounds(n.x, n.y, w, h) || vis[n.x, n.y]) continue;

                    // carve passage both sides
                    mask[cur.x, cur.y] |= DirBits[dir];
                    mask[n.x, n.y] |= DirBits[Opp(dir)];

                    vis[n.x, n.y] = true;
                    stack.Push(n);
                    stack.Push(cur); // revisit to try other neighbors
                    break;
                }
            }

            // add a few extra connections to make nicer shapes (degree ≤ 3)
            int tries = 0;
            while (extraEdges > 0 && tries < w * h * 10)
            {
                tries++;
                int x = rnd.Next(0, w);
                int y = rnd.Next(0, h);
                int dir = rnd.Next(0, 4);
                var n = new Vector2Int(x, y) + Offsets[dir];
                if (!InBounds(n.x, n.y, w, h)) continue;

                // already connected?
                if ((mask[x, y] & DirBits[dir]) != 0) continue;

                // degree limits so we don't need Cross pieces
                int degA = CountBits(mask[x, y]);
                int degB = CountBits(mask[n.x, n.y]);
                if (degA >= 3 || degB >= 3) continue;

                mask[x, y] |= DirBits[dir];
                mask[n.x, n.y] |= DirBits[Opp(dir)];
                extraEdges--;
            }

            return mask;
        }


        int FirstDir(int m)
        {
            if ((m & U) != 0) return 0;
            if ((m & R) != 0) return 1;
            if ((m & D) != 0) return 2;
            return 3; // L
        }

        int DirectionToRot(int dir) => dir * 90;

        public enum PipeType
        {
            End,
            Straight,
            Elbow,
            Tee
        }

        //newnew


        // ---- Directions as bits ----
        const int U = 1, R = 2, D = 4, L = 8;

        static readonly Vector2Int[] Offsets =
        {
            new Vector2Int(0, 1), // Up    (0)
            new Vector2Int(1, 0), // Right (1)
            new Vector2Int(0, -1), // Down  (2)
            new Vector2Int(-1, 0), // Left  (3)
        };

        static readonly int[] DirBits = { U, R, D, L };

        bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < gridWidth && y < gridHeight;
        int Opp(int d) => (d + 2) % 4;
        int DirToRot(int d) => d * 90;

        int CountBits(int m)
        {
            int c = 0;
            if ((m & U) != 0) c++;
            if ((m & R) != 0) c++;
            if ((m & D) != 0) c++;
            if ((m & L) != 0) c++;
            return c;
        }

// randomized DFS spanning tree + a few extra edges (loops) with degree<=3
        int[,] GenerateConnectionMask(Vector2Int start, int extraEdges = -1)
        {
            if (extraEdges < 0) extraEdges = (gridWidth * gridHeight) / 6;

            int[,] mask = new int[gridWidth, gridHeight];
            bool[,] vis = new bool[gridWidth, gridHeight];

            var stack = new Stack<Vector2Int>();
            stack.Push(start);
            vis[start.x, start.y] = true;

            var rnd = new System.Random();

            while (stack.Count > 0)
            {
                var cur = stack.Pop();

                // randomize directions
                int[] order = { 0, 1, 2, 3 };
                for (int i = 0; i < 4; i++)
                {
                    int j = rnd.Next(i, 4);
                    (order[i], order[j]) = (order[j], order[i]);
                }

                bool carved = false;
                foreach (int dir in order)
                {
                    var n = cur + Offsets[dir];
                    if (!InBounds(n.x, n.y) || vis[n.x, n.y]) continue;

                    mask[cur.x, cur.y] |= DirBits[dir];
                    mask[n.x, n.y] |= DirBits[Opp(dir)];
                    vis[n.x, n.y] = true;

                    stack.Push(cur); // come back to try other dirs
                    stack.Push(n);
                    carved = true;
                    break;
                }

                if (!carved && stack.Count == 0)
                {
                    // finished DFS
                    break;
                }
            }

            // add a few extra edges to reduce dead-ends, but keep degree <= 3 (so no Cross pieces needed)
            int attempts = 0;
            while (extraEdges > 0 && attempts < gridWidth * gridHeight * 10)
            {
                attempts++;
                int x = rnd.Next(0, gridWidth);
                int y = rnd.Next(0, gridHeight);
                int dir = rnd.Next(0, 4);
                var n = new Vector2Int(x, y) + Offsets[dir];
                if (!InBounds(n.x, n.y)) continue;

                if ((mask[x, y] & DirBits[dir]) != 0) continue; // already connected

                int degA = CountBits(mask[x, y]);
                int degB = CountBits(mask[n.x, n.y]);
                if (degA >= 3 || degB >= 3) continue;

                mask[x, y] |= DirBits[dir];
                mask[n.x, n.y] |= DirBits[Opp(dir)];
                extraEdges--;
            }

            return mask;
        }

        void MaskToTypeRotation(int m, out PipeType type, out int rotation)
        {
            int deg = CountBits(m);

            if (deg == 1)
            {
                type = PipeType.End;
                rotation = DirToRot((m & U) != 0 ? 0 : (m & R) != 0 ? 1 : (m & D) != 0 ? 2 : 3);
                return;
            }

            if (deg == 2)
            {
                // straight (opposites) or elbow (adjacent)
                bool vertical = (m & U) != 0 && (m & D) != 0;
                bool horizontal = (m & R) != 0 && (m & L) != 0;

                if (vertical || horizontal)
                {
                    type = PipeType.Straight;
                    rotation = vertical ? 0 : 90;
                    return;
                }

                type = PipeType.Elbow;
                if ((m & (U | R)) == (U | R))
                {
                    rotation = 0;
                    return;
                }

                if ((m & (R | D)) == (R | D))
                {
                    rotation = 90;
                    return;
                }

                if ((m & (D | L)) == (D | L))
                {
                    rotation = 180;
                    return;
                }

                /* L|U */
                {
                    rotation = 270;
                    return;
                }
            }

            if (deg == 3)
            {
                type = PipeType.Tee;
                // rotation points to the missing side
                if ((m & D) == 0)
                {
                    rotation = 0;
                    return;
                } // open U,R,L

                if ((m & L) == 0)
                {
                    rotation = 90;
                    return;
                } // open U,R,D

                if ((m & U) == 0)
                {
                    rotation = 180;
                    return;
                } // open R,D,L

                /* missing R */
                {
                    rotation = 270;
                    return;
                } // open U,D,L
            }

            // deg==4 would be cross; we avoid generating it
            // Fallback:
            type = PipeType.End;
            rotation = 0;
        }
    }
}