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
using Dalsae.WindowAndControl;
using Microsoft.Win32;
using System.Threading;
using Dalsae.Template;
using Dalsae.Data;

namespace Dalsae	
{
	public partial class MainWindow : Window
	{
		//최초 로딩 시
		private void Dalsae_Loded(object sender, RoutedEventArgs e)
		{
			gridPreview.Visibility = Visibility.Hidden;
			gridPreview.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

			Manager.AccountAgent.accountInstence.Start();
			DalsaeInstence.LoadedMainWindow();

			foreach(TreeViewManager item in dicPanel.Values)
			{
				ScrollViewer scroll = Generate.FindElementByName<ScrollViewer>(item.treeView);
				item.scrollViewer = scroll;
			}
			imageHome.Source = bitmapHome;
			imageMention.Source = bitmapMention;
			imageDm.Source = bitmapDM;
			imageFav.Source = bitmapFav;
			imageOpen.Source = bitmapOpen;

			if(FileInstence.isPatched)
			{
				MessageBoxResult mr = MessageBox.Show(this, $"달새의 오류를 자동으로 전송 하시겠습니까?{Environment.NewLine}차후 패치까지 적용 됩니다.{Environment.NewLine}어려분의 오류 보고가 달새를 더 안정적으로 만듭니다.", "알림", MessageBoxButton.YesNo, MessageBoxImage.Information);
				if(mr== MessageBoxResult.Yes)
					DataInstence.option.isSendError = true;
				else
					DataInstence.option.isSendError = false;
				FileInstence.UpdateOption(DataInstence.option);
			}

		}

		private void mainWindow_Activated(object sender, EventArgs e)
		{
			EnterInputTweet();
			if(Generate.IsOnScreen(this)==false)
			{
				this.Left = 100;
				this.Top = 100;
			}
		}

		private void mainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			notiWidth.Width = this.Width - 57;
			//SaveWindowSizeAndLocation();
		}

		private void mainWindow_LocationChanged(object sender, EventArgs e)
		{
			//SaveWindowSizeAndLocation();
		}

		private void SaveWindowSizeAndLocation()
		{
			if (WindowState != WindowState.Maximized && WindowState != WindowState.Minimized)
			{
				DataInstence.SetMainWindowLocation(Left, Top, Width, Height);
				FileInstence.UpdateOption(DataInstence.option);
			}
		}

