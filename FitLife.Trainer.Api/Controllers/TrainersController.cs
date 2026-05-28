using FitLife.Trainer.Api.DTOs;
using FitLife.Trainer.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLife.Trainer.Api.Controllers;

// Eksponerer endpoints til oprettelse, opdatering, sletning og booking af trænere via HTTP.
[ApiController]
[Route("api/trainers")]
[Authorize]
public class TrainersController : ControllerBase
{
    private readonly ITrainerService _trainerService;
    private readonly ILogger<TrainersController> _logger;

    public TrainersController(ITrainerService trainerService, ILogger<TrainersController> logger)
    {
        _trainerService = trainerService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Fetching all trainers");
        return Ok(await _trainerService.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Fetching trainer {TrainerId}", id);
        var trainer = await _trainerService.GetByIdAsync(id);
        if (trainer is null)
        {
            _logger.LogWarning("Trainer {TrainerId} was not found", id);
            return NotFound();
        }

        return Ok(trainer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TrainerRequest request)
    {
        var trainer = await _trainerService.CreateAsync(request);
        _logger.LogInformation("Created trainer {TrainerId}", trainer.Id);
        return CreatedAtAction(nameof(GetById), new { id = trainer.Id }, trainer);
    }

    [HttpPost("{id:guid}/bookings")]
    public async Task<IActionResult> Book(Guid id, BookingRequest request)
    {
        try
        {
            var booking = await _trainerService.BookAsync(id, request);
            if (booking is null)
            {
                _logger.LogWarning("Cannot book trainer {TrainerId}; it was not found", id);
                return NotFound();
            }

            _logger.LogInformation(
                "Created booking {BookingId} for member {MemberId} with trainer {TrainerId}",
                booking.Id,
                booking.MemberId,
                id);
            return Created($"/api/trainers/{id}/bookings/{booking.Id}", booking);
        }
        catch (InvalidOperationException ex)
        {
            // Kastes af PersonalTrainer.Book() hvis tidspunktet allerede er booket
            _logger.LogWarning(ex, "Booking failed for trainer {TrainerId}", id);
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}/bookings/{bookingId:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid id, Guid bookingId)
    {
        var booking = await _trainerService.CancelBookingAsync(id, bookingId);
        if (booking is null)
        {
            _logger.LogWarning("Cannot cancel booking {BookingId} for trainer {TrainerId}", bookingId, id);
            return NotFound();
        }

        _logger.LogInformation("Cancelled booking {BookingId} for trainer {TrainerId}", bookingId, id);
        return Ok(booking);
    }

    // MemberId sendes som query parameter da det ikke er en del af JWT-tokenet i denne service
    [HttpGet("bookings/mine")]
    public async Task<IActionResult> GetMyBookings([FromQuery] Guid memberId)
    {
        var bookings = await _trainerService.GetBookingsByMemberAsync(memberId);
        return Ok(bookings);
    }

    [HttpGet("{id:guid}/booked-hours")]
    public async Task<IActionResult> GetBookedHours(Guid id, [FromQuery] DateOnly date)
    {
        var hours = await _trainerService.GetBookedHoursAsync(id, date);
        return Ok(hours);
    }

    [AllowAnonymous]
    [HttpGet("version")]
    public async Task<Dictionary<string, string>> GetVersion()
    {
        var properties = new Dictionary<string, string>();
        properties.Add("service", "FitLife Trainer API");
        var ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).ProductVersion;
        properties.Add("version", ver!);
        try
        {
            var hostName = System.Net.Dns.GetHostName();
            var ips = await System.Net.Dns.GetHostAddressesAsync(hostName);
            var ipa = ips.First().MapToIPv4().ToString();
            properties.Add("hosted-at-address", ipa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            properties.Add("hosted-at-address", "Could not resolve IP-address");
        }
        return properties;
    }
}