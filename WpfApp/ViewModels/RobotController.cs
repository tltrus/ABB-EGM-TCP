using System.Net.Sockets;
using System.Net;
using Google.Protobuf;
using Abb.Egm;
using System.Numerics;
using System.Threading;

namespace WpfApp.ViewModels
{
    public class RobotController
    {
        private UdpClient udpClient;
        private IPEndPoint robotEndpoint;
        private uint sequenceNumber = 0;
        private bool isConnected = false;
        private Thread sendThread;
        private Thread receiveThread;
        private bool keepSending = false;
        private double[] currentTarget = new double[6]; // X, Y, Z, Rx, Ry, Rz (в мм и градусах)
        private double[] currentTcpPosition = new double[6]; // X, Y, Z, Rx, Ry, Rz (в мм и градусах)
        private bool targetReached = false;
        private object lockObject = new object();
        private double positionTolerance = 1.0; // Tolerance in mm for position
        private double rotationTolerance = 0.5; // Tolerance in degrees for rotation
        private bool isFirstMovement = true;
        private int localPort;

        // Events for UI updates
        public event Action<double[]> TcpPositionUpdated;
        public event Action<string> StatusMessage;

        public RobotController(string robotIP, int robotPort, int localPort = 6510)
        {
            this.localPort = localPort;
            robotEndpoint = new IPEndPoint(IPAddress.Parse(robotIP), robotPort);

            // Initialize arrays
            currentTcpPosition = new double[6] { 0, 0, 0, 0, 0, 0 };
            currentTarget = new double[6] { 0, 0, 0, 0, 0, 0 };
        }

        // RECEIVE DATA
        public void StartReading()
        {
            StatusMessage?.Invoke("Starting robot data reading...");

            try
            {
                // Create new UDP client
                if (udpClient == null)
                {
                    udpClient = new UdpClient(localPort);
                    udpClient.Client.ReceiveTimeout = 5000;
                }

                keepSending = true;

                // Stop existing threads if they're running
                if (receiveThread != null && receiveThread.IsAlive)
                {
                    receiveThread.Join(100);
                }

                receiveThread = new Thread(ReceiveData);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                isConnected = true;
            }
            catch (Exception ex)
            {
                StatusMessage?.Invoke($"Error starting reading: {ex.Message}");
                isConnected = false;
                throw;
            }
        }
        private void ReceiveData()
        {
            while (keepSending)
            {
                try
                {
                    byte[] receivedData = udpClient.Receive(ref robotEndpoint);
                    var robotMsg = EgmRobot.Parser.ParseFrom(receivedData);

                    // Process TCP data
                    if (robotMsg?.FeedBack?.Cartesian != null)
                    {
                        var pos = robotMsg.FeedBack.Cartesian.Pos;
                        var orient = robotMsg.FeedBack.Cartesian.Orient;

                        currentTcpPosition[0] = Math.Round(pos.X, 2);
                        currentTcpPosition[1] = Math.Round(pos.Y, 2);
                        currentTcpPosition[2] = Math.Round(pos.Z, 2);


                        currentTcpPosition[3] = Math.Round(orient.U0, 2); // Rx
                        currentTcpPosition[4] = Math.Round(orient.U1, 2); // Ry
                        currentTcpPosition[5] = Math.Round(orient.U2, 2); // Rz

                        // Notify UI
                        TcpPositionUpdated?.Invoke((double[])currentTcpPosition.Clone());

                        // Check if target reached
                        if (!targetReached && IsTcpPositionReached(currentTarget, currentTcpPosition))
                        {
                            targetReached = true;
                            StatusMessage?.Invoke($"Target TCP position reached: {FormatPositions(currentTcpPosition)}");
                        }
                    }

                    // Check EGM convergence flag
                    if (robotMsg.MciConvergenceMet && !targetReached && !isFirstMovement)
                    {
                        targetReached = true;
                        StatusMessage?.Invoke("EGM reports target achievement");
                    }
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    continue;
                }
                catch (SocketException ex)
                {
                    if (keepSending)
                    {
                        StatusMessage?.Invoke($"Socket error: {ex.Message}");
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    if (keepSending)
                    {
                        StatusMessage?.Invoke($"Receive error: {ex.Message}");
                        Thread.Sleep(1000);
                    }
                }
            }
        }


        public double[] GetCurrentTcpPosition() => (double[])currentTcpPosition.Clone();

        // SEND DATA
        public void StartControl()
        {
            StatusMessage?.Invoke("Starting robot control...");

            if (sendThread == null || !sendThread.IsAlive)
            {
                sendThread = new Thread(ContinuousSend);
                sendThread.IsBackground = true;
                sendThread.Start();
            }

            Thread.Sleep(500);
            StatusMessage?.Invoke("Ready to send TCP commands");
        }

        public bool MoveToTcpPosition(double[] tcpPosition, int timeoutMs = 30000)
        {
            if (tcpPosition.Length != 6)
            {
                StatusMessage?.Invoke("Error: array of 6 values required (X, Y, Z, Rx, Ry, Rz)");
                return false;
            }

            // Check if already at target
            if (!isFirstMovement && IsTcpPositionReached(tcpPosition, currentTcpPosition))
            {
                StatusMessage?.Invoke("Robot is already at target TCP position");
                targetReached = true;
                return true;
            }

            Array.Copy(tcpPosition, currentTarget, 6);
            targetReached = false;

            StatusMessage?.Invoke($"Starting movement to TCP position: {FormatPositions(tcpPosition)}");
            StatusMessage?.Invoke($"Current TCP position: {FormatPositions(currentTcpPosition)}");

            if (isFirstMovement)
            {
                isFirstMovement = false;
            }

            StartControl();

            DateTime startTime = DateTime.Now;
            bool positionVisuallyReached = false;

            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                if (IsTcpPositionReached(currentTarget, currentTcpPosition))
                {
                    if (!positionVisuallyReached)
                    {
                        StatusMessage?.Invoke($"Visual TCP position achievement: {FormatPositions(currentTcpPosition)}");
                        positionVisuallyReached = true;
                    }

                    if (targetReached || (DateTime.Now - startTime).TotalMilliseconds > 1000)
                    {
                        StatusMessage?.Invoke("Target TCP position reached!");
                        return true;
                    }
                }

                Thread.Sleep(100);
            }

            StatusMessage?.Invoke($"Timeout! Movement not completed in {timeoutMs}ms");
            StatusMessage?.Invoke($"Current TCP position: {FormatPositions(currentTcpPosition)}");
            return false;
        }

