using RTServer.Hooks.TCPNetwork;
using RTServer.Managers;
using RTShared.Commands;
using RTShared.Misc;
using RTNetwork.Packets;
using RTNetwork.Components;
using RTShared.Files.Player;

namespace RTServer.Commands
{
    public class CMD_Op : CMD_Base
    {
        public CMD_Op()
        {
            Prefix = "op";
            Description = "Gives admin privileges to the selected player";
            ParameterCount = 1;
        }

        public override void Action()
        {
            FL_Player toFind = UserManager.GetAllUserFiles().Where(x => x.Username == CMD_Base.CommandParameters[0]).FirstOrDefault();
            if (toFind == null) Printer.Warning($"User '{CMD_Base.CommandParameters[0]}' was not found");
            else
            {
                if (toFind.IsAdmin) Printer.Warning($"User '{toFind.Username}' was already an admin");
                else
                {
                    toFind.UpdateAdmin(true);

                    ServerClient client = ServerNetwork.GetConnectedClientFromUsername(toFind.Username);
                    if (client != null)
                    {
                        PKT_Command commandData = new PKT_Command();
                        commandData.Mode = PKT_Command.CommandMode.Op;

                        client.GetData<FL_Player>().UpdateAdmin(true);
                        client.Listener.EnqueuePacket(PacketHeader.Console, commandData);
                    }

                    Printer.Warning($"User '{toFind.Username}' has now admin privileges");
                }
            }
        }
    }
}
