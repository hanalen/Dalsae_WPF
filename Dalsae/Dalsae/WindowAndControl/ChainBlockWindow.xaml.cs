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
using Dalsae.API;
using Newtonsoft.Json;

namespace Dalsae.WindowAndControl
{
	/// <summary>
	/// ChainBlockWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class ChainBlockWindow : Window
	{
		private Data.UserInfo userInfo { get; set; }
		private bool isFollowing { get; set; }

		private HashSet<long> hashFriends = new HashSet<long>();
		private HashSet<long> hashBlockList = new HashSet<long>();
		//private HashSet<long> hashSkip = new HashSet<long>();

		private int blockCount { get; set; } = 0;
		private int blockAlreadyCount { get; set; } = 0;
		private int skipCount { get; set; } = 0;

		public ChainBlockWindow(Data.UserInfo userInfo, bool isFollowing)
		{
			InitializeComponent();
			this.ShowInTaskbar = true;
			SetEvent();
			this.userInfo = userInfo;
			this.isFollowing = isFollowing;
			tbBlockCount.DataContext = blockCount;
			tbAlreadyCount.DataContext = blockAlreadyCount;
			tbSkipCount.DataContext = skipCount;
			if (isFollowing)
				this.Title = $"{userInfo.screen_name}의 팔로잉 전부 차단";
			else
				this.Title = $"{userInfo.screen_name}의 팔로워 전부 차단";
		}

		public void Start()
		{
			LoadMyFriends();
			LoadCoordList();
		}

		/// <summary>
		/// 내 팔로잉/팔로워부터 hash에 저장 하고 시작
		/// </summary>
		public void LoadMyFriends()
		{
			if (cbFollowing.IsChecked.Value)
				foreach (var item in DataManager.DataInstence.dicFollwing.Keys)
					hashFriends.Add(item);
		}

		#region 패킷 전송

		/// <summary>
		/// 좌표 유저 팔로잉 혹은 팔로워 불러오기
		/// </summary>
		public void LoadCoordList(long cursor = -1)
		{
			if (isFollowing)
			{
				statusBar.Content = "좌표의 팔로잉 불러오는 중";
				Manager.APICallAgent.apiInstence.GetFollowingIDS(userInfo.screen_name, cursor);
			}
			else
			{
				statusBar.Content = "좌표의 팔로워 불러오는 중";
				Manager.APICallAgent.apiInstence.GetFollowerIDS_Chain(userInfo.screen_name, cursor);
			}
		}

		private void StartBlock()
		{
			HashSet<long> hashRemove = new HashSet<long>();//블락 할 목록에서 제외 할 개수
			foreach (var item in hashBlockList)
			{
				if (hashFriends.Contains(item))
				{
					hashRemove.Add(item);
					//hashSkip.Add(item);
					skipCount++;
					tbSkipCount.Text = skipCount.ToString();
					Manager.APICallAgent.apiInstence.GetUserInfo_Chain(item);
				}
				else if (DataManager.DataInstence.blockList.hashBlockUsers.Contains(item))
				{
					hashRemove.Add(item);
					blockAlreadyCount++;
					tbAlreadyCount.Text = blockAlreadyCount.ToString();
				}
			}
			foreach (long item in hashRemove)//차단에서 제외 할 목록 
				hashBlockList.Remove(item);

			if (hashBlockList.Count == 0)//이미 다 블락 돼있을 경우
			{
				progressBar.Value = 100;
				tbBlockCount.Text = $"{blockAlreadyCount} / {blockAlreadyCount}";
			}
			statusBar.Content = "차단 중...";
			foreach (var item in hashBlockList)
				Manager.APICallAgent.apiInstence.Block(item);
		}
		#endregion


		#region 리스폰스
		private void OnFollowingIDS(ClientBlockIds ids)
		{
			foreach (var item in ids.ids)
				hashBlockList.Add(item);

			if (ids.next_cursor <= 0)//목록을 다 불러왔으면 블락 시작
				StartBlock();
			else
				LoadCoordList(ids.next_cursor);
		}

		private void OnFollowerIDS(ClientBlockIds ids)
		{
			foreach (var item in ids.ids)
				hashBlockList.Add(item);
			if (ids.next_cursor <= 0)//목록을 다 불러왔으면 블락 시작
				StartBlock();
			else
				LoadCoordList(ids.next_cursor);
		}

		private void OnBlock(Data.UserInfo userInfo)
		{
			blockCount++;
			progressBar.Value = ((double)(blockCount) / hashBlockList.Count) * 100;
			tbBlockCount.Text = $"{blockCount.ToString()} / {hashBlockList.Count}";
			if (blockCount == hashBlockList.Count)
				statusBar.Content = "완료";
		}
		#endregion

		#region 이벤트목록
		private void SetEvent()
		{
			Manager.ResponseAgent.responseInstence.OnBlock += OnBlock;
			Manager.ResponseAgent.responseInstence.OnFollowingIDS += OnFollowingIDS;
			Manager.ResponseAgent.responseInstence.OnFollowerIDS_Chain += OnFollowerIDS;
			Manager.ResponseAgent.responseInstence.OnUserinfo_Chain += OnUserInfo;
			TwitterWeb.WebInstence.OnResponseError += ResponseError;
		}

		private void OnUserInfo(Data.UserInfo userInfo)
		{
			listBox.Items.Add($"{userInfo.name} / {userInfo.screen_name}");
		}

		/// <summary>
		/// 리밋 처리를 처리하기 위해 리스폰스 에러를 추가 해준다.
		/// </summary>
		/// <param name="packet"></param>
		/// <param name="json"></param>
		public async void ResponseError(BasePacket packet, string json)
		{
			if (json == null) return;
			ClientAPIError error = JsonConvert.DeserializeObject<ClientAPIError>(json);
			if (error == null) return;
			if (error?.errors?.Count == 0) return;
			///리밋 걸렸을 때
			if (error.errors[0].code == 88)
			{
				if (packet.eresponse == eResponse.FOLLOWER_IDS)
				{
					statusBar.Content = "좌표의 팔로워 불러오기 리밋상태, 대기 최대 15분";
					await Task.Delay(TimeSpan.FromMinutes(1));
					int? num = Task.CurrentId;
					PacketFollowerIds bpacket = packet as PacketFollowerIds;
					long cursor = -1;
					long.TryParse(bpacket.cursor.ToString(), out cursor);

					LoadCoordList(cursor);
				}
				else if (packet.eresponse == eResponse.FOLLOWING_IDS)
				{
					statusBar.Content = "좌표의 팔로잉 불러오기 리밋상태, 대기 최대 15분";
					await Task.Delay(TimeSpan.FromMinutes(1));
					int? num = Task.CurrentId;
					PacketFollowingIds bpacket = packet as PacketFollowingIds;
					long cursor = -1;
					long.TryParse(bpacket.cursor.ToString(), out cursor);

					LoadCoordList(cursor);
				}
				else if (packet.eresponse == eResponse.BLOCK_CREAE)
				{
					statusBar.Content = "차단 리밋!";
				}
			}
		}


		private void RemoveEvent()
		{
			Manager.ResponseAgent.responseInstence.OnBlock -= OnBlock;
			Manager.ResponseAgent.responseInstence.OnFollowingIDS -= OnFollowingIDS;
			Manager.ResponseAgent.responseInstence.OnFollowerIDS_Chain -= OnFollowerIDS;
			Manager.ResponseAgent.responseInstence.OnUserinfo_Chain -= OnUserInfo;
			TwitterWeb.WebInstence.OnResponseError -= ResponseError;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			RemoveEvent();
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			cbFollower.Visibility = Visibility.Hidden;
			cbFollowing.Visibility = Visibility.Hidden;
			button.Visibility = Visibility.Hidden;
			Start();
		}
		#endregion
	}
}
