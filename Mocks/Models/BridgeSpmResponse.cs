using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCCNProject.Models
{
    public class BridgeSpmResponse
    {
        public string carId { get; set; } = string.Empty;
        public string refId { get; set; } = string.Empty;
        public Enums.EProviderResponse responseCode { get; set; }
        public InfoEntity info { get; set; } = null;
    }
}
