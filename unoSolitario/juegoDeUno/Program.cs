using System;
using System.Threading;

namespace juegoDeUno
{
    class Program
    {
        static void Main(string[] args)
        {
            Uno juego = new Uno();
            Random inicio = new Random();
            string[] mesa = new string[1]; //Carta en la mesa
            mesa[0] = juego.sacarCarta(inicio.Next(1, 37)); //Carta al azar

            string[] misCartas = new string[35]; //Mano de Cartas del Jugador
            misCartas = juego.manoInicial();

            try
            {
                Console.Clear();
                while (misCartas.Length > 0) //Mientras el jugador tenga cartas
                {
                    Console.Write("Carta Actual: ");
                    juego.mostrarMano(mesa); //Muestra la carta en la mesa
                    Console.WriteLine();
                    juego.mostrarMano(misCartas); //Muestra la mano del jugador
                    try
                    {
                        string respuesta = Console.ReadLine();
                        if (respuesta == " " || respuesta == "") //Si la respuesta está vacía
                        {
                            bool hayCartasJugables = false;
                            for (int x = 0; x < misCartas.Length; x++) //Se recorren todas las cartas de la mano
                            {
                                if(juego.jugarCarta(mesa[0], misCartas[x])) //Si alguna de las cartas es jugable
                                {
                                    hayCartasJugables = true;
                                }
                            }
                            if(!hayCartasJugables) //Cuando no hay cartas jugables
                            {
                                misCartas = juego.tomarCarta(misCartas); //Se toma una carta
                                Console.Clear();
                            }
                            else 
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine("No se permite comer"); //Si hay alguna carta jugable, aparece el mensaje que no permite tomar mas cartas
                                Thread.Sleep(500);
                                Console.ResetColor();
                                Console.Clear();
                            }
                        }
                        else //Si la respuesta no está vacia se saca la carta en la posicion que el usuario escogió
                        {
                            int cartaAJugar = int.Parse(respuesta);
                            if (juego.jugarCarta(mesa[0], misCartas[cartaAJugar])) //Si la carta se puede jugar, es decir, tiene el numero o color igual al de la mesa
                            {
                                mesa[0] = misCartas[cartaAJugar]; //Esta carta se vuelve la nueva carta de la mesa
                                misCartas = juego.quitarCarta(misCartas, cartaAJugar); //Se quita la carta de la mano del jugador
                                Console.Clear();
                                if (misCartas.Length == 1) //Si tiene una sola carta en la mano
                                {
                                    Console.WriteLine("UNO!");
                                }
                            }
                            else //Si la carta no comparte color ni numero con la de la mesa
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine("Carta Invalida"); //Aparece mensaje de carta invalida
                                Thread.Sleep(500);
                                Console.ResetColor();
                                Console.Clear();
                            }
                        }
                    }
                    catch
                    {
                        Console.Clear();
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine("Escribe un numero que coincida con la carta que quieras jugar.\nSi no dispones de cartas para jugar,\nroba una carta presionando enter sin escribir nada");
                        Console.ResetColor();
                        Console.ReadKey();
                        Console.Clear();
                    }
                }
                Console.WriteLine("¡FELICIDADES, HAS GANADO!");
                Console.ReadKey();
                Console.Clear();
            }
            catch
            {
                Console.WriteLine("Ocurrió un error inesperado... por favor reinicie el juego");
                Console.ReadKey();
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
            string[] cartas = new string[manoAnterior.Length-1];
            for (int x = 0; x < manoAnterior.Length; x++)
            {
                if(x >= cartaASacar)
                {
                    if(x > cartaASacar)
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
            if(cartaActual[0] == nuevaCarta[0] || cartaActual[1] == nuevaCarta[1])
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
            for(int x = 0; x < 7; x++)
            {
                cartas[x] = sacarCarta(mano.Next(1, 37));
            }
            return cartas;
        }

        public void mostrarMano(string[] cartas)
        {
            char[] valor = new char[2];
            for(int x = 0; x < cartas.Length; x++)
            {
                if (cartas[x] != null)
                {
                    valor = cartas[x].ToCharArray();
                    Console.Write("[" + x + "] ");
                    switch (valor[0])
                    {
                        case 'b':
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("Blue " + valor[1]);
                            break;

                        case 'g':
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Green " + valor[1]);
                            break;

                        case 'r':
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Red " + valor[1]);
                            break;

                        case 'y':
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Yellow " + valor[1]);
                            break;
                    }
                    Console.ResetColor();
                }
            }
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
