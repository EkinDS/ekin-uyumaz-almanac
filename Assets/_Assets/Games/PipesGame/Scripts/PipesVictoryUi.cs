using _Assets.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Assets.PipesGame
{
    public class PipesVictoryUi : MonoBehaviour
    {
        [SerializeField] private ButtonAnimator continueButton;
        [SerializeField] private CanvasGroup timerDescriptionTextCanvasGroup;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI congratulationsText;

        private PipesGameManager assignedPipesGameManager;
        private Tween movementTween;


        public void Initialize(PipesGameManager pipesGameManager)
        {
            assignedPipesGameManager = pipesGameManager;
        }


        public void Activate(string timerString)
        {
            gameObject.SetActive(true);

            AnimateTexts();
            
            movementTween = transform.DOLocalMoveY(0F, 0.5F);

            timerText.text = timerString;
        }


        public void ContinueButton()
        {
            assignedPipesGameManager.BackButton();
        }


        private void OnDestroy()
        {
            movementTween.Kill();
        }


        private void AnimateTexts()
        {
            congratulationsText.transform.localPosition = new Vector3(0F, 470F, 0F);
            congratulationsText.DOFade(1F, 0.75F).SetDelay(0.5F);
            congratulationsText.transform.DOLocalMoveY(570F, 0.75F).SetDelay(0.5F);
            
            timerDescriptionTextCanvasGroup.transform.localPosition = new Vector3(0F, 300F, 0F);
            timerDescriptionTextCanvasGroup.DOFade(1F, 0.75F).SetDelay(1.2F);
            timerDescriptionTextCanvasGroup.transform.DOLocalMoveY(400F, 0.75F).SetDelay(1.2F).OnComplete((() =>
            {
                continueButton.Appear();
            }));
        }
    }
}