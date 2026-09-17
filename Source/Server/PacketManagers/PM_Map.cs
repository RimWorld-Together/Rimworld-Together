using RTNetwork.Components;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTServer.Core;
using RTServer.Managers;
using RTServer.Misc;
using RTShared.Files;
using RTShared.Files.Player;
using RTShared.Misc;

namespace RTServer.PacketManagers
{
    public class PM_Map : PM_Base
    {
        [HandlesPacket(PacketHeader.Map)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            PKT_Map data = Serializer.ConvertBytesToObject<PKT_Map>(bytes);

            SaveUserMap(client, data);
        }

        private static void SaveUserMap(ServerClient client, PKT_Map data)
        {
            FL_Settlement existingSettlement = PM_Settlements.GetSettlementFileFromTile(data.Tile);
            
            if (existingSettlement != null)
            {
                if (client.GetData<FL_Player>().Username == existingSettlement.Username) SaveMap();
                else ResponseShortcutManager.SendIllegalPacket(client, "Attempted to save map without ownership!");
            }
            else SaveMap();
            
            void SaveMap()
            {
                File.WriteAllBytes(Path.Combine(Master.MapsPath, data.Tile + CommonValues.DefaultSaveFormat), data.Bytes);
                PM_Leaderboard.UpdateLeaderboard(client, data.Wealth);
                InformationDisplayer.DisplaySaveMap(client);   
            }
        }

        private static string[] GetAllMaps() { return Directory.GetFiles(Master.MapsPath); }

        public static bool CheckIfMapExists(int mapTileToCheck)
        {
            string toFind = GetAllMaps().FirstOrDefault(fetch => Path.GetFileNameWithoutExtension(fetch) == mapTileToCheck.ToString());
            return toFind != null;
        }

        public static byte[] GetMapFromTile(int mapTileToGet)
        {
            string path = Path.Combine(Master.MapsPath, mapTileToGet + CommonValues.DefaultSaveFormat);
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }
    }
}
