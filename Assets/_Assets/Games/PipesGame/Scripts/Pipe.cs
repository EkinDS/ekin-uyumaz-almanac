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

        private Color connectedColor = new Color(0.46F, 0.6F, 0.8F, 1F);
        private Color disconnectedColor = new Color(0.95F, 0.94F, 0.92F, 1F);
        private Color winColor = new Color(0.78F, 0.87F, 1F, 1F);
        private Image thisImage;
        private PipesGameplayUi pipesGameplayUi;
        private bool interactionDisabled;
      
        
        public bool IsConnected { get; set; }
        
        
        public int X {get; private set;}
        
        
        public int Y {get; private set;}
        
        
        public int Rotation { get; private set; }



        public void Initialize(int x, int y, int newRotation, PipesGameplayUi responsiblePipesGameplayUi)
        {
            X = x;
            Y = y;
            Rotation = newRotation;
            pipesGameplayUi = responsiblePipesGameplayUi;

            Rotate(false);
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if (interactionDisabled)
            {
                return;
            }
            
            Rotation = (Rotation + 90) % 360;

            Rotate(true);

            pipesGameplayUi?.OnPipeRotated();
        }


        public void SetColor()
        {
            foreach (var connectionIndicatorImage in connectionIndicatorImages)
            {
                if (!((IsConnected && connectionIndicatorImage.color == connectedColor) || (!IsConnected && connectionIndicatorImage.color == disconnectedColor)))
                {
                    connectionIndicatorImage.DOColor(IsConnected ? connectedColor : disconnectedColor, 0.2F);
                }
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


        public List<int> GetRotatedConnections()
        {
            var directions = new List<int>();

            int rotationSteps = (Rotation % 360 + 360) % 360 / 90;

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
                containerTransform.DOLocalRotate(new Vector3(0f, 0f, -Rotation), 0.15f).SetEase(Ease.OutQuad);
                containerTransform.DOScale(0.8f, 0.075f).SetEase(Ease.OutQuad).OnComplete(() =>
                    containerTransform.DOScale(1f, 0.075f).SetEase(Ease.OutQuad));
            }
            else
            {
                containerTransform.localRotation = Quaternion.Euler(new Vector3(0F, 0F, -Rotation));
            }
        }
        
        
        private void OnDestroy()
        {
            DOTween.Kill(gameObject);
        }
    }
}