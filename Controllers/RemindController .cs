using Microsoft.AspNetCore.Mvc;
using SeviceSmartHopitail.Schemas.RM;
using SeviceSmartHopitail.Services.Remind;

namespace SeviceSmartHopitail.Controllers
{
    public class RemindController : ControllerBase
    {
        private readonly RmMedicineService _medicineService;
        private readonly RmExerciseService _exerciseService;
        private readonly RmSleepService _sleepService;
        private readonly RmWaterSevice _drinkWaterService;

        public RemindController(
            RmMedicineService medicineService,
            RmExerciseService exerciseService,
            RmSleepService sleepService,
            RmWaterSevice drinkWaterService)
        {
            _medicineService = medicineService;
            _exerciseService = exerciseService;
            _sleepService = sleepService;
            _drinkWaterService = drinkWaterService;
        }

        // ======================= UỐNG THUỐC ===========================
        [HttpPost("take-medicine")]
        public async Task<IActionResult> CreateTakeMedicine([FromBody] CrAllRemind model)
        {
            if (model == null) return BadRequest("Invalid data");
            var result = await _medicineService.CreateAsync(model);
            return Ok(result);
        }

        [HttpDelete("take-medicine/{id}")]
        public async Task<IActionResult> DeleteTakeMedicine(int id)
        {
            var success = await _medicineService.DeleteAsync(id);
            return success ? Ok("Deleted successfully") : NotFound("Remind not found");
        }

        // ======================= TẬP THỂ DỤC ===========================
        [HttpPost("exercise")]
        public async Task<IActionResult> CreateExercise([FromBody] CrAllRemind model)
        {
            if (model == null) return BadRequest("Invalid data");
            var result = await _exerciseService.CreateAsync(model);
            return Ok(result);
        }

        [HttpDelete("exercise/{id}")]
        public async Task<IActionResult> DeleteExercise(int id)
        {
            var success = await _exerciseService.DeleteAsync(id);
            return success ? Ok("Deleted successfully") : NotFound("Remind not found");
        }

        // ======================= NGỦ NGHỈ ===========================
        [HttpPost("sleep")]
        public async Task<IActionResult> CreateSleep([FromBody] CrAllRemind model)
        {
            if (model == null) return BadRequest("Invalid data");
            var result = await _sleepService.CreateAsync(model);
            return Ok(result);
        }

        [HttpDelete("sleep/{id}")]
        public async Task<IActionResult> DeleteSleep(int id)
        {
            var success = await _sleepService.DeleteAsync(id);
            return success ? Ok("Deleted successfully") : NotFound("Remind not found");
        }

        // ======================= UỐNG NƯỚC ===========================
        [HttpPost("drink-water")]
        public async Task<IActionResult> CreateDrinkWater([FromBody] CrAllRemind model)
        {
            if (model == null) return BadRequest("Invalid data");
            var result = await _drinkWaterService.CreateAsync(model);
            return Ok(result);
        }

        [HttpDelete("drink-water/{id}")]
        public async Task<IActionResult> DeleteDrinkWater(int id)
        {
            var success = await _drinkWaterService.DeleteAsync(id);
            return success ? Ok("Deleted successfully") : NotFound("Remind not found");
        }

        [HttpGet("all/{tkId}")]
        public async Task<IActionResult> DeleteAllReminds(int tkId)
        {
            var medicineRemind = await _medicineService.GetByTkIdAsync(tkId);
            if (medicineRemind != null)
            {
                return NotFound("Remind not found");
            }
            var exerciseRemind = await _exerciseService.GetByTkIdAsync(tkId);
            if (exerciseRemind != null)
            {
                return NotFound("Remind not found");
            }
            var sleepRemind = await _sleepService.GetByTkIdAsync(tkId);
            if (sleepRemind != null)
            {
                return NotFound("Remind not found");
            }
            var drinkWaterRemind = await _drinkWaterService.GetByTkIdAsync(tkId);
            if (drinkWaterRemind != null)
            {
                return NotFound("Remind not found");
            }
            return Ok(new
            {
                water = drinkWaterRemind != null,
                sleep = sleepRemind != null,
                exercise = exerciseRemind != null,
                medicine = medicineRemind != null
            });
        }

    }
}
