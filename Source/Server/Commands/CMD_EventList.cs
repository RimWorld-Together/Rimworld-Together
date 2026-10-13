using RTServer.PacketManagers;
using RTShared.Commands;
using RTShared.Files;
using RTShared.Files.Default;
using RTShared.Misc;

namespace RTServer.Commands
{
    public class CMD_EventList : CMD_Base
    {
        public CMD_EventList()
        {
            Prefix = "eventlist";
            Description = "Displays a list of all server events";
            ParameterCount = 0;
        }

        public override void Action()
        {
            Printer.Title($"Available events: [{PM_Events.LoadedEvents.Count}]");
            Printer.Title("----------------------------------------");
            foreach (FL_Event eventFile in PM_Events.LoadedEvents) Printer.Warning($"{eventFile.DefName}");
            Printer.Title("----------------------------------------");
        }
    }
}
