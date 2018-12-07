using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Dalsae.DataManager;
using static Dalsae.DalsaeManager;
using System.Collections.Concurrent;
using Dalsae.Data;

namespace Dalsae.WindowAndControl
{
	/// <summary>
	/// FindWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class FindWindow : Window
	{
		private Dalsae.ListFindUser listFind { get; set; } = new ListFindUser();
		public FindWindow(Window owner)
		{
			this.Owner = owner;
			ShowInTaskbar = false;
			InitializeComponent();
			listBox.ItemsSource = listFind;
		}

		private void FindFriends(string word)
		{
			if (string.IsNullOrEmpty(word)) return;

			listFind.Clear();
			listBox.SelectedIndex = -1;
			ConcurrentDictionary<long, UserSemi> dicUser = DataInstence.dicFollwing;
			foreach (UserSemi user in dicUser.Values)
			{
				if (user.screen_name.IndexOf(word, StringComparison.CurrentCultureIgnoreCase) > -1
					|| user.name.IndexOf(word, StringComparison.CurrentCultureIgnoreCase) > -1)
					listFind.Add(user);
			}
		}

		private void InputFindId()
		{
			UserSemi user = listBox.SelectedItem as UserSemi;
			if (user == null) return;
			textBox.Text = user.screen_name;
		}

		private void listBoxMentionIds_DoubleClick(object sender, MouseButtonEventArgs e)
		{
			InputFindId();
		}

		private void textBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			FindFriends(textBox.Text);
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			FollowWindow win = new FollowWindow(textBox.Text.Replace("@", "").Replace(" ", ""));
			win.Show();
			//DalsaeInstence.LoadTweet(eTweetPanel.eUser, textBox.Text.Replace("@", "").Replace(" ", ""));
			Close();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				Close();
		}

		private void Window_Activated(object sender, EventArgs e)
		{
			textBox.Focus();
		}
	}
}
