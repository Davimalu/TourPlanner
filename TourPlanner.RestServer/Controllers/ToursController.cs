using Microsoft.AspNetCore.Mvc;
using TourPlanner.RestServer.DAL.Repository.Interfaces;
using TourPlanner.Model;

namespace TourPlanner.RestServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToursController : ControllerBase
    {
        private readonly ITourRepository _tourRepository;
        private readonly ITourLogRepository _tourLogRepository;
        
        public ToursController(ITourRepository tourRepository, ITourLogRepository tourLogRepository)
        {
            _tourRepository = tourRepository;
            _tourLogRepository = tourLogRepository;
        }
        
        // ------------------------------
        // Tour Endpoints
        // ------------------------------

        // GET: api/tours
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tour>>> GetTours()
        {
            var tours = await _tourRepository.GetAllToursAsync();
            return Ok(tours);
        }

        // GET: api/tours/{id}
        [HttpGet("{tourId}")]
        public async Task<ActionResult<Tour>> GetTourById(int tourId)
        {
            var tour = await _tourRepository.GetTourByIdAsync(tourId);
            return Ok(tour);
        }

        // POST: api/tours
        [HttpPost]
        public async Task<ActionResult<Tour>> CreateTour([FromBody] Tour newTour)
        {
            var createdTour = await _tourRepository.AddTourAsync(newTour);
            
            // The first parameter is the so-called "Location header" which informs the client where the newly created resource can be found / accessed
            return CreatedAtAction(nameof(GetTourById), new { tourId = createdTour.TourId }, createdTour);
        }

        // PUT: api/tours/{id}
        [HttpPut("{tourId}")]
        public async Task<ActionResult<Tour>> UpdateTour(int tourId, [FromBody] Tour updatedTour)
        {
            if (updatedTour.TourId != tourId)
            {
                return BadRequest("Tour ID mismatch");
            }

            var tour = await _tourRepository.UpdateTourAsync(updatedTour);
            return Ok(tour);
        }

        // DELETE: api/tours/{id}
        [HttpDelete("{tourId}")]
        public async Task<ActionResult> DeleteTour(int tourId)
        {
            await _tourRepository.DeleteTourAsync(tourId);

            return NoContent();
        }

        // ------------------------------
        // TourLog Endpoints (Sub-resource of Tours)
        // ------------------------------

        // GET: api/tours/{tourId}/logs
        [HttpGet("{tourId}/logs")]
        public async Task<ActionResult<IEnumerable<TourLog>>> GetTourLogs(int tourId)
        {
            var tour = await _tourRepository.GetTourByIdAsync(tourId);
            return Ok(tour.Logs);
        }

        // GET: api/tours/logs/{logId}
        [HttpGet("logs/{logId}")]
        public async Task<ActionResult<TourLog>> GetTourLogById(int logId)
        {
            var log = await _tourLogRepository.GetTourLogByIdAsync(logId);
            return Ok(log);
        }

        // POST: api/tours/{tourId}/logs
        [HttpPost("{tourId}/logs")]
        public async Task<ActionResult<TourLog>> CreateTourLog(int tourId, [FromBody] TourLog newLog)
        {
            var createdLog = await _tourLogRepository.AddTourLogAsync(tourId, newLog);
            return CreatedAtAction(nameof(GetTourLogById), new { logId = createdLog.LogId }, createdLog);
        }

        // PUT: api/tours/logs/{logId}
        [HttpPut("logs/{logId}")]
        public async Task<ActionResult<TourLog>> UpdateTourLog(int logId, [FromBody] TourLog updatedLog)
        {
            if (updatedLog.LogId != logId)
            {
                return BadRequest("Log ID mismatch");
            }
            
            var log = await _tourLogRepository.UpdateTourLogAsync(updatedLog);
            return Ok(log);
        }

        // DELETE: api/tours/logs/{logId}
        [HttpDelete("logs/{logId}")]
        public async Task<ActionResult> DeleteTourLog(int logId)
        {
            await _tourLogRepository.DeleteTourLogAsync(logId);
            return NoContent();
        }
    }
}
