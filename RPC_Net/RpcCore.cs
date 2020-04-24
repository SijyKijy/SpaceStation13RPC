using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using System;
using System.Threading.Tasks;

namespace RPC
{
    /// <summary>
    /// Логика RPC
    /// </summary>
    class RpcCore
    {
        private DiscordRpcClient _client;
        private Parser _parser;

        private RichPresence presence = new RichPresence()
        {
            Party = new Party() { Size = 1, ID = Secrets.CreateFriendlySecret(new Random()) },
            Assets = new Assets()
            {
                LargeImageKey = "logo_large",
            }
        };

        internal RpcCore()
        {
            _client = new DiscordRpcClient(
                "680128014398259346",
                -1,
                new ConsoleLogger(LogLevel.Error | LogLevel.Info | LogLevel.Warning, true),
                true);

            _client.OnReady += OnReady;
            _client.OnClose += OnClose;
            _client.OnError += OnError;

            _client.Initialize();
        }

        internal async Task Loop()
        {
            _parser = await new Parser().Init();

            presence.Timestamps = new Timestamps(DateTime.UtcNow.Add(-TimeSpan.Parse(_parser.ServerStruct.Data.RoundTime)));
            presence.Assets.LargeImageText = _parser.ServerStruct.Name;

            while (!_client.IsDisposed)
            {
                await UpdatePresence();
                await Task.Delay(25000); // В документации говорится, что RPC обновляется раз в 15 сек.
            }
        }

        internal void Close() => _client.Dispose();

        private async Task UpdatePresence()
        {
            ServerData data = await _parser.Update();
            presence.Details = $"Map: {data.MapName ?? "Unknown"}";
            presence.State = $"Mode: {data.Mode ?? "Unknown"}";
            presence.Party.Max = data.Players;

            _client.SetPresence(presence);
        }

        #region Events
        private static void OnReady(object sender, ReadyMessage args)
        {
            Console.WriteLine($"[INIT] {args.User}");
        }
        private static void OnClose(object sender, CloseMessage args)
        {
            Console.WriteLine($"[Close] Lost Connection: {args.Reason}");
        }
        private static void OnError(object sender, ErrorMessage args)
        {
            Console.WriteLine($"[Error] (args.Code) {args.Message}");
        }
        #endregion
    }
}
