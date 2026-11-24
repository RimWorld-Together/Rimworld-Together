using Mono.Nat;
using RTShared.Misc;
using RTNetwork.Components;

namespace RTServer.Misc
{
    public class UPnP
    {
        private bool WasSuccessful { get; set; } = false;

        public UPnP()
        {
            Printer.Warning($"[UPnP] > Attempting to forward port '{Network.Port}'");
            NatUtility.DeviceFound += DeviceFound;
            TryToMapPort();
        }
        
        private void TryToMapPort()
        {
            NatUtility.StartDiscovery();

            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(1000);
                if (WasSuccessful) break;
            }

            if (!WasSuccessful) Printer.Error("Could not enable UPnP! Please check your network configuration");
        }
        
        private void DeviceFound(object sender, DeviceEventArgs args)
        {
            try
            {
                INatDevice device = args.Device;
                device.CreatePortMap(new Mapping(Protocol.Tcp, Network.Port, Network.Port));

                //This line can run multiple times if you are connected to multiple devices (There's no reason for that, so only print it once)
                if (!WasSuccessful) Printer.Warning("Successfully port-forwarded the server");
                WasSuccessful = true;

                Printer.Warning("UPnP forward successful");
            }
            catch (Exception ex) { Printer.Error(ex.ToString()); }
        }
    }
}
