using RTServer.Core;
using RTServer.Hooks.TCPNetwork;
using RTServer.Managers;
using RTNetwork.Components;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTServer.Misc;
using RTShared.Files.Default;
using RTShared.Files.Player;
using RTShared.Misc;

namespace RTServer.PacketManagers
{
    public class PM_WorldObject : PM_Base
    {
        [HandlesPacket(PacketHeader.WorldObject)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            if (!FL_PlayerCooldown.CheckIfCanWorldObject(client.GetData<FL_Player>(), Master.ActionConfigs.WorldObjectAction)) return;
            else
            {
                PKT_WorldObject packet = Serializer.ConvertBytesToObject<PKT_WorldObject>(bytes);

                switch (packet.CurrentStepMode)
                {
                    case PKT_WorldObject.StepMode.Add:
                        AddWorldObject(client, packet, packet.WorldObject);
                        break;

                    case PKT_WorldObject.StepMode.Remove:
                        RemoveWorldObject(client, packet);
                        break;

                    case PKT_WorldObject.StepMode.Bulk:
                        AddAllWorldObjects(client, packet);
                        break;
                }

                client.GetData<FL_Player>().Cooldowns.SetWorldObjectTimer(client.GetData<FL_Player>());
            }
        }

        private static void AddWorldObject(ServerClient client, PKT_WorldObject packet, FL_WorldObject file)
        {
            string path = Path.Combine(Master.WorldObjectsPath, file.Tile + ".json");
            if (File.Exists(path)) ResponseShortcutManager.SendIllegalPacket(client);
            else
            {
                Serializer.SerializeToFile(path, file);
                if (packet != null) ServerNetwork.SendPacketToAllClients(PacketHeader.WorldObject, packet, client);
                InformationDisplayer.DisplayAddWorldObject(file.Tile.ToString());
            }
        }

        private static void RemoveWorldObject(ServerClient client, PKT_WorldObject packet)
        {
            string path = Path.Combine(Master.WorldObjectsPath, packet.WorldObject.Tile + ".json");
            if (!File.Exists(path)) ResponseShortcutManager.SendIllegalPacket(client);
            else
            {
                File.Delete(path);
                ServerNetwork.SendPacketToAllClients(PacketHeader.WorldObject, packet, client);
                InformationDisplayer.DisplayRemoveWorldObject(packet.WorldObject.Tile.ToString());
            }
        }

        private static void AddAllWorldObjects(ServerClient client, PKT_WorldObject packet)
        {
            if (!client.GetData<FL_Player>().IsAdmin && PM_World.CheckIfWorldExists()) client.Listener.MarkForDisconnect();
            else
            {
                foreach (FL_WorldObject file in packet.Bulk) { AddWorldObject(null, null, file); }
            
                Printer.Warning($"[Set world objects] > {client.GetData<FL_Player>().Username}");   
            }
        }

        public static List<FL_WorldObject> GetAllWorldObjects()
        {
            List<FL_WorldObject> worldObjects = new List<FL_WorldObject>();
            foreach (string str in Directory.GetFiles(Master.WorldObjectsPath))
            {
                worldObjects.Add(Serializer.SerializeFromFile<FL_WorldObject>(str));
            }

            return worldObjects;
        }
    }
}
