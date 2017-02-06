using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LCCNProject.Models.Enums;

namespace LCCNProject.Models
{
    public class InfoEntity
    {
        public string info { get; set; } = string.Empty;
        public EProviderResponse responseCode { get; set; } = EProviderResponse.Success;
        public InfoEntity(string p_info, EProviderResponse p_status)
        {
            info = p_info;
            responseCode = p_status;
        }
    }
}
