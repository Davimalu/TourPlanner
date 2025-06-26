using NSubstitute;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model.Events;
using TourPlanner.ViewModels;

namespace TourPlanner.Test.ViewModels
{
    [TestFixture]
    public class SearchBarViewModelTest
    {
        // Mocks for dependencies
        private IEventAggregator _mockEventAggregator;
        private ILogger<SearchBarViewModel> _mockLogger;

        // System Under Test (SUT)
        private SearchBarViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            // Create mocks for the dependencies
            _mockEventAggregator = Substitute.For<IEventAggregator>();
            _mockLogger = Substitute.For<ILogger<SearchBarViewModel>>();
            _viewModel = new SearchBarViewModel(_mockEventAggregator, _mockLogger);
        }

        [Test]
        public void Constructor_WhenEventAggregatorIsNull_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => new SearchBarViewModel(null!, _mockLogger));
        }

        [Test]
        public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => new SearchBarViewModel(_mockEventAggregator, null!));
        }

        [Test]
        public void SearchQuery_WhenSet_PublishesSearchQueryChangedEvent()
        {
            // Arrange
            var newQuery = "Test Search";

            // Act
            _viewModel.SearchQuery = newQuery;

            // Assert
            _mockEventAggregator.Received(1).Publish(Arg.Is<SearchQueryChangedEvent>(e => e.SearchQuery == newQuery));
        }

        [Test]
        public void ExecuteClearSearchQuery_CanExecute_IsFalse_WhenSearchQueryIsNullOrEmpty()
        {
            // Arrange
            _viewModel.SearchQuery = string.Empty;

            // Act & Assert
            Assert.IsFalse(_viewModel.ExecuteClearSearchQuery.CanExecute(null));
        }

        [Test]
        public void ExecuteClearSearchQuery_CanExecute_IsTrue_WhenSearchQueryHasText()
        {
            // Arrange
            _viewModel.SearchQuery = "Some Text";

            // Act & Assert
            Assert.IsTrue(_viewModel.ExecuteClearSearchQuery.CanExecute(null));
        }

        [Test]
        public void ClearSearchQuery_WhenExecuted_SetsSearchQueryToEmpty()
        {
            // Arrange
            _viewModel.SearchQuery = "Initial Text";

            // Act
            _viewModel.ExecuteClearSearchQuery.Execute(null);

            // Assert
            Assert.That(_viewModel.SearchQuery, Is.Empty);
        }

        [Test]
        public void ClearSearchQuery_WhenExecuted_PublishesEventTwiceDueToRedundancy()
        {
            // Arrange
            _viewModel.SearchQuery = "Initial Text";

            // Act
            _viewModel.ExecuteClearSearchQuery.Execute(null);

            // Assert
            // The event is published twice because the ClearSearchQuery method will call it once, then the setter too
            // We might refactor this, but I think it's fine to explicitly publish it in the ClearSearchQuery method
            _mockEventAggregator.Received(2).Publish(Arg.Is<SearchQueryChangedEvent>(e => e.SearchQuery == string.Empty));
        }
    }
}
