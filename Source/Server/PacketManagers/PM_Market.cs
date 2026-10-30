using RTServer.Core;
using RTServer.Hooks.TCPNetwork;
using RTServer.Managers;
using RTNetwork.Components;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTServer.Misc;
using RTShared.Files.Marketplace;
using RTShared.Files.Player;
using RTShared.Misc;

namespace RTServer.PacketManagers
{
    public class PM_Market : PM_Base
    {
        [HandlesPacket(PacketHeader.Market)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            if (!FL_PlayerCooldown.CheckIfCanMarket(client.GetData<FL_Player>(), Master.ActionConfigs.MarketAction)) ResponseShortcutManager.SendUnavailablePacket(client);
            else
            {
                PKT_Market packet = Serializer.ConvertBytesToObject<PKT_Market>(bytes);

                switch (packet.CurrentStepMode)
                {
                    case PKT_Market.StepMode.Ask:
                        OnAsk(client, packet);
                        break;

                    case PKT_Market.StepMode.Add:
                        OnAdd(client, packet);
                        break;

                    case PKT_Market.StepMode.Remove:
                        OnRemove(client, packet);
                        break;

                    case PKT_Market.StepMode.Buy:
                        OnBuy(client, packet);
                        break;

                    case PKT_Market.StepMode.Sell:
                        throw new NotImplementedException();

                    case PKT_Market.StepMode.Payment:
                        OnPayment(client, packet);
                        break;
                }

                client.GetData<FL_Player>().Cooldowns.SetMarketTimer(client.GetData<FL_Player>());
            }
        }

        private static void OnAsk(ServerClient client, PKT_Market packet)
        {
            packet.Entries = Master.MarketFile.Entries;
            packet.ClientHash = client.GetData<FL_Player>().Hash;
            packet.Payment = client.GetData<FL_Player>().PendingMarketPayment;
            client.Listener.EnqueuePacket(PacketHeader.Market, packet);
            
            InformationDisplayer.DisplayRefreshEntry(client);
        }

        private static void OnAdd(ServerClient client, PKT_Market packet)
        {
            if (CheckIfEnoughEntriesLeft(client))
            {
                MarketEntry entry = packet.Entries[0];
                entry.ThingCost = CalculateEntryPrice(entry);
                entry.OwnerHash = client.GetData<FL_Player>().Hash;
                entry.Identifier = Hasher.GetHashFromString($"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");

                Master.MarketFile.AddEntry(packet.Entries[0]);
                client.Listener.EnqueuePacket(PacketHeader.Market, packet);
                InformationDisplayer.DisplayAddEntry(client);
                
                OnAsk(client, new PKT_Market() { CurrentStepMode = PKT_Market.StepMode.Ask });
            }

            else
            {
                packet.CurrentStepMode = PKT_Market.StepMode.MaxEntries;
                client.Listener.EnqueuePacket(PacketHeader.Market, packet);
            }
        }

        private static void OnRemove(ServerClient client, PKT_Market packet)
        {
            MarketEntry toFind = Master.MarketFile.Entries.FirstOrDefault(fetch => fetch.Identifier == packet.Entries[0].Identifier);
            if (toFind == null) ResponseShortcutManager.SendUnavailablePacket(client);
            else
            {
                Master.MarketFile.RemoveEntry(toFind);
                client.Listener.EnqueuePacket(PacketHeader.Market, packet);
                InformationDisplayer.DisplayRemoveEntry(client);
                
                OnAsk(client, new PKT_Market() { CurrentStepMode = PKT_Market.StepMode.Ask });
            }
        }

        private static void OnBuy(ServerClient client, PKT_Market packet)
        {
            MarketEntry toFind = Master.MarketFile.Entries.FirstOrDefault(fetch => fetch.Identifier == packet.Entries[0].Identifier);
            if (toFind == null) ResponseShortcutManager.SendUnavailablePacket(client);
            else
            {
                Master.MarketFile.RemoveEntry(toFind);

                GiveMoneyToSeller(toFind);
                client.Listener.EnqueuePacket(PacketHeader.Market, packet);
                InformationDisplayer.DisplayBuyEntry(client);
                
                OnAsk(client, new PKT_Market() { CurrentStepMode = PKT_Market.StepMode.Ask });
            }
        }

        private static void OnPayment(ServerClient client, PKT_Market packet)
        {
            packet.Payment = client.GetData<FL_Player>().PendingMarketPayment;
            client.Listener.EnqueuePacket(PacketHeader.Market, packet);
            client.GetData<FL_Player>().UpdatePayment(0);
            
            InformationDisplayer.DisplayPaymentEntry(client);
        }

        private static int CalculateEntryPrice(MarketEntry entry)
        {
            if (entry.ThingCost < Master.ActionConfigs.MarketAction.MinimumPrice) return Master.ActionConfigs.MarketAction.MinimumPrice;
            else return (int)(entry.ThingCost * (1 + Master.ActionConfigs.MarketAction.PriceMultiplier));
        }

        private static void GiveMoneyToSeller(MarketEntry entry)
        {
            int calculatedReturn = (int)(entry.ThingCost / (1 + (Master.ActionConfigs.MarketAction.PriceMultiplier * 2)));

            ServerClient seller = ServerNetwork.GetConnectedClientFromHash(entry.OwnerHash);
            if (seller != null)
            {
                seller.GetData<FL_Player>().UpdatePayment(seller.GetData<FL_Player>().PendingMarketPayment + calculatedReturn);

                PKT_Market packet = new PKT_Market() { CurrentStepMode = PKT_Market.StepMode.Sell };
                seller.Listener.EnqueuePacket(PacketHeader.Market, packet);

                OnAsk(seller, new PKT_Market() { CurrentStepMode = PKT_Market.StepMode.Ask });
            }

            else
            {
                FL_Player sellerFile = UserManagerH.GetUserFileFromHash(entry.OwnerHash);
                if (sellerFile != null) sellerFile.UpdatePayment(sellerFile.PendingMarketPayment + calculatedReturn);
            }
        }

        private static bool CheckIfEnoughEntriesLeft(ServerClient client)
        {
            return Master.MarketFile.Entries.Count(fetch => fetch.OwnerHash == client.GetData<FL_Player>().Username) 
                < Master.ActionConfigs.MarketAction.MaxEntriesPerPlayer;
        }
    }
}
