using _Assets.Games;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Assets.PipesGame
{
    public class PipesVictoryUi : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private TextMeshProUGUI timerText;

        private PipesGameManager assignedPipesGameManager;
        private Tween movementTween;


        public void Initialize(PipesGameManager pipesGameManager)
        {
            assignedPipesGameManager = pipesGameManager;
        }


        public void Activate(string timerString)
        {
            gameObject.SetActive(true);

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
    }
}