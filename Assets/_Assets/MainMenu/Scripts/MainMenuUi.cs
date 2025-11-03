using _Assets.Core;
using UnityEngine;

namespace _Assets.MainMenu
{
    public class MainMenuUi : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
        }
        
        
        public void GameButton(int gameType)
        {
            CoreEventHandler.OnGameTypeChosen(gameType);
        }
    }
}