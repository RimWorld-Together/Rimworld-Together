using RTServer.Hooks.TCPNetwork;
using RTShared.Commands;
using RTShared.Misc;
using RTNetwork.Packets;
using static RTNetwork.Packets.PKT_Event;
using RTNetwork.Components;
using RTServer.PacketManagers;
using RTShared.Files.Default;

namespace RTServer.Commands
{
    public class CMD_EventAll : CMD_Base
    {
        public CMD_EventAll()
        {
            Prefix = "eventall";
            Description = "Sends an event to all connected players";
            ParameterCount = 2;
        }

        public override void Action()
        {
            FL_Event toFind = PM_Events.LoadedEvents.FirstOrDefault(fetch => fetch.DefName == CMD_Base.CommandParameters[0]);
            if (toFind == null) Printer.Warning($"Event '{CMD_Base.CommandParameters[0]}' was not found");
            else
            {
                foreach (ServerClient client in ServerNetwork.GetConnectedClients())
                {
                    PKT_Event eventData = new PKT_Event();
                    eventData.CurrentStepMode = StepMode.Receive;
                    eventData.File = toFind;

                    //We set it to -1 to let the client know it will fall at any settlement
                    eventData.ToTile = -1;

                    client.Listener.EnqueuePacket(PacketHeader.Event, eventData);
                }

                Printer.Title($"Sent event '{CMD_Base.CommandParameters[0]}' to every connected player");
            }
        }
    }
}
