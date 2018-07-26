using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

namespace Dalsae
{
	public class Option:BaseNoty//옵션 추가 시 복사 생성자 유의!!!
	{
		public Option() { }
		public void CheckNullOption()
		{
			if (pointImageFormX == 0)
				pointImageFormX = 100;
			if (pointImageFormY == 0)
				pointImageFormY = 100;
			if (pointMainFormX == 0)
				pointMainFormX = 100;
			if (pointMainFormY == 0)
				pointMainFormY = 100;
			if (sizeMainFormX == 0)
				sizeMainFormX = 580;
			if (sizeMainFormY == 0)
				sizeMainFormY = 800;
			if (sizeImageFormX == 0)
				sizeImageFormX = 580;
			if (sizeImageFormY == 0)
				sizeImageFormY = 800;
			if (sizeVideoFormX == 0)
				sizeVideoFormX = 580;
			if (sizeVideoFormY == 0)
				sizeVideoFormY = 800;
			if (pointVideoFormX == 0)
				pointVideoFormX = 100;
			if (pointVideoFormY == 0)
				pointVideoFormY = 100;


			//notisound는 null이어도 상관 없음
			if (string.IsNullOrEmpty(skinName))
				skinName = "pink";
			if (string.IsNullOrEmpty(imageFolderPath))
				imageFolderPath = "Image";

			if (_font == null)
			{
				_font = new FontFamily("맑은 고딕");
				fontSize = 14;
			}


			if (listHighlight == null)
				listHighlight = new List<string>();
			if (listMuteWord == null)
				listMuteWord = new List<string>();
			if (listMuteClient == null)
				listMuteClient = new List<string>();
			if (listMuteUser == null)
				listMuteUser = new List<string>();
			if (dicMuteTweet == null)
				dicMuteTweet = new Dictionary<long, string>();
		}
		public bool isShowRetweet { get; set; } = true;//TL에 리트윗을 띄울지
		public bool isNotiRetweet { get; set; } = true;//멘션함에 리트윗을 띄울지
		//public bool isShowBlockUser { get; set; } = false;//블락 한 사람을 띄울지(기본 안 띄움)
		public bool isMuteMention { get; set; } = true;//멘션함도 뮤트(기본 해제)
		public bool isYesnoTweet { get; set; } = false;//트윗 올릴 때 물어보고 올릴지(기본 안 물어봄)
		public bool isRetweetProtectUser { get; set; } = true;//플텍 유저일 경우 수동RT해줄지(기본 함)
		public bool isSendEnter { get; set; } = false;//트윗 올릴 때 Enter / Ctrl+Enter고르기(기본 ctrl+enter)
		public bool isLoadFollwing { get; set; } = true;//프로그램 시작 시 팔로잉 목록을 가져올지
		public bool isLoadBlock { get; set; } = true;//프로그램 시작 시 차단 목록을 가져올지
		[Newtonsoft.Json.JsonIgnore]
		private bool _isShowPropic = true;
		public bool isShowPropic { get { return _isShowPropic; } set { _isShowPropic = value; OnPropertyChanged("isShowPropic"); } }
		[Newtonsoft.Json.JsonIgnore]
		private bool _isBigPropic = false;
		public bool isBigPropic { get { return _isBigPropic; } set { _isBigPropic = value; OnPropertyChanged("isBigPropic"); } }
		public string skinName { get; set; } = "pink";
		public bool isPlayNoti { get; set; } = false;
		public string notiSound { get; set; }//알림 소리(선택박스, sound폴더)
		public bool isLoadOriginalImage { get; set; } = false;
		public bool isSmallUI { get; set; } = false;
		public int pointImageFormX { get; set; } = 100;
		public int pointImageFormY { get; set; } = 100;
		public int pointMainFormX { get; set; } = 100;
		public int pointMainFormY { get; set; } = 100;
		public int sizeMainFormX { get; set; } = 580;
		public int sizeMainFormY { get; set; } = 800;
		public int sizeImageFormX { get; set; } = 580;
		public int sizeImageFormY { get; set; } = 800;
		public int sizeVideoFormX { get; set; } = 580;
		public int sizeVideoFormY { get; set; } = 800;
		public int pointVideoFormX { get; set; } = 100;
		public int pointVideoFormY { get; set; } = 100;
		public double movieVolume { get; set; } = 0.5f;
		[Newtonsoft.Json.JsonIgnore]
		private bool _isShowPreview = true;
		public bool isShowPreview { get { return _isShowPreview; } set { _isShowPreview = value; OnPropertyChanged("isShowPreview"); } }//이미지 미리보기
		public bool isShowImageTweet { get; set; } = true;//이미지 뷰어 트윗
		public bool isShowImageBottom { get; set; } = true;//이미지 뷰어 하단 바
		public string imageFolderPath { get; set; } = "Image";

