using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class ControlPanel : Form
    {
        Server server;

        public ControlPanel()
        {
            InitializeComponent();
            server = new Server(WriteLog);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if(server.status == Status.Working)
            {
                (sender as Button).Enabled = false;
                (sender as Button).Text = "Остановка...";
                server.Stop().ContinueWith((Task t) =>
                {
                    Invoke(new Action(() =>
                    {
                        (sender as Button).Enabled = true;
                        (sender as Button).Text = "Пуск";
                    }));
                });
            }
            else if(server.status == Status.Waiting)
            {
                (sender as Button).Text = "Стоп";
                server.Start();
            }
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            log.Items.Clear();
        }

        private void WriteLog(string msg)
        {
            Invoke(new Action(() =>
            {
                log.Items.Add(msg);
                log.SelectedIndex = log.Items.Count - 1;
                log.SelectedIndex = -1;
            }));
        }
    }
}
