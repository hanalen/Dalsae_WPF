using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Interop;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Dalsae.Template;
using static Dalsae.FileManager;
using static Dalsae.DalsaeManager;
using static Dalsae.DataManager;
using static Dalsae.TweetManager;
using System.Windows.Threading;

namespace Dalsae.WindowAndControl
{
	/// <summary>
	/// VideoWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class VideoWindow : Window
	{
		/// <summary>
		/// bitrate가 가장 높은 media index
		/// </summary>
		private int bigIndex { get; set; } = 0;
		/// <summary>
		/// 동영상 현재 위치를 갱신하기 위한 타이머
		/// </summary>
		private DispatcherTimer timer { get; set; } = new DispatcherTimer();
		private ClientTweet tweet;
		/// <summary>
		/// 현재 플레이중인지 여부
		/// </summary>
		private bool isPlaying { get; set; } = false;

		/// <summary>
		/// GIF루프 재생 플래그
		/// </summary>
		private bool isGifLoop { get; set; } = false;
		/// <summary>
		/// 음소거 버튼 누를 때 이전 볼륨을 저장하는 값
		/// </summary>
		private double prevVolume = 0;
		//참고사이트
		//http://www.c-sharpcorner.com/UploadFile/dpatra/seek-bar-for-media-element-in-wpf/

		private BitmapImage bitmapPrev = new BitmapImage();
		private BitmapImage bitmapNext = new BitmapImage();
		private BitmapImage bitmapPause = new BitmapImage();
		private BitmapImage bitmapPlay = new BitmapImage();
		private BitmapImage bitmapVolume = new BitmapImage();
		private BitmapImage bitmapMute = new BitmapImage();

		private string videoFileName { get; set; }

		private bool isClosed = false;
		public VideoWindow(ClientTweet tweet)
		{
			LoadResources(bitmapMute, Properties.Resources.mute);
			LoadResources(bitmapVolume, Properties.Resources.volume);
			LoadResources(bitmapNext, Properties.Resources.next);
			LoadResources(bitmapPrev, Properties.Resources.prev);
			LoadResources(bitmapPause, Properties.Resources.pause);
			LoadResources(bitmapPlay, Properties.Resources.play);

			InitializeComponent();
			labelTime.Content = "";
			gridTweet.DataContext = tweet.originalTweet;
			this.tweet = tweet;
			if (DataInstence.option.isShowImageTweet)
				gridTweet.Visibility = Visibility.Visible;

			LoadVideo();
			SetPosition();
			SetVolume();
			SetVolumeIcon();

			gridBottom.Visibility = Visibility.Collapsed;

		}

		/// <summary>
		/// 아이콘 리소스를 불러오는 함수
		/// </summary>
		/// <param name="showImage">비트맵 객체</param>
		/// <param name="loadBitmap">불러올 리소스</param>
		private void LoadResources(BitmapImage showImage, System.Drawing.Bitmap loadBitmap)
		{
			if (loadBitmap == null || showImage == null) return;

			using (MemoryStream memory = new MemoryStream())
			{
				loadBitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
				memory.Position = 0;
				showImage.BeginInit();
				showImage.StreamSource = memory;
				showImage.CacheOption = BitmapCacheOption.OnLoad;
				showImage.EndInit();
			}
		}

		/// <summary>
		/// 창 열릴 때 위치 조정
		/// </summary>
		private void SetPosition()
		{
			Left = Properties.Settings.Default.ptVideoX;
			Top = Properties.Settings.Default.ptVideoY;

			Width = Properties.Settings.Default.ptVideoWidth;
			Height = Properties.Settings.Default.ptVideoHight;
		}

		/// <summary>
		/// 창 로드 시 볼륨 설정
		/// </summary>
		private void SetVolume()
		{
			mediaElement.Volume = Properties.Settings.Default.videoVolume;
			sliderVolume.Value = mediaElement.Volume * 100;
		}

		/// <summary>
		/// 볼륨값이 변함에 따라 뮤트&볼륨 아이콘 표시 여부 결정
		/// </summary>
		private void SetVolumeIcon()
		{
			if (sliderVolume.Value == 0)
			{
				imageVolume.Visibility = Visibility.Collapsed;
				imageMute.Visibility = Visibility.Visible;
			}
			else
			{
				imageVolume.Visibility = Visibility.Visible;
				imageMute.Visibility = Visibility.Collapsed;
			}
		}

		/// <summary>
		/// 비디오 정보를 불러와 다운로드 시작
		/// </summary>
		private void LoadVideo()
		{
			if (tweet.mediaEntities.media.Count == 0) return;
			if (tweet.mediaEntities.media[0].video_info == null) return;

			List<Variant> listVariant = tweet.mediaEntities.media[0].video_info.variants?.OrderBy(x => x.bitrate).ToList();
			bigIndex = listVariant.Count - 2;
			if (bigIndex < 0)
				bigIndex = 0;
			//for (int i = 0; i < listVariant.Count; i++)//bitrate가 가장 큰 index찾아서 화질 가장 좋은 동영상으로 틀어줌
			//{
			//	if (listVariant[bigIndex].bitrate < listVariant[i].bitrate)//큰값 찾기
			//		bigIndex = i;
			//}
			if (tweet.mediaEntities.media[0].type == "animated_gif")
			{
				isGifLoop = true;
			}
			videoFileName = DateTime.Now.ToString("yyyyMMddhhmmssffffff");
			FileInstence.DownloadVideo(listVariant[bigIndex].url, videoFileName, Web_DownloadProgressChanged, Web_DownloadFileCompleted);
		}

		/// <summary>
		/// 비디오 파일을 다 불러올 경우 발생되는 이벤트
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>

		private void Web_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			if (isClosed) return;

			string uri = FileInstence.GetVideoFilePath(videoFileName);
			mediaElement.Source = new Uri(uri, UriKind.Relative);
			progressBar.Visibility = Visibility.Hidden;

			imageMute.Source = bitmapMute;
			imageVolume.Source = bitmapVolume;
			imagePrev.Source = bitmapPrev;
			imageNext.Source = bitmapNext;
			imagePlay.Source = bitmapPlay;
			imagePause.Source = bitmapPause;

			if (isGifLoop == false)
				gridBottom.Visibility = Visibility.Visible;

			Start();
		}

