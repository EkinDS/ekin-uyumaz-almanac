using _Assets.Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Assets.PipesGame
{
    public class PipesIntroductionUi : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private ButtonAnimator continueButton;
        [SerializeField] private Button backButton;

        private PipesGameManager assignedPipesGameManager;
        private Tween movementTween;
        private Tween fadeTween;


        public void Initialize(PipesGameManager pipesGameManager)
        {
            assignedPipesGameManager = pipesGameManager;
        }
        
        
        public void Activate()
        {
            gameObject.SetActive(true);
            movementTween = transform.DOLocalMoveY(0F, 0.5F);
            fadeTween = canvasGroup.DOFade(1F, 0.5F);
            
            continueButton.Appear();
        }


        public void ContinueButton()
        {
            assignedPipesGameManager.OnIntroductionFinished();
        }


        public void BackButton()
        {
            assignedPipesGameManager.BackButton();
        }


        private void OnDestroy()
        {
            movementTween.Kill();
            fadeTween.Kill();
        }
    }
}