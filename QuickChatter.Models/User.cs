using System.ComponentModel;
using System.Net.Sockets;

namespace QuickChatter.Models
{
    public class User : INotifyPropertyChanged
    {
        private bool _isAvailable;
        public bool IsAvailable
        {
            get => _isAvailable;
            set
            {
                if (_isAvailable != value)
                {
                    _isAvailable = value;
                    OnPropertyChanged(nameof(IsAvailable));
                }
            }
        }
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Ip { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