		/// <summary>
		/// 동영상 다운로드 진행 상황
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Web_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			progressBar.Value = e.ProgressPercentage;
		}

		private void contextOnOpening(object sender, ContextMenuEventArgs e)
		{
			
		}

		/// <summary>
		/// 동영상 세팅이 됐을 때 최초 정보를 적용하는 이벤트
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
		{
			if (mediaElement.NaturalDuration.HasTimeSpan)
				sliderMovie.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
			//mediaElement.Play();
			timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
			timer.Tick += Timer_Tick;
			//Start();
		}

		/// <summary>
		/// 슬라이드바, 재생시간 표시하는 역할
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Timer_Tick(object sender, EventArgs e)
		{
			sliderMovie.Value = mediaElement.Position.TotalMilliseconds;
			if(mediaElement.NaturalDuration.HasTimeSpan)
				labelTime.Content = $"{mediaElement.Position.ToString(@"mm\:ss")} / {mediaElement.NaturalDuration.TimeSpan.ToString(@"mm\:ss")}";//재생 시간 표
		}

		/// <summary>
		/// 동영상 재생이 끝까지 됐을 경우 이벤트
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
		{
			Stop();
			if (isGifLoop)
				Start();
		}

		/// <summary>
		/// 동영상 슬라이더 마우스 다운 시 일시정지 하는 이벤트
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Pause();
		}

		/// <summary>
		/// 동영상 슬라이더 마우스 업 시 재생하는 이벤트
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void slider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Play();
		}

		/// <summary>
		/// 동영상 플레이어 마우스 클릭 시 정지/재생
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mediaElement_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (mediaElement.Source == null) return;

			if (isPlaying)
				Pause();
			else
				Play();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			isClosed = true;
			timer.Stop();
			mediaElement.Stop();
			mediaElement.Source = null;
			DataInstence.SetVideoWindowLocation(Left, Top, Width, Height);
			Properties.Settings.Default.videoVolume = mediaElement.Volume;
			DalsaeInstence.FocusPanel();
		}

		private void Start()
		{
			sliderMovie.Value = 0;
			mediaElement.Position = TimeSpan.FromMilliseconds(1);
			Play();
		}

		private void Play()
		{
			imagePlay.Visibility = Visibility.Collapsed;
			imagePause.Visibility = Visibility.Visible;
			if (sliderMovie.Value == sliderMovie.Maximum) Start();
			if (isGifLoop == false)
			{
				int SliderValue = (int)sliderMovie.Value;
				TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);
				mediaElement.Position = ts;
			}
			timer.Start();
			mediaElement.Play();
			isPlaying = true;
		}

		private void Stop()
		{
			imagePlay.Visibility = Visibility.Visible;
			imagePause.Visibility = Visibility.Collapsed;
			isPlaying = false;
			timer.Stop();
		}

		private void Pause()
		{
			imagePlay.Visibility = Visibility.Visible;
			imagePause.Visibility = Visibility.Collapsed;
			isPlaying = false;
			timer.Stop();
			mediaElement.Pause();
			isPlaying = false;
		}

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space)
			{
				if (isPlaying)
					Pause();
				else
					Play();
				e.Handled = true;
			}
			else if (e.Key == Key.Right)
			{
				MoveNext();
				e.Handled = true;
			}
			else if (e.Key == Key.Left)
			{
				MovePrev();
				e.Handled = true;
			}
			else if ( e.Key== Key.Up)
			{
				VolumeUp();
			}
			else if(e.Key== Key.Down)
			{
				VolumeDown();
			}
			else if (e.Key == Key.Enter || e.Key == Key.Escape)
			{
				e.Handled = true;
				Close();
			}
		}

		private void VolumeDown()
		{
			sliderVolume.Value = sliderVolume.Value - 5;
		}

		private void VolumeUp()
		{
			sliderVolume.Value = sliderVolume.Value + 5;
		}

		private void MoveNext()
		{
			Pause();
			sliderMovie.Value += 2000;
			Play();
		}

		private void MovePrev()
		{
			Pause();
			sliderMovie.Value -= 2000;
			Play();
		}

		private void imagePrev_MouseDown(object sender, MouseButtonEventArgs e)
		{
			MovePrev();
		}

		private void imagePlay_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Play();
		}

		private void imageNext_MouseDown(object sender, MouseButtonEventArgs e)
		{
			MoveNext();
		}

		private void imagePause_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Pause();
		}

		private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			mediaElement.Volume = sliderVolume.Value / 100;
			SetVolumeIcon();
		}

		private void imageVolume_MouseDown(object sender, MouseButtonEventArgs e)
		{
			prevVolume = mediaElement.Volume;
			sliderVolume.Value = 0;
		}

		private void imageMute_MouseDown(object sender, MouseButtonEventArgs e)
		{
			sliderVolume.Value = prevVolume * 100;
		}

		private void Window_Activated(object sender, EventArgs e)
		{
			if (Generate.IsOnScreen(this) == false)
			{
				this.Left = 100;
				this.Top = 100;
			}
		}
	}
}
