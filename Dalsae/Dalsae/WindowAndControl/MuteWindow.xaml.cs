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

namespace Dalsae.WindowAndControl
{
	/// <summary>
	/// MuteWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MuteWindow : Window
	{
		public MuteWindow()
		{
			InitializeComponent();
			ShowInTaskbar = false;
		}

		//dicMuteTweet 참조 조심!!! long, string이라 참조는 안 걸리지만 조심!
		//뮤트 리스트 등록하는 항목
		public void SetMutes(string[] arrHighlight, string[] arrMuteWord, string[] arrMuteClient,
									string[] arrMuteUser, Dictionary<long, string> dicMuteTweet)
		{
			for (int i = 0; i < arrHighlight.Length; i++)
				listBoxHighlight.Items.Add(arrHighlight[i]);

			for (int i = 0; i < arrMuteWord.Length; i++)
				listBoxWord.Items.Add(arrMuteWord[i]);
			for (int i = 0; i < arrMuteClient.Length; i++)
				listBoxClient.Items.Add(arrMuteClient[i]);
			for (int i = 0; i < arrMuteUser.Length; i++)
				listBoxUser.Items.Add(arrMuteUser[i]);

			foreach(KeyValuePair<long, string> mute in dicMuteTweet)
			{
				ListBoxItem item = new ListBoxItem();
				item.Tag = mute;
				item.Content = mute.Value;
				listBoxTweet.Items.Add(item);
			}
		}

		public List<string> GetHighlight()
		{
			List<string> ret = new List<string>();
			for (int i = 0; i < listBoxHighlight.Items.Count; i++)
				ret.Add(listBoxHighlight.Items[i].ToString());

			return ret;
		}

		public List<string> GetUser()
		{
			List<string> ret = new List<string>();
			for (int i = 0; i < listBoxUser.Items.Count; i++)
				ret.Add(listBoxUser.Items[i].ToString());

			return ret;
		}

		public List<string> GetWord()
		{
			List<string> ret = new List<string>();
			for (int i = 0; i < listBoxWord.Items.Count; i++)
				ret.Add(listBoxWord.Items[i].ToString());

			return ret;
		}

		public List<string> GetClient()
		{
			List<string> ret = new List<string>();
			for (int i = 0; i < listBoxClient.Items.Count; i++)
				ret.Add(listBoxClient.Items[i].ToString());

			return ret;
		}

		public Dictionary<long, string> GetTweet()
		{
			Dictionary<long, string> ret = new Dictionary<long, string>();
			for(int i=0;i<listBoxTweet.Items.Count;i++)
			{
				ListBoxItem item = listBoxTweet.Items[i] as ListBoxItem;
				KeyValuePair<long, string> value = (KeyValuePair<long, string>)item.Tag;
				ret.Add(value.Key, value.Value);
			}
			return ret;
		}

		private void buttonOk_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void buttonCancle_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void buttonDelHighlight_Click(object sender, RoutedEventArgs e)
		{
			listBoxHighlight.Items.Remove(listBoxHighlight.SelectedItem);
		}

		private void buttonAddHighlight_Click(object sender, RoutedEventArgs e)
		{
			if (textBoxHighlight.Text.Length == 0) return;
			listBoxHighlight.Items.Add(textBoxHighlight.Text);
			textBoxHighlight.Text = string.Empty;
		}

		private void buttonAddWord_Click(object sender, RoutedEventArgs e)
		{
			if (textBoxWord.Text.Length == 0) return;
			listBoxWord.Items.Add(textBoxWord.Text);
			textBoxWord.Text = string.Empty;
		}

		private void buttonDelWord_Click(object sender, RoutedEventArgs e)
		{
			listBoxWord.Items.Remove(listBoxWord.SelectedItem);
		}

		private void buttonAddUser_Click(object sender, RoutedEventArgs e)
		{
			if (textBoxUser.Text.Length == 0) return;
			listBoxUser.Items.Add(textBoxUser.Text);
			textBoxUser.Text = string.Empty;
		}

		private void buttonDelUser_Click(object sender, RoutedEventArgs e)
		{
			listBoxUser.Items.Remove(listBoxUser.SelectedItem);
		}

		private void buttonAddClient_Click(object sender, RoutedEventArgs e)
		{
			if (textBoxClient.Text.Length == 0) return;
			listBoxClient.Items.Add(textBoxClient.Text);
			textBoxClient.Text = string.Empty;
		}

		private void buttonDelClient_Click(object sender, RoutedEventArgs e)
		{
			listBoxClient.Items.Remove(listBoxClient.SelectedItem);
		}

		private void buttonDelTweet_Click(object sender, RoutedEventArgs e)
		{
			listBoxTweet.Items.Remove(listBoxTweet.SelectedItem);
		}

		private void window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				DialogResult = false;
		}
	}
}
