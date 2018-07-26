using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using static Dalsae.FileManager;
using static Dalsae.DalsaeManager;
using static Dalsae.TwitterWeb;
using static Dalsae.DataManager;
using static Dalsae.TweetManager;
using System.Text.RegularExpressions;
using Dalsae.WindowAndControl;
using System.Threading;
using Dalsae.Template;
using Dalsae.Data;
using System.Collections.Concurrent;

namespace Dalsae
{
	public partial class MainWindow : Window
	{
		private void mainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
			{
				e.Handled = true;
				SetFindGridVisibility(true);
			}
			else if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
			{
				object obj = FocusManager.GetFocusedElement(this) as object;
				if (obj == null)
					EnterInputTweet();
			}
		}


		private void textBoxFind_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				FindTweet(textBoxFind.Text);
			}
			else if (e.Key == Key.Escape)
			{
				SetFindGridVisibility(false);
			}
			else if(e.Key== Key.Up || e.Key== Key.Down)
			{
				FocusPanel();
			}
		}

		private void treeView_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			TreeView treeView = sender as TreeView;
			if (treeView == null) return;

			Key key = e.Key;
			if (e.Key == Key.None || key == Key.ImeProcessed)
				key = e.ImeProcessedKey;

			if (key == Key.Down && Keyboard.Modifiers == ModifierKeys.Control)
			{
				FindNextUserTweet();
				e.Handled = true;
			}
			else if(key== Key.Up)
			{
				if (Keyboard.Modifiers == ModifierKeys.Control)
					FindPrevUserTweet();
				else
				{
					e.Handled = true;
					if (dicPanel[selectPanel].ArrowUp() == false)
						EnterInputTweet();
				}
			}
			else if (key== Key.Down)
			{
				e.Handled = true;
				dicPanel[selectPanel].ArrowDown();
			}
			else if (key == Key.Right)
			{
				LoadDeahwa();
			}
			else if(key== Key.Left)
			{
				CloseDeahwa();
			}
			else if (key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
			{
				TweetCopy();
				e.Handled = true;
			}
			else if (key== Key.Enter)
			{
				EnterInputTweet();
				e.Handled = true;
			}
			else if(key == Key.PageDown || key== Key.PageUp)
			{
			}
			else
				InputHotkey(e);
		}
		

		private void treeViewDM_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			Key key = e.Key;
			if (e.Key == Key.None || key == Key.ImeProcessed)
				key = e.ImeProcessedKey;

			if (key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
			{
				DMCopy();
				e.Handled = true;
				return;
			}
			else if (key == Key.PageDown || key == Key.PageUp)
			{
				return;
			}
			else if (key == Key.Up)
			{
				e.Handled = true;
				if (dicPanel[selectPanel].ArrowUp() == false)
					EnterInputTweet();
			}
			else if (key == Key.Down)
			{
				e.Handled = true;
				dicPanel[selectPanel].ArrowDown();
			}
			else
				InputDMHotKey(e);
		}

		private void InputDMHotKey(KeyEventArgs e)
		{
			Key key = e.Key;
			if (e.Key == Key.None || key == Key.ImeProcessed)
				key = e.ImeProcessedKey;

			HotKeys.eHotKey ehotkey = DataInstence.hotKey.PressHotKey(Keyboard.IsKeyDown(Key.LeftCtrl), Keyboard.IsKeyDown(Key.LeftShift), Keyboard.IsKeyDown(Key.LeftAlt), key);
			if (ehotkey != HotKeys.eHotKey.eNone && key == Key.ImeProcessed)
				e.Handled = true;
			switch (ehotkey)
			{
				case HotKeys.eHotKey.eNone:
					break;
				case HotKeys.eHotKey.eKeyInput:
					EnterInputTweet();
					break;
				case HotKeys.eHotKey.eKeyReply:
				case HotKeys.eHotKey.eKeyReplyAll:
				case HotKeys.eHotKey.eKeySendDM:
					ReplyDM();
					break;
				case HotKeys.eHotKey.eKeyShowTL:
					ShowPanel(eTweetPanel.eHome);
					break;
				case HotKeys.eHotKey.eKeyShowMention:
					ShowPanel(eTweetPanel.eMention);
					break;
				case HotKeys.eHotKey.eKeyShowDM:
					ShowPanel(eTweetPanel.eDm);
					break;
				case HotKeys.eHotKey.eKeyShowFavorite:
					ShowPanel(eTweetPanel.eFavorite);
					break;
				case HotKeys.eHotKey.eKeyGoHome:
					KeyHome();
					break;
				case HotKeys.eHotKey.eKeyGoEnd:
					KeyEnd();
					break;
				case HotKeys.eHotKey.eKeyLoad:
					LoadTweetByKey();
					break;
				case HotKeys.eHotKey.eKeyShowOpendUrl:
					ShowPanel(eTweetPanel.eOpen);
					break;
				case HotKeys.eHotKey.eKeyMenu:
					ShowMenu();
					break;
				case HotKeys.eHotKey.eKeyOpenImage:
					OpenImage();
					break;
				case HotKeys.eHotKey.eKeyCopyTweet:
					DMCopy();
					break;
				case HotKeys.eHotKey.eKeySmallPreview:
					break;
				case HotKeys.eHotKey.eKeyRefresh:
					Refresh();
					break;
				default:
					break;
			}

			if (e.Key == Key.ImeProcessed && Key.A <= e.ImeProcessedKey && e.ImeProcessedKey <= Key.Z)
				e.Handled = true;//ime가 영어가 아닐 때에 ime 창 나오는 모습 안 띄우기 위해 handle=true함

		}

		private void InputHotkey(KeyEventArgs e)
		{
			Key key = e.Key;
			if (e.Key == Key.None || key == Key.ImeProcessed)
				key = e.ImeProcessedKey;

			HotKeys.eHotKey ehotkey = DataInstence.hotKey.PressHotKey(Keyboard.IsKeyDown(Key.LeftCtrl), Keyboard.IsKeyDown(Key.LeftShift), Keyboard.IsKeyDown(Key.LeftAlt), key);
			if (ehotkey != HotKeys.eHotKey.eNone && key == Key.ImeProcessed)
				e.Handled = true;
			switch (ehotkey)
			{
				case HotKeys.eHotKey.eNone:
					break;
				case HotKeys.eHotKey.eKeyReplyAll:
					ReplyAll();
					break;
				case HotKeys.eHotKey.eKeyReply:
					Reply();
					break;
				case HotKeys.eHotKey.eKeyRetweet:
					Retweet();
					break;
				case HotKeys.eHotKey.eKeyFavorite:
					AddFavorite();
					break;
				case HotKeys.eHotKey.eKeyQRetweet:
					QTRetweet();
					break;
				case HotKeys.eHotKey.eKeyInput:
					EnterInputTweet();
					break;
				case HotKeys.eHotKey.eKeySendDM:
					AddIdDm();
					break;
				case HotKeys.eHotKey.eKeyHashTag:
					AddHashTag();
					break;
				case HotKeys.eHotKey.eKeyShowTL:
					ShowPanel(eTweetPanel.eHome);
					break;
				case HotKeys.eHotKey.eKeyShowMention:
					ShowPanel(eTweetPanel.eMention);
					break;
				case HotKeys.eHotKey.eKeyShowDM:
					ShowPanel(eTweetPanel.eDm);
					break;
				case HotKeys.eHotKey.eKeyShowFavorite:
					ShowPanel(eTweetPanel.eFavorite);
					break;
				case HotKeys.eHotKey.eKeyGoHome:
					KeyHome();
					break;
				case HotKeys.eHotKey.eKeyGoEnd:
					KeyEnd();
					break;
				case HotKeys.eHotKey.eKeyDeleteTweet:
					DeleteTweet();
					break;
				case HotKeys.eHotKey.eKeyLoad:
					LoadTweetByKey();
					break;
				case HotKeys.eHotKey.eKeyShowOpendUrl:
					ShowPanel(eTweetPanel.eOpen);
					break;
				case HotKeys.eHotKey.eKeyMenu:
					ShowMenu();
					break;
				case HotKeys.eHotKey.eKeyOpenImage:
					OpenImage();
					break;
				case HotKeys.eHotKey.eKeyCopyTweet:
					TweetCopy();
					break;
				case HotKeys.eHotKey.eKeySmallPreview:
					
					break;
				case HotKeys.eHotKey.eKeyRefresh:
					Refresh();
					break;
				default:
					break;	
			}
			if (ehotkey != HotKeys.eHotKey.eNone)
				e.Handled = true;

			if (e.Key == Key.ImeProcessed && Key.A <= e.ImeProcessedKey && e.ImeProcessedKey <= Key.Z)
				e.Handled = true;//ime가 영어가 아닐 때에 ime 창 나오는 모습 안 띄우기 위해 handle=true함
		}

		private void ShowMenu()
		{
			TreeViewItem item = dicPanel[selectPanel].GetSelectedTreeViewItem();
			Grid grid = Generate.FindElementByName<Grid>(item, "grid");
			if (grid.DataContext is ClientTweet)
				contextOnOpening(grid, null);
			else
				dmcontextOnOpening(grid, null);
			Point pt = item.PointToScreen(new Point(0, 0));
			grid.ContextMenu.PlacementRectangle = new Rect(pt, new Size(item.Width, item.Height));
			grid.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
			grid.ContextMenu.IsOpen = true;
		}

		private void CloseDeahwa()
		{
			selectTweet.uiProperty.ClearDeahwa();
		}

		private void KeyHome()
		{
			dicPanel[selectPanel].SelectHome();
		}

		private void KeyEnd()
		{
			dicPanel[selectPanel].SelectEnd();
		}

		private void Refresh()
		{
			TweetInstence.ClearTweet(selectPanel);
		}

		private void FindPrevUserTweet()
		{
			if (selectPanel == eTweetPanel.eDm) return;//dm은 기능 x
			if (selectTweet == null) return;
			if (selectTweet.user == null) return;
			dicPanel[selectPanel].FindPrevUserTweet(selectTweet.user.id);
		}


		private void FindNextUserTweet()
		{
			if (selectPanel == eTweetPanel.eDm) return;//dm은 기능 x
			if (selectTweet == null) return;
			if (selectTweet.user == null) return;
			dicPanel[selectPanel].FindNextUserTweet(selectTweet.user.id);
		}
		
		private void LoadTweetByKey()
		{
			if (selectPanel == eTweetPanel.eUser)
			{
				if (dicPanel[eTweetPanel.eUser].treeView.Items.Count < 2) return;
				ClientTweet tweet = dicPanel[eTweetPanel.eUser].treeView.Items[0] as ClientTweet;
				if (tweet.user != null)
					DalsaeInstence.LoadTweet(selectPanel, tweet.user.screen_name);
			}
			else
				DalsaeInstence.LoadTweet(selectPanel);
		}
	

		private void textBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (inputTweetBox.Text.Length == 0)
			{
				labelCount.Text = "(0/280)";
				labelCount2.Text = "(0/280)";
				inputTweetBox.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
			}
			else
			{
				MatchCollection mt = UrlMatch.Matches(inputTweetBox.Text);
				tweetLength = 0;
				for (int i = 0; i < mt.Count; i++)
					tweetLength += mt[i].Length;
				int enterCount = inputTweetBox.Text.Count(x => x == '\n');//줄바꿈문자는 \r\n인데 2글자라 entercount를 빼서 셈
				int charLength = GetCharLength(inputTweetBox.Text);
				tweetLength = charLength - tweetLength + 23 * mt.Count - enterCount;
				if (tweetLength > 280)
					inputTweetBox.Background = new SolidColorBrush(Color.FromRgb(0xff, 0xa7, 0xa7));
				else
					inputTweetBox.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

				labelCount.Text = $"({tweetLength}/280)";
				labelCount2.Text = $"({tweetLength}/280)";
			}
		}

		/// <summary>
		/// 280자 문자열 개수 세는 거
		/// </summary>
		/// <param name="str">트윗 입력칸에 있는 문자열</param>
		/// <returns></returns>
		private int GetCharLength(string str)
		{
			//0 - 0x10FF(4351), 0x2000(8192) - 0x200D(8205), 
			//0x2010(8208) - 0x201F(8223), 0x2032(8242) - 0x2037(8247)만 1byte, 나머지는 모두 2byte
			int ret = 0;
			for (int i = 0; i < str.Length; i++)
			{
				int num = Convert.ToInt32(str[i]);
				if (0 <= num && num <= 4351)
					ret+=1;
				else if (8192 <= num && num <= 8205)
					ret += 1;
				else if (8208 <= num && num <= 8223)
					ret += 1;
				else if (8242 <= num && num <= 8247)
					ret += 1;
				else if (num < 0)
					ret += 0;
				else
					ret += 2;
			}
			return ret;
		}
		private void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Enter:
					if (isShowMentionIds.isOn)
					{
						e.Handled = true;
						InputMentionId();
					}
					else
					{
						if (Keyboard.IsKeyDown(Key.LeftShift))
						{
							e.Handled = true;
							int index = inputTweetBox.SelectionStart;
							inputTweetBox.Text = inputTweetBox.Text.Insert(index, Environment.NewLine);
							inputTweetBox.SelectionStart = index + 1;
						}
						else if (DataInstence.option.isSendEnter)
						{
							e.Handled = true;
							if (inputTweetBox.Text.Length > 0 || listBitmapImage.Count > 0 || pathGif.Length > 0)
								SendTweet();
							else
								FocusPanel();
						}
						else if (Keyboard.IsKeyDown(Key.LeftCtrl))
						{
							e.Handled = true;
							SendTweet();
						}
					}
					break;
				case Key.Escape:
					ClearInput();
					break;
				case Key.Up:
					if(isShowMentionIds.isOn)
					{
						if (listBoxIds.SelectedIndex > 0)
						{
							ListBoxItem selTweet = listBoxIds.ItemContainerGenerator.ContainerFromIndex(listBoxIds.SelectedIndex - 1) as ListBoxItem;
							if (selTweet != null)
							{
								e.Handled = true;
								selTweet.IsSelected = true;
								selTweet.Focus();
								EnterInputTweet();
							}
						}
					}
					break;
				case Key.Down:
					if (isShowMentionIds.isOn)
					{
						ListBoxItem selTweet = listBoxIds.ItemContainerGenerator.ContainerFromIndex(listBoxIds.SelectedIndex + 1) as ListBoxItem;
						if (selTweet != null)
						{
							e.Handled = true;
							selTweet.IsSelected = true;
							selTweet.Focus();
							EnterInputTweet();
						}
					}
					else
					{
						int lineIndex = inputTweetBox.GetLineIndexFromCharacterIndex(inputTweetBox.SelectionStart);
						if (lineIndex + 1 == inputTweetBox.LineCount)
						{
							FocusPanel();
							e.Handled = true;
						}
					}
					break;
			}
			if(e.Key== Key.V && Keyboard.Modifiers==ModifierKeys.Control)
			{
				AddImageClipboard();
			}
		}

		private void AddImageClipboard()
		{
			if (listBitmapImage.Count >= 4) return;
			BitmapSource bitmap = Clipboard.GetImage();
			if (bitmap == null) return;
			BitmapImage image = new BitmapImage();

			JpegBitmapEncoder encoder = new JpegBitmapEncoder();
			bool isLarge = false;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				encoder.Frames.Add(BitmapFrame.Create(bitmap));
				encoder.Save(memoryStream);

				memoryStream.Position = 0;
				image.BeginInit();
				image.StreamSource = new MemoryStream(memoryStream.ToArray());
				image.EndInit();
				isLarge= CheckFileSize(memoryStream);
				if(isLarge)
				{
					ShowMessage("크기가 5MB 이상인 이미지는 등록 불가능합니다.", "오류",
								MessageBoxButton.OK, MessageBoxImage.Warning);
				}
				else
					listBitmapImage.Add(image);
				
			}
			UpdateImage();
		}
		private int idStartIndex = 0;//아이디 자동완성에서 텍스트 변경 시 필요 index 2개
		private int idEndIndex = 0;
		private void inputTweet_SelectionChanged(object sender, RoutedEventArgs e)
		{
			int lineIndex = inputTweetBox.GetLineIndexFromCharacterIndex(inputTweetBox.SelectionStart);
			string lineText = inputTweetBox.GetLineText(lineIndex);
			if (lineText.Length == 0)
			{
				HideMentionListBox();
				return;
			}
			int lineFirstIndex = inputTweetBox.GetCharacterIndexFromLineIndex(lineIndex);
			int nowIndex = inputTweetBox.SelectionStart - lineFirstIndex - 1;
			int endIndex = lineFirstIndex + lineText.Length;
			int wordStartIndex = 0;
			for (wordStartIndex = nowIndex; wordStartIndex >= 0; wordStartIndex--)
			{
				if (lineText[wordStartIndex] == ' ')
				{
					HideMentionListBox();
					return;
				}
				else if (lineText[wordStartIndex] == '@')
					break;
			}
			if (wordStartIndex == -1)
			{
				HideMentionListBox();
				return;
			}
			else if (wordStartIndex > 0)
			{
				if (lineText[wordStartIndex - 1] != ' ')
				{
					HideMentionListBox();
					return;
				}
			}

			int wordEndIndex = 0;
			for (wordEndIndex = nowIndex; wordEndIndex < lineText.Length; wordEndIndex++)
			{
				if (lineText[wordEndIndex] == '\r' || lineText[wordEndIndex] == ' ')
				{
					wordEndIndex--;
					break;
				}
			}

			idStartIndex = lineFirstIndex + wordStartIndex;
			idEndIndex = lineFirstIndex + wordEndIndex;

			string findText = lineText.Substring(wordStartIndex, wordEndIndex - wordStartIndex);
			FindFriends(findText);
		}

		private void HideMentionListBox()
		{
			if (isShowMentionIds.isOn)
				isShowMentionIds.isOn = false;
		}

		private void ShowMentionListBox()
		{
			if (isShowMentionIds.isOn == false)
				isShowMentionIds.isOn = true;
		}

		private string prevWord = string.Empty;
		private void FindFriends(string word)
		{
			if (word == "@") return;
			ShowMentionListBox();
			word = word.Replace("@", "");
			if (prevWord == word) return;//중복키면 종료
			prevWord = word;
			listMentionIds.Clear();
			listBoxIds.SelectedIndex = -1;
			ConcurrentDictionary<long, UserSemi> dicUser = DataInstence.dicFollwing;
			foreach(UserSemi user in dicUser.Values)
			{
				if (user.screen_name.IndexOf(word, StringComparison.CurrentCultureIgnoreCase) > -1
					|| user.name.IndexOf(word, StringComparison.CurrentCultureIgnoreCase) > -1)
					listMentionIds.Add(user);
			}
		}

		private void InputMentionId()
		{
			UserSemi user = listBoxIds.SelectedItem as UserSemi;
			if (user == null) return;
			StringBuilder sb = new StringBuilder(inputTweetBox.Text);
			sb.Remove(idStartIndex, idEndIndex - idStartIndex);
			sb.Insert(idStartIndex, $"@{user.screen_name} ");
			inputTweetBox.Text = sb.ToString();
			inputTweetBox.SelectionStart = inputTweetBox.Text.Length;
			HideMentionListBox();
		}
	}
}