using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Assets.PipesGame
{
    public class Pipe : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image startPipeIndicatorImage;
        [SerializeField] private Transform containerTransform;
        [SerializeField] private List<Image> connectionIndicatorImages;
        [SerializeField] private List<int> baseConnections;

        private PipesGameManager pipesGameManager;
        private Image thisImage;
        private int gridX;
        private int gridY;
        private int rotation;
        private Color connectedColor = new Color(0.46F, 0.6F, 0.8F, 1F);
        private Color disconnectedColor = new Color(0.95F, 0.94F, 0.92F, 1F);
        private Color winColor = new Color(0.78F, 0.87F, 1F, 1F);
        private bool interactionDisabled;

        public bool IsConnected { get; set; }


        public void Initialize(int x, int y, int newRotation, PipesGameManager manager)
        {
            gridX = x;
            gridY = y;
            rotation = newRotation;
            pipesGameManager = manager;

            Rotate(false);
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if (interactionDisabled)
            {
                return;
            }
            
            rotation = (rotation + 90) % 360;

            Rotate(true);

            pipesGameManager?.OnPipeRotated();
        }


        public void SetColor()
        {
            foreach (var connectionIndicatorImage in connectionIndicatorImages)
            {
                connectionIndicatorImage.DOColor(IsConnected ? connectedColor : disconnectedColor, 0.2F);
            }
        }


        public void ShowWinAnimation()
        {
            foreach (var connectionIndicatorImage in connectionIndicatorImages)
            {
                connectionIndicatorImage.DOColor(winColor, 0.2F).OnComplete((() =>
                {
                    connectionIndicatorImage.DOColor(connectedColor, 0.2F);
                }));
            }
        }


        public int GetX()
        {
            return gridX;
        }


        public int GetY()
        {
            return gridY;
        }


        public List<int> GetRotatedConnections()
        {
            var directions = new List<int>();

            int rotationSteps = (rotation % 360 + 360) % 360 / 90;

            foreach (int baseConnection in baseConnections)
            {
                directions.Add((baseConnection + rotationSteps) % 4);
            }

            return directions;
        }


        public void BecomeStartPipe()
        {
            startPipeIndicatorImage.gameObject.SetActive(true);
        }


        public void DisableInteraction()
        {
            interactionDisabled = true;
        }


        private void Rotate(bool animate)
        {
            if (animate)
            {
                containerTransform.DOLocalRotate(new Vector3(0f, 0f, -rotation), 0.15f).SetEase(Ease.OutQuad);
                containerTransform.DOScale(0.8f, 0.075f).SetEase(Ease.OutQuad).OnComplete(() =>
                    containerTransform.DOScale(1f, 0.075f).SetEase(Ease.OutQuad));
            }
            else
            {
                containerTransform.localRotation = Quaternion.Euler(new Vector3(0F, 0F, -rotation));
            }
        }
    }
}