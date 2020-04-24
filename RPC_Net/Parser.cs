using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RPC
{
    /// <summary>
    /// Парсер полученной информации о сервере (топика) и преобразование её в гибкую структуру (ServerStruct)
    /// </summary>
    class Parser
    {
        public ServerStruct ServerStruct { get; private set; }

        private string[] _ipP;

        /// <summary>
        /// Заполняет структуру сервера
        /// </summary>
        public async Task<Parser> Init()
        {
            _ipP = GetServerIp();
            ServerStruct = new ServerStruct(GetServerName(), _ipP[0], int.Parse(_ipP[1]), await ParseTopicAsync());
            return this;
        }

        /// <summary>
        /// Обновление временных данных о сервере
        /// </summary>
        public async Task<ServerData> Update() => await ParseTopicAsync();

        /// <summary>
        /// Парсит нужные данные из полученного запроса
        /// </summary>
        private async Task<ServerData> ParseTopicAsync()
        {
            var parsed = (await GetTopicAsync()).Replace("\0", "").Replace("%3a", ":").Replace("+", " ").Split('&').Where(x =>
              x.StartsWith("mode=") ||
              x.StartsWith("players=") ||
              x.StartsWith("round") ||
              x.StartsWith("map_name="))
                .Select(s => s.Split('=')).ToList();

            return new ServerData(
                mapName: parsed.Find(p => p[0] == "map_name")?[1],
                mode: parsed.Find(p => p[0] == "mode")?[1],
                players: int.Parse(parsed.Find(p => p[0] == "players")?[1]),
                roundTime: parsed.Find(p => p[0].Contains("round"))?[1]);
        }

        /// <summary>
        /// Получаем запрос (топик) с нужной основной информацией о сервере
        /// </summary>
        private async Task<string> GetTopicAsync() => await ByondApi.SendTopicCommandAsync(_ipP[0], int.Parse(_ipP[1]));

        /// <summary>
        /// Получаем IP сервера из конфиг файла в папке BYOND'a
        /// </summary>
        private string[] GetServerIp()
        {
            string path = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\BYOND\cfg\seeker.txt";

            if (File.Exists(path))
            {
                var r = Regex.Match(File.ReadAllText(path), "last-url byond:\\/\\/(.*):(\\d*)");
                if (r.Success) return new string[] { r.Groups[1].Value, r.Groups[2].Value };
            }
            throw new FileNotFoundException("Error: File not found");
        }

        /// <summary>
        /// Получаем название последнего сервера из конфиг файла в папке BYOND'a
        /// </summary>
        private string GetServerName()
        {
            string path = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\BYOND\cfg\pager.txt";

            if (File.Exists(path))
            {
                var r = Regex.Match(File.ReadAllText(path), "recent-name (.*)");
                if (r.Success) return r.Groups[1].Value.Split('|')[0];
            }
            throw new FileNotFoundException("Error: File not found");
        }
    }
}
