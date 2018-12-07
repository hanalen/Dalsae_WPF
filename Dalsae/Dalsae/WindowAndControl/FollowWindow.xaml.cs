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
using static Dalsae.TwitterWeb;
using static Dalsae.DalsaeManager;
using static Dalsae.DataManager;
using Newtonsoft.Json;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.IO;
using Dalsae.API;
using Dalsae.Data;

namespace Dalsae.WindowAndControl
{
	/// <summary>
	/// FollowWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class FollowWindow : Window
	{
		private class UserList : ObservableCollection<UserInfo>	{ public UserList() : base() { } }
		//private UserInfo userMe;
		private delegate void DeleAddUsers(Users users);
		private delegate void DeleUpdateUser(UserInfo user, bool isFollow);
		private Dictionary<eUserPanel, ListBoxUserManager> dicPanel = new Dictionary<eUserPanel, ListBoxUserManager>();
		private eUserPanel selectPanel = eUserPanel.eFollowing;

		public BitmapImage gongSikImage { get; set; } = new BitmapImage();
		public BitmapImage bitmapLock { get; set; } = new BitmapImage();
		public BitmapImage bitmapSetting { get; set; } = new BitmapImage();
		private enum eUserPanel
		{
			eFollowing,
			eFollower,
			eUserInfoing,
			eUserFollwer,
		}

		public FollowWindow(bool isFollowing)
		{
			LoadResources(bitmapLock, Properties.Resources.lockPick_Large);
			LoadResources(bitmapSetting, Properties.Resources.setting);
			
			LoadWindow(DataManager.DataInstence.userInfo.user.screen_name);

			dicPanel.Add(eUserPanel.eFollower, new ListBoxUserManager(eUserPanel.eFollower, listboxFollower));
			dicPanel.Add(eUserPanel.eFollowing, new ListBoxUserManager(eUserPanel.eFollowing, listboxFollowing));
			dicPanel.Add(eUserPanel.eUserInfoing, new ListBoxUserManager(eUserPanel.eUserInfoing, listboxUserFollowing));
			dicPanel.Add(eUserPanel.eUserFollwer, new ListBoxUserManager(eUserPanel.eUserFollwer, listboxUserFollower));


			//Manager.APICallAgent.apiInstence.GetUserInfo();
			//userMe.Init();
			//gridUserTop.DataContext = userMe;
			
			if (isFollowing)
				following_Click(null, null);
			else
				follower_Click(null, null);
		}

		public FollowWindow(string screen_name)
		{
			LoadResources(bitmapLock, Properties.Resources.lockPick_Large);
			LoadResources(bitmapSetting, Properties.Resources.setting);
			LoadWindow(screen_name);

			dicPanel.Add(eUserPanel.eFollower, new ListBoxUserManager(eUserPanel.eFollower, listboxFollower));
			dicPanel.Add(eUserPanel.eFollowing, new ListBoxUserManager(eUserPanel.eFollowing, listboxFollowing));
			dicPanel.Add(eUserPanel.eUserInfoing, new ListBoxUserManager(eUserPanel.eUserInfoing, listboxUserFollowing));
			dicPanel.Add(eUserPanel.eUserFollwer, new ListBoxUserManager(eUserPanel.eUserFollwer, listboxUserFollower));

			string json = WebInstence.SyncRequest(new PacketVerifyCredentials());
			//userMe = JsonConvert.DeserializeObject<UserInfo>(json);
			//userMe.Init();

			json = WebInstence.SyncRequest(new PacketUserShow(screen_name));
			if(string.IsNullOrEmpty(json))
			{
				Title = "상대방 계정 정보 불러오기 오류";
				MessageBox.Show(this, "상대방 계정 정보를 불러 올 수 없습니다.","오류", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}
			UserInfo user = JsonConvert.DeserializeObject<UserInfo>(json);
			user.Init();
			gridUserTop.DataContext = user;
			Title = $"{user.screen_name} 의 프로필";
			
			//if (isFollowing)
			//	following_Click(null, null);
			//else
			//	follower_Click(null, null);
		}

		private void LoadWindow(string screenName = "")
		{
			InitializeComponent();
			Manager.ResponseAgent.responseInstence.OnUserInfo += OnUserInfo;
			Manager.APICallAgent.apiInstence.GetUserInfo(screenName);
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

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			foreach (ListBoxUserManager item in dicPanel.Values)
				item.SetScrollviewer();
			
		}

		private void OnUserInfo(UserInfo userInfo)
		{
			userInfo.Init();
			gridUserTop.DataContext = userInfo;
			Title = $"{userInfo.screen_name} 의 프로필";
		}

		private void hyperLink_Click(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			Process.Start(e.Uri.AbsoluteUri);
			e.Handled = true;
		}

		private void follower_Click(object sender, RoutedEventArgs e)
		{
			//Title = "나의 팔로워";
			ShowPanel(eUserPanel.eFollower, DataInstence.userInfo.user.screen_name);
		}

		private void following_Click(object sender, RoutedEventArgs e)
		{
			//Title = "나의 팔로잉";
			ShowPanel(eUserPanel.eFollowing, DataInstence.userInfo.user.screen_name);
		}

		private void listboxUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UserInfo selUser = dicPanel[selectPanel].listbox.SelectedItem as UserInfo;
			if (selUser == null) return;
			gridUserTop.DataContext = selUser;
		}

		private void buttonFollow_Click(object sender, RoutedEventArgs e)
		{
			UserInfo user = gridUserTop.DataContext as UserInfo;
			if (user == null) return;
			if (DataInstence.CheckIsMe(user.id)) return;//출력중인 게 본인이면 작동 x

			if (buttonFollow.Content.ToString() == "언팔로우 하기")
				ThreadPool.QueueUserWorkItem(UnFollow, new PacketUnFollow(user.screen_name));
			else if (buttonFollow.Content.ToString() == "팔로우 하기")
				ThreadPool.QueueUserWorkItem(Follow, new PacketFollow(user.screen_name));
		}

		private void labelTweetCount_MouseDown(object sender, MouseButtonEventArgs e)
		{
			UserInfo user = gridUserTop.DataContext as UserInfo;
			DalsaeInstence.LoadTweet(eTweetPanel.eUser, user.screen_name);
		}

		private void followingCount_MouseDown(object sender, MouseButtonEventArgs e)
		{
			UserInfo user = gridUserTop.DataContext as UserInfo;

			Title = $"{user.screen_name} 의 팔로잉";
			ShowPanel(eUserPanel.eUserInfoing, user.screen_name);
			
		}

		private void follwerCount_MouseDown(object sender, MouseButtonEventArgs e)
		{
			UserInfo user = gridUserTop.DataContext as UserInfo;

			Title = $"{user.screen_name} 의 팔로워";
			ShowPanel(eUserPanel.eUserFollwer, user.screen_name);
		}

		private void ShowPanel(eUserPanel panel, string screenName)
		{
			dicPanel[selectPanel].Hide();
			dicPanel[panel].Show();
			selectPanel = panel;
			dicPanel[panel].LoadUser(screenName);
		}

		//---------------------------------------------------------------------------------------------
		//----------------------------------컨텍스트메뉴--------------------------------------------
		//---------------------------------------------------------------------------------------------
		private void contextOnOpening(object sender, ContextMenuEventArgs e)
		{
			Grid grid = sender as Grid;
			if (grid == null) return;
			UserInfo user = grid.DataContext as UserInfo;
			if (user == null) return;

			grid.ContextMenu = CreateContextMenu(user);
		}

		private ContextMenu CreateContextMenu(UserInfo user)
		{
			ContextMenu contextMenu = new ContextMenu();
			MenuItem mi = new MenuItem();
			Separator sp = new Separator();
			if (user.block == false)
			{
				if (user.following)
				{
					mi.Header = "언팔로우";
					mi.Click += contextClick_UnFollow;
				}
				else
				{
					mi.Header = "팔로우";
					mi.Click += contextClick_Follow;
				}
				contextMenu.Items.Add(mi);
				contextMenu.Items.Add(sp);

				mi = new MenuItem();
				mi.Header = $"{user.screen_name} 차단하기";
				mi.Click += contextClick_Block;
				contextMenu.Items.Add(mi);
				sp = new Separator();
				contextMenu.Items.Add(sp);

				mi = new MenuItem();
				mi.Header = $"{user.screen_name} 뮤트하기";
				mi.Click += contextClick_Mute;
				contextMenu.Items.Add(mi);
				sp = new Separator();
				contextMenu.Items.Add(sp);

				mi = new MenuItem();
				mi.Header = $"{user.screen_name} 블락 언블락";
				mi.Click += contextClick_BlockUnBlock;
				contextMenu.Items.Add(mi);
				sp = new Separator();
				contextMenu.Items.Add(sp);
			}
			else
			{
				mi.Header = "차단 해제";
				mi.Click += contextClick_UnBlock;
				contextMenu.Items.Add(mi);

				sp = new Separator();
				contextMenu.Items.Add(sp);
			}
		
			if (user.block == false)
			{
				mi = new MenuItem();
				mi.Header = $"{user.screen_name}의 트윗 보기";
				mi.Click += contextClick_showTweet;
				contextMenu.Items.Add(mi);
				sp = new Separator();
				contextMenu.Items.Add(sp);
			}
			
			mi = new MenuItem();
			mi.Header = $"{user.screen_name}의 팔로잉 보기";
			mi.Click += contextClick_showFollowing;
			contextMenu.Items.Add(mi);

			mi = new MenuItem();
			mi.Header = $"{user.screen_name}의 팔로워 보기";
			mi.Click += contextClick_showFollower;
			contextMenu.Items.Add(mi);

			

			return contextMenu;
		}

		private void contextClick_Mute(object sender, RoutedEventArgs e)
		{
			UserInfo user = dicPanel[selectPanel].listbox.SelectedItem as UserInfo;
			if (user == null) return;

			DataInstence.option.AddMuteUser(user.screen_name);
			FileManager.FileInstence.UpdateOption(DataInstence.option);
		}

		private void contextClick_Follow(object sender, RoutedEventArgs e)
		{
			UserInfo user = dicPanel[selectPanel].listbox.SelectedItem as UserInfo;
			if (user == null) return;

			ThreadPool.QueueUserWorkItem(Follow, new PacketFollow(user.screen_name));
		}

		private void contextClick_UnFollow(object sender, RoutedEventArgs e)
		{
			UserInfo user = dicPanel[selectPanel].listbox.SelectedItem as UserInfo;
			if (user == null) return;

			ThreadPool.QueueUserWorkItem(UnFollow, new PacketUnFollow(user.screen_name));
		}

		private void contextClick_Block(object sender, RoutedEventArgs e)
		{
			MessageBoxResult mr = MessageBox.Show("차단 하시겠습니까?", "알림", MessageBoxButton.YesNo, MessageBoxImage.None);
			if (mr != MessageBoxResult.Yes) return;

			UserInfo user = gridUserTop.DataContext as UserInfo;
			if (user == null) return;

			ThreadPool.QueueUserWorkItem(Block, new PacketBlockCreate(user.id));
		}
		private void contextClick_UnBlock(object sender, RoutedEventArgs e)
		{
			UserInfo user = dicPanel[selectPanel].listbox.SelectedItem as UserInfo;
			if (user == null) return;

			ThreadPool.QueueUserWorkItem(UnBlock, new PacketBlockDestroy(user.id));
		}
		private void contextClick_BlockUnBlock(object sender, RoutedEventArgs e)
		{
			MessageBoxResult mr = MessageBox.Show("블락 언블락 하시겠습니까?", "알림", MessageBoxButton.YesNo, MessageBoxImage.None);
			if (mr != MessageBoxResult.Yes) return;

			UserInfo user = dicPanel[selectPanel].listbox.SelectedItem as UserInfo;
			if (user == null) return;

			ThreadPool.QueueUserWorkItem(BlockUnblock, user.id);
		}

		private void contextClick_showFollowing(object sender, RoutedEventArgs e)
		{
			UserInfo user = dicPanel[selectPanel].listbox.SelectedItem as UserInfo;
			if (user == null) return;
			ShowPanel(eUserPanel.eUserInfoing, user.screen_name);
		}

		private void contextClick_showFollower(object sender, RoutedEventArgs e)
		{
			UserInfo user = dicPanel[selectPanel].listbox.SelectedItem as UserInfo;
			if (user == null) return;

			ShowPanel(eUserPanel.eUserFollwer, user.screen_name);
		}

		private void contextClick_showTweet(object sender, RoutedEventArgs e)
		{
			UserInfo user = dicPanel[selectPanel].listbox.SelectedItem as UserInfo;
			if (user == null) return;

			DalsaeInstence.LoadTweet(eTweetPanel.eUser, user.screen_name);
		}

		private void Follow(object obj)
		{
			PacketFollow parameter = obj as PacketFollow;
			if (parameter == null) return;

			string json = WebInstence.SyncRequest(parameter);
			if (json.Length == 0)
			{
				MessageBox.Show("팔로우 제한! 몇 분 뒤 다시 시도해주세요(최대 15분)", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}
			try
			{
				UserInfo user = JsonConvert.DeserializeObject<UserInfo>(json);
				UpdateUser(user, true);
			}
			catch
			{
				MessageBox.Show("팔로우 오류", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		private void UnFollow(object obj)
		{
			PacketUnFollow parameter = obj as PacketUnFollow;
			if (parameter == null) return;

			string json = WebInstence.SyncRequest(parameter);
			if (json.Length == 0)
			{
				MessageBox.Show("언팔로우 제한! 몇 분 뒤 다시 시도해주세요(최대 15분)", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}
			try
			{
				UserInfo user = JsonConvert.DeserializeObject<UserInfo>(json);
				UpdateUser(user, false);
			}
			catch
			{
				MessageBox.Show("언팔로우 오류", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		private void Block(object obj)
		{
			PacketBlockCreate parameter = obj as PacketBlockCreate;
			if (parameter == null) return;

			string json = WebInstence.SyncRequest(parameter);
			if (json.Length == 0)
			{
				MessageBox.Show("차단 제한! 몇 분 뒤 다시 시도해주세요(최대 15분)", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}
			try
			{
				UserInfo user = JsonConvert.DeserializeObject<UserInfo>(json);
				UpdateBlock(user, true);
			}
			catch
			{
				MessageBox.Show("차단 오류", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}
		private void UnBlock(object obj)
		{
			PacketBlockDestroy parameter = obj as PacketBlockDestroy;
			if (parameter == null) return;


			string json = WebInstence.SyncRequest(parameter);
			if (json.Length == 0)
			{
				MessageBox.Show("차단 해제 제한! 몇 분 뒤 다시 시도해주세요(최대 15분)", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}
			try
			{
				UserInfo user = JsonConvert.DeserializeObject<UserInfo>(json);
				UpdateBlock(user, false);
			}
			catch { MessageBox.Show("차단 해제 오류", "오류", MessageBoxButton.OK, MessageBoxImage.Warning); }
		}
		private void BlockUnblock(object obj)
		{
			if (obj is long == false) return;

			long id = (long)obj;
			PacketBlockCreate create = new PacketBlockCreate(id);
			string json = WebInstence.SyncRequest(create);
			if (json.Length == 0)
			{
				MessageBox.Show("차단 제한! 몇 분 뒤 다시 시도해주세요(최대 15분)", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}
			try { UserInfo user = JsonConvert.DeserializeObject<UserInfo>(json); }
			catch { MessageBox.Show("차단 오류", "오류", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

			PacketBlockDestroy destroy = new PacketBlockDestroy(id);
			string json2 = WebInstence.SyncRequest(destroy);
			if (json.Length == 0)
			{
				MessageBox.Show("차단 해제 제한! 몇 분 뒤 다시 시도해주세요(최대 15분)", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}
			try
			{
				UserInfo user = JsonConvert.DeserializeObject<UserInfo>(json2);
				UpdateBlock(user, false);
			}
			catch { MessageBox.Show("차단 해제 오류", "오류", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
		}

		private void UpdateBlock(UserInfo user, bool isBlock)
		{
			DeleUpdateUser dele1 = new DeleUpdateUser(UpdateTopUserBlock);
			Dispatcher.BeginInvoke(dele1, new object[] { user, isBlock });
			foreach (ListBoxUserManager listbox in dicPanel.Values)
			{
				ListBoxUserManager.DeleUpdateBlock dele = new ListBoxUserManager.DeleUpdateBlock(listbox.UpdateUserBolck);
				Dispatcher.BeginInvoke(dele, new object[] { user.id, isBlock});
			}
		}

		private void UpdateUser(UserInfo user, bool isFollow)
		{
			DeleUpdateUser dele1 = new DeleUpdateUser(UpdateTopUserInfo);
			Dispatcher.BeginInvoke(dele1, new object[] { user, isFollow });

			foreach(ListBoxUserManager listbox in dicPanel.Values)
			{
				ListBoxUserManager.DeleUpdateFollow dele = new ListBoxUserManager.DeleUpdateFollow(listbox.UpdateUserInfo);
				Dispatcher.BeginInvoke(dele, new object[] { user, isFollow });
			}
		}

		/// <summary>
		/// 상단 UI업데이트용 딜리게이트 호출 될 함수, 블락 갱신
		/// </summary>
		/// <param name="user"></param>
		/// <param name="isBlock"></param>
		private void UpdateTopUserBlock(UserInfo user, bool isBlock)
		{
			UserInfo userTop = gridUserTop.DataContext as UserInfo;
			if (userTop == null) return;

			if (userTop.id == user.id)
				userTop.block = isBlock;
		}

		/// <summary>
		/// 상단 UI업데이트용 딜리게이트 호출 될 함수, 팔로잉 갱신
		/// </summary>
		/// <param name="user"></param>
		/// <param name="isFollow"></param>
		private void UpdateTopUserInfo(UserInfo user, bool isFollow)
		{
			UserInfo userTop = gridUserTop.DataContext as UserInfo;
			if (userTop == null) return;

			if (userTop.id == user.id)
				userTop.following = isFollow;
		}
		
		//---------------------------------------------------------------------------------------------
		//----------------------------------클래스---------------------------------------------------
		//---------------------------------------------------------------------------------------------

		private class Users
		{
			public long previous_cursor { get; set; }
			public string previous_cursor_str { get; set; }
			public long next_cursor { get; set; }
			public UserInfo[] users;
		}

		private class ListBoxUserManager
		{
			public delegate void DeleUpdateFollow(UserInfo user, bool isFollow);
			public delegate void DeleUpdateBlock(long id, bool isBolck);
			private string screenName { get; set; }
			private eUserPanel epanel { get; set; }
			public UserList userList { get; private set; } = new UserList();
			public string title { get; private set; }
			public ListBox listbox { get; private set; }
			public long nextCursor { get; set; } = -1;
			private ScrollViewer scroll { get; set; }

			public ListBoxUserManager(eUserPanel epanel, ListBox listbox)
			{
				this.listbox = listbox;
				this.epanel = epanel;
				listbox.ItemsSource = userList;
			}

			public void SetScrollviewer()
			{
				scroll = Generate.FindElementByName<ScrollViewer>(listbox);
				scroll.ScrollChanged += ScrollViewer_ScrollChanged;
			}

			public void Show()
			{
				listbox.Visibility = Visibility.Visible;
			}

			public void Hide()
			{
				listbox.Visibility = Visibility.Hidden;
			}

			//패널이바뀔때마다 로딩해야되나 체크하고
			//로딩해야되면 로딩하는 방식
			//screenName: 유저 아이디가 넘어옴
			public void LoadUser(string screenName)
			{
				//이전에 로딩한 정보와 동일, 첫로딩 후 아이디를 저장하기에 자기 정보도 최초 로딩은 하게 됨
				if (screenName == this.screenName) return;

				this.screenName = screenName;
				userList.Clear();
				BasePacket parameter = null;
				if (epanel == eUserPanel.eFollower || epanel == eUserPanel.eUserFollwer)
				{
					PacketFollowerList param = new PacketFollowerList();
					param.screen_name = screenName;
					parameter = param;
				}
				else
				{
					PacketFollowingList param = new PacketFollowingList();
					param.screen_name = screenName;
					parameter = param;
				}
				ThreadPool.QueueUserWorkItem(LoadUser, parameter);
			}

			public void UpdateUserInfo(UserInfo user, bool isFollow)
			{
				for (int i = 0; i < userList.Count; i++)
				{
					if (user.id == userList[i].id)
					{
						userList[i].following = isFollow;
						break;
					}
				}
			}

			public void UpdateUserBolck(long id, bool isBlock)
			{
				for (int i = 0; i < userList.Count; i++)
				{
					if (userList[i].id == id)
					{
						userList[i].block = isBlock;
						userList[i].following = false;
						break;
					}
				}
			}

			private void AddUser(Users users)
			{
				for (int i = 0; i < users.users.Length; i++)
				{
					users.users[i].Init();
					userList.Add(users.users[i]);
				}
				nextCursor = users.next_cursor;
			}

			private void AddMore(Users users)
			{
				for (int i = 0; i < users.users.Length; i++)
				{
					users.users[i].Init();
					userList.Insert(userList.Count, users.users[i]);
				}
				nextCursor = users.next_cursor;
			}

			private void LoadMore()
			{
				if (nextCursor == 0) return;
				BasePacket parameter = null;
				if (epanel == eUserPanel.eFollower || epanel == eUserPanel.eUserFollwer)
				{
					PacketFollowerList param = new PacketFollowerList();
					param.cursor = nextCursor.ToString();
					parameter = param;
				}
				else
				{
					PacketFollowingList param = new PacketFollowingList();
					param.cursor = nextCursor.ToString();
					parameter = param;
				}
				ThreadPool.QueueUserWorkItem(LoadMore, parameter);
			}

			private void LoadUser(object obj)
			{
				BasePacket parameter = obj as BasePacket;
				if (parameter == null) return;

				string json = WebInstence.SyncRequest(parameter);
				if (json.Length == 0)
				{
					MessageBox.Show("불러오기 제한! 몇 분 뒤 다시 시도해주세요(최대 15분)", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
				Users users = JsonConvert.DeserializeObject<Users>(json);
				DeleAddUsers dele = new DeleAddUsers(AddUser);
				Application.Current.Dispatcher.BeginInvoke(dele, new object[] { users });
			}

			private void LoadMore(object obj)
			{
				BasePacket parameter = obj as BasePacket;
				if (parameter == null) return;

				string json = WebInstence.SyncRequest(parameter);
				if (json.Length == 0)
				{
					MessageBox.Show("불러오기 제한! 몇 분 뒤 다시 시도해주세요(최대 15분)", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				Users users = JsonConvert.DeserializeObject<Users>(json);
				DeleAddUsers dele = new DeleAddUsers(AddMore);
				Application.Current.Dispatcher.BeginInvoke(dele, new object[] { users });
			}

			private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
			{
				if (scroll.VerticalOffset == scroll.ScrollableHeight)
					LoadMore();
			}

		}

		private void contextClick_ChainFollowing(object sender, RoutedEventArgs e)
		{
			ChainBlockWindow win = new ChainBlockWindow(gridUserTop.DataContext as UserInfo, true);
			win.Show();
		}

		private void contextClick_ChainFollwer(object sender, RoutedEventArgs e)
		{
			ChainBlockWindow win = new ChainBlockWindow(gridUserTop.DataContext as UserInfo, false);
			win.Show();
		}

		private void image_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				Image image = sender as Image;
				ContextMenu contextMenu = image.ContextMenu;
				contextMenu.PlacementTarget = image;
				contextMenu.IsOpen = true;
			}
		}
	}
}
