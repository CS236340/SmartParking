using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCCNProject.Models
{
    public class ProviderDb
    {
        private IConfigurationRoot m_cfg;
        private string m_history = null;
        private Dictionary<string, ProviderUser> CelloParkUsers = new Dictionary<string, ProviderUser>();
        private Dictionary<string, ProviderUser> PangoUsers = new Dictionary<string, ProviderUser>();
        public ProviderDb(IConfigurationRoot p_cfg)
        {
            m_cfg = p_cfg;
        }
        public IEnumerable<ProviderUser> GetUsers()
        {
            Random r = new Random(122);
            if (!CelloParkUsers.Any())
            {
                string[] celloParkUsers = m_cfg["Configuration:CelloParkUsers"].Split(',');
                foreach (var user in celloParkUsers)
                {
                    CelloParkUsers.Add(user, new ProviderUser(user, r.Next(10, 400)) { m_provider = Enums.EProvider.CelloPark });
                }
            }
            if (!PangoUsers.Any())
            {
                string[] pangoUsers = m_cfg["Configuration:PangoUsers"].Split(',');
                foreach (var user in pangoUsers)
                {
                    PangoUsers.Add(user, new ProviderUser(user, r.Next(10, 400)) { m_provider = Enums.EProvider.Pango });
                }
            }
            return CelloParkUsers.Values.AsEnumerable().Union(PangoUsers.Values.AsEnumerable());
        }
        public Enums.EProviderResponse CanCheckIn(ProviderUser p_user)
        {
            ProviderUser user = GetUsers().Where(u => u.m_CarId == p_user.m_CarId).FirstOrDefault();
            if (user == null)
            {
                return Enums.EProviderResponse.CarIdNotFound;
            }
            if (user.m_Balance <= 0)
            {
                return Enums.EProviderResponse.NegativeBalance;
            }
            if (user.m_isCheckedIn)
            {
                return Enums.EProviderResponse.AlreadyParking;
            }
            return Enums.EProviderResponse.Success;
        }

        public Tuple<InfoEntity, BridgeSpmResponse> CheckInOrOut(ProviderDto p_providerDto, Enums.EProvider p_caller)
        {
            BridgeSpmResponse bsp = new BridgeSpmResponse() { carId = p_providerDto.carId, refId = p_providerDto.refId };
            var user = GetUsers()
                .Where(e => p_providerDto.carId == e.m_CarId && e.m_provider == p_caller)
                .FirstOrDefault();
            if (user == null)
            {
                return new Tuple<InfoEntity, BridgeSpmResponse>(new InfoEntity("User not found in the system!", Enums.EProviderResponse.CarIdNotFound), bsp);
            }
            if (p_providerDto.action == Enums.EAction.CheckIn) // CHECK-IN
            {
                Enums.EProviderResponse status = CanCheckIn(user);
                switch (status)
                {
                    case Enums.EProviderResponse.Success:
                        user.CheckIn(p_providerDto.time);
                        return new Tuple<InfoEntity, BridgeSpmResponse>(new InfoEntity($"Check-in + {DateTime.Now}", Enums.EProviderResponse.Success), bsp);
                    case Enums.EProviderResponse.NegativeBalance:
                        return new Tuple<InfoEntity, BridgeSpmResponse>(new InfoEntity($"Access denied! balance {user.m_Balance} is negative",
                            Enums.EProviderResponse.NegativeBalance), bsp);
                    case Enums.EProviderResponse.CarIdNotFound:
                        return new Tuple<InfoEntity, BridgeSpmResponse>(new InfoEntity($"Access denied! car id {p_providerDto.carId} not found",
                            Enums.EProviderResponse.CarIdNotFound), bsp);
                    case Enums.EProviderResponse.AlreadyParking:
                        return new Tuple<InfoEntity, BridgeSpmResponse>(new InfoEntity($"Car {user.m_CarId} already parks!",
                            Enums.EProviderResponse.AlreadyParking), bsp);
                }
            }
            else // CHECK-OUT
            {
                if (!user.m_isCheckedIn)
                {
                    return new Tuple<InfoEntity, BridgeSpmResponse>(new InfoEntity($"Car {user.m_CarId} is not checked-in!",
                        Enums.EProviderResponse.NotParking), bsp);
                }
                DateTime start = user.m_CheckInTime;
                if (p_providerDto.time < start)
                {
                    return new Tuple<InfoEntity, BridgeSpmResponse>(new InfoEntity($"Checkout time is earlier than Checkin time!",
                        Enums.EProviderResponse.TimesMismatch), bsp);
                }
                double cost = user.CheckOut(p_providerDto.time);
                double balance = user.m_Balance;
                return new Tuple<InfoEntity, BridgeSpmResponse>(new InfoEntity($"Parking from: {start} to: {p_providerDto.time} -"
                    + $" Total of: {(p_providerDto.time - start).TotalMinutes} minutes. Cost: {cost} ILS, and Balance: {balance} ILS",
                    Enums.EProviderResponse.Success), bsp);
            }
            return null;
        }
        public string GetHistory()
        {
            if (m_history == null)
            {
                foreach (var user in GetUsers())
                {
                    m_history = $"{m_history}\nCar: {user.m_CarId}, Balance: {user.m_Balance}, [Subscription: {user.m_provider}]";
                }
                m_history = m_history + "\n" + "***************" + "\n";
                m_history = m_history + "\n" + $"[History started: {DateTime.Now}]";
            }
            return m_history;
        }
        public void AppendToHistory(string st)
        {
            m_history = m_history + "\n" + st;
        }
    }
}
