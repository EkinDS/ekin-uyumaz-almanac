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


        public void Activate()
        {
            transform.DOLocalMoveY(0F, 0.5F);
            canvasGroup.DOFade(1F, 0.5F);
        }
    }
}