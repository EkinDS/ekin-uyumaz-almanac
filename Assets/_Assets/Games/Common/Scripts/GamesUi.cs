using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using _Assets.Core;

namespace _Assets.Games
{
    public class GamesUi : MonoBehaviour
    {
        [SerializeField] private GamePrefab[] gamePrefabs;

        private IGameManager currentGameManager;
        private GameObject currentInstanceGameObject;
        private bool gameInProgress;


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
            if (gameInProgress)
            {
                return;
            }

            gameInProgress = true;
            
            var gamePrefabOfCorrectGameType = gamePrefabs.FirstOrDefault(g => g.gameType == gameType);
            var handle = Addressables.InstantiateAsync(gamePrefabOfCorrectGameType.prefab, transform);

            handle.Completed += operation =>
            {
                currentInstanceGameObject = operation.Result;
                currentGameManager = currentInstanceGameObject.GetComponent<IGameManager>();
                currentGameManager.OnGameTypeChosen();
            };
        }


        private void GameExited()
        {
            gameInProgress = false;

            Addressables.ReleaseInstance(currentInstanceGameObject);
            currentInstanceGameObject = null;
            currentGameManager = null;
        }
    }
}


[Serializable]
public struct GamePrefab
{
    public GameType gameType;
    public AssetReference prefab;
}