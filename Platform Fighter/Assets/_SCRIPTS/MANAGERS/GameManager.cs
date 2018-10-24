using System;
using DATA;
using MISC;

namespace MANAGERS
{
    public class GameManager : Singleton<GameManager>
    {
        public bool FromSingleplayer;
        
        public Types.Character[] Characters { get; set; } =
        {
            Types.Character.TestCharacter,
            Types.Character.None
        };

        public Types.Stage Stage { get; set; } = Types.Stage.TestStage;

        private void StartGame() { }
    }
}