using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TourPlanner.DAL.ServiceAgents;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Model;

namespace TourPlanner.Test.DAL;

[TestFixture]
public class LocalTourServiceTest
{
    // Dependencies
    private ILogger<LocalTourService> _mockLogger;
    private IFileSystemWrapper _mockFileSystemWrapper;

    // System Under Test (SUT)
    private LocalTourService _sut;

    [SetUp]
    public void SetUp()
    {
        // Create mocks for the dependencies
        _mockLogger = Substitute.For<ILogger<LocalTourService>>();
        _mockFileSystemWrapper = Substitute.For<IFileSystemWrapper>();

        // Create a new instance of the service with the mocks
        _sut = new LocalTourService(_mockLogger, _mockFileSystemWrapper);
    }
    
    [Test]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Arrange: Create a null logger
        ILogger<LocalTourService>? nullLogger = null;

        // Act & Assert: Creating an instance of LocalTourService with a null logger should throw an ArgumentNullException
        Assert.Throws<ArgumentNullException>(() => new LocalTourService(nullLogger!, _mockFileSystemWrapper));
    }

    [Test]
    public void Constructor_WhenFileSystemWrapperIsNull_ThrowsArgumentNullException()
    {
        // Arrange: Create a null file system wrapper
        IFileSystemWrapper? nullFileSystemWrapper = null;

        // Act & Assert: Creating an instance of LocalTourService with a null file system wrapper should throw an ArgumentNullException
        Assert.Throws<ArgumentNullException>(() => new LocalTourService(_mockLogger, nullFileSystemWrapper!));
    }
    
    [Test]
    public async Task SaveToursToFileAsync_WhenSuccessful_ReturnsTrueAndLogsInfo()
    {
        // Arrange
        var tours = new List<Tour>
        {
            new() { TourId = 1, TourName = "Vienna City Tour" }
        };
        var path = "C:\\tours\\export.tours";
        var expectedJson = JsonConvert.SerializeObject(tours);
        
        // Act
        var result = await _sut.SaveToursToFileAsync(tours, path);

        // Assert
        Assert.That(result, Is.True);
        
        // Verify that the file system wrapper was called correctly
        await _mockFileSystemWrapper.Received(1).WriteAllTextAsync(path, expectedJson);
        
        // Verify that the correct log message was written
        _mockLogger.Received(1).Info(Arg.Any<string>());
        _mockLogger.DidNotReceive().Error(Arg.Any<string>());
    }

    [Test]
    public async Task SaveToursToFileAsync_WhenFileSystemThrowsException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var tours = new List<Tour> { new() { TourId = 1, TourName = "Vienna City Tour" } };
        var path = "C:\\tours\\export.tours";
        var exceptionMessage = "Access to the path is denied.";
        
        // Mock the file system wrapper to throw an exception when trying to write
        _mockFileSystemWrapper.WriteAllTextAsync(Arg.Any<string>(), Arg.Any<string>())
            .ThrowsAsync(new IOException(exceptionMessage));

        // Act
        var result = await _sut.SaveToursToFileAsync(tours, path);

        // Assert
        Assert.That(result, Is.False);
        
        // Verify that the error was logged
        _mockLogger.Received(1).Error(Arg.Is<string>(msg => msg.Contains(exceptionMessage)));
    }
    
    [Test]
    public async Task LoadToursFromFileAsync_WhenFileExistsAndIsValid_ReturnsToursAndLogsInfo()
    {
        // Arrange
        var path = "C:\\tours\\import.tours";
        var toursList = new List<Tour> { new() { TourId = 1, TourName = "Alpine Adventure" } };
        var jsonContent = JsonConvert.SerializeObject(toursList);

        // Mock the file system wrapper to simulate the file existing and returning valid JSON content
        _mockFileSystemWrapper.Exists(path).Returns(true);
        _mockFileSystemWrapper.ReadAllTextAsync(path).Returns(Task.FromResult(jsonContent));

        // Act
        var result = await _sut.LoadToursFromFileAsync(path);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().TourName, Is.EqualTo("Alpine Adventure"));
        
        // Verify logging
        _mockLogger.Received(1).Info(Arg.Any<string>());
        _mockLogger.DidNotReceive().Warn(Arg.Any<string>());
        _mockLogger.DidNotReceive().Error(Arg.Any<string>());
    }
    
    [Test]
    public async Task LoadToursFromFileAsync_WhenFileDoesNotExist_ReturnsNullAndLogsWarning()
    {
        // Arrange
        var path = "C:\\tours\\nonexistent.tours";
        _mockFileSystemWrapper.Exists(path).Returns(false);

        // Act
        var result = await _sut.LoadToursFromFileAsync(path);

        // Assert
        Assert.That(result, Is.Null);
        
        // Verify logging
        _mockLogger.Received(1).Warn(Arg.Any<string>());
        
        // Verify that we didn't try to read the file
        await _mockFileSystemWrapper.DidNotReceive().ReadAllTextAsync(Arg.Any<string>());
    }

    [Test]
    [TestCase("[]", "is empty or invalid")] // Empty array
    [TestCase("null", "is empty or invalid")] // JSON null
    public async Task LoadToursFromFileAsync_WhenFileContentIsInvalidOrEmptyList_ReturnsNullAndLogsWarning(string content, string expectedLogMessage)
    {
        // Arrange
        var path = "C:\\tours\\empty.tours";
        _mockFileSystemWrapper.Exists(path).Returns(true);
        _mockFileSystemWrapper.ReadAllTextAsync(path).Returns(Task.FromResult(content));

        // Act
        var result = await _sut.LoadToursFromFileAsync(path);

        // Assert
        Assert.That(result, Is.Null);
        _mockLogger.Received(1).Warn(Arg.Is<string>(msg => msg.Contains(expectedLogMessage)));
        _mockLogger.DidNotReceive().Info(Arg.Any<string>());
        _mockLogger.DidNotReceive().Error(Arg.Any<string>());
    }

    [Test]
    public async Task LoadToursFromFileAsync_WhenDeserializationFails_ReturnsNullAndLogsError()
    {
        // Arrange
        var path = "C:\\tours\\malformed.tours";
        var malformedJson = "{ not_a_valid_json }";
        
        _mockFileSystemWrapper.Exists(path).Returns(true);
        _mockFileSystemWrapper.ReadAllTextAsync(path).Returns(Task.FromResult(malformedJson));

        // Act
        var result = await _sut.LoadToursFromFileAsync(path);

        // Assert
        Assert.That(result, Is.Null);
        _mockLogger.Received(1).Error(Arg.Any<string>());
    }
}