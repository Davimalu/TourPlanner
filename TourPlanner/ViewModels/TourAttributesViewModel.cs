using System.Windows.Input;
using TourPlanner.Commands;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;
using TourPlanner.Model;

namespace TourPlanner.ViewModels;

public class TourAttributesViewModel : BaseViewModel
{
    private readonly ISelectedTourService _selectedTourService;
    private readonly IAttributeService _attributeService;
    private readonly ILoggerWrapper _logger;

    private Tour? _selectedTour;
    public Tour? SelectedTour
    {
        get { return _selectedTour; }
        set
        {
            _selectedTour = value;
            RaisePropertyChanged(nameof(SelectedTour));
        }
    }

    public TourAttributesViewModel(ISelectedTourService selectedTourService, IAttributeService attributeService)
    {
        _selectedTourService = selectedTourService ?? throw new ArgumentNullException(nameof(selectedTourService));
        _attributeService = attributeService ?? throw new ArgumentNullException(nameof(attributeService));
        _logger = LoggerFactory.GetLogger<TourAttributesViewModel>();
            
        _selectedTourService.SelectedTourChanged += (selectedTour) => SelectedTour = selectedTour; // Get the currently selected tour from the service
    }
    
    
    public ICommand ExecuteCalculateAttributes => new RelayCommand(_ =>
    {
        if (SelectedTour == null)
        {
            return;
        }

        // Calculate the tour attributes
        SelectedTour.Popularity = _attributeService.CalculatePopularity(SelectedTour);
        SelectedTour.ChildFriendlyRating = _attributeService.CalulateChildFriendliness(SelectedTour);

        // Raise property changed to update the UI
        RaisePropertyChanged(nameof(SelectedTour));
        
    }, _ => SelectedTour != null);
}