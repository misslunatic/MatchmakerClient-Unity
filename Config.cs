namespace Scripts.Matchmaker
{
    public static class Config
    {
        /// <summary>
        /// This allows you to name the game so your game cannot connect to servers it is not supposed to.
        /// </summary>
        public const string GameId = "My Game Name";

        /// <summary>
        /// The version of the matchmaker APII. This must match here and on the server for a connection.
        /// </summary>
        public const string MatchmakerAPIVersion = "1.1.6";

        /// <summary>
        /// The version of the game itself. Must match on the server for a connection.
        /// Name is also valid, as it is a string.
        /// </summary>
        public const string GameVersion = "0.0.1";

        /// <summary>
        /// What port the server will be on (0-65535). The lobby server will use a port two (2) above this.
        /// </summary>
        public const ushort Port = 26950;

        /// <summary>
        /// What address the server is at.
        /// </summary>
        public const string Address = "192.168.1.204";

        /// <summary>
        /// Makes it so it waits for a response from the server every time it sends data. Slow, advise use for debugging.
        /// </summary>
        public const bool Sync = false;
    }
}