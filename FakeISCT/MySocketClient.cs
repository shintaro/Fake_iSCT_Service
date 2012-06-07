using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;

namespace FakeISCT
{
    class MySocketClient
    {
        private TcpClient clientSock;
        private NetworkStream netStream;
        private EventLog eventLog;

        public MySocketClient(EventLog el)
        {
            eventLog = el;
        }

        public bool open(String address, int port)
        {
            try
            {
                clientSock = new System.Net.Sockets.TcpClient();
                clientSock.Connect(address, port);
                netStream = clientSock.GetStream();
            }
            catch (Exception ex)
            {
                eventLog.WriteEntry("Failed to open socket: " + ex.Message);
                return false;
            }
            return true;
        }

        public void close()
        {
            netStream.Close();
            clientSock.Close();
        }

        public bool sendSleepTime(int sec)
        {
            Byte[] dat = System.Text.Encoding.GetEncoding("utf-8").GetBytes(sec.ToString());
            try
            {
                netStream.Write(dat, 0, dat.GetLength(0));
            }
            catch (Exception ex)
            {
                eventLog.WriteEntry("Failed to send a message to server: " + ex.Message);
                return false;
            }
            return true;
        }

        public string receive()
        {
            if (clientSock.Available > 0)
            {
                Byte[] dat = new Byte[clientSock.Available];
                try
                {
                    netStream.Read(dat, 0, dat.GetLength(0));
                }
                catch (Exception ex)
                {
                    eventLog.WriteEntry("Failed to receive message" + ex.Message);
                }

                return System.Text.Encoding.GetEncoding("utf-8").GetString(dat);
            }
            return "Fail";
        }
    }
}
