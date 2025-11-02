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
            currentGameManager = (IGameManager)(Instantiate(gamePrefabs[(int)gameType].prefab, transform));
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
        public MonoBehaviour prefab;
    }
}