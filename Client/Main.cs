using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void usernameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                loginButton_Click(loginButton, new EventArgs());
            else
                e.Handled = !(char.IsLetterOrDigit(e.KeyChar) || e.KeyChar == (char)Keys.Back || e.KeyChar == (char)Keys.Delete);
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            Client client = new Client(usernameTextBox.Text);
            if (!client.Connect())
            {
                MessageBox.Show("Данное имя пользователя уже занято");
                client = null;
                return;
            }
            Hide();
            UsersForm form = new UsersForm(client);
            form.Show(this);
        }

        private void usernameTextBox_TextChanged(object sender, EventArgs e)
        {
            loginButton.Enabled = (sender as TextBox).Text != "";
        }
    }
}
