using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace ClientKingOfDices
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
            IPEndPoint EP = new IPEndPoint(ip, 9999);
            Socket socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(EP);
                MessageBox.Show("connessione al server eseguita con successo");
                
                //RICEVIMENTO PRIMO DADO
                byte[] BRecive = new byte[1024];
                socket.Receive(BRecive);
                int messaggio = BitConverter.ToInt32(BRecive, 0);
                label1.Text = messaggio.ToString();

                //RICEVIMENTO SECONDO DADO
                byte[] BRecive2 = new byte[1024];
                socket.Receive(BRecive2);
                int messaggio2 = BitConverter.ToInt32(BRecive2, 0);
                label2.Text = messaggio2.ToString();
                //MessageBox.Show(messaggio.ToString());
                //MessageBox.Show(messaggio2.ToString());

                //RICEVIMENTO DELLA SOMMA
                byte[] BReciveSum = new byte[1024];
                socket.Receive(BReciveSum);
                int messaggioSum = BitConverter.ToInt32(BReciveSum, 0);
                label3.Text = messaggioSum.ToString();
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception errore)
            {
                MessageBox.Show(errore.Message);
            }
        }
    }
}
