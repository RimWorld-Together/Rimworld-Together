using RTShared.Files;

namespace RTServer.Files
{
    public class FL_BackupsConfig : FL_Base
    {
        public static string SavePath { get; set; } = string.Empty;

        public bool AutomaticBackups { get; set; } = true;

        public int IntervalHours { get; set; } = 1;

        public bool AutomaticDeletion { get; set; } = true;

        public int Amount { get; set; } = 6;
    }
}
