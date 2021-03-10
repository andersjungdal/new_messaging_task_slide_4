using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RabbitMQ.Client;

namespace Web_App
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var factory = new ConnectionFactory() { Uri = new Uri("amqp://admin:iamadmin@localhost:5672") };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "topic_logs",
                    type: "topic");
                string invalidMessageKey = "deadletter.deadletter";
                string createRoutingKey = "";

                if (string.IsNullOrEmpty(textBox_Name.Text) || 
                    string.IsNullOrEmpty(textBox_Email.Text) || 
                    !textBox_Email.Text.Contains('@') ||
                    comboBox1.SelectedItem == null ||
                    string.IsNullOrEmpty(comboBox1.Text))
                {
                    createRoutingKey = invalidMessageKey;
                }
                else if (Btn_Book.Checked)
                {
                    createRoutingKey = "tour.booked";
                    BackOfficeQueue(createRoutingKey, $"{textBox_Name.Text}, {textBox_Email.Text}");
                }
                else if (Btn_Cancel.Checked)
                {
                    createRoutingKey = "tour.cancelled";
                    BackOfficeQueue(createRoutingKey, $"{textBox_Name.Text}, {textBox_Email.Text}");
                }
                else
                {
                    createRoutingKey = invalidMessageKey;
                }

                if (!string.IsNullOrEmpty(createRoutingKey))
                {

                    var routingKey = (createRoutingKey.Length > 0) ? createRoutingKey : "anonymous.info";
                    string[] tourStatus = createRoutingKey.Split('.');

                    var message = (createRoutingKey.Equals("deadletter.deadletter")) 
                        ? "invalid message" : $"{textBox_Name.Text}, {textBox_Email.Text} {tourStatus[1]} a tour to {comboBox1.SelectedItem}";

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "topic_logs",
                        routingKey: routingKey,
                        basicProperties: null,
                        body: body);
                    listView1.Items.Add(message + " " + routingKey);
                }
            }
        }

        private void BackOfficeQueue(string routingKey, string message)
        {
            var factory = new ConnectionFactory() { Uri = new Uri("amqp://admin:iamadmin@localhost:5672") };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //var routingKey = (args.Length > 0) ? args[0] : "anonymous.info";
                //var message = (args.Length > 1)
                //    ? string.Join(" ", args.Skip(1).ToArray())
                //    : "Hello World!";

                channel.QueueDeclare("Back_Office", true, false, false, null);

                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "",
                    routingKey: "Back_Office",
                    basicProperties: null,
                    body: body);
                Console.WriteLine(" [x] Sent '{0}':'{1}'", routingKey, message);
            }
        }
    }
}
