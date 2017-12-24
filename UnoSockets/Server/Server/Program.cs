using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

namespace Server
{
    class Program
    {
        private static TcpListener serverSocket = default(TcpListener);
        private static Socket clientSocket = default(Socket);

        private static readonly int maxPlayers = 4; //Maximo de Jugadores
        private static readonly handleClient[] clients = new handleClient[maxPlayers]; //Se crea el objeto de clientes

        static void Main(string[] args)
        {
            serverSocket = new TcpListener(IPAddress.Any, 2000); //Se crea el Socket del Servidor
            clientSocket = default(Socket); //Se crea el Socket de Cliente
            serverSocket.Start(); //Inicia el Socket Cliente

            while(true)
            {
                Console.WriteLine("Esperando Jugadores...");
                clientSocket = serverSocket.AcceptSocket(); //Cuando se conecta un jugador
                Console.WriteLine("Jugador Conectado");
                int i = 0;
                for(i = 0; i < maxPlayers; i++) //Recorremos i, que llega hasta el maximo de jugadores
                {
                    if(clients[i] == null) //Si el cliente en la posicion i no existe
                    {
                        (clients[i] = new handleClient()).startClient(clientSocket, clients); //Se Conecta el cliente, siendo i su posicion
                        break;
                    }
                }

                if(i == maxPlayers) //Si ya se conectaron el maximo de jugadores
                {
                    StreamWriter ots = new StreamWriter(new NetworkStream(clientSocket)); //Se crea ots que escribe por el socket de clientes
                    ots.AutoFlush = true; //Flusheo automatico
                    ots.WriteLine("Ya se alcanzó el maximo de jugadores");
                    ots.Close();
                    clientSocket.Close();
                }
            }
        }
    }

    public class handleClient
    {
        private Socket clientSocket;
        private handleClient[] clients;
        private int maxPlayersCount;
        private String playerName;
        private StreamReader ins;
        private StreamWriter ots;
        private string[] mesa = new string[1];

        public void startClient(Socket inClientSocket, handleClient[] clients)
        {
            this.clientSocket = inClientSocket;
            this.clients = clients;
            this.maxPlayersCount = clients.Length;

            Thread ctThread = new Thread(doPlay);
            ctThread.Start();
        }

