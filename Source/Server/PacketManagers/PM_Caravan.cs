using RTNetwork.Components;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTServer.Core;
using RTServer.Hooks.TCPNetwork;
using RTServer.Managers;
using RTShared.Files;
using RTShared.Files.Player;
using RTShared.Misc;
using static RTNetwork.Packets.PKT_Caravan;

namespace RTServer.PacketManagers
{
    public class PM_Caravan : PM_Base
    {
        [HandlesPacket(PacketHeader.Caravan)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            if (!FL_PlayerCooldown.CheckIfCanCaravan(client.GetData<FL_Player>(), Master.ActionConfigs.CaravanAction)) ResponseShortcutManager.SendUnavailablePacket(client);
            else
            {
                PKT_Caravan data = Serializer.ConvertBytesToObject<PKT_Caravan>(bytes);

                switch (data.StepMode)
                {
                    case CaravanStepMode.Add:
                        AddCaravan(client, data.CaravanFile);
                        break;

                    case CaravanStepMode.Remove:
                        RemoveCaravan(client, data.CaravanFile);
                        break;

                    case CaravanStepMode.Move:
                        MoveCaravan(client, data.CaravanFile);
                        break;
                }

                client.GetData<FL_Player>().Cooldowns.SetCaravanTimer(client.GetData<FL_Player>());
            }
        }

        private static void AddCaravan(ServerClient client, FL_Caravan file)
        {
            PKT_Caravan data = new PKT_Caravan();
            data.StepMode = CaravanStepMode.Add;
            data.CaravanFile = file;

            ServerNetwork.SendPacketToAllClients(PacketHeader.Caravan, data, client);
        }

        public static void RemoveCaravan(ServerClient client, FL_Caravan file)
        {
            PKT_Caravan data = new PKT_Caravan();
            data.StepMode = CaravanStepMode.Remove;
            data.CaravanFile = file;

            ServerNetwork.SendPacketToAllClients(PacketHeader.Caravan, data, client);
        }

        private static void MoveCaravan(ServerClient client, FL_Caravan file)
        {
            PKT_Caravan data = new PKT_Caravan();
            data.StepMode = CaravanStepMode.Move;
            data.CaravanFile = file;

            ServerNetwork.SendPacketToAllClients(PacketHeader.Caravan, data, client);
        }
    }
}
