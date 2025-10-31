using _Assets.Core;
using UnityEngine;

namespace _Assets.MainMenu
{
    public class MainMenuUi : MonoBehaviour
    {
        public void GameButton(int gameType)
        {
            print("GameButton");
            CoreEventHandler.OnGameSelected(gameType);
        }
    }
}