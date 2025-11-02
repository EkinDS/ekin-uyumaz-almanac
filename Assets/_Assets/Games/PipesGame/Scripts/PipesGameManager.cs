using _Assets.Games;
using UnityEngine;

namespace _Assets.PipesGame
{
    public class PipesGameManager : MonoBehaviour, IGameManager
    {
        [SerializeField] private PipesGameplayUi pipesGameplayUi;
        [SerializeField] private PipesIntroductionUi pipesIntroductionUi;
        [SerializeField] private PipesVictoryUi pipesVictoryUi;
        
        
        public void OnGameTypeChosen()
        {
            SetUiPositions();
            
           pipesIntroductionUi.Activate();
        }


        public void OnIntroductionFinished()
        {
            pipesGameplayUi.Activate();
        }


        public void OnGameplayFinished()
        {
            pipesVictoryUi.Activate();
        }
        
        
        private void SetUiPositions()
        {
            pipesGameplayUi.transform.localPosition = new Vector3(0F, -2000F, 0F);
            pipesIntroductionUi.transform.localPosition = new Vector3(0F, -2000F, 0F);
            pipesVictoryUi.transform.localPosition = new Vector3(0F, -2000F, 0F);
        }


        public void BackButton()
        {
            GamesEventHandler.OnGameExited();
        }
    }
}