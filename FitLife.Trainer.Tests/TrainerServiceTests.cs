using FitLife.Trainer.Api.Models;
using Moq;
using NUnit.Framework;

namespace FitLife.Trainer.Tests;

[TestFixture]
public class TrainerServiceTests
{
    [Test]
    public void Book_ValidMemberAndTime_ReturnsBooking()
    {
        var trainer = new PersonalTrainer { Name = "Marie Jensen" };
        var memberId = Guid.NewGuid();
        var sessionTime = DateTime.UtcNow.AddDays(1);

        var booking = trainer.Book(memberId, sessionTime);

        Assert.That(booking, Is.Not.Null);
        Assert.That(booking.MemberId, Is.EqualTo(memberId));
        Assert.That(booking.Status, Is.EqualTo(BookingStatus.Booked));
    }

    [Test]
    public void Book_DuplicateBooking_ThrowsInvalidOperationException()
    {
        var trainer = new PersonalTrainer { Name = "Marie Jensen" };
        var memberId = Guid.NewGuid();
        var sessionTime = DateTime.UtcNow.AddDays(1);

        trainer.Book(memberId, sessionTime);

        Assert.Throws<InvalidOperationException>(() => trainer.Book(memberId, sessionTime));
    }

    [Test]
    public void CancelBooking_ExistingBooking_ReturnsCancelledBooking()
    {
        var trainer = new PersonalTrainer { Name = "Marie Jensen" };
        var memberId = Guid.NewGuid();
        var sessionTime = DateTime.UtcNow.AddDays(1);

        var booking = trainer.Book(memberId, sessionTime);
        var cancelled = trainer.CancelBooking(booking.Id);

        Assert.That(cancelled, Is.Not.Null);
        Assert.That(cancelled!.Status, Is.EqualTo(BookingStatus.Cancelled));
    }

    [Test]
    public void CancelBooking_NonExistingBooking_ReturnsNull()
    {
        var trainer = new PersonalTrainer { Name = "Marie Jensen" };

        var result = trainer.CancelBooking(Guid.NewGuid());

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Book_SetsCorrectTrainerId()
    {
        var trainer = new PersonalTrainer { Name = "Anders Nielsen" };
        var memberId = Guid.NewGuid();
        var sessionTime = DateTime.UtcNow.AddDays(2);

        var booking = trainer.Book(memberId, sessionTime);

        Assert.That(booking.TrainerId, Is.EqualTo(trainer.Id));
    }
}