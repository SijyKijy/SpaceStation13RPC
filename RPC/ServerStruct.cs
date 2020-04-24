namespace RPC
{
    /// <summary>
    /// Полная информация о сервере
    /// </summary>
    class ServerStruct
    {
        /// <summary>
        /// Название сервера
        /// </summary>
        public string Name { get; }
        public string IP { get; }
        public int Port { get; }
        /// <summary>
        /// Данные с сервера
        /// </summary>
        public ServerData Data { get; }

        public ServerStruct(string name, string ip, int port)
        {
            Name = name;
            IP = ip;
            Port = port;
        }

        public ServerStruct(string name, string ip, int port, ServerData data)
        {
            Name = name;
            IP = ip;
            Port = port;
            Data = data;
        }
    }

    /// <summary>
    /// Структура данных сервера
    /// </summary>
    class ServerData
    {
        public string MapName { get; } = "Unknown map";
        public string Mode { get; } = "Unknown mode";
        public int Players { get; }

        private string roundTime = "0";
        public string RoundTime
        {
            get { return roundTime; }
            private set
            {
                if (value.Contains(":")) roundTime = value;
                else roundTime = "0";
            }
        }

        public ServerData(string mapName, string mode, int players, string roundTime)
        {
            MapName = mapName;
            Mode = mode;
            Players = players;
            RoundTime = roundTime;
        }
    }
}
