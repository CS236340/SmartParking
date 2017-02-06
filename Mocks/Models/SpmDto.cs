using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCCNProject.Models
{
    public class SpmDto
    {
        public string carId { get; set; } = string.Empty;
        public string refId { get; set; } = string.Empty;
        public DateTime time { get; set; } = DateTime.Now;
        public Enums.EAction action { get; set; } = Enums.EAction.CheckOut;
        public Enums.EProvider subscription { get; set; } = Enums.EProvider.Pango;

        public bool CanPark()
        {
            return carId.Length == 9 && carId.Where(l => l == '-').Count() == 2;
        }
    }
}
