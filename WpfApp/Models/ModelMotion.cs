namespace WpfApp
{
    public class ModelMotion : Model
    {
        // ----------------------------
        // Actual TCP position
        // ----------------------------

        private string _actTcpX = "0";
        private string _actTcpY = "0";
        private string _actTcpZ = "0";
        private string _actTcpRx = "0";
        private string _actTcpRy = "0";
        private string _actTcpRz = "0";

        public string ActTcpX
        {
            get => _actTcpX;
            set
            {
                _actTcpX = value;
                OnPropertyChanged();
            }
        }
        public string ActTcpY
        {
            get => _actTcpY;
            set
            {
                _actTcpY = value;
                OnPropertyChanged();
            }
        }
        public string ActTcpZ
        {
            get => _actTcpZ;
            set
            {
                _actTcpZ = value;
                OnPropertyChanged();
            }
        }
        public string ActTcpRx
        {
            get => _actTcpRx;
            set
            {
                _actTcpRx = value;
                OnPropertyChanged();
            }
        }
        public string ActTcpRy
        {
            get => _actTcpRy;
            set
            {
                _actTcpRy = value;
                OnPropertyChanged();
            }
        }
        public string ActTcpRz
        {
            get => _actTcpRz;
            set
            {
                _actTcpRz = value;
                OnPropertyChanged();
            }
        }

        // ----------------------------
        // Set TCP position
        // ----------------------------

        private string _setTcpX = "0";
        private string _setTcpY = "0";
        private string _setTcpZ = "0";
        private string _setTcpRx = "0";
        private string _setTcpRy = "0";
        private string _setTcpRz = "0";

        public string SetTcpX
        {
            get => _setTcpX;
            set
            {
                _setTcpX = value;
                OnPropertyChanged();
            }
        }
        public string SetTcpY
        {
            get => _setTcpY;
            set
            {
                _setTcpY = value;
                OnPropertyChanged();
            }
        }
        public string SetTcpZ
        {
            get => _setTcpZ;
            set
            {
                _setTcpZ = value;
                OnPropertyChanged();
            }
        }
        public string SetTcpRx
        {
            get => _setTcpRx;
            set
            {
                _setTcpRx = value;
                OnPropertyChanged();
            }
        }
        public string SetTcpRy
        {
            get => _setTcpRy;
            set
            {
                _setTcpRy = value;
                OnPropertyChanged();
            }
        }
        public string SetTcpRz
        {
            get => _setTcpRz;
            set
            {
                _setTcpRz = value;
                OnPropertyChanged();
            }
        }
    }
}