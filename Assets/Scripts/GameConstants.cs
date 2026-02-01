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

        public const string ROOM_TIME_LIMIT_HASH_PROP = "tl";
        public const string PLAYER_KICK_HASH_PROP = "kick";
        public const string PLAYER_STATUS_HASH_PROP = "status";
        public const string PLAYER_NUMBER_HASH_PROP = "pNum";
    }

    public static class UI
    {
        public const int CONTROL_COOL_TIME = 100; // milliseconds
    }
}
