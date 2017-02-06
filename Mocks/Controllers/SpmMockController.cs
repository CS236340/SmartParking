using LCCNProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LCCNProject.Controllers
{
    [Route("[controller]/api")]
    public class SpmMockController : Controller
    {
        private IConfigurationRoot m_cfg;
        private ProviderDb m_db;
        public SpmMockController(IConfigurationRoot p_cfg, ProviderDb p_db)
        {
            m_cfg = p_cfg;
            m_db = p_db;
        }
        [HttpGet]
        public IActionResult History()
        {
            return Ok(m_db.GetHistory());
        }
        [HttpPost]
        public IActionResult PostFromBridge
            ([FromBody] BridgeSpmResponse p_response)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new InfoEntity("Model is not valid", Enums.EProviderResponse.Unknown));
            }
            string append = $"[BRIDGE RESPONSE] On: {DateTime.Now}, car: {p_response.carId}, (ref: {p_response.refId}), "
                + $"status: {p_response.info.responseCode}, info-text: {p_response.info.info}, info-code: {p_response.info.responseCode}";
            m_db.AppendToHistory(append);
            if (p_response.refId == "0004")
            {
                throw new Exception();
            }
            return Ok();
        }

        [HttpPost]
        [Route("car/")]
        public async Task<IActionResult> PostFromCar([FromBody]SpmDto p_dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new InfoEntity("Model is not valid", Enums.EProviderResponse.Unknown));
            }
            string append = $"[CAR EVENT] On: {p_dto.time}, car: {p_dto.carId}, (ref: {p_dto.refId}), "
                + $"action: {p_dto.action.ToString()}, subscriptor: {p_dto.subscription.ToString()}";
            m_db.AppendToHistory(append);
            var client = new HttpClient
            {
                BaseAddress = new Uri(m_cfg["Configuration:SpmBridgeUrl"])
                //BaseAddress = new Uri("http://132.68.43.203:9890/")
            };
            var response = await client.PostAsJsonAsync("", p_dto);
            var responseValue = await response.Content.ReadAsAsync<string>();
            return StatusCode((int)response.StatusCode, responseValue);
        }
    }
}