		public List<string> listHighlight = new List<string>();//단어 하이라이트(리스트 박스?)
		public List<string> listMuteWord = new List<string>();//단어 뮤트(리스트 박스)
		public List<string> listMuteClient = new List<string>();//클라이언트 뮤트(리스트 박스)
		public List<string> listMuteUser = new List<string>();//유저 뮤트(리스트 박스)
		public Dictionary<long, string> dicMuteTweet = new Dictionary<long, string>();//트윗 뮤트
		private System.Drawing.Font _orgFont = new System.Drawing.Font("맑은 고딕", 14);
		public System.Drawing.Font orgFont
		{
			get { return _orgFont; }
			set
			{
				_orgFont = value;
				_font = new FontFamily(value.Name);
				isBoldFont = value.Bold;
				fontSize = (int)value.Size;
			}
		}
		[Newtonsoft.Json.JsonIgnore]
		private FontFamily _font;
		[Newtonsoft.Json.JsonIgnore]
		public FontFamily font { get { return _font; } set { _font = value; OnPropertyChanged("font"); } }
		[Newtonsoft.Json.JsonIgnore]
		private bool _isBoldFont;
		[Newtonsoft.Json.JsonIgnore]
		public bool isBoldFont { get { return _isBoldFont; } set { _isBoldFont = value; OnPropertyChanged("isBoldFont"); } }
		[Newtonsoft.Json.JsonIgnore]
		private int _fontSize;
		[Newtonsoft.Json.JsonIgnore]
		public int fontSize { get { return _fontSize; } set { _fontSize = value; OnPropertyChanged("fontSize"); } }

		public bool MatchHighlight(string text)
		{
			bool ret = false;
			for (int i = 0; i < listHighlight.Count; i++)
				if (text.IndexOf(listHighlight[i]) > -1)
				{
					ret = true;
					break;
				}
			return ret;
		}

		public bool MatchMuteWord(string text)
		{
			bool ret = false;
			for (int i = 0; i < listMuteWord.Count; i++)
				if (text.IndexOf(listMuteWord[i], StringComparison.OrdinalIgnoreCase) > -1)
				{
					ret = true;
					break;
				}

			return ret;
		}

		public bool MatchMuteClient(string text)
		{
			bool ret = false;
			for (int i = 0; i < listMuteClient.Count; i++)
				if(string.Equals(listMuteClient[i], text, StringComparison.OrdinalIgnoreCase))
				{
					ret = true;
					break;
				}

			return ret;
		}

		public bool MatchMuteUser(string screenName)
		{
			bool ret = false;
			for (int i = 0; i < listMuteUser.Count; i++)
				if (string.Equals(listMuteUser[i], screenName, StringComparison.OrdinalIgnoreCase))
				{
					ret = true;
					break;
				}

			return ret;
		}

		public bool MatchMuteTweet(long id)
		{
			return dicMuteTweet.ContainsKey(id);
		}

		//답멘 한 거 체크 할 때는 string만 체크 돼서 이렇게
		public bool MatchMuteTweet(string id)
		{
			long longId = 0;
			if (long.TryParse(id, out longId))
				return dicMuteTweet.ContainsKey(longId);
			else
				return false;
		}

		public void AddMuteUser(string user)
		{
			listMuteUser.Add(user.Replace("@", ""));
			//Properties.Settings.Default.listMuteUser.Add(user);
			//Properties.Settings.Default.Save();
		}

		public void AddMuteClient(string client)
		{
			listMuteClient.Add(client);
			//Properties.Settings.Default.listMuteClient.Add(client);
			//Properties.Settings.Default.Save();
		}

		public void AddMuteTweet(ClientTweet tweet)
		{
			if (dicMuteTweet.ContainsKey(tweet.originalTweet.id) == false)
				dicMuteTweet.Add(tweet.originalTweet.id, tweet.originalTweet.text);
		}
	}

