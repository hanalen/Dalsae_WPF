using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static Dalsae.FileManager;
using static Dalsae.DalsaeManager;
using static Dalsae.TwitterWeb;
using static Dalsae.DataManager;
using System.Collections.ObjectModel;
using Dalsae.API;
using Dalsae.Data;

namespace Dalsae
{

	public class TweetManager
	{
		private const int ConMaxTweet = 1000;
		//----------------------------------------------------------------------------------------------------
		//--------------------------------------기본 생성--------------------------------------------------
		//----------------------------------------------------------------------------------------------------
		private static readonly object lockObject = new object();
		private static TweetManager instence;
		public static TweetManager TweetInstence
		{
			get
			{
				if (instence == null)
					instence = new TweetManager();
				return instence;
			}
		}
		private TweetManager(){	Init();	}
		private void Init()
		{
			dicTweetTree.Add(eTweetPanel.eHome, new TweetTree());
			dicTweetTree.Add(eTweetPanel.eMention, new TweetTree());
			dicTweetTree.Add(eTweetPanel.eOpen, new TweetTree());
			dicTweetTree.Add(eTweetPanel.eUser, new TweetTree());
			dicTweetTree.Add(eTweetPanel.eFavorite, new TweetTree());
			dicTweetTree.Add(eTweetPanel.eUserMedia, new TweetTree());

			//----------더불러오기 버튼-----------
			dicTweetTree[eTweetPanel.eHome].Add(new ClientTweet());
			dicTweetTree[eTweetPanel.eMention].Add(new ClientTweet());
			dicTweetTree[eTweetPanel.eUser].Add(new ClientTweet());
			dicTweetTree[eTweetPanel.eFavorite].Add(new ClientTweet());
			dicTweetTree[eTweetPanel.eUserMedia].Add(new ClientTweet());
			//----------------------------------------

			dicHashs.Add(eTweetPanel.eHome, new HashSet<long>());
			dicHashs.Add(eTweetPanel.eMention, new HashSet<long>());
			dicHashs.Add(eTweetPanel.eOpen, new HashSet<long>());
			dicHashs.Add(eTweetPanel.eUser, new HashSet<long>());
			dicHashs.Add(eTweetPanel.eFavorite, new HashSet<long>());
			dicHashs.Add(eTweetPanel.eDm, new HashSet<long>());
			dicHashs.Add(eTweetPanel.eUserMedia, new HashSet<long>());

			dicBack.Add(eTweetPanel.eHome, true);
			dicBack.Add(eTweetPanel.eMention, true);
			dicBack.Add(eTweetPanel.eDm, true);
			dicBack.Add(eTweetPanel.eFavorite, true);
			dicBack.Add(eTweetPanel.eUser, true);
			dicBack.Add(eTweetPanel.eUserMedia, true);
			dicBack.Add(eTweetPanel.eOpen, true);
		}


		//----------------------------------------------------------------------------------------------------
		//--------------------------------------변수---------------------------------------------------------
		//----------------------------------------------------------------------------------------------------
		//public delegate void DeleAddTweet(eTweetPanel panel, ClientTweet tweet);
		//public delegate void DeleAddTweetList(eTweetPanel panel, List<ClientTweet> listTweet, bool isMore);
		//public delegate void DeleAddDM(ClientDirectMessage dm);
		//public delegate void DeleAddDMList(List<ClientDirectMessage> listDm);
		//public delegate void DeleRrAndFav(ClientTweet tweet, bool isActive);
		//public delegate void DDeleteTweet(ClientStreamDelete delete);
		public delegate void DeleClear();
		public delegate void DQTTweet(ClientTweet tweet, UIProperty property);
		public event DQTTweet OnQt = null;
		//패널별로 트윗 저장, 유저트윗의 경우 남겨두지 않음
		private Dictionary<eTweetPanel, ObservableCollection<ClientTweet>> dicTweetTree = new Dictionary<eTweetPanel, ObservableCollection<ClientTweet>>();
		private Dictionary<eTweetPanel, HashSet<long>> dicHashs = new Dictionary<eTweetPanel, HashSet<long>>();
		public ObservableCollection<ClientTweet> treeHome { get { return dicTweetTree[eTweetPanel.eHome]; } }
		public ObservableCollection<ClientTweet> treeMention { get { return dicTweetTree[eTweetPanel.eMention]; } }
		public ObservableCollection<ClientTweet> treeOpen { get { return dicTweetTree[eTweetPanel.eOpen]; } }
		public ObservableCollection<ClientTweet> treeUser { get { return dicTweetTree[eTweetPanel.eUser]; } }
		public ObservableCollection<ClientTweet> treeUserMedia { get { return dicTweetTree[eTweetPanel.eUserMedia]; } }
		public ObservableCollection<ClientTweet> treeFavorite { get { return dicTweetTree[eTweetPanel.eFavorite]; } }
		public ObservableCollection<ClientDirectMessage> treeDM { get; private set; } = new ObservableCollection<ClientDirectMessage>();

