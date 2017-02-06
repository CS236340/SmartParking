using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCCNProject.Models
{
    public class Enums
    {
        public enum EProvider : int
        {
            CelloPark = 0,
            Pango = 1
        }

        public enum EAction : int
        {
            CheckIn = 0,
            CheckOut = 1
        }

        public enum EProviderResponse : int
        {
            Success = 0,
            CarIdNotFound = 1,
            NegativeBalance = 2,
            Unknown = 3,
            AlreadyParking = 4,
            TimesMismatch = 5,
            NotParking = 6,
            ProviderNotAvailable = 7,
            ProviderBridgeNotAvailable = 8
        }
    }
}
