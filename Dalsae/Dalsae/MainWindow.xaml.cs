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
using System.Collections.Specialized;
using System.Windows.Threading;
using Dalsae.WindowAndControl;
using Dalsae.Template;
using Dalsae.API;
using Dalsae.Data;
using Dalsae.Web;
using System.Windows.Controls.Primitives;

/*작업 순서
 * 잠금 이미지는 팔로우쪽에서 그리드에 한 것처럼 아이디 옆에 자물쇠 가게 하고 이미지 소스도
 * 자체 리소스에서 가져올 수 있게 수정하기
 * 이미지 메모리 누수 원인 대충 찾음. 이미지 이전에 열어본 걸 따로 캐시하고있어서
 * 해당 이미지가 메모리 누수....의 원인이 되는 거 같기도 한데 이미지 닫고 몇초 뒤에 열어보면 누수가 없음
 * 아마 GC가 회수는 하고있는 거 같다. 다른 원인을 찾아봐야할듯....
 * 
 * 유저 프로필을 보게 해주세요
 * 알림 올 때 진동 해달라는 건의
 * //링크 복사 해달라는 건의
 * 특정유저가 글 쓰면 알람울리게?
 * ctrl+화살표로 현재 선택중인 유저의 다음 트윗 선택하게
 * SelectParentGridAndMove에서 index 음수 나오는 문제 있음
 * 이미지뷰어에서 한글일때 단축키 안먹힘
 * 이미지 등록할 거 미리보기에 마우스 올리면 미리보기를 확대해서 보여주는 방식도 괜찮을듯
 * ui작을때 이미지 미리보기는 숨겼다가 첨부할때 사이즈 늘어나는 방식도 괜찮을듯
 * //shift+enter했을때 줄바꿈을 하기는 하는데 문장 중간에 할 경우 추가가 안 됨
 * 이미지 뷰어 띄울 떄 사이즈가 작은 이미지일 경우에 이미지 다운로드가 더 빠를경우 확대돼서 출력됨
 * 입력칸에서 버튼에 포커스 주고 리스트박스 포커스 가면 단축키가 안 먹힘. ime꼬임
 * 클립보드 이미지 추가하고 빠르게 엔터를 칠 경우? 이미지가 안 날아감
 * //유저 트윗 보기 하면 트을 자동으로 불러오잖아요? 불러오는 중에 스페이스 바 누르면 터짐
 * >>>>다른 계정 선택 시 터짐<<<< 이거 안그러네? 대체 뭐냐
 * bitmapimage가 메모리에서 꺠끘하게 안 사라짐
 * 메뉴띄우는거 더블클릭으로도 해달라는 건의
 * 계정 추가 문제, 추가 후 작동이 이상함 <- ?????
 * 대화 추적 동기라서 프로그램 뻗을 문제 있음. 스레드로 변경하자
 * 대화 라벨 클릭해서 대화 로딩하게
 * 동영상 재생
 * 이미지 추가 한 거 왼클릭으로 열기 추가
 * 플텍계 리트윗쪽 이미지관련 수정필요
 * UI옵션 관련 작업, 메인화면 레이아웃 추가
 * 이미지 뷰어에서 dm이미지도 열어볼 수 있게
 * 리트윗에 답누를때 답변가는 거 리트윗유저로 바꾸기?
 * 아이디 입력에 커서 놔두고 엔터치면 트윗 전송안됨
 * 다중계정 추가 후 메뉴에 아이디 추가가 안 됨
 * 
 * dc링크같은 거 여러개 올라오면  header에 같은 문자만 올라가서 다른 링크가 안 열림
 * 답은 menuitem의 tag를 사용하는 건데 왜인지 안 됨
 * 
 * ----우선순위 밖----
 * 스킨 변경시 백그라운드는 안 바뀜. 이건 좀 힘들 거 같아 그냥 보류
 * 다중계정용 아이디 추가 한 거에 언더바가 있으면 단축키로 인식됨
 * 프로그램 시작 시 블락 유저, 팔로잉 리스트, 관심글 다 가져오게함.
 *		해당 기능은 옵션에서 저장해둘지 여부 체크
 * 이미지 뷰어 확대/드래그 기능 추가
 * 창 줄일 때 오른쪽 자물쇠가 사라짐. 레이아웃 문제
 * DM보낼떄 이미지 안 감
 * 트윗에 이미지 미리보기
 * 
 */
