using RTShared.Files;
using RTShared.Misc;

namespace RTServer.Files
{
    public class FL_IPBanConfig : FL_Base
    {
        public static string SavePath { get; set; } = string.Empty;

        public List<string> BannedIPs { get; set; } = [];
        
        private Semaphore Semaphore { get; set; } = new Semaphore(1, 1);

        public void Add(string ip)
        {
            Semaphore.WaitOne();

            try { BannedIPs.Add(ip); }
            catch (Exception ex) { Printer.Error(ex); }

            Semaphore.Release();

            Save(SavePath, this);
        }

        public void Remove(string ip)
        {
            Semaphore.WaitOne();

            try { BannedIPs.Remove(ip); }
            catch (Exception ex) { Printer.Error(ex); }

            Semaphore.Release();

            Save(SavePath, this);
        }
        
        public bool CheckForIPBan(string ip) { return BannedIPs.Contains(ip); }
    }
}