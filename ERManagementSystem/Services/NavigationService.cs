using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERManagementSystem.Services
{
    public class NavigationService : INavigationService
    {
        private Frame? _frame;

        public void Initialize(Frame frame)
        {
            _frame = frame;
        }

        public void Navigate(Type pageType)
        {
            _frame?.Navigate(pageType);
        }

        public void Navigate(Type pageType, object parameter) // parameter - the ViewModel associated with the page
        {
            _frame?.Navigate(pageType, parameter);
        }
    }
}
