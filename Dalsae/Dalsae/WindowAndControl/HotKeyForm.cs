using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Dalsae.FileManager;
using static Dalsae.DalsaeManager;
using static Dalsae.TwitterWeb;
using static Dalsae.DataManager;
using static Dalsae.TweetManager;
using System.Windows.Input;
using Dalsae.Data;

namespace Dalsae.WindowAndControl
{
	public partial class HotKeyForm : Form
	{
		private HotKeys hotkey;
		HashSet<string> hashKeys = new HashSet<string>();
		public HotKeyForm()
		{
			InitializeComponent();
			hotkey = new HotKeys(DataInstence.hotKey);
			SetHashs();
			SetComboBox();
			SetKeys();
		}

		private void SetHashs()
		{
			hashKeys.Add("0");
			hashKeys.Add("1");
			hashKeys.Add("2");
			hashKeys.Add("3");
			hashKeys.Add("4");
			hashKeys.Add("5");
			hashKeys.Add("6");
			hashKeys.Add("7");
			hashKeys.Add("8");
			hashKeys.Add("9");
			for (char i = 'A'; i <= 'Z'; i++)
			{
				hashKeys.Add(i.ToString());
			}
			hashKeys.Add("F1");
			hashKeys.Add("F2");
			hashKeys.Add("F3");
			hashKeys.Add("F4");
			hashKeys.Add("F5");
			hashKeys.Add("F6");
			hashKeys.Add("F7");
			hashKeys.Add("F8");
			hashKeys.Add("F9");
			hashKeys.Add("F10");
			hashKeys.Add("F11");
			hashKeys.Add("F12");
			//hashKeys.Add("`");
			//hashKeys.Add(".");
			//hashKeys.Add(",");
			//hashKeys.Add("/");
			//hashKeys.Add("\\");
			hashKeys.Add("Space");
			hashKeys.Add("Delete");
			hashKeys.Add("Home");
			hashKeys.Add("End");
			hashKeys.Add("Insert");
		}

		private void SetComboBox()
		{
			string[] arrayItems = hashKeys.ToArray();
			textReplyAll.Items.AddRange(arrayItems);
			textReply.Items.AddRange(arrayItems);
			textRetweet.Items.AddRange(arrayItems);
			textFavorite.Items.AddRange(arrayItems);
			textQRtweet.Items.AddRange(arrayItems);
			textInput.Items.AddRange(arrayItems);
			textSendDM.Items.AddRange(arrayItems);
			textDelete.Items.AddRange(arrayItems);
			textHashTag.Items.AddRange(arrayItems);
			textTL.Items.AddRange(arrayItems);
			textMention.Items.AddRange(arrayItems);
			textShowDM.Items.AddRange(arrayItems);
			textShowFavorite.Items.AddRange(arrayItems);
			textGoEnd.Items.AddRange(arrayItems);
			textGoHome.Items.AddRange(arrayItems);
			textLoad.Items.AddRange(arrayItems);
			textMenu.Items.AddRange(arrayItems);
			textUrl.Items.AddRange(arrayItems);
			textImage.Items.AddRange(arrayItems);
		}

		private void SetKeys()
		{
			textReplyAll.Text = GetStringFromKey(hotkey.keyReplyAll);
			textReply.Text = GetStringFromKey(hotkey.keyReply);
			textRetweet.Text = GetStringFromKey(hotkey.keyRetweet);
			textFavorite.Text = GetStringFromKey(hotkey.keyFavorite);
			textQRtweet.Text = GetStringFromKey(hotkey.keyQRetweet);
			textInput.Text = GetStringFromKey(hotkey.keyInput);
			textSendDM.Text = GetStringFromKey(hotkey.keyDM);
			textDelete.Text = GetStringFromKey(hotkey.keyDeleteTweet);
			textHashTag.Text = GetStringFromKey(hotkey.keyHashTag);
			textTL.Text = GetStringFromKey(hotkey.keyShowTL);
			textMention.Text = GetStringFromKey(hotkey.keyShowMention);
			textShowDM.Text = GetStringFromKey(hotkey.keyShowDM);
			textShowFavorite.Text = GetStringFromKey(hotkey.keyShowFavorite);
			textGoEnd.Text = GetStringFromKey(hotkey.keyGoEnd);
			textGoHome.Text = GetStringFromKey(hotkey.keyGoHome);
			textLoad.Text = GetStringFromKey(hotkey.keyLoad);
			textMenu.Text = GetStringFromKey(hotkey.keyMenu);
			textUrl.Text = GetStringFromKey(hotkey.keyShowOpendURL);
			textImage.Text = GetStringFromKey(hotkey.keyOpenImage);
		}

