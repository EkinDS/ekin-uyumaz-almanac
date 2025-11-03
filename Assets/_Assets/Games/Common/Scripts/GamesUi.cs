using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using _Assets.Core;

namespace _Assets.Games
{
    public class GamesUi : MonoBehaviour
    {
        [SerializeField] private GamePrefab[] gamePrefabs;

        private IGameManager currentGameManager;
        private GameObject currentInstanceGameObject;
        private AsyncOperationHandle<GameObject>? currentHandle;


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
            var entry = gamePrefabs.FirstOrDefault(g => g.gameType == gameType);

            var handle = Addressables.InstantiateAsync(entry.prefab, transform);
            currentHandle = handle;

            handle.Completed += operation =>
            {
                currentInstanceGameObject = operation.Result;
                currentGameManager = currentInstanceGameObject.GetComponent<IGameManager>();
                currentGameManager.OnGameTypeChosen();
            };
        }


        private void GameExited()
        {
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
    public AssetReferenceGameObject prefab;
}