using ADAProjectAPIVerticalSlice.Features.Assessments.CreateAssessment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADAProjectAPIVerticalSlice.Tests.Features.Assessments.CreateAssessment;

[TestClass]
public class EmailTimingCalculatorTests
{
    [TestMethod]
    public void Calculate_NormalWeekday_ReturnsCorrectEmailDates()
    {
        // Arrange
        var calculator = new EmailTimingCalculator();

        var startDate = new DateTime(2026, 10, 13); // Tuesday
        var endDate = new DateTime(2026, 10, 20);

        // Act
        var result = calculator.Calculate(startDate, endDate);

        // Assert
        Assert.AreEqual(
            new DateTime(2026, 10, 12, 9, 0, 0),
            result.HeadsUpDate);

        Assert.AreEqual(
            new DateTime(2026, 10, 13, 9, 0, 0),
            result.InvitationDate);

        Assert.AreEqual(
            new DateTime(2026, 10, 16, 9, 0, 0),
            result.ReminderDate);

        Assert.AreEqual(
            0,
            result.Warnings.Count);
    }
    [TestMethod]
    public void Calculate_MondayInvitation_ReturnsMondayWarning()
    {
        // Arrange
        var calculator = new EmailTimingCalculator();

        var startDate = new DateTime(2026, 10, 12); // Monday
        var endDate = new DateTime(2026, 10, 20);

        // Act
        var result = calculator.Calculate(startDate, endDate);

        // Assert
        Assert.IsTrue(
            result.Warnings.Any(warning =>
                warning.Contains("mandag")));
    }

    [TestMethod]
    public void Calculate_FridayInvitation_ReturnsFridayWarning()
    {
        // Arrange
        var calculator = new EmailTimingCalculator();

        var startDate = new DateTime(2026, 10, 16); // Friday
        var endDate = new DateTime(2026, 10, 23);

        // Act
        var result = calculator.Calculate(startDate, endDate);

        // Assert
        Assert.IsTrue(
            result.Warnings.Any(warning =>
                warning.Contains("fredag")));
    }

    [TestMethod]
    public void Calculate_WeekendInvitation_MovesInvitationToMonday()
    {
        // Arrange
        var calculator = new EmailTimingCalculator();

        var startDate = new DateTime(2026, 10, 17); // Saturday
        var endDate = new DateTime(2026, 10, 24);

        // Act
        var result = calculator.Calculate(startDate, endDate);

        // Assert
        Assert.AreEqual(
            new DateTime(2026, 10, 19, 9, 0, 0),
            result.InvitationDate);

        Assert.IsTrue(
            result.Warnings.Any(warning =>
                warning.Contains("weekend")));
    }

    [TestMethod]
    public void Calculate_ReminderAfterEndDate_ReturnsWarning()
    {
        // Arrange
        var calculator = new EmailTimingCalculator();

        var startDate = new DateTime(2026, 10, 13);
        var endDate = new DateTime(2026, 10, 15);

        // Act
        var result = calculator.Calculate(startDate, endDate);

        // Assert
        Assert.IsTrue(
            result.Warnings.Any(warning =>
                warning.Contains("reminder")));
    }

    [TestMethod]
    public void Calculate_StartDateWithDifferentTime_SetsInvitationToNineAM()
    {
        // Arrange
        var calculator = new EmailTimingCalculator();

        var startDate = new DateTime(
            2026, 10, 13, 15, 30, 0);

        var endDate = new DateTime(2026, 10, 20);

        // Act
        var result = calculator.Calculate(startDate, endDate);

        // Assert
        Assert.AreEqual(
            new DateTime(2026, 10, 13, 9, 0, 0),
            result.InvitationDate);
    }
    [TestMethod]
    public void Calculate_ReminderFallsOnWeekend_MovesReminderToMonday()
    {
        // Arrange
        var calculator = new EmailTimingCalculator();

        var startDate = new DateTime(2026, 10, 15); // Thursday
        var endDate = new DateTime(2026, 10, 25);

        // Act
        var result = calculator.Calculate(startDate, endDate);

        // Assert
        Assert.AreEqual(
            new DateTime(2026, 10, 19, 9, 0, 0),
            result.ReminderDate);
    }
}
