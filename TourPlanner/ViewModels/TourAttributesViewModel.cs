using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;
using TourPlanner.Model.Events;

namespace TourPlanner.ViewModels;

public class TourAttributesViewModel : BaseViewModel
{
    // Dependencies
    private readonly ITourService _tourService;
    private readonly IAttributeService _attributeService;
    private readonly ILogger<TourAttributesViewModel> _logger;
    
    // Commands
    private RelayCommandAsync? _executeCalculateAttributes;
        
    public ICommand ExecuteCalculateAttributes => _executeCalculateAttributes ??= 
        new RelayCommandAsync(CalculateAttributes, _ => SelectedTour != null);

    // Element Bindings
    private Tour? _selectedTour;
    public Tour? SelectedTour
    {
        get => _selectedTour;
        set
        {
            _selectedTour = value;
            RaisePropertyChanged(nameof(SelectedTour));
        }
    }

    public TourAttributesViewModel(ITourService tourService, IAttributeService attributeService, IEventAggregator eventAggregator, ILogger<TourAttributesViewModel> logger) : base(eventAggregator)
    {
        _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
        _attributeService = attributeService ?? throw new ArgumentNullException(nameof(attributeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
        // Subscribe to changes in the selected tour (so that we can display its attributes and enable/disable the CalculateAttributes command)
        EventAggregator.Subscribe<SelectedTourChangedEvent>(OnSelectedTourChanged);
    }
    
    
    /// <summary>
    /// Event handler for when the selected tour changes.
    /// </summary>
    /// <param name="e">The event containing the newly selected tour</param>
    private async void OnSelectedTourChanged(SelectedTourChangedEvent e)
    {
        SelectedTour = e.SelectedTour;
        
        // Inform the UI that the command execution state may have changed
        _executeCalculateAttributes?.RaiseCanExecuteChanged();
    }
    
    
    /// <summary>
    /// Calculates the attributes for the selected tour asynchronously
    /// </summary>
    /// <param name="parameter"></param>
    private async Task CalculateAttributes(object? parameter)
    {
        if (SelectedTour == null)
        {
            _logger.Warn("No tour selected for attribute calculation.");
            return;
        }

        _logger.Info($"Calculating attributes for tour: {SelectedTour.TourName} (ID: {SelectedTour.TourId})");
        
        // Calculate the tour attributes asynchronously
        SelectedTour.Popularity = await _attributeService.CalculatePopularityAsync(SelectedTour);
        SelectedTour.ChildFriendlyRating = _attributeService.CalculateChildFriendliness(SelectedTour);
        SelectedTour.AiSummary = await _attributeService.GetAiSummaryAsync(SelectedTour);

        // Raise property changed to update the UI
        RaisePropertyChanged(nameof(SelectedTour));
        
        // Update the tour in the database
        await _tourService.UpdateTourAsync(SelectedTour);
    }
}