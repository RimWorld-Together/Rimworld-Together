using RTNetwork.Components;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTServer.Core;
using RTServer.Misc;
using RTShared.Files.Configs;
using RTShared.Files.Player;
using RTShared.Misc;
using static RTNetwork.Packets.PKT_World;

namespace RTServer.PacketManagers
{
    public class PM_World : PM_Base
    {
        [HandlesPacket(PacketHeader.World)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            PKT_World data = Serializer.ConvertBytesToObject<PKT_World>(bytes);

            switch (data.StepMode)
            {
                case WorldStepMode.Sent:
                    ReceiveWorld(client, data);
                    break;
            }
        }

        public static bool CheckIfWorldExists() { return File.Exists(FL_PlanetConfig.SavePath); }

        public static void RequireWorldFile(ServerClient client)
        {
            PKT_World worldData = new PKT_World();
            worldData.StepMode = WorldStepMode.AskFor;
            client.Listener.EnqueuePacket(PacketHeader.World, worldData);
        }

        public static void SendWorld(ServerClient client)
        {
            PKT_World data = new PKT_World();
            data.StepMode = WorldStepMode.Sent;
            data.File = Serializer.SerializeFromFile<FL_PlanetConfig>(FL_PlanetConfig.SavePath);

            client.Listener.EnqueuePacket(PacketHeader.World, data);
        }

        private static void ReceiveWorld(ServerClient client, PKT_World data)
        {
            if (Master.WorldValues != null || PM_WorldObject.GetAllWorldObjects().Count == 0 || PM_Roads.GetAllRoads().Count == 0)
            {
                client.Listener.MarkForDisconnect();
            }
            
            else
            {
                client.GetData<FL_Player>().UpdateAdmin(true);
                
                PKT_Command commandData = new PKT_Command();
                commandData.Mode = PKT_Command.CommandMode.Op;
                client.Listener.EnqueuePacket(PacketHeader.Console, commandData);
                
                Master.WorldValues = data.File;
                FL_PlanetConfig.Save(FL_PlanetConfig.SavePath, Master.WorldValues);
                
                InformationDisplayer.DisplaySetWorld(client);
                Printer.Warning($"Giving first join admin permission to {client.GetData<FL_Player>().Username}");
            }
        }
    }
}
