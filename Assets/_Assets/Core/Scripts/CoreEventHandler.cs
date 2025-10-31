using System;

namespace _Assets.Core
{
    public static class CoreEventHandler
    {
        public static event Action<GameType> GameSelected;

        
        public static void OnGameSelected(int gameTypeIndex)
        {
            GameSelected?.Invoke((GameType)gameTypeIndex);
        }
    }
}