using PureVPN.ViewModels.Base;

namespace PureVPN.Helpers
{
    public class NavigationStore : ViewModel
    {
        private ViewModel _currentViewModel;

        public ViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => Set(ref _currentViewModel, value);
        }
    }
}
