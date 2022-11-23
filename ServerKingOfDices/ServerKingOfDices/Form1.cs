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

namespace ServerKingOfDices
{
    public partial class Form1 : Form
    {
        public Form1()
        {
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
                socket.Listen(10);
                while (true)
                {
                    Socket num = socket.Accept();
                    MessageBox.Show("client connesso con sucesso");
                    int RollDice = 0;
                    Random rn = new Random();
                    RollDice = rn.Next(1, 7);
                    //MessageBox.Show("numero" + RollDice);
                    //listView1.Items.Add(num.LocalEndPoint.ToString() + " " + NumCas.ToString());
                    byte[] BRollDice = BitConverter.GetBytes(RollDice);
                    num.Send(BRollDice);
                    int RollDice2 = 0;
                    Random rn2 = new Random();
                    RollDice2 = rn2.Next(1, 7);
                    //MessageBox.Show("numero" + RollDice2);
                    byte[] BRollDice2 = BitConverter.GetBytes(RollDice2);
                    num.Send(BRollDice2);
                    num.Shutdown(SocketShutdown.Both);
                    num.Close();
                }
            }

            catch (Exception errore)
            {
                MessageBox.Show(errore.Message);
            }
        }
    }
}
