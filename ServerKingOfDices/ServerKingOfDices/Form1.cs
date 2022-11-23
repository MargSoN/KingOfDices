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
                MessageBox.Show("server avviato con successo");
                int i = 0;
                socket.Listen(10);
                while (true)
                {
                    Socket num = socket.Accept();
                    MessageBox.Show("client connesso con sucesso");
                    Thread ThreadMulticlient = new Thread(() => {
                       // doClient(socket);
                        /*i=*/ClientThread(num);
                    });
                    ThreadMulticlient.Start();
                    ThreadMulticlient.Join();
                    num.Close();
                }
            }

            catch (Exception errore)
            {
                MessageBox.Show(errore.Message);
            }
        }

        private void ClientThread(Socket num)
        {
            //PRIMO TIRO DI DADI
            int RollDice = 0;
            Random rn = new Random();
            RollDice = rn.Next(1, 7);
            label1.Text = "numero dado 1=" + RollDice.ToString();
            //MessageBox.Show("numero" + RollDice); //Messagebox che se tolgo non va
                                                  //listView1.Items.Add(num.LocalEndPoint.ToString() + " " + NumCas.ToString());
            byte[] BRollDice = BitConverter.GetBytes(RollDice);
            num.Send(BRollDice);
            Thread.Sleep(500);

            //SECONDO TIRO DI DADI
            int RollDice2 = 0;
            Random rn2 = new Random();
            RollDice2 = rn2.Next(1, 7);
            label2.Text = "numero dado 2=" + RollDice2.ToString();
            //MessageBox.Show("numero" + RollDice2); //Messagebox che se tolgo non va
            byte[] BRollDice2 = BitConverter.GetBytes(RollDice2);
            num.Send(BRollDice2);
            Thread.Sleep(500);

            //SOMMA DEI DUE DADI
            int addrolls = 0;
            addrolls = RollDice + RollDice2;
            byte[] BAddrolls = BitConverter.GetBytes(addrolls);
            num.Send(BAddrolls);
            num.Shutdown(SocketShutdown.Both);
            /*return RollDice;
            return RollDice2;
            return addrolls;*/
        }

        /*public void doClient(Socket num)
        {
            byte[] bytes = new Byte[1024];
            String data = "";
            while (data != "Quit$")
            {
                // An incoming connection needs to be processed.  
                data = "";
                while (data.IndexOf("$") == -1)
                {
                    int bytesRec = num.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                }

                // Show the data on the console.  
                Console.WriteLine("Messaggio ricevuto : {0}", data);

                // Echo the data back to the client.  
                byte[] msg = Encoding.ASCII.GetBytes(data);

                num.Send(msg);
            }
            num.Shutdown(SocketShutdown.Both);
            num.Close();
            data = "";

        }*/
    }
}
