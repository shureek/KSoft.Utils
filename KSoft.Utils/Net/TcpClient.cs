using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Sockets = System.Net.Sockets;

namespace KSoft.Net
{
    /// <summary>
    /// TCP client, that reconnects if connection is lost.
    /// </summary>
    /// <remarks>
    /// Not ready.
    /// </remarks>
    sealed class TcpClient : IDisposable
    {
        Sockets.Socket socket;
        bool active;
        Sockets.AddressFamily family = Sockets.AddressFamily.InterNetwork;
        bool disposed = false;

        public string RemoteAddress { get; set; }
        public int RemotePort { get; set; }

        Sockets.SocketError Send(byte[] buffer, int offset, int size, Sockets.SocketFlags socketFlags = Sockets.SocketFlags.None)
        {
            Sockets.SocketError result = Sockets.SocketError.Success;
            int bytesSent = 0;
            while (bytesSent < size && result == Sockets.SocketError.Success)
            {
                bytesSent += socket.Send(buffer, offset + bytesSent, size - bytesSent, socketFlags, out result);
            }
            if (result == Sockets.SocketError.Success && bytesSent < size)
                throw new System.IO.IOException("Сообщение отправлено не полностью");
            return result;
        }

        void IDisposable.Dispose()
        {
            if (disposed)
                return;

            var _socket = socket;
            if (_socket != null)
            {
                try
                {
                    _socket.Shutdown(Sockets.SocketShutdown.Both);
                }
                finally
                {
                    _socket.Close();
                    socket = null;
                }
            }
            GC.SuppressFinalize(this);
            disposed = true;
        }
    }
}
