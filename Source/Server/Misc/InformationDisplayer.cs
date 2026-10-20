using RTShared.Misc;
using RTNetwork.Components;
using RTShared.Files.Player;

namespace RTServer.Misc
{
    public static class InformationDisplayer
    {
        public static void DisplayConnect(ServerClient client) { Printer.Message($"[Connect] > {client.IP}"); }

        public static void DisplayDisconnect(ServerClient client) { Printer.Message($"[Disconnect] > {client.IP}"); }

        public static void DisplayLogin(ServerClient client) { Printer.Message($"[Log in] > {client.GetData<FL_Player>().Username} | {client.GetData<FL_Player>().LatestIP}"); }
        
        public static void DisplayLogOut(ServerClient client) { Printer.Message($"[Log out] > {client.GetData<FL_Player>().Username} | {client.GetData<FL_Player>().LatestIP}"); }

        public static void DisplayRegister(ServerClient client) { Printer.Message($"[Register] > {client.GetData<FL_Player>().Username}"); }

        public static void DisplaySaveGame(ServerClient client) { Printer.Message($"[Save game] > {client.GetData<FL_Player>().Username}"); }

        public static void DisplaySaveMap(ServerClient client) { Printer.Message($"[Save Map] > {client.GetData<FL_Player>().Username}"); }

        public static void DisplaySetMods(ServerClient client) { Printer.Warning($"[Set mods] > {client.GetData<FL_Player>().Username}"); }

        public static void DisplaySetWorld(ServerClient client) { Printer.Warning($"[Set world] > {client.GetData<FL_Player>().Username}"); }

        public static void DisplaySetEvents(ServerClient client) { Printer.Warning($"[Set events] > {client.GetData<FL_Player>().Username}"); }

        public static void DisplayChatMap(string label, string message) { Printer.Message($"[Chat - {label}] > {message}"); }

        public static void DisplayAddSettlement(string value) { Printer.Message($"[Add settlement] > {value}"); }

        public static void DisplayRemoveSettlement(string value) { Printer.Message($"[Remove settlement] > {value}"); }

        public static void DisplayAddSite(string value) { Printer.Message($"[Add site] > {value}"); }

        public static void DisplayRemoveSite(string value) { Printer.Message($"[Remove site] > {value}"); }
        
        public static void DisplayAddWorldObject(string value) { Printer.Message($"[Add world object] > {value}"); }

        public static void DisplayRemoveWorldObject(string value) { Printer.Message($"[Remove world object] > {value}"); }

        public static void DisplayAddGuild(string value) { Printer.Message($"[Add guild] > {value}"); }

        public static void DisplayRemoveGuild(string value) { Printer.Message($"[Remove guild] > {value}"); }

        public static void DisplayAddRoad(string value) { Printer.Message($"[Add road] > {value}"); }

        public static void DisplayRemoveRoad(string value) { Printer.Message($"[Remove road] > {value}"); }

        public static void DisplayServerBackup(string value) { Printer.Warning($"[Server backup] > {value}"); }

        public static void DisplayUserBackup(string value) { Printer.Message($"[User backup] > {value}"); }

        public static void DisplayResetPlayer(string value) { Printer.Message($"[Reset player] > {value}"); }
        
        public static void DisplayModMismatch(string value) { Printer.Warning($"[Mod mismatch] > {value}"); }
        
        public static void DisplayModConfigMismatch(string value) { Printer.Warning($"[Mod config mismatch] > {value}"); }
        
        public static void DisplayModOrderMismatch(string value) { Printer.Warning($"[Mod order mismatch] > {value}"); }

        public static void DisplayVersionMismatch(ServerClient client) { Printer.Warning($"[Version mismatch] > {client.IP}"); }

        public static void DisplaySetScenario(string value) { Printer.Warning($"[Set scenario] > {value}"); }

        public static void DisplaySetStoryteller(string value) { Printer.Warning($"[Set storyteller] > {value}"); }

        public static void DisplaySetDifficulty(string value) { Printer.Warning($"[Set difficulty] > {value}"); }

        public static void DisplayChangedCustomizations(string value) { Printer.Warning($"[Change customizations] > {value}"); }
    }
}