		private string GetStringFromKey(Key key)
		{
			string ret = string.Empty;
			if (Key.D0 <= key && key <= Key.D9)
				ret = key.ToString()[1].ToString();
			else
				ret = key.ToString();

			return ret;
		}

		private Key GetKeyFromString(string key)
		{
			Key ret = Key.None;
			if (key == "1")
				ret = Key.D1;
			else if (key == "2")
				ret = Key.D2;
			else if (key == "3")
				ret = Key.D3;
			else if (key == "4")
				ret = Key.D4;
			else if (key == "5")
				ret = Key.D5;
			else if (key == "6")
				ret = Key.D6;
			else if (key == "7")
				ret = Key.D7;
			else if (key == "8")
				ret = Key.D8;
			else if (key == "9")
				ret = Key.D9;
			else if (key == "0")
				ret = Key.D0;
			else
				ret = (Key)Enum.Parse(typeof(Key), key, false);

			return ret;
		}

		private void SaveHotkey()
		{
			DataInstence.hotKey = hotkey;
			FileManager.FileInstence.UpdateHotkey(hotkey);
		}
		
		private void UpdateKey()
		{
			hotkey.UpdateKey(HotKeys.eHotKey.eKeyReplyAll, GetKeyFromString(textReplyAll.Text));
			hotkey.UpdateKey(HotKeys.eHotKey.eKeyReply, GetKeyFromString(textReply.Text));
			hotkey.UpdateKey(HotKeys.eHotKey.eKeyRetweet, GetKeyFromString(textRetweet.Text));
			hotkey.UpdateKey(HotKeys.eHotKey.eKeyFavorite, GetKeyFromString(textFavorite.Text));
			hotkey.UpdateKey(HotKeys.eHotKey.eKeyQRetweet, GetKeyFromString(textQRtweet.Text));
			hotkey.UpdateKey(HotKeys.eHotKey.eKeyInput, GetKeyFromString(textInput.Text));
			hotkey.UpdateKey(HotKeys.eHotKey.eKeySendDM, GetKeyFromString(textSendDM.Text));
			hotkey.UpdateKey(HotKeys.eHotKey.eKeyDeleteTweet, GetKeyFromString(textDelete.Text));
			hotkey.UpdateKey(HotKeys.eHotKey.eKeyHashTag, GetKeyFromString(textHashTag.Text));
			hotkey.UpdateKey(HotKeys.eHotKey.eKeyShowTL, GetKeyFromString(textTL.Text));
			hotkey.UpdateKey(HotKeys.eHotKey.eKeyShowMention, GetKeyFromString(textMention.Text));
			hotkey.UpdateKey(HotKeys.eHotKey.eKeyShowDM, GetKeyFromString(textShowDM.Text));
			hotkey.UpdateKey(HotKeys.eHotKey.eKeyShowFavorite, GetKeyFromString(textShowFavorite.Text));
			hotkey.UpdateKey(HotKeys.eHotKey.eKeyGoEnd, GetKeyFromString(textGoEnd.Text));
			hotkey.UpdateKey(HotKeys.eHotKey.eKeyGoHome, GetKeyFromString(textGoHome.Text));
			hotkey.UpdateKey(HotKeys.eHotKey.eKeyLoad, GetKeyFromString(textLoad.Text));
			hotkey.UpdateKey(HotKeys.eHotKey.eKeyShowOpendUrl, GetKeyFromString(textUrl.Text));
			hotkey.UpdateKey(HotKeys.eHotKey.eKeyMenu, GetKeyFromString(textMenu.Text));
			hotkey.UpdateKey(HotKeys.eHotKey.eKeyOpenImage, GetKeyFromString(textImage.Text));
		}

