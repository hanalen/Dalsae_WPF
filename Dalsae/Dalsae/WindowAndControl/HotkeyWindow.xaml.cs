using System;
using System.Collections.Generic;
using System.Globalization;
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
using Dalsae.Data;
using static Dalsae.Data.HotKeys;

namespace Dalsae.WindowAndControl
{
	/// <summary>
	/// HotkeyWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class HotkeyWindow : Window
	{
		public HotkeyWindow(HotKeys hotkeys)
		{
			InitializeComponent();
			ShowInTaskbar = false;
			CreateDataContext(hotkeys);
		}

		private void CreateDataContext(HotKeys hotkeys)
		{
			tbCopy.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyCopyTweet]);
			tbDelete.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyDeleteTweet]);
			tbFav.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyFavorite]);
			tbEnd.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyGoEnd]);
			tbHome.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyGoHome]);
			tbHash.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyHashTag]);
			tbInput.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyInput]);
			tbLoad.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyLoad]);
			tbMenu.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyMenu]);
			tbOpenImage.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyOpenImage]);
			tbQt.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyQRetweet]);
			tbReply.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyReply]);
			tbReplyAll.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyReplyAll]);
			tbRt.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyRetweet]);
			tbDM.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeySendDM]);
			tbShowDM.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyShowDM]);
			tbShowFav.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyShowFavorite]);
			tbShowMention.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyShowMention]);
			tbShowOpen.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyShowOpendUrl]);
			tbShowHome.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyShowTL]);
			tbSmallPreview.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeySmallPreview]);
			tbRefresh.DataContext= new HotKey(hotkeys.dicHotKey[eHotKey.eKeyRefresh]);
			tbClear.DataContext = new HotKey(hotkeys.dicHotKey[eHotKey.eKeyClear]);
		}

		private void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;
			TextBox tb = sender as TextBox;
			if (tb == null) return;

			if ((e.Key < Key.D0 || Key.Z < e.Key) && e.Key != Key.LeftAlt && e.Key != Key.LeftShift && e.Key != Key.LeftAlt
				&& e.Key != Key.System && (e.Key < Key.Space || Key.Home < e.Key) && e.Key != Key.ImeProcessed)
				return;

			Key key = e.Key;
			if (key == Key.None || key == Key.ImeProcessed)
				key = e.ImeProcessedKey;

			if (key == Key.LeftAlt || key == Key.LeftShift || key == Key.LeftCtrl || e.SystemKey == Key.LeftAlt) return;

			HotKey hotkey = tb.DataContext as HotKey;
			if (hotkey == null) return;

			hotkey.isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl);
			hotkey.isShift= Keyboard.IsKeyDown(Key.LeftShift);
			hotkey.isAlt = Keyboard.IsKeyDown(Key.LeftAlt);

			if (key == Key.None || key == Key.ImeProcessed)
				hotkey.key = e.ImeProcessedKey;
			if (key == Key.System)
				hotkey.key = e.SystemKey;
			else
				hotkey.key = key;

			SetText(tb, hotkey);
		}

		private void SetText(TextBox tb, HotKey hotkey)
		{
			string str = string.Empty;
			if (hotkey.isCtrl)
				str += "Ctrl +";
			if (hotkey.isAlt)
				str += "Alt +";
			if (hotkey.isShift)
				str += "Shift +";
			if (Key.D0 <= hotkey.key && hotkey.key <= Key.D9)
			{
				str += hotkey.key;
				str = str.Replace("D", "");
			}
			else
			{
				str += hotkey.key;
			}
			tb.Text = str;
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			HashSet<string> hash = new HashSet<string>();
			hash.Add(tbCopy.Text);
			hash.Add(tbDelete.Text);
			hash.Add(tbFav.Text);
			hash.Add(tbEnd.Text);
			hash.Add(tbHome.Text);
			hash.Add(tbHash.Text);
			hash.Add(tbInput.Text);
			hash.Add(tbLoad.Text);
			hash.Add(tbMenu.Text);
			hash.Add(tbOpenImage.Text);
			hash.Add(tbQt.Text);
			hash.Add(tbReply.Text);
			hash.Add(tbReplyAll.Text);
			hash.Add(tbRt.Text);
			hash.Add(tbDM.Text);
			hash.Add(tbShowDM.Text);
			hash.Add(tbShowFav.Text);
			hash.Add(tbShowMention.Text);
			hash.Add(tbShowOpen.Text);
			hash.Add(tbShowHome.Text);
			hash.Add(tbSmallPreview.Text);
			hash.Add(tbRefresh.Text);
			hash.Add(tbClear.Text);

			if (hash.Count != 23)
			{
				MessageBox.Show(this, "단축키가 중복됩니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			else
			{
				SaveHotkey();
				Close();	
			}
		}

		private void SaveHotkey()
		{
			Dictionary<eHotKey, HotKey> dicHotKey = new Dictionary<eHotKey, HotKey>();
			dicHotKey.Add(eHotKey.eKeyCopyTweet, tbCopy.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyClear, tbClear.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyDeleteTweet, tbDelete.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyFavorite, tbFav.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyGoEnd, tbEnd.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyGoHome, tbHome.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyHashTag, tbHash.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyInput, tbInput.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyLoad, tbLoad.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyMenu, tbMenu.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyOpenImage, tbOpenImage.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyQRetweet, tbQt.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyRefresh, tbRefresh.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyReply, tbReply.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyReplyAll, tbReplyAll.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyRetweet, tbRt.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeySendDM, tbDM.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyShowDM, tbShowDM.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyShowFavorite, tbShowFav.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyShowMention, tbShowMention.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyShowOpendUrl, tbShowOpen.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeyShowTL, tbShowHome.DataContext as HotKey);
			dicHotKey.Add(eHotKey.eKeySmallPreview, tbSmallPreview.DataContext as HotKey);

			foreach (HotKey key in dicHotKey.Values)
			{
				if (key == null)
				{
					MessageBox.Show(this, "단축키 설정에 문제가 생겼습니다.\r\n창을 닫습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
					Close();
				}
			}

			HotKeys hotkeys = new Data.HotKeys();
			hotkeys.dicHotKey = dicHotKey;

			DataManager.DataInstence.UpdateHotkeys(hotkeys);
		}

		private void btnCancle_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key== Key.Escape)
			{
				MessageBoxResult mr = MessageBox.Show(this, "창을 닫으시겠습니까?", "알림", MessageBoxButton.YesNo, MessageBoxImage.Information);
				if (mr == MessageBoxResult.Yes)
					Close();
			}
		}
	}

	public class UIHotKeyTextConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//ClientTweet tweet = value as ClientTweet;
			//if (tweet == null) return null;
			object ret = null;
			HotKey key = value as HotKey;
			if (key != null)
			{
				string str = string.Empty;
				if (key.isCtrl)
					str += "Ctrl +";
				if (key.isAlt)
					str += "Alt +";
				if (key.isShift)
					str += "Shift +";
				if (Key.D0 <= key.key && key.key <= Key.D9)
				{
					str += key.key;
					str = str.Replace("D", "");
				}
				else
				{
					str += key.key;
				}
				ret = str;
			}
			return ret;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