		private Dictionary<eTweetPanel, bool> dicBack = new Dictionary<eTweetPanel, bool>();

		//클라이언트 초기화 시 전부 초기화
		public void Clear()
		{
			treeHome.Clear();
			treeMention.Clear();
			treeOpen.Clear();
			treeUser.Clear();
			treeFavorite.Clear();
			treeDM.Clear();
			treeUserMedia.Clear();
			foreach(HashSet<long> item in dicHashs.Values)
			{
				item.Clear();
			}
			dicTweetTree[eTweetPanel.eHome].Add(new ClientTweet());
			dicTweetTree[eTweetPanel.eMention].Add(new ClientTweet());
			dicTweetTree[eTweetPanel.eUser].Add(new ClientTweet());
			dicTweetTree[eTweetPanel.eFavorite].Add(new ClientTweet());
			dicTweetTree[eTweetPanel.eUserMedia].Add(new ClientTweet());
		}

		//스트리밍에서 호출하게 될 트윗 추가
		public void AddTweet(eTweetPanel panel, ClientTweet tweet)
		{
			lock (lockObject)
			{
				tweet.Init();
				//if (DataInstence.CheckIsMe(tweet.user.id))//내트윗일 경우 작업
				//{
				//	DataInstence.UpdateMyScreenName(tweet.user.screen_name);//아이디 변경 체크
				//	DataInstence.UpdateMyProfilePicture(tweet.user.profile_image_url);//인장 변경 체크
				//}

				if (dicHashs[panel].Contains(tweet.id)) return;//중복 트윗
				if (DataInstence.isShowTweet(tweet, panel) == false) return;//트윗 미표시

				bool isShowTweet = true;
				if (panel == eTweetPanel.eHome)
				{
					if (DataInstence.option.MatchHighlight(tweet.originalTweet.text) || tweet.isMention)//하이라이트,멘션 멘션에 추가
						AddTweetData(eTweetPanel.eMention, tweet);
					else if (DataInstence.CheckIsMe(tweet.originalTweet.user.id) && tweet.retweeted)//내 트윗 체크(리트윗용)
					{
						isShowTweet = DataInstence.option.isShowRetweet;//리트윗을 TL표시할경우 추가
						if (isShowTweet && DataInstence.option.isNotiRetweet)//리트윗 멘션함에 오게 할 경우 추가
							AddTweetData(eTweetPanel.eMention, tweet);
						//if (DataInstence.CheckIsMe(tweet.user.id) && tweet.retweeted_status != null)//내가 한 리트윗
						//	Retweet(tweet, true);
					}

				}
				if (isShowTweet)
					AddTweetData(panel, tweet);

				//if (DataInstence.CheckIsMe(tweet.user.id) && tweet.retweeted_status != null)//리트윗 갱신
				//	Retweet(tweet, true);
			}
		}

		public void ClearUserTweet()//유저 트윗은 하나만 저장
		{
			dicTweetTree[eTweetPanel.eUser].Clear();
			dicHashs[eTweetPanel.eUser].Clear();
			dicTweetTree[eTweetPanel.eUser].Add(new ClientTweet());
		}