namespace Dalsae
{
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window
	{
		private Regex UrlMatch = new Regex(@"[(http|ftp|https):\/\/]*[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?");
		private int tweetLength = 0;
		private ClientTweet replyTweet = null;
		private eTweetPanel selectPanel = eTweetPanel.eHome;
		public delegate void DeleShowMessageBox(string text, string title, MessageBoxButton button, MessageBoxImage icon, Delegate dele, BasePacket parameter = null);
		public delegate void DeleShowPanel(eTweetPanel panel);
		public delegate void DeleReplyAndQTRetweet(ClientTweet tweet);
		public delegate void DeleFocusPanel();
		public delegate void DeleChangeTopUI(bool isSmall);
		public delegate void DeleAddMessage(string message);
		private List<BitmapImage> listBitmapImage = new List<BitmapImage>();//전송 이미지 리스트
		private List<Image> listImage = new List<Image>();//이미지 미리보기 리스트
		private Dictionary<eTweetPanel, TreeViewManager> dicPanel = new Dictionary<eTweetPanel, TreeViewManager>();
		private TreeView selectedTreeView { get { return dicPanel[selectPanel].treeView; } }
		private bool isAddedGif { get; set; } = false;
		private string pathGif { get; set; } = string.Empty;
		public BitmapImage gongSikImage { get; set; } = new BitmapImage();
		public BitmapImage lockImage { get; set; } = new BitmapImage();
		public BitmapImage bitmapHome { get; set; } = new BitmapImage();
		public BitmapImage bitmapMention { get; set; } = new BitmapImage();
		public BitmapImage bitmapDM { get; set; } = new BitmapImage();
		public BitmapImage bitmapFav { get; set; } = new BitmapImage();
		public BitmapImage bitmapOpen { get; set; } = new BitmapImage();
		public BitmapImage bitmapGif { get; set; } = new BitmapImage();
		public BitmapImage bitmapPlay { get; set; } = new BitmapImage();
		public BoolFlagNoti notiMention { get; set; } = new BoolFlagNoti();
		public BoolFlagNoti notiDm { get; set; } = new BoolFlagNoti();
		public WidthNoti notiWidth { get; private set; } = new WidthNoti();
		public BoolFlagNoti isShowMentionIds { get; set; } = new BoolFlagNoti();
		public MessageQueue messageQueue { get; set; } = new MessageQueue();
		private ListFindUser listMentionIds { get; set; } = new ListFindUser();
		private ClientDirectMessage selectDM { get; set; }
		private ClientTweet selectTweet { get; set; }
		private DispatcherTimer timer { get; set; } = new DispatcherTimer();

		public RoutedCommand findCommand = new RoutedCommand();
		public MainWindow()
		{
			DalsaeInstence.SetMainWindow(this);
			SetEvent();
			LoadResources(lockImage, Properties.Resources.LockPic);
			LoadResources(gongSikImage, Properties.Resources.gongSik_Small);
			LoadResources(bitmapGif, Properties.Resources.gif);
			LoadResources(bitmapPlay, Properties.Resources.play2);

			LoadResources(bitmapHome, Properties.Resources.home_icon);
			LoadResources(bitmapMention, Properties.Resources.noti_icon);
			LoadResources(bitmapDM, Properties.Resources.dm_icon);
			LoadResources(bitmapFav, Properties.Resources.favorite);
			LoadResources(bitmapOpen, Properties.Resources.unlink);

			InitializeComponent();//리소스 로드 하고 이니셜 해야 메모리 문제 없음
			gridProfilePicture.DataContext = DataInstence.userInfo.user;
			SetListBox();
			SetPosition();
			
			LoadAccount();
			SetTopUI(DataInstence.option.isSmallUI);
			timer.Tick += CheckMessageQueue;
			timer.Interval = new TimeSpan(0, 0, 0, 4);
			timer.Start();
			findCommand.InputGestures.Add(new KeyGesture(Key.F, ModifierKeys.Control));//찾기 단축키 등록
			SetFindGridVisibility(false);
		}
		
		private void SetEvent()
		{
			//UserStreaming.usInstence.OnChangedStatus += UsInstence_OnChangedStatus;
			Manager.ResponseAgent.responseInstence.OnUserTweet += ResponseUserTweet;
			Manager.ResponseAgent.responseInstence.OnUserMedia += ResponseUserMediaTweet;
			Manager.APICallAgent.apiInstence.OnApiCall += OnApiCall;
			Manager.ResponseAgent.responseInstence.OnResponse += OnResponse;
		}

		private void ResponseUserTweet(List<ClientTweet> listTweet)
		{
			ShowPanel(eTweetPanel.eUser);
		}

		private void ResponseUserMediaTweet(List<ClientTweet> listTweet)
		{
			ShowPanel(eTweetPanel.eUserMedia);
		}

		private void OnApiCall(eResponse response)
		{
			if(response== eResponse.BLOCK_IDS)
			{
				statusBlock.Visibility = Visibility.Visible;
				sepaBlock.Visibility = Visibility.Visible;
			}
			else if(response== eResponse.FOLLOWING_LIST)
			{
				statusFollow.Visibility = Visibility.Visible;
				sepaFollow.Visibility = Visibility.Visible;
			}
			else if(response== eResponse.RETWEET_OFF_IDS)
			{
				statusRetweet.Visibility = Visibility.Visible;
				sepaRetweet.Visibility = Visibility.Visible;
			}
			else if(response== eResponse.TIME_LINE)
			{
				statusHome.Visibility = Visibility.Visible;
				sepaHome.Visibility = Visibility.Visible;
			}
			else if( response== eResponse.MENTION)
			{
				statusMention.Visibility = Visibility.Visible;
				sepaMention.Visibility = Visibility.Visible;
			}
		}

		private void OnResponse(eResponse response)
		{
			if (response == eResponse.BLOCK_IDS)
			{
				statusBlock.Visibility = Visibility.Collapsed;
				sepaBlock.Visibility = Visibility.Collapsed;
			}
			else if (response == eResponse.FOLLOWING_LIST)
			{
				statusFollow.Visibility = Visibility.Collapsed;
				sepaFollow.Visibility = Visibility.Collapsed;
			}
			else if (response == eResponse.RETWEET_OFF_IDS)
			{
				statusRetweet.Visibility = Visibility.Collapsed;
				sepaRetweet.Visibility = Visibility.Collapsed;
			}
			else if (response == eResponse.TIME_LINE)
			{
				statusHome.Visibility = Visibility.Collapsed;
				sepaHome.Visibility = Visibility.Collapsed;
			}
			else if (response == eResponse.MENTION)
			{
				statusMention.Visibility = Visibility.Collapsed;
				sepaMention.Visibility = Visibility.Collapsed;
			}
		}

		private void UsInstence_OnChangedStatus(bool isConnected)
		{
			if (isConnected)
				statusStream.Content= "스트리밍 On";
			else
				statusStream.Content = "스트리밍 Off";
		}

		private void SetListBox()
		{
			treeHome.ItemsSource = TweetInstence.treeHome;
			treeMention.ItemsSource = TweetInstence.treeMention;
			treeDM.ItemsSource = TweetInstence.treeDM;
			treeFav.ItemsSource = TweetInstence.treeFavorite;
			treeUser.ItemsSource = TweetInstence.treeUser;
			treeOpen.ItemsSource = TweetInstence.treeOpen;
			treeUserMedia.ItemsSource = TweetInstence.treeUserMedia;
			dicPanel.Add(eTweetPanel.eHome, new TreeViewManager(treeHome, TweetInstence.treeHome));
			dicPanel.Add(eTweetPanel.eMention, new TreeViewManager(treeMention, TweetInstence.treeMention));
			dicPanel.Add(eTweetPanel.eDm, new TreeViewManager(treeDM, TweetInstence.treeDM));
			dicPanel.Add(eTweetPanel.eFavorite, new TreeViewManager(treeFav, TweetInstence.treeFavorite));
			dicPanel.Add(eTweetPanel.eUser, new TreeViewManager(treeUser, TweetInstence.treeUser));
			dicPanel.Add(eTweetPanel.eOpen, new TreeViewManager(treeOpen, TweetInstence.treeOpen));
			dicPanel.Add(eTweetPanel.eUserMedia, new TreeViewManager(treeUserMedia, TweetInstence.treeUserMedia));

			listImage.Add(image0);
			listImage.Add(image1);
			listImage.Add(image2);
			listImage.Add(image3);

			listBoxIds.ItemsSource = listMentionIds;//자동완성용
		}

		private void SetPosition()
		{
			Left = Properties.Settings.Default.ptMainX;
			Top = Properties.Settings.Default.ptMainY;
			Width = Properties.Settings.Default.ptMainWidth;
			Height = Properties.Settings.Default.ptMainHeight;
		}

		private int tickCount = 0;
		public void RefreshTick(object sender, EventArgs e)
		{
			tickCount++;
			if (tickCount == 61)
				tickCount = 1;
			statusTick.Content = $"({tickCount}/60)";

		}

		public void SetTopUI(bool isSmall)
		{
			if(isSmall)
			{
				imageGrid.Visibility = Visibility.Collapsed;
				labelCount.Visibility = Visibility.Collapsed;
				labelCount2.Visibility = Visibility.Visible;
				button.Visibility = Visibility.Collapsed;
				gridProfilePicture.Visibility = Visibility.Collapsed;
				inputTweetBox.Height = 20;
				listBoxIds.Margin = new Thickness(83, 19, 0, 0);
			}
			else
			{
				labelCount.Visibility = Visibility.Visible;
				labelCount.Visibility = Visibility.Visible;
				labelCount2.Visibility = Visibility.Collapsed;
				button.Visibility = Visibility.Visible;
				gridProfilePicture.Visibility = Visibility.Visible;
				inputTweetBox.Height = 60;
				listBoxIds.Margin = new Thickness(83, 64, 0, 0);
			}
		}

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

		//Timer에서 체크하는 함수
		private void CheckMessageQueue(object sender, EventArgs e)
		{
			if (messageQueue.NextMessage() == false)
				timer.Stop();
		}

		public void AddMessage(string message)
		{
			if (timer.IsEnabled == false)
			{
				timer.Start();
				messageQueue.NextMessage();
			}
			messageQueue.AddMessage(message);
		}

		public void ShowMessage(string text, string title, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Asterisk,
			Delegate dele = null, BasePacket parameter = null)
		{
			MessageBoxResult mr = MessageBox.Show(text, title, button, icon);
			if (dele != null)
			{
				if (parameter == null)
					dele.DynamicInvoke(new object[] { mr });
				else
					dele.DynamicInvoke(new object[] { mr, parameter });
			}
		}


		private void ClearInput()
		{
			isAddedGif = false;
			pathGif = string.Empty;
			replyTweet = null;
			inputTweetBox.Text = "";
			ClearPreviewImage();
			listBitmapImage.Clear();
			UpdateImage();
			FocusPanel();
		}

		private void dmcontextOnOpening(object sender, ContextMenuEventArgs e)
		{
			ClientDirectMessage dm = treeDM.SelectedItem as ClientDirectMessage;
			Grid grid = sender as Grid;
			if (dm == null || grid == null) return;
			grid.ContextMenu = CreateDmContext(dm);
		}

		private ContextMenu CreateDmContext(ClientDirectMessage dm)
		{
			ContextMenu contextMenu = new ContextMenu();
			MenuItem mi = new MenuItem();
			mi.Header = "입력하기";
			mi.Click += contextClick_EnterInput;
			contextMenu.Items.Add(mi);

			Separator sp = new Separator();
			contextMenu.Items.Add(sp);
			//------------------미디어---------------------
			if (dm.entities.media.Count > 0)
			{
				MenuItem gifMi = new MenuItem();
				gifMi.Header = "gif / 동영상";

				bool isAddPhoto = false;
				foreach (ClientMedia item in dm.entities.media)
				{
					if (item.type == "photo" && isAddPhoto == false)
					{
						MenuItem imageItem = new MenuItem();
						imageItem.Header = item.display_url;
						imageItem.Click += contextClickDM_Image;
						contextMenu.Items.Add(imageItem);
					}
					else
					{
						MenuItem gifItem = new MenuItem();
						gifItem.Header = item.display_url;
						gifItem.Click += contextClick_Video;
						gifMi.Items.Add(gifItem);
					}
				}
				if (gifMi.Items.Count > 0)
					contextMenu.Items.Add(gifMi);

				sp = new Separator();
				contextMenu.Items.Add(sp);
			}
			//----------------------------------------------

			//------------------URL----------------------
			if (dm.entities.urls.Count > 0)
			{
				mi = new MenuItem();
				mi.Header = "URL";
				for (int i = 0; i < dm.entities.urls.Count; i++)
				{
					MenuItem url = new MenuItem();
					url.Header = dm.entities.urls[i].display_url;
					url.Click += contextClickDM_Url;
					mi.Items.Add(url);
				}
				contextMenu.Items.Add(mi);
				sp = new Separator();
				contextMenu.Items.Add(sp);
			}
			//----------------------------------------------
			mi = new MenuItem();
			mi.Header = "답장";
			mi.Click += contextClick_Reply;
			contextMenu.Items.Add(mi);

			sp = new Separator();
			contextMenu.Items.Add(sp);

			//----------------------------------------------
			//------------------사용자---------------------
			//----------------------------------------------
			mi = new MenuItem();
			mi.Header = "사용자 기능";
			contextMenu.Items.Add(mi);

			//------------------USER----------------------
			{
				MenuItem usermi = new MenuItem();
				usermi.Header = "유저 트윗";
				HashSet<string> hashUser = new HashSet<string>();
				hashUser.Add(dm.sender.screen_name);//보낸 사람
				hashUser.Add(dm.recipient.screen_name);//받는 사람
				foreach (string name in hashUser)
				{
					MenuItem umi = new MenuItem();
					umi.Header = name.Replace("_","__");
					umi.Click += dmcontextClick_UserTweet;
					usermi.Items.Add(umi);
				}
				mi.Items.Add(usermi);
			}
			sp = new Separator();
			contextMenu.Items.Add(sp);

			//------------------------------------------------
			mi = new MenuItem();
			mi.Header = "쪽지";
			mi.Click += contextClick_DM;
			contextMenu.Items.Add(mi);
			
			sp = new Separator();
			contextMenu.Items.Add(sp);

			mi = new MenuItem();
			mi.Header = "쪽지 복사";
			mi.Click += contextClick_DMCopy;
			contextMenu.Items.Add(mi);

			return contextMenu;
		}

		private void contextOnOpening(object sender, ContextMenuEventArgs e)
		{
			ClientTweet tweet = selectTweet;
			Grid grid = sender as Grid;
			if (tweet == null || grid == null) return;
			grid.ContextMenu = CreateContextMenu(tweet);
		}

		private ContextMenu CreateContextMenu(ClientTweet tweet)
		{
			if (tweet.user == null || tweet.originalTweet == null) return new ContextMenu();
			ContextMenu contextMenu = new ContextMenu();
			MenuItem mi = new MenuItem();
			mi.Header = "입력하기";
			mi.Click += contextClick_EnterInput;
			contextMenu.Items.Add(mi);

			Separator sp = new Separator();
			contextMenu.Items.Add(sp);
			//------------------미디어---------------------
			if(tweet.isMedia)
			{
				foreach (ClientMedia item in tweet.dicPhoto.Values)
				{
					if (tweet.isPhoto)
					{
						MenuItem imageItem = new MenuItem();
						imageItem.Header = item.display_url;
						imageItem.Click += contextClick_Image;
						contextMenu.Items.Add(imageItem);
						break;
					}
				}
				if (tweet.isMovie)//동영상 추가
				{
					MenuItem gifMi = new MenuItem();
					gifMi.Header = "gif / 동영상";
					MenuItem gifItem = new MenuItem();
					gifItem.Header = tweet.tweetMovie.display_url;
					gifItem.Click += contextClick_Video;
					gifMi.Items.Add(gifItem);
					contextMenu.Items.Add(gifMi);
				}

				sp = new Separator();
				contextMenu.Items.Add(sp);
			}
			//----------------------------------------------

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
					url.Tag = tweet.listUrl[i];
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

			sp = new Separator();
			contextMenu.Items.Add(sp);
			//----------------------------------------------
			if(tweet.isReply)
			{
				mi = new MenuItem();
				mi.Header = "대화 보기";
				mi.Click += contextClick_LoadDeahwa;
				contextMenu.Items.Add(mi);
				sp = new Separator();
				contextMenu.Items.Add(sp);
			}

			//----------------------------------------------
			//------------------사용자---------------------
			//----------------------------------------------
			mi = new MenuItem();
			mi.Header = "사용자 기능";
			contextMenu.Items.Add(mi);

			//------------------USER----------------------
			{
				MenuItem usermi = new MenuItem();
				usermi.Header = "유저 트윗";
				HashSet<string> hashUser = new HashSet<string>();
				hashUser.Add(tweet.user.screen_name);//리트윗 유저
				hashUser.Add(tweet.originalTweet.user.screen_name);//리트윗 원본 작성자
				foreach (string name in tweet.hashMention)
					hashUser.Add(name);
				foreach (string name in hashUser)
				{
					MenuItem umi = new MenuItem();
					umi.Header = name.Replace("_","__");
					umi.Click += contextClick_UserTweet;
					usermi.Items.Add(umi);
				}
				mi.Items.Add(usermi);
				MenuItem usermedia = new MenuItem();
				usermedia.Header = "유저 미디어";
				foreach (string name in hashUser)
				{
					MenuItem umi = new MenuItem();
					umi.Header = name.Replace("_", "__");
					umi.Click += contextClick_UserMediaTweet;
					usermedia.Items.Add(umi);
				}
				mi.Items.Add(usermedia);

				//------------------MUTE----------------------
				MenuItem muteItem = new MenuItem();
				muteItem.Header = "유저 뮤트";
				foreach(string name in hashUser)
				{
					MenuItem umi = new MenuItem();
					umi.Header = name.Replace("_", "__");
					umi.Click += contextClick_UserMute;
					umi.Tag = name;
					muteItem.Items.Add(umi);
				}
				mi.Items.Add(muteItem);

				//------------------프로필----------------------
				MenuItem profileItem = new MenuItem();
				profileItem.Header = "유저 프로필 보기";
				foreach (string name in hashUser)
				{
					MenuItem umi = new MenuItem();
					umi.Header = name.Replace("_", "__");
					umi.Click += contextClick_UserProfile;
					profileItem.Items.Add(umi);
				}
				mi.Items.Add(profileItem);
				sp = new Separator();
				mi.Items.Add(sp);
			}
			//------------------------------------------------
			{
				MenuItem usermi = new MenuItem();
				if (DataInstence.hashRetweetOff.Contains(tweet.user.id))
					usermi.Header = $"{tweet.user.screen_name.Replace("_", "__")}의 리트윗 켜기";
				else
					usermi.Header = $"{tweet.user.screen_name.Replace("_", "__")}의 리트윗 끄기";
				usermi.Click += contextClick_UserRetweetOff;
				mi.Items.Add(usermi);

				sp = new Separator();
				mi.Items.Add(sp);

				usermi = new MenuItem();
				usermi.Header = "클라이언트 뮤트";
				usermi.Click += contextClick_ClientMute;
				mi.Items.Add(usermi);

				usermi = new MenuItem();
				usermi.Header = "트윗 뮤트";
				usermi.Click += contextClick_TweetMute;
				mi.Items.Add(usermi);
			}
			sp = new Separator();
			contextMenu.Items.Add(sp);

			//------------------해시태그----------------------
			if (tweet.lastEntities.hashtags.Count > 0)
			{
				mi = new MenuItem();
				mi.Header = "해시 태그";
				for (int i = 0; i < tweet.lastEntities.hashtags.Count; i++)
				{
					MenuItem hash = new MenuItem();
					hash.Header = tweet.lastEntities.hashtags[i].text.Replace("_", "__");
					hash.Click += contextClick_Hashtag;
					mi.Items.Add(hash);
				}
				contextMenu.Items.Add(mi);
				sp = new Separator();
				contextMenu.Items.Add(sp);
			}
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
			mi.Header = "쪽지";
			mi.Click += contextClick_DM;
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

			
			mi = new MenuItem();
			mi.Header = "삭제";
			mi.Click += contextClick_TweetDelete;
			mi.IsEnabled = DataInstence.CheckIsMe(tweet.user.id);
			contextMenu.Items.Add(mi);
			
			return contextMenu;
		}

		private void SendTweet()
		{
			if (listBitmapImage.Count == 0 && inputTweetBox.Text.Length == 0 && pathGif.Length == 0) return;
			//bool isSend = true;
			bool isDm = false;
			if (inputTweetBox.Text.Length > 2)
			{
				if (inputTweetBox.Text[0] == 'd' && inputTweetBox.Text[1] == ' ')
				{
					isDm = true;
				}
			}
			if (DataInstence.option.isYesnoTweet)
			{
				MessageBoxResult result;
				if (isDm)
				{
					result = MessageBox.Show(this, "쪽지를 보내시겠습니까?",
									"보낸다", MessageBoxButton.YesNo, MessageBoxImage.None);
				}
				else
				{
					result = MessageBox.Show(this, "트윗을 등록 하시겠습니까?",
									"등록한다", MessageBoxButton.YesNo, MessageBoxImage.None);
					
				}
				if (result == MessageBoxResult.No)
					return;
			}
			if (isDm)
			{
				bool isSendDM = true;
				int index = inputTweetBox.Text.IndexOf(' ');
				if (inputTweetBox.Text.Length > index)
					index = inputTweetBox.Text.IndexOf(' ', index + 1);
				else
					isSendDM = false;
				if (index == -1)
					isSendDM = false;
				else
				{
					if (inputTweetBox.Text.Length - 1 <= index)
						isSendDM = false;
				}
				if (isSendDM == false)
				{
					MessageBox.Show(this, "쪽지에 내용이 없습니다.\r이미지만 보내기는 안 됩니다.",
								   "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
			}

			if (tweetLength > 280 && isDm == false)
			{
				MessageBox.Show(this, "트윗이 280자를 넘어 전송할 수 없습니다.",
								   "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			PacketUpdate parameter = new PacketUpdate(!isDm);//true: tweet / false: dm
			if (isDm)
			{
				string tweet = inputTweetBox.Text;
				int idEndIndex = tweet.IndexOf(' ', 2);
				if (idEndIndex < 2)
				{
					MessageBox.Show(this, "쪽지를 보낼 아이디를 잘못 입력 했습니다.",
								   "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
				string screenName = tweet.Substring(0, idEndIndex).Replace("d ", "");

				parameter.screen_name = screenName;
				if (idEndIndex + 1 <= tweet.Length)
					parameter.text = tweet.Substring(idEndIndex + 1);
			}
			else
			{
				parameter.status = inputTweetBox.Text;
				if (replyTweet != null)
				{
					parameter.in_reply_to_status_id = replyTweet.originalTweet.id.ToString();
				}
			}
			
			//if (isAddedGif)
			//	DalsaeInstence.SendMultimedia(parameter, pathGif);
			//else
			//	DalsaeInstence.SendTweet(parameter, listBitmapImage.ToArray());
			ClientSendTweet sendPacket = new Web.ClientSendTweet();
			if (isAddedGif)
				sendPacket.SetTweet(parameter, pathGif);
			else
				sendPacket.SetTweet(parameter, listBitmapImage.ToArray());

			DalsaeInstence.SendTweet(sendPacket);
			ClearInput();
			FocusPanel();
		}

		public void FocusPanel()
		{
			dicPanel[selectPanel].Focus();
		}

		public void NotiMention()
		{
			if (selectPanel != eTweetPanel.eMention)
				notiMention.isOn = true;
			FlashWindow();
		}

		public void NotiDM()
		{
			if (selectPanel != eTweetPanel.eDm)
				notiDm.isOn = true;
			FlashWindow();
		}

		private void FlashWindow()
		{
			if (IsActive == false)
				Generate.FlashWindowEx(this);
		}

		public void ClearWindow()
		{
			notiDm.isOn = false;
			notiMention.isOn = false;
			ClearInput();
		}

		private void LoadAccount()
		{
			List<string> listAccount = FileInstence.GetAccountArray();
			for(int i=0;i<listAccount.Count;i++)
			{
				MenuItem item = new MenuItem();
				item.Header = listAccount[i];
				item.Name = listAccount[i];
				item.Click += menuItemChangeAccount_Click;
				menuItemAccount.Items.Add(item);
			}
		}

		private void DeleteAccount()
		{
			foreach(object obj in menuItemAccount.Items)
			{
				MenuItem item = obj as MenuItem;
				if (item == null) continue;
				if (item.Name == FileInstence.SelectedAccountName)
				{
					menuItemAccount.Items.Remove(item);
					break;
				}
			}
		}

		private void treeUser_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

			if (treeViewItem != null)
			{
				treeViewItem.Focus();
				//e.Handled = true;
			}
		}
		private TreeViewItem VisualUpwardSearch(DependencyObject source)
		{
			while (source != null && !(source is TreeViewItem))
				source = VisualTreeHelper.GetParent(source);

			return source as TreeViewItem;
		}

		private void inputTweetBox_Drop(object sender, DragEventArgs e)
		{
			mainWindow_Drop(sender, e);
		}

		private void inputTweetBox_PreviewDragOver(object sender, DragEventArgs e)
		{
			e.Handled = true;
			e.Effects = DragDropEffects.Copy;
		}

		private void inputTweetBox_PreviewDragEnter(object sender, DragEventArgs e)
		{
			e.Handled = true;
			e.Effects = DragDropEffects.Copy;
		}

		private void inputTweetBox_PreviewDrop(object sender, DragEventArgs e)
		{
			e.Handled = true;
			e.Effects = DragDropEffects.Copy;
			mainWindow_Drop(sender, e);
		}
	}			
}				
