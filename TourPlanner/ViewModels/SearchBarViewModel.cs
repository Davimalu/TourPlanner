using System.Windows.Input;
using TourPlanner.Commands;

namespace TourPlanner.ViewModels
{
    public class SearchBarViewModel : BaseViewModel
    {
        private string _searchText = string.Empty;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                RaisePropertyChanged(nameof(SearchText));
            }
        }

        public event EventHandler<string>? SearchButtonClicked;

        public ICommand ExecuteSearch => new RelayCommand(_ =>
        {
            SearchButtonClicked?.Invoke(this, _searchText);
        }, _ => !string.IsNullOrEmpty(_searchText));
    }
}
