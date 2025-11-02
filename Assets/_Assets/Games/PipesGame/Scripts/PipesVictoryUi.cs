using _Assets.Games;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Assets.PipesGame
{
    public class PipesVictoryUi : MonoBehaviour
    {
        [SerializeField] private Button continueButton;

        
        public void Activate()
        {
            transform.DOLocalMoveY(0F, 0.5F);
        }


        public void ContinueButton()
        {
            GamesEventHandler.OnGameExited();
        }
    }
}