	public class Skin : BaseNoty
	{
		//스킨 바꿀 떄 스레드 문제 안 생기게 스킨 변경도 스레드로 해야할 거 같은데..상황 봐서
		[Newtonsoft.Json.JsonIgnore]
		private SolidColorBrush _blockOne = new SolidColorBrush(Color.FromRgb(255, 224, 224));
		[Newtonsoft.Json.JsonIgnore]
		private SolidColorBrush _blockTwo = new SolidColorBrush(Color.FromRgb(255, 207, 207));
		[Newtonsoft.Json.JsonIgnore]
		private SolidColorBrush _mentionOne = new SolidColorBrush(Color.FromRgb(0xe6, 0xff, 0xe6));
		[Newtonsoft.Json.JsonIgnore]
		private SolidColorBrush _mentionTwo = new SolidColorBrush(Color.FromRgb(0xff, 0xff, 0xe0));
		[Newtonsoft.Json.JsonIgnore]
		private SolidColorBrush _tweet = new SolidColorBrush(Color.FromRgb(0, 0, 0));
		[Newtonsoft.Json.JsonIgnore]
		private SolidColorBrush _retweet = new SolidColorBrush(Color.FromRgb(0x7e, 0x7b, 0xff));
		[Newtonsoft.Json.JsonIgnore]
		private SolidColorBrush _mention = new SolidColorBrush(Color.FromRgb(0xff, 0x4b, 0x6a));
		[Newtonsoft.Json.JsonIgnore]
		private SolidColorBrush _defaultColor = new SolidColorBrush(Color.FromRgb(255, 224, 224));
		[Newtonsoft.Json.JsonIgnore]
		private SolidColorBrush _leaveColor = new SolidColorBrush(Color.FromRgb(0xd9, 0xb0, 0xb0));
		[Newtonsoft.Json.JsonIgnore]
		private SolidColorBrush _topbar = new SolidColorBrush(Color.FromRgb(255, 207, 207));
		[Newtonsoft.Json.JsonIgnore]
		private SolidColorBrush _bottomBar = new SolidColorBrush(Color.FromRgb(0xff, 0xbf, 0xbf));
		[Newtonsoft.Json.JsonIgnore]
		private SolidColorBrush _menuColor = new SolidColorBrush(Color.FromRgb(0xff, 0x4b, 0x6a));
		[Newtonsoft.Json.JsonIgnore]
		private SolidColorBrush _select = new SolidColorBrush(Color.FromRgb(0xcd, 0xc1, 0xd8));
		public SolidColorBrush blockOne
		{
			get { return _blockOne; }
			set { _blockOne = value; OnPropertyChanged("blockOne"); }
		}
		public SolidColorBrush blockTwo
		{
			get { return _blockTwo; }
			set { _blockTwo = value; OnPropertyChanged("blockTwo"); }
		}
		public SolidColorBrush mentionOne
		{
			get { return _mentionOne; }
			set { _mentionOne = value; OnPropertyChanged("mentionOne"); }
		}
		public SolidColorBrush mentionTwo
		{
			get { return _mentionTwo; }
			set { _mentionTwo = value; OnPropertyChanged("mentionTwo"); }
		}
		public SolidColorBrush tweet
		{
			get { return _tweet; }
			set { _tweet = value; OnPropertyChanged("tweet"); }
		}
		public SolidColorBrush retweet
		{
			get { return _retweet; }
			set { _retweet = value; OnPropertyChanged("retweet"); }
		}
		public SolidColorBrush mention
		{
			get { return _mention; }
			set { _mention = value; OnPropertyChanged("mention"); }
		}
		public SolidColorBrush defaultColor
		{
			get { return _defaultColor; }
			set { _defaultColor = value; OnPropertyChanged("defaultColor"); }
		}
		public SolidColorBrush leaveColor
		{
			get { return _leaveColor; }
			set { _leaveColor = value; OnPropertyChanged("leaveColor"); }
		}
	
		public SolidColorBrush topbar
		{
			get { return _topbar; }
			set { _topbar = value; OnPropertyChanged("topbar"); }
		}
		public SolidColorBrush bottomBar
		{
			get { return _bottomBar; }
			set { _bottomBar = value; OnPropertyChanged("bottomBar"); }
		}
		public SolidColorBrush menuColor
		{
			get { return _menuColor; }
			set { _menuColor = value; OnPropertyChanged("menuColor"); }
		}
		public SolidColorBrush select
		{
			get { return _select; }
			set { _select = value; OnPropertyChanged("select"); }
		}
		public Skin() { }
	}
}
