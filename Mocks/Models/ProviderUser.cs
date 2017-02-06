using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCCNProject.Models
{
    public class ProviderUser
    {
        // We consider 0.1 ILS for minute => 1 ILS for 10 minutes
        private const double COST_PER_MINUTE = 0.1;
        public string m_CarId { get; set; } = string.Empty;
        public double m_Balance { get; set; } = 0.0;
        public DateTime m_CheckInTime { get; set; }
        public bool m_isCheckedIn { get; private set; } = false;
        public Enums.EProvider m_provider { get; set; }

        public ProviderUser(string p_CarId, double p_initialBalance)
        {
            m_CarId = p_CarId;
            m_Balance = p_initialBalance;
        }

        public double GetMaxParkingTimeInMinutes()
        { 
            return m_Balance / COST_PER_MINUTE;
        }
        public void CheckIn(DateTime p_time)
        {
            m_isCheckedIn = true;
            m_CheckInTime = p_time;
        }

        public double CheckOut(DateTime p_time)
        {
            m_isCheckedIn = false;
            double cost = MinutesToCost(p_time.Subtract(m_CheckInTime).TotalMinutes);
            m_Balance -= cost;
            return cost;
        }

        private double MinutesToCost(double p_totalMinutes)
        {
            return p_totalMinutes * COST_PER_MINUTE;
        }
    }
}
