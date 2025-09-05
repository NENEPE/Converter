using Currency;
using System.Net.Sockets;
using System.Xml.Serialization;

namespace ClientSide
{
    public partial class Form1 : Form
    {
        public SynchronizationContext ui;
        UdpClient client;
        XmlSerializer serializerClient;
        XmlSerializer serializerServer;
        CurrencyClient cl;
        public Form1()
        {
            InitializeComponent();
            ui = SynchronizationContext.Current;
            serializerClient = new XmlSerializer(typeof(CurrencyClient));
            serializerServer = new XmlSerializer(typeof(CurrencyServer));
            cl = new CurrencyClient();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                try
                {
                    client = new UdpClient(textBox1.Text, 2077);

                    using (MemoryStream stream = new MemoryStream())
                    {
                        cl.user = Environment.UserDomainName + @"\" + Environment.UserName + " connected";
                        serializerClient.Serialize(stream, cl);
                        byte[] arr = stream.ToArray();
                        await client.SendAsync(arr, arr.Length);
                    }

                    ui.Send(u =>
                    {
                        button2.Enabled = true;
                        button1.Enabled = false;
                        comboBox1.Enabled = true;
                        comboBox2.Enabled = true;
                        button3.Enabled = true;

                        comboBox1.DataSource = Enum.GetValues(typeof(CurrencyName));
                        comboBox2.DataSource = Enum.GetValues(typeof(CurrencyName));
                    }, null);
                }
                catch (Exception ex)
                {
                    ui.Send(u => MessageBox.Show("[CLIENT ERROR] " + ex.Message), null);
                }
            });
        }
        private async void button2_Click(object sender, EventArgs e)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                cl.user = Environment.UserDomainName + @"\" + Environment.UserName + " left";
                serializerClient.Serialize(stream, cl);
                byte[] arr = stream.ToArray();
                await client.SendAsync(arr, arr.Length);
            }

            client?.Close();

            ui.Send(u =>
            {
                button2.Enabled = false;
                button1.Enabled = true;
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
                button3.Enabled = false;
            }, null);
        }
    }
}