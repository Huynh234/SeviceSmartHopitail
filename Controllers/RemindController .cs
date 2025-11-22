using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeviceSmartHopitail.Schemas.RM;
using SeviceSmartHopitail.Services.Remind;

namespace SeviceSmartHopitail.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RemindController : ControllerBase
    {
        private readonly RmAllrSevice _drinkWaterService;

        public RemindController(RmAllrSevice drinkWaterService)
        {
            _drinkWaterService = drinkWaterService;
        }

        // ======================= UỐNG NƯỚC ===========================
        [Authorize(Roles = "user")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateDrinkWater([FromBody] CrAllRemind model)
        {
            if (model == null) return BadRequest("Invalid data");
            var result = await _drinkWaterService.CreateAsync(model);
            return Ok(result);
        }

        [Authorize(Roles = "user")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteDrinkWater(int id)
        {
            var success = await _drinkWaterService.DeleteAsync(id);
            return success ? Ok("Deleted successfully") : NotFound("Remind not found");
        }

        [Authorize(Roles = "user")]
        [HttpGet("all/{tkId}")]
        public async Task<IActionResult> DeleteAllReminds(int tkId)
        {
            var drinkWaterRemind = await _drinkWaterService.GetByTkIdAsync(tkId);
            if (drinkWaterRemind == null)
            {
                return Ok(new {Message = "Chưa có nhắc nhở nào. Hãy thêm nhắc nhở mới để duy trì thói quen tốt"});
            }
            return Ok(new
            {
                water = drinkWaterRemind
        });
        }

    }
}