        private double AngularDifference(double a, double b)
        {
            double diff = Math.Abs(NormalizeAngle(a) - NormalizeAngle(b));
            return diff > 180 ? 360 - diff : diff;
        }

        private bool IsTcpPositionReached(double[] target, double[] current)
        {
            for (int i = 0; i < 3; i++)
            {
                if (Math.Abs(target[i] - current[i]) > positionTolerance)
                    return false;
            }

            for (int i = 3; i < 6; i++)
            {
                if (AngularDifference(target[i], current[i]) > rotationTolerance)
                    return false;
            }

            return true;
        }

        private double NormalizeAngle(double angle)
        {
            angle = (angle + 180) % 360;
            if (angle < 0) angle += 360;
            return angle - 180;
        }

        private string FormatPositions(double[] positions)
        {
            return $"X:{positions[0]:F2} mm, Y:{positions[1]:F2} mm, Z:{positions[2]:F2} mm, " +
                   $"Rx:{positions[3]:F2}°, Ry:{positions[4]:F2}°, Rz:{positions[5]:F2}°";
        }

        private void ContinuousSend()
        {
            while (keepSending)
            {
                try
                {
                    if (isConnected && udpClient != null)
                    {
                        var sensorMsg = new EgmSensor
                        {
                            Header = new EgmHeader
                            {
                                Seqno = sequenceNumber++,
                                Tm = (uint)DateTime.Now.Ticks,
                                Mtype = EgmHeader.Types.MessageType.MsgtypeCorrection
                            },
                            Planned = new EgmPlanned
                            {
                                Cartesian = new EgmPose()
                            }
                        };

                        lock (lockObject)
                        {
                            // Position: convert mm to meters if needed
                            // Assume currentTarget[0..2] are in mm
                            sensorMsg.Planned.Cartesian.Pos = new EgmCartesian
                            {
                                X = currentTarget[0],
                                Y = currentTarget[1],
                                Z = currentTarget[2]
                            };

                            // Orientation: ABB Euler (Rx,Ry,Rz in degrees) → quaternion
                            //double[] quat = EulerToQuaternionABB(
                            //    currentTarget[3], // Rx
                            //    currentTarget[4], // Ry
                            //    currentTarget[5]  // Rz
                            //);

                            sensorMsg.Planned.Cartesian.Orient = new EgmQuaternion
                            {
                                U0 = currentTarget[3],
                                U1 = currentTarget[4],
                                U2 = currentTarget[5],
                                U3 = 0
                            };
                        }

                        byte[] data = sensorMsg.ToByteArray();
                        udpClient.Send(data, data.Length, robotEndpoint);
                    }

                    Thread.Sleep(4); // ~250 Hz
                }
                catch (Exception ex)
                {
                    if (keepSending)
                    {
                        StatusMessage?.Invoke($"Send error: {ex.Message}");
                        Thread.Sleep(10);
                    }
                }
            }
        }

        public void Stop()
        {
            keepSending = false;
            isConnected = false;
            targetReached = true;

            sendThread?.Join(500);
            receiveThread?.Join(500);

            if (udpClient != null)
            {
                try
                {
                    udpClient.Close();
                    udpClient = null;
                }
                catch (Exception ex)
                {
                    StatusMessage?.Invoke($"Error closing UDP client: {ex.Message}");
                }
            }

            sendThread = null;
            receiveThread = null;

            StatusMessage?.Invoke("Control stopped");
        }

        public bool IsActive() => isConnected && keepSending;
    }
}