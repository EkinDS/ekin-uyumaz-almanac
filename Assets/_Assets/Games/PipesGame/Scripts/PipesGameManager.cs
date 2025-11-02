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


        public void BackButton()
        {
            GamesEventHandler.OnGameExited();
        }

        
        private void OnGameplayFinished(string timerString)
        {
            pipesVictoryUi.Activate(timerString);
        }
        
        
        private void SetUiPositions()
        {
            pipesGameplayUi.transform.localPosition = new Vector3(0F, -2000F, 0F);
            pipesIntroductionUi.transform.localPosition = new Vector3(0F, -2000F, 0F);
            pipesVictoryUi.transform.localPosition = new Vector3(0F, -2000F, 0F);
            
            pipesGameplayUi.gameObject.SetActive(false);
            pipesIntroductionUi.gameObject.SetActive(false);
            pipesVictoryUi.gameObject.SetActive(false);
        }

        
        private void OnEnable()
        {
            GamesEventHandler.GameplayCompleted += OnGameplayFinished;
        }


        private void OnDisable()
        {
            GamesEventHandler.GameplayCompleted -= OnGameplayFinished;
        }

    }
}