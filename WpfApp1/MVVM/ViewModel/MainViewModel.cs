using System;
using System.Windows;
using System.Diagnostics;
using WpfApp1.Core;

namespace WpfApp1.MVVM.ViewModel
{
    class MainViewModel : ObservableObjects
    {

        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand ConnectViewCommand { get; set; }


        public homeViewModel HomeVM { get; set; }
        public connectViewModel ConnectVM { get; set; }
        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set 
            { 
                _currentView = value;
                OnPropertyChanged();
            }
        }


        public MainViewModel() 
        {
            HomeVM = new homeViewModel();
            ConnectVM = new connectViewModel();

            CurrentView = ConnectVM;

            HomeViewCommand = new RelayCommand(o => 
            {
                CurrentView = HomeVM;
                
            });

            ConnectViewCommand = new RelayCommand(o =>
            {
                CurrentView = ConnectVM;

            });
        }
    }
}
