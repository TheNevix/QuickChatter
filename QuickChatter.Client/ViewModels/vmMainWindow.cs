using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuickChatter.Client.ViewModels
{
    public class vmMainWindow
    {
        public ICommand ConnectCommand { get; }

        public vmMainWindow()
        {
            ConnectCommand = new RelayCommand(ConnectToServer, CanButtonClick); ;
        }

        private void ConnectToServer(object sender)
        {
            // Your button click logic here
            System.Diagnostics.Debug.WriteLine("Button clicked!");
        }

        private bool CanButtonClick(object parameter)
        {
            // Return true or false depending on whether the button should be enabled
            return true;
        }
    }
}
