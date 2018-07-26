using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;
using static Dalsae.FileManager;
using static Dalsae.DalsaeManager;
using static Dalsae.TwitterWeb;
using static Dalsae.DataManager;
using static Dalsae.TweetManager;
using Dalsae.API;

namespace Dalsae.WindowAndControl
{
	/// <summary>
	/// FavoriteToolWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class FavoriteToolWindow : Window
	{
		private int countFav = 0;
		private int index = -1;
		private List<ClientTweet> listTweet = new List<ClientTweet>();
		private delegate void DeleChangeText(string text);
		private delegate void DeleLoadEnd();
		private List<BitmapImage> listBitmapImage = new List<BitmapImage>();
		private List<ImageAndProgress> listImageProgress = new List<ImageAndProgress>();
		public FavoriteToolWindow()
		{
			InitializeComponent();
			listImageProgress.Add(new ImageAndProgress(image0, progbar0));
			listImageProgress.Add(new ImageAndProgress(image1, progbar1));
			listImageProgress.Add(new ImageAndProgress(image2, progbar2));
			listImageProgress.Add(new ImageAndProgress(image3, progbar3));
			textState.Text = "";
			textNowFavCount.Text = "";
			textFavCount.Text = "";
			ClearImagGrid();
			gridTweet.Visibility = Visibility.Hidden;
			gridBottom.Visibility = Visibility.Hidden;
			Left = 100;
			Top = 100;
		}

		private enum eState
		{
			eIdle,
			eLoadMyInfo,
			eLoadFavorite,
			eLoadEnd,
		}
		private eState state;
		private void NextState()
		{
			state++;
			switch (state)
			{
				case eState.eIdle:
					break;
				case eState.eLoadMyInfo:
					ThreadPool.QueueUserWorkItem(LoadMyInfo);
					break;
				case eState.eLoadFavorite:
					ThreadPool.QueueUserWorkItem(LoadFavoriteTweet);
					break;
				case eState.eLoadEnd:
					DeleLoadEnd dele = new DeleLoadEnd(LoadEnd);
					Application.Current.Dispatcher.BeginInvoke(dele);
					break;
				default:
					break;
			}
		}

		private void LoadMyInfo(object obj)
		{
			DeleChangeText dele = new DeleChangeText(StateTextChange);
			Application.Current.Dispatcher.BeginInvoke(dele, new object[] { "정보 불러오는 중" });

			PacketVerifyCredentials parameter = new PacketVerifyCredentials();
			string json = WebInstence.SyncRequest(parameter);
			User user = JsonConvert.DeserializeObject<User>(json);
			if (user == null)
			{
				Application.Current.Dispatcher.BeginInvoke(dele, new object[] { "정보 불러오기 실패" });
				return;
			}
			countFav = user.favourites_count;
			Application.Current.Dispatcher.BeginInvoke(dele, new object[] { "정보 불러오기 완료!" });
			NextState();
		}

		private void LoadFavoriteTweet(object obj)
		{
			PacketFavoritesList parameter = new PacketFavoritesList();
			parameter.count = 200.ToString();
			int loadFavCount = 0;
			DeleChangeText dele = new DeleChangeText(StateTextChange);
			for (int j = 0; j < 15; j++)
			{
				string json = WebInstence.SyncRequest(parameter);
				if (json.Length == 0)
				{
					MessageBox.Show("불러오기 제한! 몇 분 뒤 다시 시도해주세요(최대 15분)", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
					break;
				}
				List<ClientTweet> listTemp = JsonConvert.DeserializeObject<List<ClientTweet>>(json);
				loadFavCount += listTemp.Count;
				Application.Current.Dispatcher.BeginInvoke(dele, new object[] { $"불러오는중: {loadFavCount}/{countFav}" });
				for (int i = 0; i < listTemp.Count; i++)
				{
					listTemp[i].Init();
					if (listTemp[i].isMedia)
					{
						if (listTemp[i].dicPhoto.Count == 1)//media가 있는데 photo일 경우에만 저장
							foreach (ClientMedia item in listTemp[i].dicPhoto.Values)
							{
								if (item.type == "photo")
									listTweet.Add(listTemp[i]);
								break;
							}
						else
							listTweet.Add(listTemp[i]);
					}
				}
				if (loadFavCount >= countFav) break;
				parameter.max_id = listTemp[listTemp.Count - 1].id;
			}
			Application.Current.Dispatcher.BeginInvoke(dele, new object[] { "불러오기 완료" });
			NextState();
		}

		private void LoadEnd()
		{
			SetNextFav();
			textNowFavCount.Text = $"이미지 관심글: {index + 1} / {listTweet.Count}";
			textFavCount.Text = $"관심글 수: {countFav}";
			gridTweet.Visibility = Visibility.Visible;
			gridBottom.Visibility = Visibility.Visible;
		}

		private void StateTextChange(string text)
		{
			textState.Text = text;
		}

		private void SetNextFav()
		{
			index++;
			if (index >= listTweet.Count)
			{
				index--;
				return;
			}
			gridTweet.DataContext = listTweet[index];
			SetImage();
			SetIndexText();
		}

		private void SetPrevFav()
		{
			index--;
			if (index < 0)
			{
				index = 0;
				return;
			}
			gridTweet.DataContext = listTweet[index];
			SetImage();
			SetIndexText();
		}

		private void ClearImagGrid()
		{
			for (int i = 0; i < listImageProgress.Count; i++)
				listImageProgress[i].Clear();
			listBitmapImage.Clear();
		}

		private void SetIndexText()
		{
			textNowFavCount.Text = $"이미지 관심글: {index + 1} / {listTweet.Count}";
		}

		private void SetImage()
		{
			ClearImagGrid();
			List<ClientMedia> listMedia = listTweet[index].mediaEntities.media;
			for (int i = 0; i < listMedia.Count; i++)
			{
				BitmapImage bmp = new BitmapImage();
				bmp.BeginInit();
				bmp.UriSource = new Uri($"{listMedia[i].media_url_https}:orig");
				bmp.EndInit();
				
				listImageProgress[i].SetBitmap(bmp);
				listImageProgress[i].image.Source = bmp;
				listBitmapImage.Add(bmp);
			}
		}

		//------------------------------------------------------------------------------------------------
		//--------------------------------------클릭 이벤트---------------------------------------------
		//------------------------------------------------------------------------------------------------
		private void buttonStart_Click(object sender, RoutedEventArgs e)
		{
			textInfo.Visibility = Visibility.Collapsed;
			buttonStart.Visibility = Visibility.Collapsed;
			NextState();
		}

		private void buttonFav_Click(object sender, RoutedEventArgs e)
		{
			Fav();
		}

		private void buttonUnFav_Click(object sender, RoutedEventArgs e)
		{
			Unfav();
		}

		private void buttonSave_Click(object sender, RoutedEventArgs e)
		{
			Save();
		}

		private void buttonSaveUnFav_Click(object sender, RoutedEventArgs e)
		{
			SaveUnfav();
		}

		private void buttonPrev_Click(object sender, RoutedEventArgs e)
		{
			SetPrevFav();
		}

		private void buttonNext_Click(object sender, RoutedEventArgs e)
		{
			SetNextFav();
		}
		private void button_Click(object sender, RoutedEventArgs e)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("← 이전 트윗"); sb.AppendLine();
			sb.Append("→ 다음 트윗"); sb.AppendLine();
			sb.Append("1 관심글 재등록"); sb.AppendLine();
			sb.Append("2 관심글 해제"); sb.AppendLine();
			sb.Append("3 저장"); sb.AppendLine();
			sb.Append("4 저장 후 관심글 해제");

			MessageBox.Show(sb.ToString(), "이미지 관심글 도구 단축키", MessageBoxButton.OK, MessageBoxImage.Information);
		}
		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			switch(e.Key)
			{
				case Key.Left:
					SetPrevFav();
					e.Handled = true;
					break;
				case Key.Right:
					SetNextFav();
					e.Handled = true;
					break;
				case Key.D1:
					Fav();
					e.Handled = true;
					break;
				case Key.D2:
					Unfav();
					e.Handled = true;
					break;
				case Key.D3:
					Save();
					e.Handled = true;
					break;
				case Key.D4:
					SaveUnfav();
					e.Handled = true;
					break;
			}
		}