		private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (WindowState != WindowState.Maximized && WindowState != WindowState.Minimized)
			{
				DataInstence.SetMainWindowLocation(Left, Top, Width, Height);
			}
			DalsaeInstence.ProgramClosing();
		}

		private void tweetGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			Grid grid = sender as Grid;
			selectTweet = grid.DataContext as ClientTweet;
		}

		private void dmGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			Grid grid = sender as Grid;
			selectDM = grid.DataContext as ClientDirectMessage;
		}

		private void treeView_GotFocus(object sender, RoutedEventArgs e)
		{
			TreeView treeView = sender as TreeView;
			if (treeView.SelectedItem is ClientTweet)
				selectTweet = treeView.SelectedItem as ClientTweet;
			else if (treeView.SelectedItem is ClientDirectMessage)
				selectDM = treeView.SelectedItem as ClientDirectMessage;
		}

		//private void listboxItem_GotFocus(object sender, RoutedEventArgs e)
		//{
		//	//ListBoxItem item = sender as ListBoxItem;
		//	return;
		//	selectListBoxItem = null;
		//	if (selectListBoxItem.DataContext is ClientTweet)
		//	{
		//		selectTweet = selectListBoxItem.DataContext as ClientTweet;
		//		e.Handled = true;
		//	}
		//	else if (selectListBoxItem.DataContext is ClientDirectMessage)
		//	{
		//		selectDM = selectListBoxItem.DataContext as ClientDirectMessage;
		//		e.Handled = true;
		//	}
		//}
		private void listBoxMentionIds_DoubleClick(object sender, MouseButtonEventArgs e)
		{
			InputMentionId();
			EnterInputTweet();
		}

		//-------------------------------------------------------------------------------------------
		//---------------------------------컨텍스트 클릭-------------------------------------------
		//-------------------------------------------------------------------------------------------
		private void tweetGrid_MouseLeftDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount >= 2)//더블 클릭이 아닐 경우 종료
			{
				if (selectTweet == null) return;
				if (selectTweet.originalTweet == null) return;
				if (string.IsNullOrEmpty(selectTweet.originalTweet.in_reply_to_status_id_str)) return;
				DalsaeInstence.LoadSingleTweet(selectTweet);
			}
		}

		private void contextClick_EnterInput(object sender, RoutedEventArgs e)
		{
			EnterInputTweet();
		}

		private void contextClick_Image(object sender, RoutedEventArgs e)
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;
			MenuItem item = sender as MenuItem;
			if (item == null) return;

			ImageWindow win = new ImageWindow(tweet, item.Header.ToString());
			win.Show();
		}

		private void contextClickDM_Url(object sender, RoutedEventArgs e)
		{
			MenuItem item = sender as MenuItem;
			ClientDirectMessage dm = dicPanel[eTweetPanel.eDm].treeView.SelectedItem as ClientDirectMessage;
			if (dm == null || item == null) return;

			for (int i = 0; i < dm.entities.urls.Count; i++)
			{
				if (dm.entities.urls[i].display_url == item.Header.ToString())
				{
					System.Diagnostics.Process.Start(dm.entities.urls[i].expanded_url);
					break;
				}
			}
		}

		private void contextClickDM_Image(object sender, RoutedEventArgs e)
		{
			OpenImageDM();
		}

		private void contextClick_Video(object sender, RoutedEventArgs e)
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;
			MenuItem item = sender as MenuItem;
			if (item == null) return;
			string url = item.Header.ToString();

			VideoWindow window = new VideoWindow(selectTweet);
			window.Show();

			//if(tweet.tweetMovie.display_url==url)
			//{
			//	System.Diagnostics.Process.Start(tweet.tweetMovie.expanded_url);
			//	TweetInstence.AddTweet(ePanel.eOpen, tweet);
			//}

			//foreach(ClientMedia media in tweet.dicPhoto.Values)
			//{
			//	if(media.display_url==url)
			//	{
			//		System.Diagnostics.Process.Start(media.expanded_url);
			//		TweetInstence.AddTweet(ePanel.eOpen, tweet);
			//		break;
			//	}
			//}
			//VideoWindow win = new VideoWindow(tweet, item.Header.ToString());
			//win.Show();
		}

		private void contextClick_Url(object sender, RoutedEventArgs e)
		{
			MenuItem item = sender as MenuItem;
			if (item == null) return;
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;

			for (int i = 0; i < tweet.listUrl.Count; i++)
			{
				ClientURL url = item.Tag as ClientURL;
				if (url == null) continue;
				if (tweet.listUrl[i].expanded_url == url.expanded_url)
				{
					System.Diagnostics.Process.Start(tweet.listUrl[i].expanded_url);
					TweetInstence.AddTweet(eTweetPanel.eOpen, tweet);
					break;
				}
			}
		}

		private void contextClick_Reply(object sender, RoutedEventArgs e)
		{
			Reply();
		}

		private void contextClick_LoadDeahwa(object sender, RoutedEventArgs e)
		{
			LoadDeahwa();
		}

		private void contextClick_ReplyAll(object sender, RoutedEventArgs e)
		{
			ReplyAll();
		}

		private void contextClick_UserTweet(object sender, RoutedEventArgs e)
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;
			MenuItem item = sender as MenuItem;
			if (item == null) return;
			string strUser = item.Header.ToString().Replace("__", "_");
			DalsaeInstence.LoadTweet(eTweetPanel.eUser, strUser);
			//ShowPanel(eTweetPanel.eUser);
		}

		private void contextClick_UserMediaTweet(object sender, RoutedEventArgs e)
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;
			MenuItem item = sender as MenuItem;
			if (item == null) return;
			string strUser = item.Header.ToString().Replace("__", "_");
			DalsaeInstence.LoadTweet(eTweetPanel.eUserMedia, strUser);
			//ShowPanel(eTweetPanel.eUser);
		}

		private void dmcontextClick_UserTweet(object sender, RoutedEventArgs e)
		{
			ClientDirectMessage dm = selectDM;
			if (dm == null) return;
			MenuItem item = sender as MenuItem;
			if (item == null) return;
			string strUser = item.Header.ToString().Replace("__", "_");
			DalsaeInstence.LoadTweet(eTweetPanel.eUser, strUser);
			ShowPanel(eTweetPanel.eUser);
		}

		private void contextClick_UserRetweetOff(object sender, RoutedEventArgs e)
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;

			DalsaeInstence.RetweetOff(tweet.user.id);
		}

		//TODO
		//옵션 폼 만들고 체크
		private void contextClick_UserMute(object sender, RoutedEventArgs e)
		{
			MenuItem item = sender as MenuItem;
			if (item == null) return;

			DataInstence.option.AddMuteUser(item.Tag.ToString());
			FileManager.FileInstence.UpdateOption(DataInstence.option);
			MessageBox.Show($"{item.Tag.ToString()}유저가 뮤트목록에 추가되었습니다.", "알림", MessageBoxButton.OK,
				MessageBoxImage.None);
		}

		private void contextClick_UserProfile(object sender, RoutedEventArgs e)
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;
			MenuItem item = sender as MenuItem;
			if (item == null) return;
			string strUser = item.Header.ToString().Replace("__", "_");

			FollowWindow win = new FollowWindow(strUser);
			win.Show();
		}

		//TODO
		//옵션 폼 만들고 체크
		private void contextClick_ClientMute(object sender, RoutedEventArgs e)
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;

			DataInstence.option.AddMuteClient(tweet.originalTweet.source);
			FileManager.FileInstence.UpdateOption(DataInstence.option);
			MessageBox.Show($"클라이언트 {tweet.originalTweet.source} 가 뮤트에 추가되었습니다.", "알림", MessageBoxButton.OK,
				MessageBoxImage.Information);
		}

		private void contextClick_TweetMute(object sender, RoutedEventArgs e)
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;

			DataInstence.option.AddMuteTweet(tweet);
			FileManager.FileInstence.UpdateOption(DataInstence.option);
			MessageBox.Show($"트윗 '{tweet.originalTweet.text}' 가 뮤트에 추가되었습니다.\r\n해당 트윗과 해당 트윗에 온 답글을 표시하지 않습니다.", "알림", MessageBoxButton.OK,
				MessageBoxImage.Information);
		}

		private void contextClick_Hashtag(object sender, RoutedEventArgs e)
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;
			MenuItem item = sender as MenuItem;
			if (item == null) return;
			string strHash = item.Header.ToString().Replace("__", "_");
			inputTweetBox.Text += $" #{strHash}";
		}

		private void contextClick_ViewWeb(object sender, RoutedEventArgs e)
		{
			ShowWeb();
		}

		private void contextClick_Retweet(object sender, RoutedEventArgs e)
		{
			Retweet();
		}

		private void contextClick_Qt(object sender, RoutedEventArgs e)
		{
			QTRetweet();
		}

		private void contextClick_DM(object sender, RoutedEventArgs e)
		{
			AddIdDm();
		}

		private void contextClick_Favorite(object sender, RoutedEventArgs e)
		{
			AddFavorite();
		}

		private void contextClick_TweetCopy(object sender, RoutedEventArgs e)
		{
			TweetCopy();
		}

		private void contextClick_DMCopy(object sender, RoutedEventArgs e)
		{
			DMCopy();
		}

		private void contextClick_TweetDelete(object sender, RoutedEventArgs e)
		{
			DeleteTweet();
		}

		//-------------------------------------------------------------------------------------------
		//---------------------------------메뉴 클릭----------------------------------------
		//-------------------------------------------------------------------------------------------

		private void following_Click(object sender, RoutedEventArgs e)
		{
			FollowWindow win = new FollowWindow(true);
			win.Show();
		}

		private void follower_Click(object sender, RoutedEventArgs e)
		{
			FollowWindow win = new FollowWindow(false);
			win.Show();
		}

		private void help_Click(object sender, RoutedEventArgs e)
		{
			HelpWindow help = new WindowAndControl.HelpWindow();
			help.Owner = this;
			help.ShowDialog();
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			SendTweet();
		}

		private void click_MoreButton(object sender, RoutedEventArgs e)
		{
			if (dicPanel[selectPanel].treeView.Items.Count < 2)//2개 이상일 경우 로딩된 트윗이 있는 거
				return;

			if (selectPanel == eTweetPanel.eUser|| selectPanel== eTweetPanel.eUserMedia)
			{
				ClientTweet tweet = dicPanel[selectPanel].treeView.Items[dicPanel[selectPanel].treeView.Items.Count - 2] as ClientTweet;
				DalsaeInstence.LoadTweetMore(selectPanel, tweet.id, tweet.user.screen_name);
			}
			else
			{
				ClientTweet tweet = dicPanel[selectPanel].treeView.Items[dicPanel[selectPanel].treeView.Items.Count - 2] as ClientTweet;
				DalsaeInstence.LoadTweetMore(selectPanel, tweet.id);
			}
		}

		private void favTool_Click(object sender, RoutedEventArgs e)
		{
			FavoriteToolWindow win = new FavoriteToolWindow();
			win.Show();
		}

		private void menuItemAddAccount_Click(object sender, RoutedEventArgs e)
		{
			DalsaeInstence.AddAccount();
		}

		private void menuItemDeleteAccount_Click(object sender, RoutedEventArgs e)
		{
			DeleteAccount();
			//DalsaeInstence.DeleteNowAccount();
		}

		private void menuItemChangeAccount_Click(object sender, RoutedEventArgs e)
		{
			MenuItem item = sender as MenuItem;
			if (item == null) return;
			DalsaeInstence.ChangeAccount(item.Name);
		}
		private void showPanel_Click(object sender, RoutedEventArgs e)
		{
			MenuItem item = sender as MenuItem;
			if (item == null) return;
			switch (item.Header.ToString())
			{
				case "홈":
					ShowPanel(eTweetPanel.eHome);
					break;
				case "알림":
					ShowPanel(eTweetPanel.eMention);
					break;
				case "쪽지":
					ShowPanel(eTweetPanel.eDm);
					break;
				case "관심글":
					ShowPanel(eTweetPanel.eFavorite);
					break;
				case "최근 연 링크":
					ShowPanel(eTweetPanel.eOpen);
					break;
			}
		}

		public void Retweet(ClientTweet tweet)
		{
			if (tweet.user.Protected && DataInstence.CheckIsMe(tweet.user.id) == false
				&& DataInstence.option.isRetweetProtectUser == false)
			{
				DalsaeInstence.ShowMessageBox("잠금 계정의 트윗은 리트윗 할 수 없습니다.\r", "알림");
				return;
			}

			if (tweet.originalTweet.retweeted == false)
			{
				MessageBoxResult mr = DalsaeInstence.ShowMessageBox("선택한 트윗을 리트윗 하시겠습니까?", "리트윗 확인", MessageBoxButton.YesNo);
				if (mr != MessageBoxResult.Yes) return;
			}
			if (tweet.user.Protected && tweet.isRetweet == false && DataInstence.CheckIsMe(tweet.user.id) == false)
				DalsaeInstence.RetweetProtect(tweet);
			else
				DalsaeInstence.Retweet(tweet.originalTweet.id, !tweet.originalTweet.retweeted);
		}

		private void searchUser_Click(object sender, RoutedEventArgs e)
		{
			FindWindow win = new FindWindow(this);
			win.ShowDialog();
		}

		private void click_Hotkey(object sender, RoutedEventArgs e)
		{
			HotkeyWindow win = new WindowAndControl.HotkeyWindow(DataInstence.hotKey);
			win.Owner = this;
			win.ShowDialog();
		}

		private void click_Option(object sender, RoutedEventArgs e)
		{
			WindowAndControl.OptionWIndow window = new OptionWIndow();
			window.Owner = this;
			window.ShowDialog();
		}

		private void made_Click(object sender, RoutedEventArgs e)
		{
			MaidWindow win = new MaidWindow(this);
			win.ShowDialog();
		}

		private void menuItemBlock_Click(object sender, RoutedEventArgs e)
		{
			Manager.APICallAgent.apiInstence.GetBlockids();
			//ThreadPool.QueueUserWorkItem(DalsaeInstence.GetBlockIds, null);
		}

		private void menuItemFollow_Click(object sender, RoutedEventArgs e)
		{
			//ThreadPool.QueueUserWorkItem(DalsaeInstence.GetFollowList, null);
		}

		private void bottomGrid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			FrameworkElement item = e.OriginalSource as FrameworkElement;
			if (item == null) return;
			switch (item.Name)
			{
				case "imageHome":
				case "bottomLabelHome":
					ShowPanel(eTweetPanel.eHome);
					break;
				case "imageMention":
				case "bottomLabelMention":
					ShowPanel(eTweetPanel.eMention);
					break;
				case "imageDm":
				case "bottomLabelDM":
					ShowPanel(eTweetPanel.eDm);
					break;
				case "imageFav":
				case "bottomLabelFav":
					ShowPanel(eTweetPanel.eFavorite);
					break;
				case "imageOpen":
				case "bottomLabelOpen":
					ShowPanel(eTweetPanel.eOpen);
					break;
			}
		}
		//-------------------------------------------------------------------------------------------
		//---------------------------------이미지 추가/전송----------------------------------------
		//-------------------------------------------------------------------------------------------
		private void imageGrid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Image img = sender as Image;
			if (img == null) return;
			string name = img.Name;

			if (e.LeftButton == MouseButtonState.Pressed)
			{
				if (previewImage.Source == img.Source)//숨기기
				{
					ClearPreviewImage();
				}
				else//띄우기
				{
					previewImage.Visibility = Visibility.Visible;
					previewImage.Source = img.Source;
					gridBigPreview.Visibility = Visibility.Visible;
					//미리보기 띄운 후 크기 조절
					//???
					//if (previewImage.Source.Width > listBoxHome.ActualWidth ||
					//	previewImage.Source.Height > listBoxHome.ActualHeight)
					//	previewImage.Stretch = Stretch.Uniform;
					//else
					//	previewImage.Stretch = Stretch.None;
				}
				
			}
			else if(e.RightButton== MouseButtonState.Pressed)
			{
				BitmapImage image = img.Source as BitmapImage;
				if (image == null) return;
				ClearPreviewImage();
				listBitmapImage.Remove(image);
				UpdateImage();
			}
		}

		private void previewImage_MouseDown(object sender, MouseButtonEventArgs e)
		{
			ClearPreviewImage();
		}

		private void ClearPreviewImage()
		{
			previewImage.Source = null;
			previewImage.Visibility = Visibility.Collapsed;
			gridBigPreview.Visibility = Visibility.Collapsed;
		}


		private void mainWindow_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;

			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
			List<string> addedFile = new List<string>();
			foreach (string file in files)
			{
				string fileText = System.IO.Path.GetExtension(file);
				if (fileText.Equals(".png", StringComparison.CurrentCultureIgnoreCase) ||
					fileText.Equals(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
					fileText.Equals(".jpeg", StringComparison.CurrentCultureIgnoreCase))
				{
					//if (CheckFileSize(file))
					//	ShowMessage("크기가 5MB 이상인 이미지는 등록 불가능합니다.", "오류",
					//			MessageBoxButton.OK, MessageBoxImage.Warning);
					//else
						addedFile.Add(file);//메모리에 올리면서 파일 체크하게 수정
				}
			}
			if (addedFile.Count + listBitmapImage.Count <= 4)
			{
				AddFile(addedFile);
			}
		}

		private void AddFile(List<string> listFile)
		{
			for (int i = 0; i < listFile.Count; i++)
			{
				using (FileStream stream = File.OpenRead(listFile[i]))
				{
					BitmapImage bitmap = new BitmapImage();
					bitmap.BeginInit();
					bitmap.CacheOption = BitmapCacheOption.OnLoad;
					bitmap.StreamSource = stream;
					bitmap.EndInit();
					bool isLarge = CheckFileSize(stream);

					if(isLarge)
					{
						ShowMessage("크기가 5MB 이상인 이미지는 등록 불가능합니다.", "오류",
								MessageBoxButton.OK, MessageBoxImage.Warning);
					}
					else
						listBitmapImage.Add(bitmap);
				}
			}
			UpdateImage();
		}

		private void UpdateImage()
		{
			for (int i = 0; i < listBitmapImage.Count; i++)
				listImage[i].Source = listBitmapImage[i];
			if (listBitmapImage.Count < 4)
				for (int i = listBitmapImage.Count; i < 4; i++)
					listImage[i].Source = null;
			if (listBitmapImage.Count == 0)
				imageGrid.Visibility = Visibility.Collapsed;
			else
				imageGrid.Visibility = Visibility.Visible;
		}

		private bool CheckFileSize(string path)
		{
			bool isLarge = false;
			FileInfo fi = new FileInfo(path);
			if (fi.Length > 5242880)
				isLarge = true;

			return isLarge;
		}

		private bool CheckFileSize(Stream stream)
		{
			if (stream.Length > 5242880)
				return true;
			else
				return false;
		}

		//private bool CheckFileSize(FileStream stream)
		//{
		//	if (stream.Length > 5242880)
		//		return true;
		//	else
		//		return false;
		//}

		private void imageAdd_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Multiselect = true;
			ofd.Title = "열기";
			ofd.Filter = "이미지 파일(*.jpg, *jpeg, *png)|*.jpg; *.jpeg; *.png; | 모든 파일(*.*) | *.*";

			if (ofd.ShowDialog() == true)
			{
				for (int i = 0; i < ofd.SafeFileNames.Length; i++)
				{
					string fileText = System.IO.Path.GetExtension(ofd.SafeFileNames[i]);
					if (fileText.Equals(".png", StringComparison.CurrentCultureIgnoreCase) == false &&
							fileText.Equals(".jpg", StringComparison.CurrentCultureIgnoreCase) == false &&
							fileText.Equals(".jpeg", StringComparison.CurrentCultureIgnoreCase) == false)
					{
						ShowMessage("이미지 파일을 선택 해주세요", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
						return;
					}
				}

				if (ofd.SafeFileNames.Length > 4)
				{
					ShowMessage("최대 4개까지 선택 가능", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				if (listBitmapImage.Count + ofd.SafeFileNames.Length > 4)
				{
					ShowMessage("최대 4개까지 선택 가능", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
			}
			List<string> listAddFile = new List<string>();
			foreach (string path in ofd.FileNames)
			{
				//if (CheckFileSize(path))
				//	ShowMessage("크기가 5MB 이상인 이미지는 등록 불가능합니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
				//else
					listAddFile.Add(path);//메모리에 올리면서 파일 용량 체크하게 변경
			}

			AddFile(listAddFile);
		}



		private void gifAdd_Click(object sender, RoutedEventArgs e)
		{
			if (listBitmapImage.Count > 0)
			{
				ShowMessage("이미지와 gif는 동시에 보낼 수 없습니다.", "오류");
				return;
			}
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Title = "열기";
			ofd.Filter = "gif 파일(*.gif)|*.gif;";

			if (ofd.ShowDialog() == true)
			{
				string fileText = System.IO.Path.GetExtension(ofd.SafeFileName);
				if (fileText.Equals(".gif", StringComparison.CurrentCultureIgnoreCase))
				{
					FileInfo fi = new FileInfo(ofd.FileName);
					if (fi.Length > 15728640)
					{
						ShowMessage("크기가 15MB 이상인 gif는 등록 불가능합니다.\r공식 홈페이지를 이용 바랍니다.", "오류");
						return;
					}
					isAddedGif = true;
					pathGif = ofd.FileName;
					listImage[0].Source = bitmapGif;
					imageGrid.Visibility = Visibility.Visible;
				}
			}
		}


		//-------------------------------------------------------------------------------------------
		//---------------------------------이미지 미리보기용--------------------------------------
		//-------------------------------------------------------------------------------------------
		private void Image_MouseEnter(object sender, MouseEventArgs e)
		{
			Grid grid = sender as Grid;
			if (grid == null) return;
			ClientTweet tweet = grid.DataContext as ClientTweet;
			if (tweet == null) return;

			Image i1 = Generate.FindElementByName<Image>(grid, "image1");
			Image i2 = Generate.FindElementByName<Image>(grid, "image2");
			Image i3 = Generate.FindElementByName<Image>(grid, "image3");
			Image i4 = Generate.FindElementByName<Image>(grid, "image4");

			imagePreview1.Source = i1?.Source;
			imagePreview2.Source = i2?.Source;
			imagePreview3.Source = i3?.Source;
			imagePreview4.Source = i4?.Source;
			gridPreview.Width = (tweet.mediaEntities.media.Count * 100) + (5 * tweet.mediaEntities.media.Count) + 5;

			MoveGridPreview(grid);

			gridPreview.Visibility = Visibility.Visible;
		}

		private void Image_MouseLeave(object sender, MouseEventArgs e)
		{
			gridPreview.Visibility = Visibility.Hidden;

			Grid grid = sender as Grid;

			imagePreview1.Source = null;
			imagePreview2.Source = null;
			imagePreview3.Source = null;
			imagePreview4.Source = null;
		}

		private void Grid_MouseMove(object sender, MouseEventArgs e)
		{
			Grid grid = sender as Grid;
			MoveGridPreview(grid);
		}

		private void MoveGridPreview(Grid grid)
		{
			if (grid == null) return;
			Point point = grid.TransformToAncestor(selectedTreeView).Transform(new Point(0, 0));
			gridPreview.Margin = new Thickness(point.X - gridPreview.Width - 19, point.Y, 125, 0);
		}

		//-------------------------------------------------------------------------------------------
		//---------------------------------딜리게이트 호출용-----------------------------------
		//-------------------------------------------------------------------------------------------
		public void Reply(ClientTweet tweet)
		{
			if (tweet == null) return;
			if (tweet.originalTweet == null) return;

			inputTweetBox.Text = $"@{tweet.user.screen_name} ";
			inputTweetBox.SelectionStart = inputTweetBox.Text.Length;
			replyTweet = tweet;
			EnterInputTweet();
		}

		public void ReplyAll(ClientTweet tweet)
		{
			if (tweet == null) return;
			if (tweet.originalTweet == null) return;

			StringBuilder sb = new StringBuilder();
			sb.Append($"@{tweet.user.screen_name} ");
			if (tweet.retweeted)
				sb.Append($"@{tweet.originalTweet.user.screen_name} ");
			foreach (string name in tweet.hashMention)
			{
				if (DataInstence.CheckIsMe(name) == false)
					sb.Append($"@{name} ");
			}
			inputTweetBox.Text = sb.ToString();
			inputTweetBox.SelectionStart = inputTweetBox.Text.Length;
			replyTweet = tweet;
			EnterInputTweet();
		}

		public void QTRetweet(ClientTweet tweet)
		{
			if (tweet == null) return;
			if (tweet.originalTweet == null) return;

			string url = $"https://twitter.com/{tweet.originalTweet.user.screen_name}/status/{tweet.originalTweet.id}";
			inputTweetBox.Text = url;
			inputTweetBox.SelectionStart = 0;
			EnterInputTweet();
		}

		public void ShowPanel_ByDele(eTweetPanel panel)
		{
			ShowPanel(panel);
		}
		//-------------------------------------------------------------------------------------------
		//---------------------------------각종 공용 함수------------------------------------------
		//-------------------------------------------------------------------------------------------

		private void ShowPanel(eTweetPanel panel)
		{
			dicPanel[selectPanel].HideTreeView();
			dicPanel[panel].ShowTreeView();
			TweetInstence.ClearTweet(selectPanel);
			selectPanel = panel;
			if (panel == eTweetPanel.eMention)
				notiMention.isOn = false;
			else if (panel == eTweetPanel.eDm)
				notiDm.isOn = false;
			//패널 띄웠을 때 로딩된 글이 없을 경우 API콜
			if (selectPanel == eTweetPanel.eDm && dicPanel[selectPanel].treeView.Items.Count < 2)
				DalsaeInstence.LoadTweet(eTweetPanel.eDm);
			else if (selectPanel == eTweetPanel.eFavorite && dicPanel[selectPanel].treeView.Items.Count < 2)
				DalsaeInstence.LoadTweet(eTweetPanel.eFavorite);
			else if (selectPanel == eTweetPanel.eMention && dicPanel[selectPanel].treeView.Items.Count < 2)
				DalsaeInstence.LoadTweet(eTweetPanel.eMention);
			dicPanel[selectPanel].Focus();
		}

		private void AddIdDm()
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;
			if (tweet.originalTweet == null) return;

			inputTweetBox.Text = $"d @{tweet.user.screen_name} ";
			inputTweetBox.SelectionStart = inputTweetBox.Text.Length;
			EnterInputTweet();
		}
		private void AddFavorite()
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;
			if (tweet.originalTweet == null) return;

			DalsaeInstence.Favorite(tweet.originalTweet.id, !tweet.originalTweet.favorited);
		}

		private void EnterInputTweet()
		{
			inputTweetBox.Focus();
		}


		private void DMCopy()
		{
			ClientDirectMessage clientdm = treeDM.SelectedItem as ClientDirectMessage;
			if (clientdm == null) return;

			string copy = string.Empty;
			copy = clientdm.text;
			if (clientdm.entities.urls.Count != 0)
				for (int i = 0; i < clientdm.entities.urls.Count; i++)
					copy = copy.Replace(clientdm.entities.urls[i].display_url, clientdm.entities.urls[i].expanded_url);

			if (copy != string.Empty)
			{
				copy = copy.Replace("\n", Environment.NewLine);
				Clipboard.SetText(copy);
				ShowMessage("쪽지 내용이 클립보드에 복사되었습니다.", "알림");
			}
		}

		private void TweetCopy()
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;
			if (tweet.originalTweet == null) return;

			string copy = Generate.ReplaceTextExpend(tweet);
			copy = copy.Replace("\n", Environment.NewLine);
			Clipboard.SetText(copy);

			ShowMessage("트윗이 클립보드에 복사 되었습니다.", "알림");
		}

		private void ReplyDM()
		{
			ClientDirectMessage dm = dicPanel[eTweetPanel.eDm].treeView.SelectedItem as ClientDirectMessage;
			if (dm == null) return;

			if (DataInstence.CheckIsMe(dm.sender_screen_name))
				inputTweetBox.Text = $"d @{dm.recipient_screen_name} ";
			else
				inputTweetBox.Text = $"d @{dm.sender_screen_name} ";
			inputTweetBox.SelectionStart = inputTweetBox.Text.Length;
			EnterInputTweet();
		}

		private void Reply()
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;
			if (tweet.originalTweet == null || tweet.user == null) return;

			inputTweetBox.Text = $"@{tweet.originalTweet.user.screen_name} ";
			inputTweetBox.SelectionStart = inputTweetBox.Text.Length;
			replyTweet = tweet;
			EnterInputTweet();
		}
		private void ReplyAll()
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;
			if (tweet.originalTweet == null || tweet.user == null) return;

			StringBuilder sb = new StringBuilder();
			sb.Append($"@{tweet.user.screen_name} ");
			if (tweet.retweeted)
				sb.Append($"@{tweet.originalTweet.user.screen_name} ");
			foreach (string name in tweet.hashMention)
			{
				if (DataInstence.CheckIsMe(name) == false)
					sb.Append($"@{name} ");
			}
			inputTweetBox.Text = sb.ToString();
			inputTweetBox.SelectionStart = inputTweetBox.Text.Length;
			replyTweet = tweet;
			EnterInputTweet();
		}

		private void LoadDeahwa()
		{
			if (selectTweet == null) return;
			if (selectTweet.originalTweet == null) return;
			DalsaeInstence.LoadSingleTweet(selectTweet);
			//if (string.IsNullOrEmpty(selectTweet.originalTweet.in_reply_to_status_id_str) == false)
			//	DalsaeInstence.LoadDeahwa(selectTweet);
			//DalsaeInstence.LoadSingleTweet(selectTweet);
			//if (selectTweet.isQTRetweet)
			//	ThreadPool.QueueUserWorkItem(new WaitCallback(LoadQTRetweet), new object[] { selectTweet, selectPanel });
		}

		private void ShowWeb()
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;
			if (tweet.originalTweet == null) return;

			string url = $"https://twitter.com/{tweet.originalTweet.user.screen_name}/status/{tweet.originalTweet.id}";
			System.Diagnostics.Process.Start(url);
			TweetInstence.AddTweet(eTweetPanel.eOpen, tweet);
		}

		private void Retweet()
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;
			if (tweet.originalTweet == null) return;

			Retweet(tweet);
			FocusPanel();
		}

		private void DeleteTweet()
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;
			if (tweet.originalTweet == null) return;

			if (tweet.user == null) return;
			if (tweet.originalTweet.user.id != DataInstence.userInfo.user.id) return;//자기 트윗 아니면 종료
			if (tweet.uiProperty.isDeleteTweet) return;//이미 삭제된 트윗이면 종료

			MessageBoxResult mbr = MessageBox.Show("트윗을 삭제 하시겠습니까?", "삭제", MessageBoxButton.YesNo, MessageBoxImage.None);
			if (mbr == MessageBoxResult.Yes)
				DalsaeInstence.TweetDelete(tweet.originalTweet.id);
			FocusPanel();
		}

		private void AddHashTag()
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;
			if (tweet.originalTweet == null) return;

			if (tweet.lastEntities.hashtags.Count == 0) return;

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < tweet.lastEntities.hashtags.Count; i++)
				sb.Append($" #{tweet.lastEntities.hashtags[i].text} ");
			inputTweetBox.Text = sb.ToString();
			inputTweetBox.SelectionStart = 0;
			EnterInputTweet();

		}
		private void QTRetweet()
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;
			if (tweet.user == null || tweet.originalTweet == null) return;

			string url = $"https://twitter.com/{tweet.originalTweet.user.screen_name}/status/{tweet.originalTweet.id}";
			inputTweetBox.Text = url;
			inputTweetBox.SelectionStart = 0;
			EnterInputTweet();
		}
		private void OpenImageDM()
		{
			ClientDirectMessage dm = dicPanel[eTweetPanel.eDm].treeView.SelectedItem as ClientDirectMessage;
			if (dm == null) return;
			if (dm.entities.media.Count == 0) return;
			System.Diagnostics.Process.Start(dm.entities.media[0].expanded_url);
		}

		private void preview_MouseDown(object sender, MouseButtonEventArgs e)
		{
			OpenImage();
		}

		private void textBoxFind_TextChanged(object sender, TextChangedEventArgs e)
		{

		}

		private void labelClose_MouseDown(object sender, MouseButtonEventArgs e)
		{
			SetFindGridVisibility(false);
		}


		private void FindTweet(string text)
		{
			TreeViewManager panel = dicPanel[selectPanel];

			int index = TweetInstence.FindTweet(selectPanel, panel.treeView.SelectedItem as ClientTweet, text);

			if (index == -1) return;

			TreeViewItem selTweet = panel.treeView.ItemContainerGenerator.ContainerFromIndex(index) as TreeViewItem;
			//panel.listBox.Items.MoveCurrentTo(selTweet);
			selTweet.IsSelected = true;
			selTweet.Focus();
			textBoxFind.Focus();
		}

		private void SetFindGridVisibility(bool isVisible)
		{
			if (isVisible)
			{
				gridFind.Visibility = Visibility.Visible;
				textBoxFind.Text = string.Empty;
				textBoxFind.Focus();
			}
			else
			{
				gridFind.Visibility = Visibility.Collapsed;
				EnterInputTweet();
			}
		}

		private void OpenImage()
		{
			ClientTweet tweet = selectTweet;
			if (tweet == null) return;
			if (tweet.originalTweet == null) return;

			if (tweet.isMedia == false) return;
			string url = string.Empty;
			foreach (ClientMedia item in tweet.dicPhoto.Values)
			{
				if (item.type == "photo")
				{
					url = item.display_url;
					ImageWindow win = new ImageWindow(tweet, url);
					win.Show();
					break;
				}
			}

			for (int i = 0; i < tweet.mediaEntities.media.Count; i++)
			{
				if (tweet.mediaEntities.media[i].type == "animated_gif" || tweet.mediaEntities.media[i].type == "video")
				{
					VideoWindow win = new VideoWindow(tweet);
					win.Show();
					break;
				}
			}
		}
	}//class
}
