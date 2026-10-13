using RTNetwork.Components;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTServer.Core;
using RTServer.Hooks.TCPNetwork;
using RTShared.Details.Planet;
using RTShared.Files.Configs;
using RTShared.Files.Player;
using RTShared.Misc;

namespace RTServer.PacketManagers
{
    public class PM_Pollution : PM_Base
    {
        [HandlesPacket(PacketHeader.Pollution)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            if (!FL_PlayerCooldown.CheckIfCanPollute(client.GetData<FL_Player>(), Master.ActionConfigs.PollutionAction)) return;
            else
            {
                PKT_Pollution data = Serializer.ConvertBytesToObject<PKT_Pollution>(bytes);
                AddPollutionToTile(data, client);

                client.GetData<FL_Player>().Cooldowns.SetPollutionTimer(client.GetData<FL_Player>());
            }
        }

        public static void AddPollutionToTile(PKT_Pollution data, ServerClient client)
        {
            bool isNewPollutedTile = false;
            PollutionDetail toSearch = Master.WorldValues.PollutedTiles.FirstOrDefault(T => T.Tile == data.Pollution.Tile);
            if (toSearch == null)
            {
                toSearch = new PollutionDetail();
                isNewPollutedTile = true;
            }

            toSearch.Tile = data.Pollution.Tile;
            toSearch.Quantity += data.Pollution.Quantity;

            if (isNewPollutedTile)
            {
                List<PollutionDetail> existingPollutedTiles = Master.WorldValues.PollutedTiles.ToList();
                existingPollutedTiles.Add(toSearch);
                Master.WorldValues.PollutedTiles = existingPollutedTiles;
            }

            FL_PlanetConfig.Save(FL_PlanetConfig.SavePath, Master.WorldValues);
            ServerNetwork.SendPacketToAllClients(PacketHeader.Pollution, data, client);
        }
    }
}
