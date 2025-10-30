using UnityEngine;
using Random = UnityEngine.Random;

public class PipeGridManager : MonoBehaviour
{
    [Header("Grid Settings")] public int gridWidth = 5;
    public int gridHeight = 5;
    public float cellSize = 100F;

    [Header("References")] public Pipe pipePrefab;

    private PipeTile[,] gridData;
    private Pipe[,] pipes;
    private Vector2Int startTile;

    private static readonly Color StartTint = new Color(0.6F, 1F, 0.6F);
    private static readonly Color ErrorTint = new Color(1F, 0.5F, 0.5F);
    private static readonly Color NormalTint = Color.white;


    private void Start()
    {
        GenerateTestGrid();
        ChooseStartTileOnce();
        InstantiateGridViews();
        ValidateAndHighlight();
    }


    private void GenerateTestGrid()
    {
        gridData = new PipeTile[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                gridData[x, y] = new PipeTile((PipeType)Random.Range(3, 4), 90 * Random.Range(0, 4));
            }
        }
    }


    private void ChooseStartTileOnce()
    {
        startTile = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));
    }


    private void InstantiateGridViews()
    {
        pipes = new Pipe[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Pipe pipe = Instantiate(pipePrefab, transform);
                pipe.name = $"Pipe({x},{y})";

                ((RectTransform)pipe.transform).anchoredPosition = new Vector2(x * cellSize, y * cellSize);

                pipe.Initialize(x, y, gridData[x, y], this);
                pipes[x, y] = pipe;
            }
        }
    }
    

    public void OnPipeRotated()
    {
        ValidateAndHighlight();
    }

    
    private void ValidateAndHighlight()
    {
        PipeValidationResult result = PipeGridValidator.ValidateGrid(gridData, startTile);

        foreach (var view in pipes)
        {
            view.SetColor(NormalTint);
        }

        if (!result.isFullyConnected)
        {
            foreach (var view in pipes)
            {
                Vector2Int p = new Vector2Int(view.GetX(), view.GetY());
                if (!result.connectedTiles.Contains(p) && p != startTile)
                {
                    view.SetColor(ErrorTint);
                }
            }
        }

        pipes[startTile.x, startTile.y].SetColor(StartTint);

        Debug.Log(result.isFullyConnected ? "All pipes connected!" : "Connection errors found.");
    }
}