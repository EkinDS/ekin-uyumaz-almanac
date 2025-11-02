using System;
using _Assets.Core;
using UnityEngine;

namespace _Assets.Games
{
    public class GamesUi : MonoBehaviour
    {
        [SerializeField] private GamePrefab[] gamePrefabs;

        private IGameManager currentGameManager;


        private void OnEnable()
        {
            CoreEventHandler.GameTypeChosen += OnGameTypeChosen;
            GamesEventHandler.GameExited += GameExited;
        }


        private void OnDisable()
        {
            CoreEventHandler.GameTypeChosen -= OnGameTypeChosen;
            GamesEventHandler.GameExited -= GameExited;
        }


        private void OnGameTypeChosen(GameType gameType)
        {
            GamePrefab selectedGame = default;

            foreach (GamePrefab gamePrefab in gamePrefabs)
            {
                if (gamePrefab.gameType == gameType)
                {
                    selectedGame = gamePrefab;
                    break;
                }
            }

            currentGameManager = Instantiate(selectedGame.prefab, transform) as IGameManager;
            currentGameManager.OnGameTypeChosen();
        }


        private void GameExited()
        {
            if (currentGameManager != null)
            {
                Destroy(((MonoBehaviour)currentGameManager).gameObject);
                currentGameManager = null;
            }
        }
    }


    [Serializable]
    public struct GamePrefab
    {
        public GameType gameType;
        public MonoBehaviour prefab;
    }
}