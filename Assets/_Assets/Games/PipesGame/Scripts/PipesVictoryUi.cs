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

        private Tween movementTween;


        public void Activate(string timerString)
        {
            gameObject.SetActive(true);

            movementTween = transform.DOLocalMoveY(0F, 0.5F);

            timerText.text = timerString;
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