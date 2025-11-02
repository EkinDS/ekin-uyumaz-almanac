using System;

namespace _Assets.Core
{
    public static class CoreEventHandler
    {
        public static event Action<GameType> GameTypeChosen;

        
        public static void OnGameTypeChosen(int gameTypeIndex)
        {
            GameTypeChosen?.Invoke((GameType)gameTypeIndex);
        }
    }
}