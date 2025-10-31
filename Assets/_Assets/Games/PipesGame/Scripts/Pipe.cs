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

        private PipesGameManager _gameManager;
        private int gridX;
        private int gridY;

        public int rotation;


        public void Initialize(int x, int y, int newRotation, PipesGameManager manager)
        {
            gridX = x;
            gridY = y;
            rotation = newRotation;
            _gameManager = manager;

            Rotate(false);
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            rotation = (rotation + 90) % 360;

            Rotate(true);

            _gameManager?.OnPipeRotated();
        }


        public void BecomeConnected()
        {
            foreach (var connectionIndicatorImage in connectionIndicatorImages)
            {
                connectionIndicatorImage.color = Color.cornflowerBlue;
            }
        }


        public void BecomeDisconnected()
        {
            foreach (var connectionIndicatorImage in connectionIndicatorImages)
            {
                connectionIndicatorImage.color = Color.white;
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


        public void BecomeNormalPipe()
        {
            startPipeIndicatorImage.gameObject.SetActive(false);
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