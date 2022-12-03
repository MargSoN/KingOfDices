using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace ServerKingOfDices
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            CheckForIllegalCrossThreadCalls=false;
            InitializeComponent();
        }

        private IPAddress getIpAddress()
        {
            List<IPAddress> allIP = new List<IPAddress>(Dns.GetHostAddresses(Dns.GetHostName()));
            //dns.gethostaddress dato il nome della macchina mi restutuisce gli indirizzi attivi, e per trovare l'host faccio dns.gethostname
            IPAddress ip = IPAddress.Parse("127.0.0.1");//indizzo default locale

            foreach (var inf in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())//itero le schede di rete
            {
                if (inf.Name.Equals("Ethernet") || inf.Name.Equals("Wi-Fi"))//mi controlla che la scheda di rete sia ethernet o wifi
                {
                    foreach (UnicastIPAddressInformation ip_interface in inf.GetIPProperties().UnicastAddresses)//mi itera gli indirizzi unicast dell'itnterfaccia
                        //unicast uso per la connessione
                    {
                        // Controllare che sia IPv4 e che sia nella lista delle interfacce attive
                        if (ip_interface.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && allIP.Contains(ip_interface.Address))
                        {
                            ip = ip_interface.Address;//metto in IP l'indirizzo trovato
                            return ip;//do la priorita' a ethernet
                        }
                    }
                }
            }
            return ip;
        }

            private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                IPAddress ip = getIpAddress();
                Socket server = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endpoint = new IPEndPoint(ip, 9999);
                server.Bind(endpoint);//mi associa l'indirizzo IP alla porta
                server.Listen(10);

                Thread Accept = new Thread(() =>
                {
                    ClientAccept(server);
                    button1.Enabled = true;
                });
                Accept.Start();

                button1.Enabled = false;
            }
            catch (Exception errore)
            {
                MessageBox.Show(errore.Message);
            }
        }

        /**
         * I thread.Sleep servono per rallentare l'invio sul canale socket altrimenti il tempo per raggiungere 
         * l'host remoto non è sufficiente e si perderebbero i pacchetti. In caso non arrivano tutti i numeri ai vari client,
         * aumentare il thread sleep in modo da lasciare il tempo di far arrivare tutti i pacchetti
         */
        private int ClientThread(Socket client)
        {
            int addrolls = 0;
            try
            {
                int RollDice;
                byte[] buffer;
                Random rn = new Random(Guid.NewGuid().GetHashCode());//seed

                RollDice = rn.Next(1, 7);
                addrolls = RollDice;
                buffer = BitConverter.GetBytes(RollDice);
                client.Send(buffer);
                Thread.Sleep(200);

                RollDice = rn.Next(1, 7);
                buffer = BitConverter.GetBytes(RollDice);
                client.Send(buffer);
                Thread.Sleep(200);

                addrolls += RollDice;
                buffer = BitConverter.GetBytes(addrolls);
                client.Send(buffer);
                Thread.Sleep(200);
            }
            catch (Exception errore)
            {
                MessageBox.Show(errore.Message);
            }
            return addrolls;
        }

        private void ClientAccept (Socket server)
        {
            int N_client = 0;
            Dictionary<Socket,int> ValoriClient = new Dictionary<Socket, int>();
            while (true)
            {
                if (N_client < 2)
                {
                    try
                    {
                        Socket client = server.Accept();

                        if (client.Connected == true)
                        {
                            N_client++;
                            ValoriClient.Add(client, 0);
                        }

                        Thread ThreadMulticlient = new Thread(() =>//lambda function funzione anonima
                        {
                            if (N_client == 2) //Entrerà soltanto il secondo client
                            {
                                int somma_dadi = ClientThread(ValoriClient.ElementAt(0).Key);
                                if (somma_dadi == 0)
                                    throw new NullReferenceException("Non sono stati lanciati i dati a seguito di una eccezione");
                                ValoriClient[ValoriClient.ElementAt(1).Key] = somma_dadi;

                                somma_dadi = ClientThread(ValoriClient.ElementAt(1).Key);
                                if (somma_dadi == 0)
                                    throw new NullReferenceException("Non sono stati lanciati i dati a seguito di una eccezione");
                                ValoriClient[ValoriClient.ElementAt(0).Key] = somma_dadi;
                            }
                        });
                        ThreadMulticlient.Start();
                        ThreadMulticlient.Join();//aspetta che i client ricevano tutti i dati
                    }
                    catch (Exception exp)
                    {
                        MessageBox.Show(exp.Message);
                    }
                } else //se n client e'2
                {
                    try
                    {
                        Thread player1 = new Thread(() =>
                        {
                            int counter = 0, ris = 0;
                            bool stop = false;
                            while (counter < 3 && !stop)//se arrivo a 3 tiri o se faccio fine gioco
                            {
                                ris = gioca_turno(ValoriClient.ElementAt(0).Key,ValoriClient.ElementAt(1).Key);//clicco player 1 cambio player 2
                                if (ris != 0)//se non e' ancora finita
                                    ValoriClient[ValoriClient.ElementAt(1).Key] = ris;//modofico il risultato del player 2
                                else
                                    stop = true;//mi ferma tutto e mi finisce la partita di player 1
                                counter++;
                            }
                        });
                        player1.Start();

                        Thread player2 = new Thread(() =>
                        {
                            int counter = 0, ris = 0;
                            bool stop = false;
                            while (counter < 3 && !stop)
                            {
                                ris = gioca_turno(ValoriClient.ElementAt(1).Key,ValoriClient.ElementAt(0).Key);
                                if (ris != 0)
                                    ValoriClient[ValoriClient.ElementAt(0).Key] = ris;
                                else
                                    stop = true;
                                counter++;
                            }
                        });
                        player2.Start();

                        player1.Join();
                        player2.Join();

                        switch (who_wins(ValoriClient))
                        {
                            case 0:
                                ValoriClient.ElementAt(0).Key.Send(BitConverter.GetBytes(0));
                                ValoriClient.ElementAt(1).Key.Send(BitConverter.GetBytes(0));
                                break;
                            case -1:
                                ValoriClient.ElementAt(0).Key.Send(BitConverter.GetBytes(-2));
                                ValoriClient.ElementAt(1).Key.Send(BitConverter.GetBytes(-1));
                                break;
                            case -2:
                                ValoriClient.ElementAt(0).Key.Send(BitConverter.GetBytes(-1));
                                ValoriClient.ElementAt(1).Key.Send(BitConverter.GetBytes(-2));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("Valore non presente nel range");
                        }

                        ValoriClient.ElementAt(0).Key.Shutdown(SocketShutdown.Both);
                        ValoriClient.ElementAt(0).Key.Close();
                        ValoriClient.ElementAt(1).Key.Shutdown(SocketShutdown.Both);
                        ValoriClient.ElementAt(1).Key.Close();
                    } catch(Exception exp)
                    {
                        MessageBox.Show(exp.Message);
                    }                        

                    N_client = 0;
                    ValoriClient.Clear();
                }
            }
        }

        private int gioca_turno(Socket player1, Socket player2)
        {
            byte[] buffer = new byte[1024];
            int risultato = 0;
            try
            {
                player1.Receive(buffer);//salvo in buffer cosa mi sa player1 (un valore booleano)
                if (BitConverter.ToBoolean(buffer, 0))
                {
                    int somma_dadi = ClientThread(player2);//modifico le lable del player 2 (i miei dadi)
                    if (somma_dadi == 0)
                        throw new NullReferenceException("Non sono stati lanciati i dati a seguito di una eccezione");
                    risultato = somma_dadi;
                }
            }
            catch(Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
            return risultato;
        }

        private int who_wins(Dictionary<Socket, int> mappa)
        {
            if (mappa.ElementAt(0).Value > mappa.ElementAt(1).Value)
            {
                //MessageBox.Show("Ha vinto il giocatore: " + mappa.ElementAt(1).Key.RemoteEndPoint + " totalizzando " + mappa.ElementAt(0).Value + " punti contro i " + mappa.ElementAt(1).Value + " punti");
                return -1;

            } 
            
            if (mappa.ElementAt(0).Value < mappa.ElementAt(1).Value)
            {
                //MessageBox.Show("Ha vinto il giocatore: " + mappa.ElementAt(0).Key.RemoteEndPoint + " totalizzando " + mappa.ElementAt(1).Value + " punti contro i " + mappa.ElementAt(0).Value + " punti");
                return -2;
            }

            //MessageBox.Show("La partita è finita in pareggio con un punteggio di " + mappa.ElementAt(0).Value + " punti");
            return 0;
        }
    }
}
