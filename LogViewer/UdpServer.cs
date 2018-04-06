using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer
{
    public class UdpServer
    {
        public event Action<string> LogReceived;
        public event Action ServerStarted;
        public event Action ServerStoped;
        private IPEndPoint _remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);
        private UdpClient _udpClient;
        private Task _mainTask;
        private bool _stop;
        public void Start(int port)
        {
            _stop = false;
            _udpClient = new UdpClient(port);
            Task.Factory.StartNew(ReadThread);
        }

        public void Stop()
        {
            _stop = true;
            _udpClient.Close();
        }

        void ReadThread()
        {
            ServerStarted?.Invoke();
            while (!_stop)
            {
                try
                {
                    var data = _udpClient.Receive(ref _remoteIPEndPoint);
                    var loggingEvent = Encoding.UTF8.GetString(data);
                    if (LogReceived != null)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                LogReceived(loggingEvent);
                            }
                            catch (Exception ex)
                            {

                            }
                        });
                    }
                }
                catch (Exception ex)
                {

                }
            }
            ServerStoped?.Invoke();
        }
    }
}
