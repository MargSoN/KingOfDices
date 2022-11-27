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

namespace ServerKingOfDices
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            CheckForIllegalCrossThreadCalls=false;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
  
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            Socket socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endpoint = new IPEndPoint(ip, 9999);
            try
            {
                socket.Bind(endpoint);
                socket.Listen(10);
                MessageBox.Show("server avviato correttamente");
                Thread Accept = new Thread(() =>
                {
                    ClientAccept(socket);
                });
                Accept.Start();
            }

            catch (Exception errore)
            {
                MessageBox.Show(errore.Message);
            }
        }

        private Dictionary<Socket,List<int>> ClientThread(Socket client)
        {
            try
            {
                Random seed = new Random(); //creo un seed casuale
                //PRIMO TIRO DI DADI
                Dictionary<Socket, List<int>> ValoriClient = new Dictionary<Socket, List<int>>();
                List<int> ValoreDadi = new List<int>();
                int RollDice = 0;
                int addrolls = 0;
                Random rn = new Random(seed.Next());
                RollDice = rn.Next(1, 7);
                ValoreDadi.Add(RollDice);
                addrolls = RollDice;
                label1.Text = "numero dado 1=" + RollDice.ToString();
                //listView1.Items.Add(num.LocalEndPoint.ToString() + " " + NumCas.ToString());
                byte[] BRollDice = BitConverter.GetBytes(RollDice);
                client.Send(BRollDice);

                //SECONDO TIRO DI DADI
                RollDice = rn.Next(1,7);
                ValoreDadi.Add(RollDice);
                addrolls += RollDice;
                ValoreDadi.Add(addrolls);
                label2.Text = "numero dado 2=" + RollDice.ToString();
                BRollDice = BitConverter.GetBytes(RollDice);
                client.Send(BRollDice);
                ValoriClient.Add(client, ValoreDadi);

                //INVIO SOMMA
                byte[] BAddrolls = BitConverter.GetBytes(addrolls);
                client.Send(BAddrolls);
                //client.Shutdown(SocketShutdown.Both);
                return ValoriClient;
            }
            
            catch (Exception errore)
            {
                MessageBox.Show(errore.Message);
            }

            return null;
        }

        private void ClientAccept (Socket socket)
        {
            int N_client = 0;
            Dictionary<Socket,List<int>> ValoriClient = new Dictionary<Socket, List<int>>();
            while (true)
            {
                if (N_client < 2)
                {
                    Socket client = socket.Accept();
                    if (client.Connected == true)
                    {
                        N_client++;
                        ValoriClient.Add(client, null);
                    }
                    Thread ThreadMulticlient = new Thread(() => {
                        if (N_client==2)
                        {
                            Dictionary<Socket, List<int>> MapTemp;
                            foreach (var ValoriLetti in ValoriClient)
                            {
                                if (ValoriLetti.Key.Equals(ValoriClient.ElementAt(0).Key))
                                {
                                    MapTemp = ClientThread(ValoriClient.ElementAt(1).Key);
                                }
                                else
                                {
                                    MapTemp = ClientThread(ValoriClient.ElementAt(0).Key);
                                }
                                ValoriClient.Append(ValoriLetti.Key, MapTemp.ElementAt(0).Value);
                                ValoriClient.
                            }

                        }

                        /*byte[] buffer = new byte[1024];
                        client.Receive(buffer);
                        Dictionary<Socket, List<int>> MapTemp = ClientThread(client);
                        foreach (var ValoriLetti in MapTemp)
                        {
                            ValoriClient.Add(ValoriLetti.Key, ValoriLetti.Value);
                        }
                        if (N_client==2)
                        {
                            N_client = 0;
                            List<int> ListaSwap = new List<int>();
                            ListaSwap = ValoriClient.ElementAt(0).Value;
                            ValoriClient.Add(ValoriClient.ElementAt(0).Key, ValoriClient.ElementAt(1).Value);
                            ValoriClient.Add(ValoriClient.ElementAt(1).Key, ListaSwap);
                            foreach (var ValoriLetti in ValoriClient)
                            {
                                MessageBox.Show(ValoriLetti.Key.RemoteEndPoint.ToString());
                                foreach (var ValoriLista in ValoriLetti.Value)
                                {
                                    MessageBox.Show(ValoriLista.ToString());
                                }
                            }
                        }
                    */
                    });
                    ThreadMulticlient.Start();
                }
            }
        }
    }
}
