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
using static Dalsae.FileManager;
using static Dalsae.DalsaeManager;
using static Dalsae.DataManager;
using static Dalsae.TweetManager;
using System.Net;
using System.IO;
using System.Diagnostics;
using Dalsae.API;
using Dalsae.Data;

namespace Dalsae.WindowAndControl
{
	/// <summary>
	/// ImageWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class ImageWindow : Window
	{
		//확대 중일 때 마우스 커서 바꾸기
		//확대
		private bool isSmallImage { get { return (mainImage.Source.Width < scrollViewer.ActualWidth && mainImage.Source.Height < scrollViewer.ActualHeight); } }
		private bool isLoadOriginal = false;
		private delegate void DeleSaveImage();
		private Point prevMousePoint;
		private delegate void DeleSetImage();
		private List<ClientMedia> listMedia = new List<ClientMedia>();
		private int index = 0;
		private List<Image> listImageControl = new List<Image>();
		private List<ImageProgress> listProgress = new List<ImageProgress>();
		private List<BitmapImage> listBitmapImage = new List<BitmapImage>();
		private bool isDM = false;
		ClientDirectMessage clientDM = null;
		private ImageProgress mainImageProgress = null;
		private ClientTweet tweet;
		private DeleSaveImage deleSave = null;
		public ImageWindow(ClientTweet tweet, string url)
		{
			InitializeComponent();
			listImageControl.Add(image0);
			listImageControl.Add(image1);
			listImageControl.Add(image2);
			listImageControl.Add(image3);

			listProgress.Add(new ImageProgress(progressBar0));
			listProgress.Add(new ImageProgress(progressBar1));
			listProgress.Add(new ImageProgress(progressBar2));
			listProgress.Add(new ImageProgress(progressBar3));
			mainImageProgress= new ImageProgress(mainProgress);

			grid.DataContext = tweet.originalTweet;
			this.tweet = tweet;
			if (DataInstence.option.isShowImageTweet)
			{
				grid.Visibility = Visibility.Visible;
			}
			if (DataInstence.option.isShowImageBottom == false)
			{
				bottomGrid.Visibility = Visibility.Collapsed;
				rowBottom.Height = new GridLength(0);
			}
			for (int i = 0; i < tweet.mediaEntities.media.Count; i++)
				if (tweet.mediaEntities.media[i].display_url == url && tweet.mediaEntities.media[i].type == "photo")
					listMedia.Add(tweet.mediaEntities.media[i]);

			LoadImage(DataInstence.option.isLoadOriginalImage);
		}

		private void LoadImage(bool isOriginal)
		{
			if (isLoadOriginal) return;
			isLoadOriginal = isOriginal;
			for (int i = 0; i < listMedia.Count; i++)
			{
				BitmapImage bmp = new BitmapImage();
				bmp.BeginInit();
				if (isOriginal)
					bmp.UriSource = new Uri($"{listMedia[i].media_url_https}:orig");
				else
					bmp.UriSource = new Uri($"{listMedia[i].media_url_https}");
				bmp.EndInit();
				if (i == index)
				{
					mainImage.Source = bmp;
					bmp.DownloadCompleted += Image_LoadEnd;
					mainImageProgress.SetBitmap(bmp);
				}
				listProgress[i].SetBitmap(bmp);
				listImageControl[i].Source = bmp;
				listBitmapImage.Add(bmp);
				bmp.DownloadCompleted += Bitmap_LoadEnd;
			}
			SetPosition();
		}

		private void contextOnOpening(object sender, ContextMenuEventArgs e)
		{
			Grid grid = sender as Grid;
			if (tweet == null || grid == null) return;
			grid.ContextMenu = CreateContextMenu(tweet);
		}

		private ContextMenu CreateContextMenu(ClientTweet tweet)
		{
			ContextMenu contextMenu = new ContextMenu();
			MenuItem mi = new MenuItem();
			Separator sp = new Separator();
			//------------------URL----------------------
			if (tweet.isUrl)
			{
				mi = new MenuItem();
				mi.Header = "URL";
				for (int i = 0; i < tweet.listUrl.Count; i++)
				{
					MenuItem url = new MenuItem();
					url.Header = tweet.listUrl[i].display_url;
					url.Click += contextClick_Url;
					mi.Items.Add(url);
				}
				contextMenu.Items.Add(mi);
				sp = new Separator();
				contextMenu.Items.Add(sp);
			}
			//----------------------------------------------
			mi = new MenuItem();
			mi.Header = "답글";
			mi.Click += contextClick_Reply;
			contextMenu.Items.Add(mi);

			mi = new MenuItem();
			mi.Header = "모두에게 답글";
			mi.Click += contextClick_ReplyAll;
			contextMenu.Items.Add(mi);

			//------------------------------------------------
			mi = new MenuItem();
			mi.Header = "웹에서 보기";
			mi.Click += contextClick_ViewWeb;
			contextMenu.Items.Add(mi);

			mi = new MenuItem();
			mi.Header = "리트윗(RT)";
			mi.Click += contextClick_Retweet;
			contextMenu.Items.Add(mi);

			mi = new MenuItem();
			mi.Header = "인용하기(QT)";
			mi.Click += contextClick_Qt;
			contextMenu.Items.Add(mi);

			mi = new MenuItem();
			mi.Header = "관심글";
			mi.Click += contextClick_Favorite;
			contextMenu.Items.Add(mi);

			sp = new Separator();
			contextMenu.Items.Add(sp);

			mi = new MenuItem();
			mi.Header = "트윗 복사";
			mi.Click += contextClick_TweetCopy;
			contextMenu.Items.Add(mi);

			return contextMenu;
		}

		//최초 이미지 설정 시 확대 안 되게 세팅하는 로드 엔드 함수
		private void Image_LoadEnd(object sender, EventArgs e)
		{
			BitmapImage image = sender as BitmapImage;
			if (image == null) return;
			ChangeFitSize();
		}

		//bitmap로드 완료 시 체크. 원본 로딩이 아닐 때 원본 로딩 후 저장 할 때 체크
		private void Bitmap_LoadEnd(object sender, EventArgs e)
		{
			for (int i = 0; i < listBitmapImage.Count; i++)
				if (listBitmapImage[i].IsDownloading) return;

			deleSave?.Invoke();
		}

		public ImageWindow(ClientDirectMessage dm, string url)
		{
			InitializeComponent();
			listImageControl.Add(image0);
			listImageControl.Add(image1);
			listImageControl.Add(image2);
			listImageControl.Add(image3);

			clientDM = dm;
			isDM = true;
			SetPosition();
		}

		private void AsyncLoadBitmap(IAsyncResult ar)
		{
			try
			{
				HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
				using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar))
				{
					using (Stream stream = response.GetResponseStream())
					{
						using (MemoryStream memoryStream = new MemoryStream())
						{
							stream.CopyTo(memoryStream);
							BitmapImage bmp = new BitmapImage();
							bmp.BeginInit();
							bmp.StreamSource = memoryStream;
							bmp.EndInit();
							listBitmapImage.Add(bmp);
						}
					}
				}
			}
			catch (Exception e) { }
		}

		private void SetDMImage()
		{
			mainImage.Source = listBitmapImage[0];
		}

		private void SetPosition()
		{
			Left = Properties.Settings.Default.ptImgX;
			Top = Properties.Settings.Default.ptImgY;

			Width = Properties.Settings.Default.ptImgWidth;
			Height = Properties.Settings.Default.ptImgHeight;
			Title = $"이미지 보기 ({index + 1} / {listMedia.Count})";
		}


		//--------------------------------------------------------------------
		//----------------------나중에 noty바꿀 그놈-----------------------
		//ImageSource _imageSource;
		//public ImageSource imageSource
		//{
		//	get { return imageSource; }
		//	set
		//	{
		//		_imageSource = value;
		//		//OnPropertyChanged("ImageSource");
		//	}
		//}

		private void imageWindow_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			Key key = e.Key;
			if (e.Key == Key.None || key == Key.ImeProcessed)
				key = e.ImeProcessedKey;
			switch (key)
			{
				case Key.Left:
					ChangeImage(index - 1);
					e.Handled = true;
					break;
				case Key.Right:
					ChangeImage(index + 1);
					e.Handled = true;
					break;
				case Key.Enter:
				case Key.Escape:
					e.Handled = true;
					Close();
					break;
				case Key.Q:
					ZoomOut();
					break;
				case Key.E:
					Zoom();
					break;
				case Key.A:
					if (Keyboard.Modifiers == ModifierKeys.Control)
						SaveImageAll();
					else
						MoveLeft();
					break;
				case Key.W:
					MoveUp();
					break;
				case Key.S:
					if (Keyboard.Modifiers == ModifierKeys.Control)
						SaveImage();
					else
						MoveDown();
					break;
				case Key.D:
					MoveRight();
					break;
				case Key.Space:
					ZoomOriginal();
					break;
				case Key.D1:
					if (listBitmapImage.Count > 0)
						ChangeImage(0);
					break;
				case Key.D2:
					if (listBitmapImage.Count > 1)
						ChangeImage(1);
					break;
				case Key.D3:
					if (listBitmapImage.Count > 2)
						ChangeImage(2);
					break;
				case Key.D4:
					if (listBitmapImage.Count > 3)
						ChangeImage(3);
					break;
				case Key.V:
					ShowMenu();
					break;
				case Key.R:
					LoadImage(true);
					break;
				case Key.C:
					if (ModifierKeys.Control == Keyboard.Modifiers)
					{
						Clipboard.SetImage(listBitmapImage[index]);
						MessageBox.Show(this, "현재 이미지가 클립보드에 저장 되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
					}
					break;
				case Key.F3:
					OpenFolder();
					break;
			}
			if (e.Key == Key.ImeProcessed && Key.A <= e.ImeProcessedKey && e.ImeProcessedKey <= Key.Z)
				e.Handled = true;
		}

		//이미지를 인덱스에 따라 바꿔주는 함수 키보드 조작과 마우스 클릭에서 호출
		//index: 바꿀 이미지 index
		private void ChangeImage(int changeIndex)
		{
			if (isDM) return;

			scrollViewer.ScrollToHorizontalOffset(0);
			scrollViewer.ScrollToVerticalOffset(0);

			if (changeIndex < 0)
				changeIndex = listBitmapImage.Count - 1;
			else if (changeIndex >= listBitmapImage.Count)
				changeIndex = 0;

			index = changeIndex;
			mainImage.Source = listImageControl[changeIndex].Source;
			ChangeFitSize();

			Title = $"이미지 보기 ({changeIndex + 1} / {listMedia.Count})";
		}

		private void saveImage_Click(object sender, RoutedEventArgs e)
		{
			SaveImage();
		}

		private void saveImageAll_Click(object sender, RoutedEventArgs e)
		{
			SaveImageAll();
		}
		
		private void SaveImage()
		{
			if (isLoadOriginal == false)
			{
				LoadImage(true);
				deleSave = new DeleSaveImage(SaveImage);
			}
			else
			{
				if (CheckDownloadingImage(index))
				{
					MessageBox.Show("이미지를 불러오는 중입니다.", "알림", MessageBoxButton.OK);
					return;
				}
				FileInstence.SaveImage(listMedia[index].media_url_https, listBitmapImage[index]);
				MessageBox.Show("이미지를 저장했습니다.", "알림", MessageBoxButton.OK);
				deleSave = null;
			}
		}

		private void SaveImageAll()
		{
			if (isLoadOriginal == false)
			{
				LoadImage(true);
				deleSave = new DeleSaveImage(SaveImageAll);
			}
			else
			{
				if(CheckDownloadingImage())
				{
					MessageBox.Show("이미지를 불러오는 중입니다.", "알림", MessageBoxButton.OK);
					return;
				}
				for (int i = 0; i < listMedia.Count; i++)
					FileInstence.SaveImage(listMedia[i].media_url_https, listBitmapImage[i]);
				MessageBox.Show("모든 이미지를 저장했습니다.", "알림", MessageBoxButton.OK);
				deleSave = null;
			}
		}

		private bool CheckDownloadingImage(int index = -1)
		{
			if (index != -1)//단일 이미지 
				return listBitmapImage[index].IsDownloading;
			else//전체 이미지
				for (int i = 0; i < listBitmapImage.Count; i++)
					if (listBitmapImage[i].IsDownloading)
						return true;

			return false;
		}

		private void image_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Image img = sender as Image;
			if (img == null) return;

			int selIndex = listImageControl.IndexOf(img);
			if (selIndex == -1) return;

			ChangeImage(selIndex);
		}

		private void imageWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (WindowState != WindowState.Maximized && WindowState != WindowState.Minimized)
			{
				DataInstence.SetImageWidowLocation(Left, Top, Width, Height);
			}
			for (int i = 0; i < listBitmapImage.Count; i++)
			{
				listBitmapImage[i].StreamSource?.Dispose();
			}
		}

		private void imageWindow_Loaded(object sender, RoutedEventArgs e)
		{
			mainImage.Width = scrollViewer.ActualWidth;
			mainImage.Height = scrollViewer.ActualHeight;

			if (isDM == false || clientDM == null) return;
			if (clientDM.entities.media.Count == 0) return;
			if (clientDM.entities.media[0].type != "photo") return;

			PacketImage parameter = new PacketImage(clientDM.entities.media[0].media_url_https);
			WebRequest req = (HttpWebRequest)WebRequest.Create(parameter.MethodGetUrl());
			req.Headers.Add("Authorization", OAuth.GetInstence().GetHeader(parameter));
			req.BeginGetResponse(new AsyncCallback(AsyncLoadBitmap), req);
		}
		//----------------------------------------------------------------------------------------------
		//--------------------------------------리사이즈구간------------------------------------------
		//----------------------------------------------------------------------------------------------
		private void ChangeFitSize()
		{
			if (isSmallImage)
			{
				mainImage.Width = mainImage.Source.Width;
				mainImage.Height = mainImage.Source.Height;
			}
			else
			{
				mainImage.Width = scrollViewer.ActualWidth;
				mainImage.Height = scrollViewer.ActualHeight;
			}
		}

		private void mainImage_MouseDown(object sender, MouseButtonEventArgs e)
		{
			prevMousePoint = e.GetPosition(null);
		}

		private void mainImage_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (isSmallImage) return;

			if (mainImage.IsMouseCaptured)//마우스 똇을 때 마우스 누르고 있던 상태면 해제
				mainImage.ReleaseMouseCapture();
			else
				ZoomOriginal();
		}

		private void ZoomOriginal()
		{
			if (listBitmapImage[index].IsDownloading) return;//로딩중에 작동 안 하게

			if (mainImage.Width < mainImage.Source.Width || mainImage.Height < mainImage.Source.Height)//원본이미지로 확대
			{
				mainImage.Width = mainImage.Source.Width;
				mainImage.Height = mainImage.Source.Height;
			}
			else//창에 맞춘 사이즈로 축소
			{
				mainImage.Width = scrollViewer.ViewportWidth;
				mainImage.Height = scrollViewer.ViewportHeight;
			}
		}

		private void mainImage_MouseMove(object sender, MouseEventArgs e)
		{
			//마우스가 눌리고있을 때랑 작은이미지가 아닐 때 드래그로 이미지 이동
			if (e.LeftButton == MouseButtonState.Pressed && isSmallImage == false)
			{
				if (mainImage.IsMouseCaptured == false)
					mainImage.CaptureMouse();
				Point mousePos = e.GetPosition(null);
				double moveX = prevMousePoint.X - mousePos.X;
				double moveY = prevMousePoint.Y - mousePos.Y;
				prevMousePoint = mousePos;
				scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + moveX);
				scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + moveY);
			}
		}

		private void MoveLeft()
		{
			scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - 30);
		}

		private void MoveRight()
		{
			scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + 30);
		}

		private void MoveUp()
		{
			scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - 30);
		}

		private void MoveDown()
		{
			scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + 30);
		}

		private void Zoom()
		{
			double width = mainImage.Width + mainImage.Source.Width * 0.1;
			double height = mainImage.Height + mainImage.Source.Height * 0.1;

			if (width > mainImage.Source.Width) width = mainImage.Source.Width;
			if (height > mainImage.Source.Height) height = mainImage.Source.Height;

			mainImage.Width = width;
			mainImage.Height = height;
		}

		private void ZoomOut()
		{
			double width = mainImage.Width - mainImage.Source.Width * 0.1;
			double height = mainImage.Height - mainImage.Source.Height * 0.1;

			if (width < scrollViewer.ActualWidth) width = scrollViewer.ActualWidth;
			if (height < scrollViewer.ActualHeight) height = scrollViewer.ActualHeight;

			if (width > mainImage.Source.Width) width = mainImage.Source.Width;//원본사이즈보다 클 경우 원본사이즈로 재수정
			if (height > mainImage.Source.Height) height = mainImage.Source.Height;
			mainImage.Width = width;
			mainImage.Height = height;
		}

		private void mainImage_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta > 0)//확대
				Zoom();	
			else//축소
				ZoomOut();
			e.Handled = true;
		}

		private void helpImageWindow_Click(object sender, RoutedEventArgs e)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("← 이전 이미지 / → 다음 이미지");
			sb.AppendLine();
			sb.Append("1~4 첫 번째~네 번째 이미지 표시");
			sb.AppendLine();
			sb.Append("CTRL+C 현재 이미지 클립보드에 복사");
			sb.AppendLine();
			sb.Append("Space 이미지 원본으로 확대 / 축소");
			sb.AppendLine();
			sb.Append("WASD 이미지 확대 후 상하좌우 이동");
			sb.AppendLine();
			sb.Append("Q 이미지 축소 / E 이미지 확대");
			sb.AppendLine();
			sb.Append("V 트윗 메뉴");
			sb.AppendLine();
			sb.Append("R 원본 불러오기");
			sb.AppendLine();
			sb.Append("F3 저장 폴더 열기");
			sb.AppendLine();
			sb.Append("-----------------------------------------");
			sb.AppendLine();
			sb.Append("클릭 이미지 원본으로 확대 / 축소");
			sb.AppendLine();
			sb.Append("드래그 이미지 확대 후 이동");
			sb.AppendLine();
			sb.Append("휠 이미지 확대 축소");
			


			MessageBox.Show(this, sb.ToString(), "이미지 뷰어 사용법", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void imageWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			ChangeFitSize();
		}

		private void imageWindow_Closed(object sender, EventArgs e)
		{
			DalsaeInstence.FocusPanel();
		}

		private class ImageProgress//이미지 다운로드 현황을 보여주기 위한 프로그레스 바 관리 클래스
		{
			private BitmapImage bitmap;
			private ProgressBar progressBar;
			public ImageProgress(ProgressBar progress)
			{
				this.progressBar = progress;
			}
			public void SetBitmap(BitmapImage bitmap)//bitmap만 넣어주면 이벤트 알아서 연결
			{
				this.bitmap = bitmap;
				progressBar.Value = 0;
				bitmap.DownloadProgress += bitmap_DownloadProgress;
				bitmap.DownloadFailed += Bmp_DownloadFailed;
				bitmap.DownloadCompleted += bitmap_LoadEnd;
				if (bitmap.IsDownloading)//GC에 회수되기 전이면 다운로드 시작을 안 한다
					Show();
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
				bitmap.DownloadProgress -= bitmap_DownloadProgress;
				bitmap.DownloadFailed -= Bmp_DownloadFailed;
				bitmap.DownloadCompleted -= bitmap_LoadEnd;
			}
			private void bitmap_LoadEnd(object sender, EventArgs e)
			{
				bitmap.DownloadProgress -= bitmap_DownloadProgress;
				bitmap.DownloadFailed -= Bmp_DownloadFailed;
				bitmap.DownloadCompleted -= bitmap_LoadEnd;
				progressBar.Visibility = Visibility.Collapsed;
			}
		}

		private void viewWeb_Click(object sender, RoutedEventArgs e)
		{
			if (tweet == null) return;
			string url = $"https://twitter.com/{tweet.user.screen_name}/status/{tweet.id}";
			System.Diagnostics.Process.Start(url);
			TweetInstence.AddTweet(eTweetPanel.eOpen, tweet);
		}

		//-------------------------------------------------------------------------------------
		//----------------------------------컨텍스트-----------------------------------------
		//-------------------------------------------------------------------------------------
		private void ShowMenu()
		{
			contextOnOpening(grid, null);
			Point pt = grid.PointToScreen(new Point(0, 0));
			grid.ContextMenu.PlacementRectangle = new Rect(pt, new Size(grid.Width, grid.Height));
			grid.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
			grid.ContextMenu.IsOpen = true;
		}

		private void contextClick_Url(object sender, RoutedEventArgs e)
		{
			MenuItem item = sender as MenuItem;
			if (item == null) return;
			if (tweet == null) return;

			for (int i = 0; i < tweet.listUrl.Count; i++)
			{
				if (tweet.listUrl[i].display_url == item.Header.ToString())
				{
					System.Diagnostics.Process.Start(tweet.listUrl[i].expanded_url);
					TweetInstence.AddTweet(eTweetPanel.eOpen, tweet);
					break;
				}
			}
		}

		private void contextClick_Favorite(object sender, RoutedEventArgs e)
		{
			if (tweet == null) return;
			DalsaeInstence.Favorite(tweet.originalTweet.id, !tweet.originalTweet.favorited);
		}
		
		private void contextClick_ViewWeb(object sender, RoutedEventArgs e)
		{
			viewWeb_Click(null, null);
		}
		private void contextClick_Retweet(object sender, RoutedEventArgs e)
		{
			if (tweet == null) return;

			if (tweet.user.Protected && DataInstence.CheckIsMe(tweet.user.id) == false
				&& DataInstence.option.isRetweetProtectUser == false)
			{
				DalsaeInstence.ShowMessageBox("잠금 계정의 트윗은 리트윗 할 수 없습니다.\r", "알림");
				return;
			}

			MessageBoxResult mr = DalsaeInstence.ShowMessageBox("선택한 트윗을 리트윗 하시겠습니까?", "리트윗 확인", MessageBoxButton.YesNo);
			if (mr != MessageBoxResult.Yes) return;

			if (tweet.user.Protected && DataInstence.CheckIsMe(tweet.user.id) == false)
				DalsaeInstence.RetweetProtect(tweet);
			else
				DalsaeInstence.Retweet(tweet.originalTweet.id, !tweet.retweeted);
		}
		
		private void contextClick_TweetCopy(object sender, RoutedEventArgs e)
		{
			if (tweet == null) return;

			string copy = Generate.ReplaceTextExpend(tweet.originalTweet);
			copy = copy.Replace("\n", Environment.NewLine);
			Clipboard.SetText(copy);

			MessageBox.Show("트윗이 클립보드에 복사 되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
		}
		private void contextClick_Reply(object sender, RoutedEventArgs e)
		{
			DalsaeInstence.Reply(tweet);
		}
		private void contextClick_ReplyAll(object sender, RoutedEventArgs e)
		{
			DalsaeInstence.ReplyAll(tweet);
		}
		private void contextClick_Qt(object sender, RoutedEventArgs e)
		{
			DalsaeInstence.QTRetweet(tweet);
		}

		//폴더 열기
		private void OpenFolder_Click(object sender, RoutedEventArgs e)
		{
			OpenFolder();
		}
		private void OpenFolder()
		{
			Process.Start(DataInstence.option.imageFolderPath);
		}

		private void Window_Activated(object sender, EventArgs e)
		{
			if (Generate.IsOnScreen(this) == false)
			{
				this.Left = 100;
				this.Top = 100;
			}
		}
	}//class
}
