using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMTPSender
{
    public partial class Form1 : Form
    {
        Client client;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            client = new Client("smtp.poczta.onet.pl", 25, textBox1.Text, textBox2.Text);
            if (client.IsError == false)
            {
                Height = 441;
            }
            else
            {
                MessageBox.Show("There is no answer from server, try again");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox4.Text == "" )
            client.SendTo(textBox3.Text, textBox5.Text);
            else client.SendTo(textBox3.Text, textBox5.Text, textBox4.Text);


        }

        private void button3_Click(object sender, EventArgs e)
        {
            client.Close();
            Height = 163;
        }
    }
}
