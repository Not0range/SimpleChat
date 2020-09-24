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
    public partial class UsersForm : Form
    {
        Client client;

        Dictionary<string, List<string>> chats = new Dictionary<string, List<string>>();

        public UsersForm(Client client)
        {
            InitializeComponent();
            Text += $" ({client.username})";
            this.client = client;
            client.UsersRefresh += Client_UsersRefresh;
            client.MessageReceived += Client_MessageReceived;
            usersListBox.Items.AddRange(client.SendRequest());
        }

        private void Client_MessageReceived(string from, string message)
        {
            if (!chats.ContainsKey(from))
                chats.Add(from, new List<string>());
            chats[from].Add($"[{from}]: {message}");
            Task.Run(() =>
            {
                Invoke(new Action(() =>
                {
                    if (usersListBox.SelectedItem == null || usersListBox.SelectedItem.ToString() != from)
                    {
                        int i = 0;
                        for (; i < usersListBox.Items.Count; i++)
                            if (usersListBox.Items[i].ToString() == from)
                                break;
                        if(i < usersListBox.Items.Count)
                            usersListBox.Items[i] = usersListBox.Items[i] + "*";
                        else
                            usersListBox.Items.Add(from + "*");
                    }    
                    else
                        dialogListBox.Items.Add($"[{from}]: {message}");
                }));
            });
        }

        private void Client_UsersRefresh(string[] users)
        {
            Invoke(new Action(() =>
            {
                string temp = (usersListBox.SelectedItem != null) ? usersListBox.SelectedItem.ToString() : "";
                List<string> list = new List<string>();
                foreach (string s in usersListBox.Items)
                    if (s.IndexOf('*') != -1)
                        list.Add(s);
                usersListBox.Items.Clear();
                usersListBox.Items.AddRange(users);
                if (temp != "")
                {
                    int i = 0;
                    for (; i < usersListBox.Items.Count; i++)
                        if (usersListBox.Items[i].ToString() == temp)
                            break;
                    if (i < usersListBox.Items.Count)
                        usersListBox.SelectedIndex = i;
                    else
                        dialogListBox.Items.Clear();
                }

                if(list.Count != 0 && usersListBox.Items.Count != 0)
                {
                    for (int i = 0; i < usersListBox.Items.Count; i++)
                        if (list.IndexOf(usersListBox.Items[i].ToString() + '*') != -1)
                            usersListBox.Items[i] = usersListBox.Items[i].ToString() + '*';
                }
            }));
        }

        private void messageTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void UsersForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что желаете отключиться?", "Выход из чата", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                client.SendDisconnect();
                Owner.Show();
            }
            else
                e.Cancel = true;
        }

        private void usersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            dialogListBox.Enabled = (sender as ListBox).SelectedIndex != -1;
            messageTextBox.Enabled = (sender as ListBox).SelectedIndex != -1;
            SendButton.Enabled = (sender as ListBox).SelectedIndex != -1;
            if ((sender as ListBox).SelectedIndex != -1)
            {
                string username = (sender as ListBox).SelectedItem.ToString();
                if (username.EndsWith("*"))
                    (sender as ListBox).Items[(sender as ListBox).SelectedIndex] = username.Replace("*", "");
                dialogListBox.Items.Clear();
                if (chats.ContainsKey(username))
                    dialogListBox.Items.AddRange(chats[username].ToArray());
                else
                    chats.Add(username, new List<string>());
            }
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            if (client.SendMessage(usersListBox.SelectedItem.ToString(), messageTextBox.Text) == 0)
            {
                chats[usersListBox.SelectedItem.ToString()].Add($"[{client.username}]: {messageTextBox.Text}");
                dialogListBox.Items.Add($"[{client.username}]: {messageTextBox.Text}");
                messageTextBox.Text = "";
            }
            else
                MessageBox.Show("Ошибка отправки сообщения");
        }
    }
}
