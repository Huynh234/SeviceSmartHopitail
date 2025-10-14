using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models;
using SeviceSmartHopitail.Schemas;
using SeviceSmartHopitail.Services.RAG;
using System.Linq;
using System.Text;

namespace SeviceSmartHopitail.Controllers
{
    [ApiController]
    [Route("api/qa")]
    public class QaController : ControllerBase
    {
        private readonly QaServices _qa;

        public QaController(QaServices qa)
        {
            _qa = qa;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AskDto dto, CancellationToken ct)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Question))
                return BadRequest(new { error = "Question is required." });

            string answe = await _qa.Ask(dto, ct);

            return Ok(new { answer = answe });
        }

        [HttpGet("History/{id}")]
        public async Task<IActionResult> GetHistory(int id)
        {
            var history = await _qa.GetAllLogsAsync(id);
            if (history == null)
                return Ok(new { history = new List<object>() }); 
            return Ok(new {data = history });
        }
    }
}
