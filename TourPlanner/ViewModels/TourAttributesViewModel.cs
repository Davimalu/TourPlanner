using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.DAL.Interfaces;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;

namespace TourPlanner.ViewModels;

public class TourAttributesViewModel : BaseViewModel
{
    private readonly ISelectedTourService _selectedTourService;
    private readonly ITourService _tourService;
    private readonly IAttributeService _attributeService;
    private readonly ILoggerWrapper _logger;
    
    private RelayCommandAsync? _executeCalculateAttributes;
        
    public ICommand ExecuteCalculateAttributes => _executeCalculateAttributes ??= 
        new RelayCommandAsync(CalculateAttributes, _ => SelectedTour != null);

    private Tour? _selectedTour;
    public Tour? SelectedTour
    {
        get => _selectedTour;
        set
        {
            _selectedTour = value;
            RaisePropertyChanged(nameof(SelectedTour));

            // Inform the UI that the command execution state may have changed
            _executeCalculateAttributes?.RaiseCanExecuteChanged();
        }
    }

    public TourAttributesViewModel(ISelectedTourService selectedTourService, ITourService tourService, IAttributeService attributeService)
    {
        _selectedTourService = selectedTourService ?? throw new ArgumentNullException(nameof(selectedTourService));
        _tourService = tourService ?? throw new ArgumentNullException(nameof(tourService));
        _attributeService = attributeService ?? throw new ArgumentNullException(nameof(attributeService));
        _logger = LoggerFactory.GetLogger<TourAttributesViewModel>();
            
        _selectedTourService.SelectedTourChanged += (selectedTour) => SelectedTour = selectedTour; // Get the currently selected tour from the service
    }
    
    
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
        SelectedTour.ChildFriendlyRating = _attributeService.CalulateChildFriendliness(SelectedTour);

        // Raise property changed to update the UI
        RaisePropertyChanged(nameof(SelectedTour));
        
        // Update the tour in the database
        await _tourService.UpdateTourAsync(SelectedTour);
    }
}