		public long GetTopTweet(eTweetPanel panel)
		{
			if (dicTweetTree[panel].Count > 0)
				return dicTweetTree[panel][0].id;
			else
				return -1;
		}

		public void ClearTweet(eTweetPanel panel)
		{
			if(panel== eTweetPanel.eUser)
			{
				ClearUserTweet();
			}
			else if(panel == eTweetPanel.eFavorite)
			{
				dicTweetTree[panel].Clear();
				dicHashs[panel].Clear();
				dicTweetTree[panel].Add(new ClientTweet());
			}
			else if(panel== eTweetPanel.eDm)
			{
				treeDM.Clear();
				dicHashs[panel].Clear();
			}
		}

		public void AddTweet(eTweetPanel panel, List<ClientTweet> listTweet, bool isMore)
		{
			if (isMore)//더 불러오기는 순서가 일반과 반대
			{
				for (int i = 0; i<listTweet.Count; i++)//API콜은 역순으로 등록
				{
					listTweet[i].Init();
					if (dicHashs[panel].Contains(listTweet[i].id)) continue;//중복 트윗
					if (DataInstence.isShowTweet(listTweet[i], panel) == false) continue;//트윗 미표시

					AddTweetData(panel, listTweet[i], isMore);
				}
			}
			else
			{
				if (panel == eTweetPanel.eUser) ClearUserTweet();//유저트윗 초기화
				for (int i = listTweet.Count - 1; i >= 0; i--)//더 불러오기가 아닌 거
				{
					listTweet[i].Init();
					if (dicHashs[panel].Contains(listTweet[i].id)) continue;//중복 트윗
					if (DataInstence.isShowTweet(listTweet[i], panel) == false) continue;//트윗 미표시

					AddTweetData(panel, listTweet[i], isMore);
				}
			}

			SortTweet(panel);
		}

		public void DeleteTweet(ClientStreamDelete tweet)
		{
			ClientTweet deleteRetweet = null;
			lock (lockObject)
			{
				foreach (KeyValuePair<eTweetPanel, ObservableCollection<ClientTweet>> list in dicTweetTree)
				{
					for (int i = 0; i < list.Value.Count; i++)
					{
						if (list.Value[i].originalTweet == null) continue;//버튼!!!
						if (list.Value[i].id == tweet.delete.status.id)//일반 트윗 체크
						{
							list.Value[i].uiProperty.isDeleteTweet = true;
							dicHashs[list.Key].Remove(list.Value[i].id);
							list.Value[i].originalTweet.retweeted = false;
							deleteRetweet = list.Value[i];
						}
						else if (list.Value[i].originalTweet.id == tweet.delete.status.id)//리트윗 체크, 원본 터지면 같이 삭제
						{
							list.Value[i].uiProperty.isDeleteTweet = true;
							dicHashs[list.Key].Remove(list.Value[i].originalTweet.id);
							list.Value[i].originalTweet.retweeted = false;
							deleteRetweet = list.Value[i];
						}
					}
				}
			}

			//if (deleteRetweet != null)
				//Retweet(deleteRetweet, false);
		}

