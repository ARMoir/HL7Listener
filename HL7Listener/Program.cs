using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;



namespace HL7Listener
{
    class Program

    {
        static void Main(string[] args)

        {

            try

            {

                var PortSTR = Properties.Settings.Default.Port;

                var Directory = Properties.Settings.Default.Directory;

                string LocalIP;

                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))

                {

                    socket.Connect("8.8.8.8", 65530);

                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;

                    LocalIP = endPoint.Address.ToString();

                }

                Console.WriteLine("LISTENING ON "+ LocalIP + ":" + PortSTR);

                Console.WriteLine("WRITING TO " + Directory);

                Console.WriteLine("SYSTEM READY");

                var Count = 0;

                int Port = int.Parse(PortSTR);

                var Listener = new TcpListener(IPAddress.Any, Port);

                Listener.Start();

                var Client = Listener.AcceptTcpClient();

                byte[] Bytes = new byte[4097];

                string Recived = "";

                do

                {

                    NetworkStream Stream = Client.GetStream();

                    byte[] Buffer = new byte[Client.ReceiveBufferSize + 1];

                    StringBuilder Message = new StringBuilder();

                    int Read = 0;

                    do

                    {

                        Read = Stream.Read(Buffer, 0, Buffer.Length);

                        Message.AppendFormat("{0}", Encoding.ASCII.GetString(Buffer, 0, Read));

                    }

                    while (Stream.DataAvailable);

                    Recived = Message.ToString();

                    if (Recived.Length > 10)

                    {       

                        char[] Pipe = new char[] { '|' };

                        string[] Fields = Recived.Split(Pipe);

                        string MSH10 = Fields[9];

                        string MSH9 = Fields[8];

                        string ACKMSH = null;

                        for (int i = 0; i < 16; i++)

                        {

                            ACKMSH = ACKMSH + Fields[i] + "|";

                        }

                        ACKMSH = ACKMSH.Replace(MSH9, "ACK");

                        string ACK = ACKMSH + (Char)13 + "MSA|AA|" + MSH10 + "|";

                        ACK = (Char)11 + ACK + (Char)28 + (Char)13;

                        byte[] ACK_Bytes = System.Text.Encoding.ASCII.GetBytes(ACK);

                        Stream.Write(ACK_Bytes, 0, ACK_Bytes.Length);

                        ACK_Bytes = new byte[257];

                        Count++;

                        Console.WriteLine("");

                        Console.WriteLine("MESSAGE: " + Count + " CONTROL ID: " + MSH10);

                        Console.WriteLine("");

                        Console.WriteLine("BYTES");

                        Console.WriteLine("");

                        Console.WriteLine(Read);

                        Console.WriteLine("");

                        string HL7Clean = Regex.Replace(Recived, @"[^\u0020-\u007E]", string.Empty);

                        Console.WriteLine(HL7Clean);

                        Console.WriteLine("");

                        Console.WriteLine("ACK " + Count + " CONTROL ID: " + MSH10);

                        string ACKClean = Regex.Replace(ACK, @"[^\u0020-\u007E]", string.Empty);

                        Console.WriteLine("");

                        Console.WriteLine(ACKClean);

                        Console.WriteLine("");

                        if (!string.IsNullOrEmpty(Directory))

                        {

                            try

                            {

                                System.IO.File.WriteAllText(Directory + MSH10 + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".txt", Recived.ToString());

                            }

                            catch (Exception ex)

                            {

                                Console.WriteLine(ex.Message.ToString());

                            }

                        }

                    }

                    else

                    {

                        Console.WriteLine("HOST DISCONECTED");

                        Listener.Stop();

                        Main(args);

                    }

                }

                while (true);

            }

            catch (Exception ex)

            {

                Console.WriteLine(ex.Message.ToString());

            }

        }

    }

}
