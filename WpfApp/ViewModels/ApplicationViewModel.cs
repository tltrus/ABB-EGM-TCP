using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp.ViewModels
{
    public class ApplicationViewModel : ViewModel
    {
        private ModelMotion motion;
        private RobotController controller;

        public ModelMotion Motion
        {
            get => motion;
            set
            {
                motion = value;
                OnPropertyChanged();
            }
        }

        #region Properties for button state management
        private bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDisconnected));
                OnPropertyChanged(nameof(CanSendCommands));
            }
        }

        public bool IsDisconnected => !_isConnected;
        public bool CanSendCommands => _isConnected;


        #endregion

        #region Data binding properties
        public string ActTcpX => motion.ActTcpX;
        public string ActTcpY => motion.ActTcpY;
        public string ActTcpZ => motion.ActTcpZ;
        public string ActTcpRx => motion.ActTcpRx;
        public string ActTcpRy => motion.ActTcpRy;
        public string ActTcpRz => motion.ActTcpRz;

        public string SetTcpX
        {
            get => motion.SetTcpX;
            set
            {
                if (motion.SetTcpX != value)
                {
                    motion.SetTcpX = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SetTcpY
        {
            get => motion.SetTcpY;
            set
            {
                if (motion.SetTcpY != value)
                {
                    motion.SetTcpY = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SetTcpZ
        {
            get => motion.SetTcpZ;
            set
            {
                if (motion.SetTcpZ != value)
                {
                    motion.SetTcpZ = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SetTcpRx
        {
            get => motion.SetTcpRx;
            set
            {
                if (motion.SetTcpRx != value)
                {
                    motion.SetTcpRx = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SetTcpRy
        {
            get => motion.SetTcpRy;
            set
            {
                if (motion.SetTcpRy != value)
                {
                    motion.SetTcpRy = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SetTcpRz
        {
            get => motion.SetTcpRz;
            set
            {
                if (motion.SetTcpRz != value)
                {
                    motion.SetTcpRz = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Commands with execution checks
        private RelayCommand connectCommand;
        public RelayCommand ConnectCommand
        {
            get
            {
                return connectCommand ?? (connectCommand = new RelayCommand(obj =>
                {
                    ConnectToRobot();
                },
                obj => IsDisconnected));
            }
        }

        private RelayCommand disconnectCommand;
        public RelayCommand DisconnectCommand
        {
            get
            {
                return disconnectCommand ?? (disconnectCommand = new RelayCommand(obj =>
                {
                    DisconnectFromRobot();
                },
                obj => IsConnected));
            }
        }

        private RelayCommand clearConsoleCommand;
        public RelayCommand ClearConsoleCommand
        {
            get
            {
                return clearConsoleCommand ?? (clearConsoleCommand = new RelayCommand(obj =>
                {
                    ClearConsole();
                },
                obj => true)); // Команда всегда доступна
            }
        }

        private RelayCommand readTcpCommand;
        public RelayCommand ReadTcpCommand
        {
            get
            {
                return readTcpCommand ?? (readTcpCommand = new RelayCommand(obj =>
                {
                    ReadTcp();
                },
                obj => CanSendCommands));
            }
        }
        private RelayCommand sendTcpCommand;
        public RelayCommand SendTcpCommand
        {
            get
            {
                return sendTcpCommand ?? (sendTcpCommand = new RelayCommand(obj =>
                {
                    SendTcp();
                },
                obj => CanSendCommands));
            }
        }

        const double step = 10.0;

        private RelayCommand sendXminusCommand;
        private RelayCommand sendXplusCommand;
        private RelayCommand sendYminusCommand;
        private RelayCommand sendYplusCommand;
        private RelayCommand sendZminusCommand;
        private RelayCommand sendZplusCommand;
        private RelayCommand sendRXminusCommand;
        private RelayCommand sendRXplusCommand;
        private RelayCommand sendRYminusCommand;
        private RelayCommand sendRYplusCommand;
        private RelayCommand sendRZminusCommand;
        private RelayCommand sendRZplusCommand;

        public RelayCommand SendXminusCommand
        {
            get
            {
                return sendXminusCommand ?? (sendXminusCommand = new RelayCommand(obj =>
                {
                    SendMovement(-step, 0, 0, 0, 0, 0);
                },
                obj => CanSendCommands));
            }
        }

        public RelayCommand SendXplusCommand
        {
            get
            {
                return sendXplusCommand ?? (sendXplusCommand = new RelayCommand(obj =>
                {
                    SendMovement(step, 0, 0, 0, 0, 0);
                },
                obj => CanSendCommands));
            }
        }

        public RelayCommand SendYminusCommand
        {
            get
            {
                return sendYminusCommand ?? (sendYminusCommand = new RelayCommand(obj =>
                {
                    SendMovement(0, -step, 0, 0, 0, 0);
                },
                obj => CanSendCommands));
            }
        }

        public RelayCommand SendYplusCommand
        {
            get
            {
                return sendYplusCommand ?? (sendYplusCommand = new RelayCommand(obj =>
                {
                    SendMovement(0, step, 0, 0, 0, 0);
                },
                obj => CanSendCommands));
            }
        }

        public RelayCommand SendZminusCommand
        {
            get
            {
                return sendZminusCommand ?? (sendZminusCommand = new RelayCommand(obj =>
                {
                    SendMovement(0, 0, -step, 0, 0, 0);
                },
                obj => CanSendCommands));
            }
        }

        public RelayCommand SendZplusCommand
        {
            get
            {
                return sendZplusCommand ?? (sendZplusCommand = new RelayCommand(obj =>
                {
                    SendMovement(0, 0, step, 0, 0, 0);
                },
                obj => CanSendCommands));
            }
        }

        public RelayCommand SendRXminusCommand
        {
            get
            {
                return sendRXminusCommand ?? (sendRXminusCommand = new RelayCommand(obj =>
                {
                    SendMovement(0, 0, 0, -step, 0, 0);
                },
                obj => CanSendCommands));
            }
        }

        public RelayCommand SendRXplusCommand
        {
            get
            {
                return sendRXplusCommand ?? (sendRXplusCommand = new RelayCommand(obj =>
                {
                    SendMovement(0, 0, 0, step, 0, 0);
                },
                obj => CanSendCommands));
            }
        }

        public RelayCommand SendRYminusCommand
        {
            get
            {
                return sendRYminusCommand ?? (sendRYminusCommand = new RelayCommand(obj =>
                {
                    SendMovement(0, 0, 0, 0, -step, 0);
                },
                obj => CanSendCommands));
            }
        }

        public RelayCommand SendRYplusCommand
        {
            get
            {
                return sendRYplusCommand ?? (sendRYplusCommand = new RelayCommand(obj =>
                {
                    SendMovement(0, 0, 0, 0, step, 0);
                },
                obj => CanSendCommands));
            }
        }

        public RelayCommand SendRZminusCommand
        {
            get
            {
                return sendRZminusCommand ?? (sendRZminusCommand = new RelayCommand(obj =>
                {
                    SendMovement(0, 0, 0, 0, 0, -step);
                },
                obj => CanSendCommands));
            }
        }

        public RelayCommand SendRZplusCommand
        {
            get
            {
                return sendRZplusCommand ?? (sendRZplusCommand = new RelayCommand(obj =>
                {
                    SendMovement(0, 0, 0, 0, 0, step);
                },
                obj => CanSendCommands));
            }
        }
        #endregion

        private string _consoleOutput = string.Empty;
        public string ConsoleOutput
        {
            get => _consoleOutput;
            set
            {
                _consoleOutput = value;
                OnPropertyChanged();
            }
        }

        public ApplicationViewModel()
        {
            Motion = new ModelMotion();

            InitializeController();

            // Initial state - disconnected
            IsConnected = false;
        }

        private void InitializeController()
        {
            // Create new controller instance
            controller = new RobotController("127.0.0.1", 6510);
            controller.TcpPositionUpdated += OnTcpPositionUpdated;
            controller.StatusMessage += OnStatusMessage;
        }

        private void OnTcpPositionUpdated(double[] tcpPosition)
        {
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                motion.ActTcpX = tcpPosition[0].ToString("F2");
                motion.ActTcpY = tcpPosition[1].ToString("F2");
                motion.ActTcpZ = tcpPosition[2].ToString("F2");
                motion.ActTcpRx = tcpPosition[3].ToString("F2");
                motion.ActTcpRy = tcpPosition[4].ToString("F2");
                motion.ActTcpRz = tcpPosition[5].ToString("F2");

                OnPropertyChanged(nameof(ActTcpX));
                OnPropertyChanged(nameof(ActTcpY));
                OnPropertyChanged(nameof(ActTcpZ));
                OnPropertyChanged(nameof(ActTcpRx));
                OnPropertyChanged(nameof(ActTcpRy));
                OnPropertyChanged(nameof(ActTcpRz));
            });
        }

        private void OnStatusMessage(string message) => AddConsoleMessage(message);

        public void AddConsoleMessage(string message)
        {
            ConsoleOutput += $"{DateTime.Now:HH:mm:ss} - {message}\n";
            OnPropertyChanged(nameof(ConsoleOutput));
        }
        private void ClearConsole()
        {
            ConsoleOutput = string.Empty;
            AddConsoleMessage("Console cleared");
        }

        private void ConnectToRobot()
        {
            try
            {
                // If controller was stopped, reinitialize it
                if (controller == null)
                {
                    InitializeController();
                }

                controller.StartReading();
                IsConnected = true;
                AddConsoleMessage("Connected to robot - Reading data\n");
            }
            catch (Exception ex)
            {
                AddConsoleMessage($"Connection error: {ex.Message}");
                IsConnected = false;

                // Reset controller on error
                controller?.Stop();
                controller = null;
            }
        }
        private void DisconnectFromRobot()
        {
            try
            {
                if (controller != null)
                {
                    controller.Stop();
                    IsConnected = false;
                    AddConsoleMessage("Disconnected from robot");
                }
            }
            catch (Exception ex)
            {
                AddConsoleMessage($"Disconnection error: {ex.Message}");
                IsConnected = false;
                controller = null;
            }
        }

        private string FormatPositions(double[] positions)
        {
            string[] formatted = new string[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                formatted[i] = Math.Round(positions[i], 2).ToString("F2");
            }
            return $"[{string.Join(", ", formatted)}]";
        }

        private void ReadTcp()
        {
            SetTcpX = motion.ActTcpX;
            SetTcpY = motion.ActTcpY;
            SetTcpZ = motion.ActTcpZ;
            SetTcpRx = motion.ActTcpRx;
            SetTcpRy = motion.ActTcpRy;
            SetTcpRz = motion.ActTcpRz;
        }

        private void SendTcp()
        {
            try
            {
                var culture = System.Globalization.CultureInfo.CurrentCulture;

                if (double.TryParse(SetTcpX, System.Globalization.NumberStyles.Any, culture, out double x) &&
                    double.TryParse(SetTcpY, System.Globalization.NumberStyles.Any, culture, out double y) &&
                    double.TryParse(SetTcpZ, System.Globalization.NumberStyles.Any, culture, out double z) &&
                    double.TryParse(SetTcpRx, System.Globalization.NumberStyles.Any, culture, out double rx) &&
                    double.TryParse(SetTcpRy, System.Globalization.NumberStyles.Any, culture, out double ry) &&
                    double.TryParse(SetTcpRz, System.Globalization.NumberStyles.Any, culture, out double rz))
                {
                    double[] tcpPosition = new double[] { x, y, z, rx, ry, rz };

                    //controller.StartControl();

                    AddConsoleMessage($"Sending TCP position: {FormatPositions(tcpPosition)}");

                    Task.Run(() =>
                    {
                        bool success = controller.MoveToTcpPosition(tcpPosition, 5000);
                        // Start control only when sending first command

                        if (success)
                        {
                            AddConsoleMessage($"Successfully reached TCP position: {FormatPositions(tcpPosition)}\n");
                        }
                        else
                        {
                            AddConsoleMessage($"Failed to reach target TCP position");
                        }
                    });
                }
                else
                {
                    AddConsoleMessage("Error: invalid data format for TCP position");
                }
            }
            catch (Exception ex)
            {
                AddConsoleMessage($"Error sending TCP position: {ex.Message}");
            }
        }

        private void SendMovement(double x, double y, double z, double rx, double ry, double rz)
        {
            try
            {
                var culture = CultureInfo.CurrentCulture;

                double X = double.Parse(motion.ActTcpX, culture);
                double Y = double.Parse(motion.ActTcpY, culture);
                double Z = double.Parse(motion.ActTcpZ, culture);
                double RX = double.Parse(motion.ActTcpRx, culture);
                double RY = double.Parse(motion.ActTcpRy, culture);
                double RZ = double.Parse(motion.ActTcpRz, culture);

                double[] tcpPosition = new double[] { X+x, Y+y, Z+z, RX+rx, RY+ry, RZ+rz };

                AddConsoleMessage($"Sending TCP position: {FormatPositions(tcpPosition)}");

                Task.Run(() =>
                {
                    bool success = controller.MoveToTcpPosition(tcpPosition, 5000);
                    if (success)
                    {
                        AddConsoleMessage($"Successfully reached TCP position: {FormatPositions(tcpPosition)}\n");
                    }
                    else
                    {
                        AddConsoleMessage($"Failed to reach target TCP position");
                    }
                });
            }
            catch (Exception ex)
            {
                AddConsoleMessage($"Error sending TCP position: {ex.Message}");
            }
        }
    }
}