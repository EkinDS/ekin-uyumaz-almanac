using _Assets.Games;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Assets.PipesGame
{
    public class PipesIntroductionUi : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button continueButton;

        private Tween movementTween;
        private Tween fadeTween;


        public void Activate()
        {
            movementTween = transform.DOLocalMoveY(0F, 0.5F);
            fadeTween = canvasGroup.DOFade(1F, 0.5F);
        }


        private void OnDestroy()
        {
            movementTween.Kill();
            fadeTween.Kill();
        }
    }
}