		//------------------------------------------------------------------------------------------------
		//--------------------------------------기능 함수------------------------------------------------
		//------------------------------------------------------------------------------------------------
		private void SaveUnfav()
		{
			for (int i = 0; i < listBitmapImage.Count; i++)
			{
				if (listBitmapImage[i].IsDownloading)
				{
					MessageBox.Show("이미지를 로딩 중입니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
			}

			for (int i = 0; i < listBitmapImage.Count; i++)
				FileInstence.SaveImage(listTweet[index].mediaEntities.media[i].media_url_https, listBitmapImage[i]);
			//MessageBox.Show("모든 이미지를 저장했습니다.", "알림", MessageBoxButton.OK);

			PacketFavorites_Destroy parameter = new PacketFavorites_Destroy();
			parameter.id = listTweet[index].id;
			string json = WebInstence.SyncRequest(parameter);
			if (json.Length > 50)
			{
				listTweet[index].favorited = false;
				MessageBox.Show("저장, 관심글 해제 성공", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
			}

		}

		private void Save()
		{
			for (int i = 0; i < listBitmapImage.Count; i++)
			{
				if (listBitmapImage[i].IsDownloading)
				{
					MessageBox.Show("이미지를 로딩 중입니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
			}

			for (int i = 0; i < listBitmapImage.Count; i++)
				FileInstence.SaveImage(listTweet[index].mediaEntities.media[i].media_url_https, listBitmapImage[i]);
			MessageBox.Show("모든 이미지를 저장했습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void Unfav()
		{
			PacketFavorites_Destroy parameter = new PacketFavorites_Destroy();
			parameter.id = listTweet[index].id;
			string json = WebInstence.SyncRequest(parameter);
			if (json.Length > 50)
			{
				listTweet[index].favorited = false;
				MessageBox.Show("관심글 해제 성공", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		private void Fav()
		{
			PacketFavorites_Create parameter = new PacketFavorites_Create();
			parameter.id = listTweet[index].id;
			string json = WebInstence.SyncRequest(parameter);
			if (json.Length > 50)
			{
				MessageBox.Show("관심글 등록 완료", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
				listTweet[index].favorited = true;
			}
		}

	

		//------------------------------------------------------------------------------------------------
		//--------------------------------------편하게 하는 클래스-------------------------------------
		//------------------------------------------------------------------------------------------------
		private class ImageAndProgress
		{
			public Image image { get; set; }
			private ProgressBar progressBar;
			public ImageAndProgress(Image image, ProgressBar progress)
			{
				this.progressBar = progress;
				this.image = image;
			}
			public void SetBitmap(BitmapImage bitmap)//bitmap만 넣어주면 이벤트 알아서 연결
			{
				progressBar.Value = 0;
				bitmap.DownloadProgress += bitmap_DownloadProgress;
				bitmap.DownloadFailed += Bmp_DownloadFailed;
				bitmap.DownloadCompleted += bitmap_LoadEnd;
				if (bitmap.IsDownloading)//GC에 회수되기 전이면 다운로드 시작을 안 한다
					Show();
			}
			public void Clear()
			{
				image.Source = null;
				progressBar.Visibility = Visibility.Collapsed;
			}
			private void Show()
			{
				progressBar.Visibility = Visibility.Visible;
			}
			private void bitmap_DownloadProgress(object sender, DownloadProgressEventArgs e)
			{
				progressBar.Value = e.Progress;
			}
			private void Bmp_DownloadFailed(object sender, ExceptionEventArgs e)
			{
				progressBar.Visibility = Visibility.Collapsed;
			}
			private void bitmap_LoadEnd(object sender, EventArgs e)
			{
				progressBar.Visibility = Visibility.Collapsed;
			}
		}

		
	}
}