		public void FavoriteTweet(ClientTweet tweet, bool isFavorite)
		{
			lock (lockObject)
			{
				foreach (TweetTree list in dicTweetTree.Values)
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].originalTweet == null) continue;//버튼!!!
						if (list[i].originalTweet.id == tweet.id)
							list[i].originalTweet.favorited = isFavorite;
					}
			}
		}

		public void Retweet(ClientTweet tweet, bool isRetweet)
		{
			lock (lockObject)
			{
				foreach (TweetTree list in dicTweetTree.Values)
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].originalTweet == null) continue;//버튼!!!
						if (list[i].originalTweet.id == tweet.originalTweet.id)
						{
							list[i].originalTweet.retweeted = isRetweet;
							list[i].retweeted = isRetweet;
						}
						if (list[i].originalTweet.id == tweet.id)
						{
							list[i].originalTweet.retweeted = isRetweet;
							list[i].retweeted = isRetweet;
						}
						if (list[i].id == tweet.id)
						{
							list[i].originalTweet.retweeted = isRetweet;
							list[i].retweeted = isRetweet;
						}
						if (list[i].id == tweet.originalTweet.id)
						{
							list[i].originalTweet.retweeted = isRetweet;
							list[i].retweeted = isRetweet;
						}
					}
			}
		}

		private void AddTweetData(eTweetPanel panel, ClientTweet tweet, bool isMore = false)
		{
			dicHashs[panel].Add(tweet.id);
			SetBackground(panel, tweet);
			if (isMore == false)
			{
				if (panel == eTweetPanel.eHome || panel == eTweetPanel.eDm)
				{
					if (dicTweetTree[panel].Count > ConMaxTweet)
					{
						dicHashs[panel].Remove(dicTweetTree[panel][dicTweetTree[panel].Count - 1].id);
						dicTweetTree[panel].RemoveAt(dicTweetTree[panel].Count - 1);
					}
				}
				else
				{
					if (dicTweetTree[panel].Count > ConMaxTweet)
					{
						dicHashs[panel].Remove(dicTweetTree[panel][dicTweetTree[panel].Count - 2].id);
						dicTweetTree[panel].RemoveAt(dicTweetTree[panel].Count - 2);
					}
				}
				dicTweetTree[panel].Insert(0, tweet);//list의 앞에 등록해야 순차적으로 표시됨
			}
			else
				dicTweetTree[panel].Insert(dicTweetTree[panel].Count - 1, tweet);//더 불러오기 버튼 앞에 추가
			if (panel == eTweetPanel.eMention)
				tweet.uiProperty.isHighlight = true;

			if (panel == eTweetPanel.eMention || panel == eTweetPanel.eDm)//알림
				DalsaeInstence.OnMentionNoti();
			if(tweet.isQTRetweet)
			{
				Manager.APICallAgent.apiInstence.LoadSingleTweet(tweet, tweet.originalTweet.quoted_status_id_str);
			}
			//Application.Current.Dispatcher.BeginInvoke(OnQt, new object[] { tweet });
			//OnQt?.Invoke(tweet);
			//if (tweet.isQTRetweet)
			//{
			//	ThreadPool.QueueUserWorkItem(new WaitCallback(LoadQTRetweet), new object[] { tweet, panel });
			//}
		}

		private void SetBackground(eTweetPanel panel, ClientTweet tweet)
		{
			tweet.uiProperty.isBackOne = dicBack[panel];
			dicBack[panel] = !dicBack[panel];
		}

		private void SortTweet(eTweetPanel panel)
		{
			if (panel != eTweetPanel.eHome && panel != eTweetPanel.eMention) return;

			lock (lockObject)
			{
				List<ClientTweet> listTweet = dicTweetTree[panel].OrderByDescending(x => x.dateTime).ToList();
				dicTweetTree[panel].Clear();
				for (int i = 0; i < listTweet.Count; i++)
					dicTweetTree[panel].Add(listTweet[i]);
			}
		}

		public int FindTweet(eTweetPanel panel, ClientTweet startTweet, string findText)
		{
			ObservableCollection<ClientTweet> tweetList = dicTweetTree[panel];
			int startIndex = tweetList.IndexOf(startTweet);
			int ret = -1;
			lock (lockObject)
			{
				for (int i = startIndex + 1; i < tweetList.Count; i++)
				{
					if (tweetList[i].originalTweet == null) continue;
					if (tweetList[i].nameText.IndexOf(findText, StringComparison.CurrentCultureIgnoreCase) > -1 ||
						tweetList[i].full_text.IndexOf(findText, StringComparison.CurrentCultureIgnoreCase) > -1)
					{
						ret = i;
						break;
					}
				}
				if (ret == -1)//끝까지 못 찾았으니 한 번 더 검색
				{
					for (int i = 0; i < tweetList.Count; i++)
					{
						if (tweetList[i].originalTweet == null) continue;
						if (tweetList[i].nameText.IndexOf(findText, StringComparison.CurrentCultureIgnoreCase) > -1 ||
						tweetList[i].full_text.IndexOf(findText, StringComparison.CurrentCultureIgnoreCase) > -1)
						{
							ret = i;
							break;
						}
					}
				}
			}
			return ret;
		}
		
		public int FindNextUserTweet(ObservableCollection<ClientTweet> tweetList, int startIndex, long userId)
		{
			int ret = -1;

			lock (lockObject)
			{
				for (int i = startIndex + 1; i < tweetList.Count; i++)
				{
					if (tweetList[i].user?.id == userId)
					{
						ret = i;
						break;
					}
				}
			}
			return ret;
		}

		public int FindPrevUserTweet(ObservableCollection<ClientTweet> tweetList, int startIndex, long userId)
		{
			int ret = -1;

			if (startIndex < 1)
				startIndex = 1;

			lock (lockObject)
			{
				for (int i = startIndex - 1; i > -1; i--)
				{
					if (tweetList[i].user.id == userId)
					{
						ret = i;
						break;
					}
				}
			}
			return ret;
		}

		//public static void LoadQTRetweet(object obj)
		//{
		//	object[] arrObj = obj as object[];
		//	if (arrObj == null) return;
		//	ClientTweet tweet = arrObj[0] as ClientTweet;
		//	if (tweet == null) return;
		//	PacketSingleTweet parameter = new PacketSingleTweet(tweet.originalTweet.quoted_status_id_str);
		//	string json = WebInstence.SyncRequest(parameter);
		//	ClientTweet qtTweet =JsonConvert.DeserializeObject<ClientTweet>(json);
		//	if (qtTweet == null)//플텍계나 블락유저일 경우 API콜 했을 때 패킷이 안 온다!
		//		qtTweet = new ClientTweet("트윗을 표시할 수 없습니다.");
		//	else
		//		qtTweet.Init();

		//	if (DataInstence.isShowTweet(qtTweet, (eTweetPanel)arrObj[1]) == false)//인용트윗 뮤트 확인 하고 뮤트 시 트윗 내용 엎음
		//		qtTweet = new ClientTweet("트윗이 뮤트되었습니다.");
		//	UIProperty.DeleAddSingleTweet dele = new UIProperty.DeleAddSingleTweet(tweet.uiProperty.AddSingleTweet);
		//	Application.Current.Dispatcher.BeginInvoke(dele, new object[] { qtTweet, tweet.uiProperty, true });
		//}
		//----------------------------------------------------------------------------------------------------
		//--------------------------------------DM---------------------------------------------------------
		//----------------------------------------------------------------------------------------------------
		public void AddDM(ClientDirectMessage dm)
		{
			dm.Init();
			AddDMData(dm);
			DalsaeInstence.OnDmNoti();
		}

		public void AddDM(List<ClientDirectMessage> listdm)
		{
			for (int i = listdm.Count - 1; i >= 0; i--)//dm은 일반 트윗이란 json이 반대로 옴
			{
				listdm[i].Init();
				AddDMData(listdm[i]);
			}
			SortDM();
		}

		private void SortDM()
		{
			lock (lockObject)
			{
				List<ClientDirectMessage> listDM = treeDM.OrderByDescending(x => x.dateTime).ToList();
				treeDM.Clear();
				for (int i = 0; i < listDM.Count; i++)
					treeDM.Add(listDM[i]);
			}
		}


		private void AddDMData(ClientDirectMessage dm)
		{
			if (dicHashs[eTweetPanel.eDm].Contains(dm.id))
				return;
			dicHashs[eTweetPanel.eDm].Add(dm.id);
			treeDM.Insert(0, dm);
		}
	}//class

	public class DirectMessageList : ObservableCollection<ClientDirectMessage>
	{
		public DirectMessageList() : base()
		{
		}
	}
	public class ListFindUser : ObservableCollection<UserSemi>
	{
		public ListFindUser() : base() { }
	}

	public class TweetTree: ObservableCollection<ClientTweet>
	{
		public TweetTree() : base() { }
	}

	public class TweetList : ObservableCollection<ClientTweet>
	{
		public TweetList() : base()
		{
		}
	}
}
