using _Assets.Games;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Assets.PipesGame
{
    public class PipesVictoryUi : MonoBehaviour
    {
        [SerializeField] private Button continueButton;

        private Tween movementTween;

        
        public void Activate()
        {
            movementTween =   transform.DOLocalMoveY(0F, 0.5F);
        }


        public void ContinueButton()
        {
            GamesEventHandler.OnGameExited();
        }
        
        
        private void OnDestroy()
        {
            movementTween.Kill();
        }
    }
}