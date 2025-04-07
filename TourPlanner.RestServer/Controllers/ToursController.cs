using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;
using TourPlanner.Enums;
using TourPlanner.RestServer.Models;

namespace TourPlanner.RestServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToursController : ControllerBase
    {
        // In-memory storage for tours
        // TODO: Replace with database for persistent storage
        private static ObservableCollection<Tour> _tours = new ObservableCollection<Tour>
        {
            new Tour
            {
                TourId = 1,
                TourName = "Wienerwaldrundweg",
                TourDescription = "Eine malerische Fahrradtour durch den Wienerwald mit herrlichem Ausblick auf Wien.",
                StartLocation = "Hütteldorf, Wien",
                EndLocation = "Hütteldorf, Wien",
                TransportationType = Transport.Bicycle,
                Distance = 35,
                EstimatedTime = 110,
                Logs = new List<TourLog>()
                {
                    new TourLog
                    {
                        LogId = 1,
                        TimeStamp = DateTime.Now,
                        Comment = "Erster Log",
                        Difficulty = Difficulty.Easy,
                        DistanceTraveled = 10,
                        TimeTaken = 20,
                        Rating = Rating.Good
                    },
                    new TourLog
                    {
                        LogId = 2,
                        TimeStamp = DateTime.Now,
                        Comment = "Zweiter Log",
                        Difficulty = Difficulty.Medium,
                        DistanceTraveled = 15,
                        TimeTaken = 25,
                        Rating = Rating.Okay
                    }
                }
            },
            new Tour
            {
                TourId = 2,
                TourName = "Weinviertel Panoramatour",
                TourDescription = "Eine entspannte Autofahrt durch das Weinviertel mit Stopps bei Weingütern.",
                StartLocation = "Korneuburg",
                EndLocation = "Retz",
                TransportationType = Transport.Car,
                Distance = 75,
                EstimatedTime = 200,
            },
            new Tour
            {
                TourId = 3,
                TourName = "Donauinsel Spaziergang",
                TourDescription = "Ein gemütlicher Spaziergang entlang der Donauinsel mit vielen Rastmöglichkeiten.",
                StartLocation = "Reichsbrücke, Wien",
                EndLocation = "Floridsdorfer Brücke, Wien",
                TransportationType = Transport.Foot,
                Distance = 7,
                EstimatedTime = 110,
                Logs = new List<TourLog>()
                {
                    new TourLog
                    {
                        LogId = 3,
                        TimeStamp = DateTime.Now,
                        Comment = "Erster Log",
                        Difficulty = Difficulty.Easy,
                        DistanceTraveled = 5,
                        TimeTaken = 16,
                        Rating = Rating.Good
                    },
                    new TourLog
                    {
                        LogId = 4,
                        TimeStamp = DateTime.Now,
                        Comment = "Zweiter Log",
                        Difficulty = Difficulty.Medium,
                        DistanceTraveled = 2,
                        TimeTaken = 28,
                        Rating = Rating.Okay
                    }
                }
            },
            new Tour
            {
                TourId = 4,
                TourName = "Thermenradweg",
                TourDescription = "Eine schöne Radtour entlang des Thermenradwegs von Wien nach Baden.",
                StartLocation = "Wienerberg, Wien",
                EndLocation = "Baden bei Wien",
                TransportationType = Transport.Bicycle,
                Distance = 28,
                EstimatedTime = 70
            },
            new Tour
            {
                TourId = 5,
                TourName = "Leopoldsberg-Wanderung",
                TourDescription = "Eine anspruchsvolle Wanderung mit atemberaubender Aussicht über Wien.",
                StartLocation = "Kahlenbergerdorf, Wien",
                EndLocation = "Leopoldsberg",
                TransportationType = Transport.Foot,
                Distance = 5,
                EstimatedTime = 60
            }
        };


        // GET: api/tours
        [HttpGet]
        public ActionResult<ObservableCollection<Tour>> GetTours()
        {
            return Ok(_tours);
        }

        // GET: api/tours/{id}
        [HttpGet("{id}")]
        public ActionResult<Tour> GetTourById(int id)
        {
            var tour = _tours.FirstOrDefault(t => t.TourId == id);
            if (tour == null)
            {
                return NotFound();
            }
            return Ok(tour);
        }

        // POST: api/tours
        [HttpPost]
        public ActionResult<Tour> CreateTour([FromBody] Tour newTour)
        {
            newTour.TourId = _tours.Count + 1; // Simple ID generation | will later be handled by the database
            _tours.Add(newTour); // Add the new tour to the in-memory collection

            return CreatedAtAction(nameof(GetTourById), new { id = newTour.TourId }, newTour);
        }

        // PUT: api/tours/{id}
        [HttpPut("{id}")]
        public ActionResult UpdateTour(int id, [FromBody] Tour updatedTour)
        {
            // Check if the ID in the route matches the TourId in the body
            if (updatedTour.TourId != id)
            {
                return BadRequest("Tour ID mismatch");
            }

            // Find the existing tour in the in-memory collection
            var tour = _tours.FirstOrDefault(t => t.TourId == id);
            if (tour == null)
            {
                return NotFound();
            }

            // Update the properties of the existing tour
            tour.TourName = updatedTour.TourName;
            tour.TourDescription = updatedTour.TourDescription;
            tour.StartLocation = updatedTour.StartLocation;
            tour.EndLocation = updatedTour.EndLocation;
            tour.TransportationType = updatedTour.TransportationType;
            tour.Distance = updatedTour.Distance;
            tour.EstimatedTime = updatedTour.EstimatedTime;
            tour.Logs = updatedTour.Logs;

            return NoContent();
        }

        // DELETE: api/tours/{id}
        [HttpDelete("{id}")]
        public ActionResult DeleteTour(int id)
        {
            // Find the tour in the in-memory collection
            var tour = _tours.FirstOrDefault(t => t.TourId == id);
            if (tour == null)
            {
                return NotFound();
            }

            _tours.Remove(tour); // Remove the tour from the in-memory collection

            return NoContent();
        }
    }
}
