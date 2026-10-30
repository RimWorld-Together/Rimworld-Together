using RTServer.Core;
using RTShared.Commands;
using RTShared.Misc;

namespace RTServer.Commands
{
    public class CMD_WipeMarket : CMD_Base
    {
        public CMD_WipeMarket()
        {
            Prefix = "wipemarket";
            Description = "Wipes all market entries";
            ParameterCount = 1;
        }

        public override void Action()
        {
            if (CMD_Base.CommandParameters[0] != "CONFIRM") Printer.Error("Command will only execute if parameter is 'CONFIRM'");
            else
            {
                Master.MarketFile.WipeEntries();
                Printer.Title("All market entries have been removed");   
            }
        }
    }
}