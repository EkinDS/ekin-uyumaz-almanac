using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Pipe : MonoBehaviour, IPointerClickHandler
{
    [Header("Pipe Visuals")] [SerializeField]
    private GameObject straight;

    [SerializeField] private GameObject elbow;
    [SerializeField] private GameObject tee;
    [SerializeField] private GameObject end;

    private PipeGridManager gridManager;
    private Image thisImage;
    private PipeTile tileData;
    private int gridX;
    private int gridY;


    public void Initialize(int x, int y, PipeTile data, PipeGridManager manager)
    {
        thisImage = GetComponent<Image>();

        gridX = x;
        gridY = y;
        tileData = data;
        gridManager = manager;

        UpdateVisualType();
        ApplyRotationInstant();
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        tileData.rotation = (tileData.rotation + 90) % 360;

        ApplyRotationInstant();

        gridManager?.OnPipeRotated();
    }


    public void SetColor(Color c)
    {
        thisImage.color = c;
    }


    public int GetX()
    {
        return gridX;
    }


    public int GetY()
    {
        return gridY;
    }


    private void UpdateVisualType()
    {
        if (straight) straight.SetActive(false);
        if (elbow) elbow.SetActive(false);
        if (tee) tee.SetActive(false);
        if (end) end.SetActive(false);

        switch (tileData.type)
        {
            case PipeType.Straight:
                if (straight) straight.SetActive(true);
                break;
            case PipeType.Elbow:
                if (elbow) elbow.SetActive(true);
                break;
            case PipeType.Tee:
                if (tee) tee.SetActive(true);
                break;
            case PipeType.End:
                if (end) end.SetActive(true);
                break;
            default: Debug.LogWarning($"Unhandled PipeType {tileData.type}"); break;
        }
    }

    private void ApplyRotationInstant()
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, -tileData.rotation);
    }
}