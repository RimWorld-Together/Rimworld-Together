using RTShared.Files;

namespace RTServer.Files
{
    public class FL_ServerConfig : FL_Base
    {
        public static string SavePath { get; set; } = string.Empty;

        public string Name { get; set; } = "RimWorld Together Server";

        public string Description { get; set; } = string.Empty;

        public string DiscordURL { get; set; } = string.Empty;

        public string SteamWorkshopURL { get; set; } = string.Empty;
        
        public string ServerImageURL { get; set; } = string.Empty;

        public string IP { get; set; } = "0.0.0.0";

        public int Port { get; set; } = 25555;

        public int MaxPlayers { get; set; } = 100;

        public int Verbosity { get; set; } = 0;

        public bool DisplayChatInConsole { get; set; } = false;

        public bool UseUPnP { get; set; } = false;

        public bool UseClientSave { get; set; } = true;

        public bool EnableServerBrowser { get; set; } = true;

        public bool EnableServerTelemetry { get; set; } = true;
    }
}
