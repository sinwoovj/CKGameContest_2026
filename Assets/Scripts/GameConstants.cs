using System;

public static class GameConstants
{
    public static class Network
    {
        public const int MIN_PLAYERS_PER_ROOM = 2;
        public const int MAX_PLAYERS_PER_ROOM = 4;

        public const int LOBBY_READY_COOL_TIME = 500; // milliseconds
        public const int LOBBY_BUSY_COOL_TIME = 500; // milliseconds

        public const int ROOM_LOBBY_UPDATE_DELAY = 500; // milliseconds

        // Room Property Keys
        //public const string ROOM_TIME_LIMIT_KEY = "tl";
        public const string ROOM_PASSWORD_KEY = "pw";
        public const string ROOM_PRIVATE_KEY = "pvr";
        public const string GAME_DIFFICULTY_KEY = "df";
        public const string GAME_STATE_KEY = "state";
        public const string GAME_HP_KEY = "gHp";
        public const string GAME_PLAYTIME_KEY = "pT";

        // Player Property Keys
        public const string PLAYER_KICK_KEY = "kick";
        public const string PLAYER_STATUS_KEY = "status";
        public const string PLAYER_NUMBER_KEY = "pNum";
    }

    public static class Scene
    {
        public const string MAIN_SCENE_NAME = "Main";
    }

    public static class UI
    {
        public const int CONTROL_COOL_TIME = 100; // milliseconds
    }
}
