using RTServer.Hooks.TCPNetwork;
using RTServer.Managers;
using RTShared.Commands;
using RTShared.Misc;
using RTNetwork.Packets;
using RTNetwork.Components;
using RTShared.Files.Player;

namespace RTServer.Commands
{
    public class CMD_Deop : CMD_Base
    {
        public CMD_Deop()
        {
            Prefix = "deop";
            Description = "Removes admin privileges from the selected player";
            ParameterCount = 1;
        }

        public override void Action()
        {
            FL_Player toFind = UserManager.GetAllUserFiles().Where(x => x.Username == CMD_Base.CommandParameters[0]).FirstOrDefault();

            if (toFind == null) Printer.Warning($"User '{CMD_Base.CommandParameters[0]}' was not found");
            else
            {
                if (!toFind.IsAdmin) Printer.Warning($"User '{toFind.Username}' was not an admin");
                else
                {
                    toFind.UpdateAdmin(false);
                    ServerClient client = ServerNetwork.GetConnectedClientFromUsername(toFind.Username);
                    if (client != null)
                    {
                        PKT_Command commandData = new PKT_Command();
                        commandData.Mode = PKT_Command.CommandMode.Deop;

                        client.GetData<FL_Player>().UpdateAdmin(false);
                        client.Listener.EnqueuePacket(PacketHeader.Console, commandData);
                    }

                    Printer.Warning($"User '{toFind.Username}' is no longer an admin");
                }
            }
        }
    }
}
