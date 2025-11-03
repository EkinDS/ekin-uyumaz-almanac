using System;
using _Assets.Games;
using UnityEngine;

namespace _Assets.PipesGame
{
    public class PipesGameManager : MonoBehaviour, IGameManager
    {
        [SerializeField] private PipesIntroductionUi pipesIntroductionUi;
        [SerializeField] private PipesGameplayUi pipesGameplayUi;
        [SerializeField] private PipesVictoryUi pipesVictoryUi;


        public async void OnGameTypeChosen()
        {
            try
            {
                SetUiPositions();

                bool loaded = await pipesGameplayUi.LoadPipePrefabsAsync();
                if (!loaded)
                {
                    Debug.LogError("Failed to load pipe prefabs.");
                    return;
                }

                pipesIntroductionUi.Initialize(this);
                pipesGameplayUi.Initialize(this);
                pipesVictoryUi.Initialize(this);

                pipesIntroductionUi.Activate();
            }
            catch (Exception e)
            {
                Debug.Log("Failed to get UI's ready.");
            }
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