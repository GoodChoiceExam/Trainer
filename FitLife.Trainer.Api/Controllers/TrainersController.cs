using FitLife.Trainer.Api.DTOs;
using FitLife.Trainer.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitLife.Trainer.Api.Controllers;

[ApiController]
[Route("api/trainers")]
[Authorize]
public class TrainersController : ControllerBase
{
    private readonly ITrainerService _trainerService;

    public TrainersController(ITrainerService trainerService)
    {
        _trainerService = trainerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _trainerService.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var trainer = await _trainerService.GetByIdAsync(id);
        if (trainer is null)
            return NotFound();

        return Ok(trainer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TrainerRequest request)
    {
        var trainer = await _trainerService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = trainer.Id }, trainer);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, TrainerRequest request)
    {
        var trainer = await _trainerService.UpdateAsync(id, request);
        if (trainer is null)
            return NotFound();

        return Ok(trainer);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _trainerService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id:guid}/bookings")]
    public async Task<IActionResult> Book(Guid id, BookingRequest request)
    {
        try
        {
            var booking = await _trainerService.BookAsync(id, request);
            if (booking is null)
                return NotFound();

            return Created($"/api/trainers/{id}/bookings/{booking.Id}", booking);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}/bookings/{bookingId:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid id, Guid bookingId)
    {
        var booking = await _trainerService.CancelBookingAsync(id, bookingId);
        if (booking is null)
            return NotFound();

        return Ok(booking);
    }
    
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
}