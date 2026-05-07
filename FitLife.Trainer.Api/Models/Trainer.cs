namespace FitLife.Trainer.Api.Models;

public class PersonalTrainer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public List<string> Specialties { get; set; } = [];
    public int ExperienceYears { get; set; }
    public double Rating { get; set; }
    public int Sessions { get; set; }
    public List<TrainerBooking> Bookings { get; set; } = [];

    public TrainerBooking Book(Guid memberId, DateTime sessionTime)
    {
        if (Bookings.Any(b => b.MemberId == memberId && b.SessionTime == sessionTime && b.Status == BookingStatus.Booked))
            throw new InvalidOperationException("Medlem har allerede en aktiv booking på dette tidspunkt.");

        var booking = new TrainerBooking
        {
            MemberId = memberId,
            TrainerId = Id,
            SessionTime = sessionTime,
            BookedAt = DateTime.UtcNow,
            Status = BookingStatus.Booked
        };

        Bookings.Add(booking);
        return booking;
    }

    public TrainerBooking? CancelBooking(Guid bookingId)
    {
        var booking = Bookings.FirstOrDefault(b => b.Id == bookingId);
        if (booking is null)
            return null;

        booking.Status = BookingStatus.Cancelled;
        return booking;
    }
}