		private bool CheckKeys()
		{
			bool ret = true;

			HashSet<Key> keys = new HashSet<Key>();
			keys.Add(hotkey.keyReplyAll);
			keys.Add(hotkey.keyReply);
			keys.Add(hotkey.keyRetweet);
			keys.Add(hotkey.keyFavorite);
			keys.Add(hotkey.keyQRetweet);
			keys.Add(hotkey.keyInput);
			keys.Add(hotkey.keyDM);
			keys.Add(hotkey.keyDeleteTweet);
			keys.Add(hotkey.keyHashTag);
			keys.Add(hotkey.keyShowTL);
			keys.Add(hotkey.keyShowMention);
			keys.Add(hotkey.keyShowDM);
			keys.Add(hotkey.keyShowFavorite);
			keys.Add(hotkey.keyGoEnd);
			keys.Add(hotkey.keyMenu);
			keys.Add(hotkey.keyGoHome);
			keys.Add(hotkey.keyShowOpendURL);
			keys.Add(hotkey.keyLoad);
			keys.Add(hotkey.keyOpenImage);

			if (keys.Count != 19)
				ret = false;
			return ret;
		}

		//private bool UpdateKey(TextBox textBox, Keys key)
		//{
		//	bool ret = true;

		//	if(textBox.Equals(textReplyAll))
		//	{
		//		ret = hotkey.UpdateKey(HotKeys.eHotKeys.eKeyReplyAll, key);
		//	}
		//	else if(textBox.Equals(textReply))
		//	{
		//		ret = hotkey.UpdateKey(HotKeys.eHotKeys.eKeyReply, key);
		//	}
		//	else if (textBox.Equals(textRetweet))
		//	{
		//		ret = hotkey.UpdateKey(HotKeys.eHotKeys.eKeyRetweet, key);
		//	}
		//	else if (textBox.Equals(textFavorite))
		//	{
		//		ret = hotkey.UpdateKey(HotKeys.eHotKeys.eKeyFavorite, key);
		//	}
		//	else if (textBox.Equals(textQRtweet))
		//	{
		//		ret = hotkey.UpdateKey(HotKeys.eHotKeys.eKeyQRetweet, key);
		//	}
		//	else if (textBox.Equals(textInput))
		//	{
		//		ret = hotkey.UpdateKey(HotKeys.eHotKeys.eKeyInput, key);
		//	}
		//	else if (textBox.Equals(textSendDM))
		//	{
		//		ret = hotkey.UpdateKey(HotKeys.eHotKeys.eKeySendDM, key);
		//	}
		//	else if (textBox.Equals(textDelete))
		//	{
		//		ret = hotkey.UpdateKey(HotKeys.eHotKeys.eKeyDeleteTweet, key);
		//	}
		//	else if (textBox.Equals(textHashTag))
		//	{
		//		ret = hotkey.UpdateKey(HotKeys.eHotKeys.eKeyHashTag, key);
		//	}
		//	else if (textBox.Equals(textTL))
		//	{
		//		ret = hotkey.UpdateKey(HotKeys.eHotKeys.eKeyShowTL, key);
		//	}
		//	else if (textBox.Equals(textMention))
		//	{
		//		ret = hotkey.UpdateKey(HotKeys.eHotKeys.eKeyShowMention, key);
		//	}
		//	else if (textBox.Equals(textShowDM))
		//	{
		//		ret = hotkey.UpdateKey(HotKeys.eHotKeys.eKeyShowDM, key);
		//	}
		//	else if (textBox.Equals(textShowFavorite))
		//	{
		//		ret = hotkey.UpdateKey(HotKeys.eHotKeys.eKeyShowFavorite, key);
		//	}
		//	else if (textBox.Equals(textGoEnd))
		//	{
		//		ret = hotkey.UpdateKey(HotKeys.eHotKeys.eKeyGoEnd, key);
		//	}
		//	else if (textBox.Equals(textGoHome))
		//	{
		//		ret = hotkey.UpdateKey(HotKeys.eHotKeys.eKeyGoHome, key);
		//	}
		//	else if (textBox.Equals(textLoad))
		//	{
		//		ret = hotkey.UpdateKey(HotKeys.eHotKeys.eKeyLoad, key);
		//	}

		//	return ret;
		//}

		private void okButton_Click(object sender, EventArgs e)
		{
			UpdateKey();
			if (CheckKeys())
			{
				SaveHotkey();
				this.Close();
			}
			else
			{
				DalsaeInstence.ShowMessageBox("단축키가 중복됩니다.", "오류");
			}
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
