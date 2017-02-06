using LCCNProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LCCNProject.Controllers
{
    [Route("[controller]/api")]
    public class PangoMockController : Controller
    {
        private ProviderDb m_db;
        private IConfigurationRoot m_cfg;
        public PangoMockController(ProviderDb p_db, IConfigurationRoot p_cfg)
        {
            m_db = p_db;
            m_cfg = p_cfg;
        }
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(m_db.GetUsers().Where(u => u.m_provider == Enums.EProvider.Pango));
        }
        [HttpPost]
        public IActionResult CheckInOrOut(
            [FromBody] ProviderDto p_providerDto)
        {
            if (p_providerDto == null)
            {
                return BadRequest(new InfoEntity("User is null", Enums.EProviderResponse.CarIdNotFound));
            }
            string x = $"[PANGO] Car: {p_providerDto.carId}, RefId: {p_providerDto.refId}, Action: {p_providerDto.action}, Time: {p_providerDto.time}";
            m_db.AppendToHistory(x);
            if (p_providerDto.refId == "0001")
            {
                throw new HttpRequestException();
            }
            if (p_providerDto.refId == "0003")
            {
                return Unauthorized();
            }
            var ret = m_db.CheckInOrOut(p_providerDto, Enums.EProvider.Pango);
            ReplyToBridge(ret.Item1, ret.Item2);
            return Ok(p_providerDto.refId);
        }
        private async void ReplyToBridge(InfoEntity p_response, BridgeSpmResponse p_bridgeResponse)
        {
            p_bridgeResponse.responseCode = p_response.responseCode;
            p_bridgeResponse.info = p_response;
            await Task.Run(() =>
            {
                Thread.Sleep(7000);
                var client = new HttpClient
                {
                    BaseAddress = new Uri(m_cfg["Configuration:PangoBridgeUrl"])
                };
                var response = client.PostAsJsonAsync("", p_bridgeResponse);
            });
        }
    }

}
