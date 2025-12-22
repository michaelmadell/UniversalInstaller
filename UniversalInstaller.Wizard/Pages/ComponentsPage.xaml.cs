using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using UniversalInstaller.Core.Models;

namespace UniversalInstaller.Wizard.Pages
{
    public partial class ComponentsPage : UserControl
    {
        private readonly InstallerConfig _config;
        private ObservableCollection<ComponentViewModel> _components = new ObservableCollection<ComponentViewModel>();

        public ComponentsPage(InstallerConfig config)
        {
            InitializeComponent();
            _config = config;
            LoadComponents();
        }

        private void LoadComponents()
        {
            foreach (var component in _config.Components)
            {
                _components.Add(new ComponentViewModel
                {
                    Name = component.Name,
                    Description = component.Description,
                    IsSelected = !component.Fixed,
                    IsFixed = component.Fixed
                });
            }

            ComponentsListBox.ItemsSource = _components;
        }

        public string[] GetSelectedComponents()
        {
            return _components.Where(c => c.IsSelected).Select(c => c.Name).ToArray();
        }
    }

    public class ComponentViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsFixed { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (!IsFixed)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
