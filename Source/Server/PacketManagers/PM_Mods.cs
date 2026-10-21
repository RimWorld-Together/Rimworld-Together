using RTNetwork.Components;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTServer.Core;
using RTServer.Hooks.TCPNetwork;
using RTServer.Managers;
using RTServer.Misc;
using RTShared.Files.Configs;
using RTShared.Files.Mods;
using RTShared.Files.Player;
using RTShared.Misc;
using static RTNetwork.Packets.PKT_Login;
using static RTNetwork.Packets.PKT_ModConfig;

namespace RTServer.PacketManagers
{
    public class PM_Mods : PM_Base
    {
        [HandlesPacket(PacketHeader.Mod)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            PKT_ModConfig data = Serializer.ConvertBytesToObject<PKT_ModConfig>(bytes);

            switch (data.StepMode)
            {
                case ModConfigStepMode.Send:
                    SaveModConfig(client, data.File);
                    break;
            }
        }

        private static void SaveModConfig(ServerClient client, FL_ModConfig file)
        {
            if (Master.WorldValues != null && !client.GetData<FL_Player>().IsAdmin)
            {
                UserManager.BanPlayerFromName(client.GetData<FL_Player>().Username);
                Printer.Warning($"Player {client.GetData<FL_Player>().Username} tried to change mod config without being admin");
            }

            else
            {
                Master.ModConfig = file;
                FL_ModConfig.Save(FL_ModConfig.SavePath, file);
                InformationDisplayer.DisplaySetMods(client);

                PKT_ModConfig packet = new PKT_ModConfig();
                packet.File = Master.ModConfig;
                packet.StepMode = ModConfigStepMode.Send;

                ServerNetwork.SendPacketToAllClients(PacketHeader.Mod, packet);
            }
        }

        public static bool CheckIfModConflict(ServerClient client, FL_ModConfig file)
        {
            if (Master.ModConfig.AllowAllMods) return false;
            
            else if (!PM_World.CheckIfWorldExists()) return false;
            
            else if (client.GetData<FL_Player>().IsAdmin) return false;
            
            else
            {
                bool isConflicting = false;
                
                //Check if missing required mods
                foreach (FL_ModData config in Master.ModConfig.ModConfigs.Where(fetch => fetch.Type == FL_ModConfig.ModType.Required))
                {
                    FL_ModData toFind = file.ModConfigs.Find(fetch => fetch.ModID == config.ModID);
                    if (toFind == null)
                    {
                        isConflicting = true;
                        break;
                    }
                }

                //Check if has mods that aren't required or optional
                foreach (FL_ModData config in file.ModConfigs)
                {
                    FL_ModData toFind = Master.ModConfig.ModConfigs.Find(fetch => fetch.ModID == config.ModID
                        && (fetch.Type == FL_ModConfig.ModType.Required || fetch.Type == FL_ModConfig.ModType.Optional));

                    if (toFind == null)
                    {
                        isConflicting = true;
                        break;
                    }
                }
                
                if (!isConflicting) return false;
                else
                {
                    InformationDisplayer.DisplayModMismatch(client.GetData<FL_Player>().Username);
                    PM_Login.DenyConnectionWithReason(client, LoginResponse.Mods);
                    return true;
                }
            }
        }

        public static bool CheckForModConfigs(ServerClient client, FL_ModConfig file)
        {
            if (Master.ModConfig.AllowAllMods) return false;
            
            else if (!Master.ModConfig.EnforceSettings) return false;
            
            else if (!PM_World.CheckIfWorldExists()) return false;
            
            else if (client.GetData<FL_Player>().IsAdmin) return false;
            
            else
            {
                foreach (FL_ModData config in Master.ModConfig.ModConfigs.Where(fetch => fetch.Type == FL_ModConfig.ModType.Required))
                {
                    FL_ModData toFind = file.ModConfigs.FirstOrDefault(fetch => fetch.ModID == config.ModID);
                    
                    if (config.Settings == null) continue;
                    else if (toFind.Settings == null || config.Settings.Hash != toFind.Settings.Hash)
                    {
                        InformationDisplayer.DisplayModConfigMismatch(client.GetData<FL_Player>().Username);
                        PM_Login.DenyConnectionWithReason(client, LoginResponse.ModConfigs);
                        return true;
                    }
                }
                
                foreach (FL_ModData config in file.ModConfigs)
                {
                    FL_ModData toFind = Master.ModConfig.ModConfigs.Find(fetch => fetch.ModID == config.ModID);
                    
                    if (toFind.Settings == null) continue;
                    else if (config.Settings == null || config.Settings.Hash != toFind.Settings.Hash)
                    {
                        InformationDisplayer.DisplayModConfigMismatch(client.GetData<FL_Player>().Username);
                        PM_Login.DenyConnectionWithReason(client, LoginResponse.ModConfigs);
                        return true;
                    }
                }

                return false;
            }
        }

        public static bool CheckForModOrder(ServerClient client, FL_ModConfig file)
        {
            if (Master.ModConfig.AllowAllMods) return false;
            
            if (Master.ModConfig.ModOrder == null) return false;
            
            else if (!PM_World.CheckIfWorldExists()) return false;
            
            else if (client.GetData<FL_Player>().IsAdmin) return false;

            else
            {
                if (file.ModOrder.Hash == Master.ModConfig.ModOrder.Hash) return false;
                else
                {
                    InformationDisplayer.DisplayModOrderMismatch(client.GetData<FL_Player>().Username);
                    PM_Login.DenyConnectionWithReason(client, LoginResponse.ModOrder);
                    return true;
                }
            }
        }
    }
}
