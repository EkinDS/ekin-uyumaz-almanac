using System;
using _Assets.Core;

namespace _Assets.Games
{
    public static class GamesEventHandler
    {
        public static event Action<GameType> GameSelected;
        public static event Action GameExited;

        
        public static void OnGameSelected(int gameTypeIndex)
        {
            GameSelected?.Invoke((GameType)gameTypeIndex);
        }


        public static void OnGameExited()
        {
            GameExited?.Invoke();
        }
    }
}