        private void doPlay()
        {
            int maxPlayersCount = this.maxPlayersCount;
            handleClient[] clients = this.clients;

            Uno juego = new Uno(); //Se crea instancia del objeto Uno
            Random inicio = new Random(); //Se crea un objeto Random que marca la carta inicio del juego

            try
            {
                ins = new StreamReader(new NetworkStream(clientSocket));
                ots = new StreamWriter(new NetworkStream(clientSocket));
                ots.AutoFlush = true;
                
                //Bienvenida al Usuario
                ots.WriteLine("Bienvenido a UNO");
                lock (this)
                {
                    for(int i = 0; i < maxPlayersCount; i++) //Recorremos los lugares de los jugadores
                    {
                        if(clients[i] != null && clients[i] != this) //Mientras no sea un lugar sin jugador ni uno mismo
                        {
                            clients[i].ots.WriteLine("Un nuevo usuario se conectó"); //Avisa a los demas de la llegada de un jugador
                        }
                    }
                }

                string[] misCartas = new string[35]; //Mano de cartas del jugador
                misCartas = juego.manoInicial();

                while(misCartas.Length > 0) // Mientras el jugador tenga cartas
                {
                    if(clients[maxPlayersCount-1] != null) //Cuando los lugares estén llenos
                    {
                        if (mesa[0] == null)
                        {
                            mesa[0] = juego.sacarCarta(inicio.Next(1, 37));
                            for (int i = 0; i < maxPlayersCount; i++)
                            {
                                clients[i].mesa = this.mesa;
                            }
                        }
                        ots.Write("Carta Actual: ");
                        ots.WriteLine(juego.mostrarMano(mesa)); //Muestra la carta en la mesa
                        ots.WriteLine();
                        ots.WriteLine(juego.mostrarMano(misCartas)); //Muestra la mano del jugador
                        //Comprobación de mensaje
                        string respuesta = ins.ReadLine();
                        if(respuesta.StartsWith("/quit")) //Si el mensaje inicia con /quit se sale del ciclo
                        {
                            break;
                        }
                        else
                        {
                            lock (this)
                            {
                                if(respuesta == " " || respuesta == "") //Si la respuesta es vacía, se añade una carta a la mano
                                {
                                    misCartas = juego.tomarCarta(misCartas);
                                    Console.Clear();
                                }
                                else
                                {
                                    int cartaAJugar = int.Parse(respuesta);
                                    if(juego.jugarCarta(mesa[0], misCartas[cartaAJugar])) //Si la carta corresponde con color o numero al de la mesa
                                    {
                                        mesa[0] = misCartas[cartaAJugar]; //Se pone la carta en la mesa
                                        misCartas = juego.quitarCarta(misCartas, cartaAJugar); //Se quita la carta de la mano del jugador
                                        for (int i = 0; i < maxPlayersCount; i++)
                                        {
                                            clients[i].mesa = this.mesa;
                                            clients[i].ots.WriteLine("Carta Actual: " + juego.mostrarMano(mesa)); //Envia el mensaje a todos los clientes
                                        }
                                        if (misCartas.Length == 1) //Si sólo queda una carta
                                        {
                                            for (int i = 0; i < maxPlayersCount; i++)
                                            {
                                                clients[i].ots.WriteLine("UNO!"); //Se envia el mensaje UNO a todos los clientes
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //Salida del usuario
                Console.WriteLine("Usuario desconectado");
                lock (this)
                {
                    for(int i = 0; i < maxPlayersCount; i++)
                    {
                        if(clients[i] !=null)
                        {
                            clients[i].ots.WriteLine("Un usuario se ha desconectado"); //Le dice a todos los clientes de la salida de un usuario
                        }
                    }
                }

                lock (this)
                {
                    for(int i = 0; i < maxPlayersCount; i++)
                    {
                        if (clients[i] == this) //Si la posicion del cliente es esta
                        {
                            clients[i] = null; //La libera para que alguien mas pueda conectarse en su lugar sin tener que reiniciar el servidor
                        }
                    }
                }
                //Se cierran los escritores de stream y el socket
                ins.Close();
                ots.Close();
                clientSocket.Close();
            }
            catch(Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }
    }


    public class Uno
    {
        public string[] tomarCarta(string[] manoAnterior)
        {
            Random tomar = new Random();
            string[] cartas = new string[manoAnterior.Length + 1];
            for (int x = 0; x < manoAnterior.Length; x++)
            {
                cartas[x] = manoAnterior[x];
            }
            cartas[manoAnterior.Length] = sacarCarta(tomar.Next(1, 37));
            return cartas;
        }

        public string[] quitarCarta(string[] manoAnterior, int cartaASacar)
        {
            string[] cartas = new string[manoAnterior.Length - 1];
            for (int x = 0; x < manoAnterior.Length; x++)
            {
                if (x >= cartaASacar)
                {
                    if (x > cartaASacar)
                    {
                        cartas[x - 1] = manoAnterior[x];
                    }
                }
                else
                {
                    cartas[x] = manoAnterior[x];
                }
            }
            return cartas;
        }

        public bool jugarCarta(string cartaEnJuego, string cartaJugada)
        {
            char[] cartaActual = cartaEnJuego.ToCharArray();
            char[] nuevaCarta = cartaJugada.ToCharArray();
            if (cartaActual[0] == nuevaCarta[0] || cartaActual[1] == nuevaCarta[1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string[] manoInicial()
        {
            Random mano = new Random();
            string[] cartas = new string[7];
            for (int i = 0; i < 7; i++)
            {
                cartas[i] = sacarCarta(mano.Next(1, 37));
            }
            return cartas;
        }

        public string mostrarMano(string[] cartas)
        {
            string respuesta = "";
            char[] valor = new char[2];
            for (int i = 0; i < cartas.Length; i++)
            {
                if (cartas[i] != null)
                {
                    valor = cartas[i].ToCharArray();
                    respuesta += ("[" + i + "] ");
                    switch (valor[0])
                    {
                        case 'b':
                            //Console.ForegroundColor = ConsoleColor.Blue;
                            respuesta += ("Blue " + valor[1] + "\r\n");
                            break;

                        case 'g':
                            //Console.ForegroundColor = ConsoleColor.Green;
                            respuesta += ("Green " + valor[1] + "\r\n");
                            break;

                        case 'r':
                            //Console.ForegroundColor = ConsoleColor.Red;
                            respuesta += ("Red " + valor[1] + "\r\n");
                            break;

                        case 'y':
                            //Console.ForegroundColor = ConsoleColor.Yellow;
                            respuesta += ("Yellow " + valor[1] + "\r\n");
                            break;
                    }
                    //Console.ResetColor();
                }
            }
            return respuesta;
        }

        public string sacarCarta(int sacar)
        {
            string carta;
            switch (sacar)
            {
                case 1:
                    carta = "b1";
                    break;

                case 2:
                    carta = "g1";
                    break;

                case 3:
                    carta = "r1";
                    break;

                case 4:
                    carta = "y1";
                    break;

                case 5:
                    carta = "b2";
                    break;

                case 6:
                    carta = "g2";
                    break;

                case 7:
                    carta = "r2";
                    break;

                case 8:
                    carta = "y2";
                    break;

                case 9:
                    carta = "b3";
                    break;

                case 10:
                    carta = "g3";
                    break;

                case 11:
                    carta = "r3";
                    break;

                case 12:
                    carta = "y3";
                    break;

                case 13:
                    carta = "b4";
                    break;

                case 14:
                    carta = "g4";
                    break;

                case 15:
                    carta = "r4";
                    break;

                case 16:
                    carta = "y4";
                    break;

                case 17:
                    carta = "b5";
                    break;

                case 18:
                    carta = "g5";
                    break;

                case 19:
                    carta = "r5";
                    break;

                case 20:
                    carta = "y5";
                    break;

                case 21:
                    carta = "b6";
                    break;

                case 22:
                    carta = "g6";
                    break;

                case 23:
                    carta = "r6";
                    break;

                case 24:
                    carta = "y6";
                    break;

                case 25:
                    carta = "b7";
                    break;

                case 26:
                    carta = "g7";
                    break;

                case 27:
                    carta = "r7";
                    break;

                case 28:
                    carta = "y7";
                    break;

                case 29:
                    carta = "b8";
                    break;

                case 30:
                    carta = "g8";
                    break;

                case 31:
                    carta = "r8";
                    break;

                case 32:
                    carta = "y8";
                    break;

                case 33:
                    carta = "b9";
                    break;

                case 34:
                    carta = "g9";
                    break;

                case 35:
                    carta = "r9";
                    break;

                case 36:
                    carta = "y9";
                    break;

                default:
                    carta = "w2";
                    break;
            }
            return carta;
        }
    }
}
