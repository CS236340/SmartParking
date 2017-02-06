using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LCCNProject.Models.Enums;

namespace LCCNProject.Models
{
    public class ProviderDto
    {
        public string carId { get; set; } = string.Empty;
        public string refId { get; set; } = string.Empty;
        public DateTime time { get; set; } = DateTime.Now;
        public EAction action { get; set; } = EAction.CheckIn;
    }
}
