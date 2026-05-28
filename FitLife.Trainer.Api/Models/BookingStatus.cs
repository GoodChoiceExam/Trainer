namespace FitLife.Trainer.Api.Models;

// Status på en booking — annullerede bookinger slettes ikke men markeres som Cancelled.
public enum BookingStatus
{
    Booked,
    Cancelled
}