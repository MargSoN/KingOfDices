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
        public Form1() {
            CheckForIllegalCrossThreadCalls=false;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            Thread connessioni = new Thread(new ThreadStart(connessioniClient));
            connessioni.Start();
            button1.Enabled = false; // Altrimenti puoi continuare a creare server
        }

        private void connessioniClient() {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            Socket socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endpoint = new IPEndPoint(ip, 9999);
            try {
                socket.Bind(endpoint);
                socket.Listen(10);
                //MessageBox.Show("server avviato con successo");

                while (true) {
                    Socket client = socket.Accept();
                    //MessageBox.Show("client connesso con sucesso");
                    Thread ThreadMulticlient = new Thread(() =>
                    {
                        /*i=*/
                        ClientThread(client);
                    });
                    ThreadMulticlient.Start();
                }
            } catch (Exception errore) {
                MessageBox.Show(errore.Message);
            }
        }

        private void ClientThread(Socket client)
        {
            //PRIMO TIRO DI DADI
            int RollDice = 0;
            //Random rn = new Random(Guid.NewGuid().GetHashCode()); // La funzione Guid.NewGuid().GetHashCode() passata all'interno del costrutture della classe Random, serve per generare un seed random
            Random rn = new Random();
            RollDice = rn.Next(1, 7);
            label1.Text = "numero dado 1=" + RollDice.ToString();
            byte[] BRollDice = BitConverter.GetBytes(RollDice);
            client.Send(BRollDice);

            //SECONDO TIRO DI DADI
            int RollDice2 = 0;
            RollDice2 = rn.Next(1, 7);
            label2.Text = "numero dado 2=" + RollDice2.ToString();
            byte[] BRollDice2 = BitConverter.GetBytes(RollDice2);
            client.Send(BRollDice2);

            //SOMMA DEI DUE DADI
            byte[] BAddrolls = BitConverter.GetBytes(RollDice + RollDice2);
            client.Send(BAddrolls);

            client.Shutdown(SocketShutdown.Both);
            client.Close();
            /*return RollDice;
            return RollDice2;
            return addrolls;*/
        }
    }
}
