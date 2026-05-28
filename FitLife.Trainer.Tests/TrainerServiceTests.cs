using FitLife.Trainer.Api.DTOs;
using FitLife.Trainer.Api.Models;
using FitLife.Trainer.Api.Repositories;
using FitLife.Trainer.Api.Services;
using Moq;
using NUnit.Framework;

namespace FitLife.Trainer.Tests;

[TestFixture]
public class TrainerServiceTests
{
    private Mock<ITrainerRepository> _repositoryMock = null!;
    private TrainerService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<ITrainerRepository>();
        _service = new TrainerService(_repositoryMock.Object);
    }

    [Test]
    public async Task BookAsync_ValidMemberAndTime_ReturnsBooking()
    {
        var trainer = new PersonalTrainer { Name = "Marie Jensen" };
        var memberId = Guid.NewGuid();
        var sessionTime = DateTime.UtcNow.AddDays(1);

        _repositoryMock.Setup(r => r.GetByIdAsync(trainer.Id)).ReturnsAsync(trainer);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<PersonalTrainer>())).ReturnsAsync((PersonalTrainer t) => t);

        var booking = await _service.BookAsync(trainer.Id, new BookingRequest(memberId, sessionTime));

        Assert.That(booking, Is.Not.Null);
        Assert.That(booking!.MemberId, Is.EqualTo(memberId));
        Assert.That(booking.Status, Is.EqualTo(BookingStatus.Booked));
    }

    [Test]
    public void BookAsync_DuplicateBooking_ThrowsInvalidOperationException()
    {
        var trainer = new PersonalTrainer { Name = "Marie Jensen" };
        var memberId = Guid.NewGuid();
        var sessionTime = DateTime.UtcNow.AddDays(1);

        trainer.Bookings.Add(new TrainerBooking
        {
            MemberId = memberId,
            TrainerId = trainer.Id,
            SessionTime = sessionTime,
            Status = BookingStatus.Booked
        });

        _repositoryMock.Setup(r => r.GetByIdAsync(trainer.Id)).ReturnsAsync(trainer);

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.BookAsync(trainer.Id, new BookingRequest(memberId, sessionTime)));
    }

    [Test]
    public async Task CancelBookingAsync_ExistingBooking_ReturnsCancelledBooking()
    {
        var trainer = new PersonalTrainer { Name = "Marie Jensen" };
        var memberId = Guid.NewGuid();
        var sessionTime = DateTime.UtcNow.AddDays(1);

        _repositoryMock.Setup(r => r.GetByIdAsync(trainer.Id)).ReturnsAsync(trainer);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<PersonalTrainer>())).ReturnsAsync((PersonalTrainer t) => t);

        var booking = await _service.BookAsync(trainer.Id, new BookingRequest(memberId, sessionTime));
        var cancelled = await _service.CancelBookingAsync(trainer.Id, booking!.Id);

        Assert.That(cancelled, Is.Not.Null);
        Assert.That(cancelled!.Status, Is.EqualTo(BookingStatus.Cancelled));
    }

    [Test]
    public async Task CancelBookingAsync_NonExistingBooking_ReturnsNull()
    {
        var trainer = new PersonalTrainer { Name = "Marie Jensen" };

        _repositoryMock.Setup(r => r.GetByIdAsync(trainer.Id)).ReturnsAsync(trainer);

        var result = await _service.CancelBookingAsync(trainer.Id, Guid.NewGuid());

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task BookAsync_SetsCorrectTrainerId()
    {
        var trainer = new PersonalTrainer { Name = "Anders Nielsen" };
        var memberId = Guid.NewGuid();
        var sessionTime = DateTime.UtcNow.AddDays(2);

        _repositoryMock.Setup(r => r.GetByIdAsync(trainer.Id)).ReturnsAsync(trainer);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<PersonalTrainer>())).ReturnsAsync((PersonalTrainer t) => t);

        var booking = await _service.BookAsync(trainer.Id, new BookingRequest(memberId, sessionTime));

        Assert.That(booking!.TrainerId, Is.EqualTo(trainer.Id));
    }
}
