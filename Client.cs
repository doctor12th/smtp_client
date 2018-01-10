
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SMTPSender
{
    public class Client
    {
        private Socket _Socket = null;
        private string _Host = String.Empty;
        private int _Port = 25;
        private string _UserName = String.Empty;
        private string _Password = String.Empty;
        
        public bool IsError { get; set; }

        public Client(string host, string userName, string password) : this(host, 25, userName, password) { }
        public Client(string host, int port, string userName, string password)
        {
            if (String.IsNullOrEmpty(host)) throw new Exception("Необходимо указать адрес smtp-сервера.");
            if (String.IsNullOrEmpty(userName)) throw new Exception("Необходимо указать логин пользователя.");
            if (String.IsNullOrEmpty(password)) throw new Exception("Необходимо указать пароль пользователя.");
            if (port <= 0) port = 25;
            // --
            _Host = host;
            _Password = password;
            _Port = port;
            _UserName = userName;
            Connect();
        }

        /// <summary>
        /// Метод осуществляет подключение к почтовому серверу
        /// </summary>
        public void Connect()
        {
            IPHostEntry myIPHostEntry = Dns.GetHostEntry(_Host);

            if (myIPHostEntry == null || myIPHostEntry.AddressList == null || myIPHostEntry.AddressList.Length <= 0)
            {
                throw new Exception("Не удалось определить IP-адрес по хосту.");
            }
            
            IPEndPoint myIPEndPoint = new IPEndPoint(myIPHostEntry.AddressList[0], _Port);
            
            _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _Socket.ReceiveBufferSize = 512; 
            
            WriteToLog("Соединяюсь с сервером {0}:{1}", _Host, _Port);
            _Socket.Connect(myIPEndPoint);
            ReadLine();

            Command("HELO local");
            ReadLine();

            Command("AUTH LOGIN");
            ReadLine();
            
            Command(Convert.ToBase64String(Encoding.ASCII.GetBytes(_UserName)));
            ReadLine();

            Command(String.Format(Convert.ToBase64String(Encoding.ASCII.GetBytes(_Password))));
            string s = ReadLine();
            if (s == "" || s.StartsWith("502"))
            {
                IsError = true;
            }
            else IsError = false;

        }

        /// <summary>
        /// Метод завершает сеанс связи с сервером
        /// </summary>
        public void Close()
        {
            if (_Socket == null) { return; }
            Command("QUIT");
            ReadLine();
            _Socket.Close();
        }
        /// <summary>
        /// Метод отправляет команду почтовому серверу
        /// </summary>
        /// <param name="cmd">Команда</param>
        public void Command(string cmd)
        {
            if (_Socket == null) throw new Exception("Connection is interrupt.");
            WriteToLog("Command {0}", cmd);// logging
            byte[] b = System.Text.Encoding.ASCII.GetBytes(String.Format("{0}\r\n", cmd));
            if (_Socket.Send(b, b.Length, SocketFlags.None) != b.Length)
            {
                throw new Exception("An error has occured");
            }
        }
        
        /// <summary>
        /// Считывает первую строку ответа сервера из буфера
        /// </summary>
        public string ReadLine()
        {
            byte[] b = new byte[_Socket.ReceiveBufferSize];
            StringBuilder result = new StringBuilder(_Socket.ReceiveBufferSize);
            int s = 0;

            while (_Socket.Poll(1000000, SelectMode.SelectRead) && (s = _Socket.Receive(b, _Socket.ReceiveBufferSize, SocketFlags.None)) > 0)
            {
                result.Append(System.Text.Encoding.ASCII.GetChars(b, 0, s));
            }

            WriteToLog(result.ToString().TrimEnd("\r\n".ToCharArray()));

            return result.ToString().TrimEnd("\r\n".ToCharArray());
        }

        /// <summary>
        /// Читает и возвращает все содержимое ответа сервера из буфера
        /// </summary>
        public string ReadToEnd()
        {
            byte[] b = new byte[_Socket.ReceiveBufferSize];
            StringBuilder result = new StringBuilder(_Socket.ReceiveBufferSize);
            int s = 0;

            while (_Socket.Poll(1000000, SelectMode.SelectRead) && ((s = _Socket.Receive(b, _Socket.ReceiveBufferSize, SocketFlags.None)) > 0))
            {
                result.Append(System.Text.Encoding.ASCII.GetChars(b, 0, s));
            }


            if (result.Length > 0 && result.ToString().IndexOf("\r\n") != -1)
            {
                WriteToLog(result.ToString().Substring(0, result.ToString().IndexOf("\r\n")));
            }


            return result.ToString();
        }
        /// <summary>
        /// Отправить письмо
        /// </summary>
        /// <param name="address">Получатель письма</param>
        /// <param name="message">Текст письма</param>
        /// <param name="title">Опционально: тема письма</param>
        public void SendTo(string address, string message, string title = null)
        {
            Command("MAIL FROM:<" + _UserName + ">");
            ReadLine();
            Command("RCPT TO:<" + address + ">");
            ReadLine();
            Command("DATA");
            ReadLine();
            if (title!=null)
            {
                Command("Subject:" + title);
                ReadLine();
            }
            Command(message);
            ReadLine();
            Command(".");
            ReadLine();
        }
        private void WriteToLog(string msg, params object[] args)
        {
            Console.WriteLine("{0}: {1}", DateTime.Now, String.Format(msg, args));
        }
    }
}
