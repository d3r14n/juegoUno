using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    class Program
    {
        private static TcpClient client;
        private static StreamReader ins;
        private static StreamWriter ots;

        static void Main(string[] args)
        {
            try
            {
                client = new TcpClient("127.0.0.1", 2000);
                ins = new StreamReader(client.GetStream());
                ots = new StreamWriter(client.GetStream());
                ots.AutoFlush = true;
            }
            catch (Exception error)
            {
                Console.WriteLine(error.ToString());
            }

            if (client != null && ots != null && ins != null) //Si Existe la conexión con el servidor
            {
                try
                {
                    cThread cli = new cThread(client, ins, ots); //Se crea un objeto para el cliente en el que se envia la informacion de conexión, asi como el escritor y lector
                    Thread ctThread = new Thread(cli.run); //Se crea un hilo para el objeto del cliente
                    ctThread.Start(); //Se inicia el hilo

                    while(!cli.closed) //Mientras el hilo no se haya cerrado
                    {
                        string msg = Console.ReadLine().Trim();
                        ots.WriteLine(msg); //Se envia un mensaje
                    }
                    //Se cierran el escritor, lector y conexion
                    ots.Close();
                    ins.Close();
                    client.Close();
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.ToString());
                }
            }
        }
    }

    class cThread
    {
        public bool closed = false;
        private TcpClient client;
        private StreamReader ins;
        private StreamWriter ots;

        public cThread(TcpClient client, StreamReader ins, StreamWriter ots)
        {
            this.client = client;
            this.ins = ins;
            this.ots = ots;
        }

        public void run()
        {
            string responseLine;
            try
            {
                while((responseLine = ins.ReadLine()) != null) //Mientras la respuesta no sea nula
                {
                    Console.WriteLine(responseLine); //Mostrar la respuesta
                }
                closed = true;
            }
            catch (Exception error)
            {
                Console.WriteLine(error.ToString());
            }
            Environment.Exit(0);
        }
    }
}
