using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dalsae.API;
using static Dalsae.DataManager;

namespace Dalsae
{
	public class ClientAPIError
	{
		public List<ClientError> errors { get; set; }
	}

	public class ClientError
	{
		public int code { get; set; }
	}

	
	public class ClientTweet : BaseNoty
	{
		//--------------------------------------------------------------------------------
		//-------------------------외부 참조용 변수-------------------------------------
		//--------------------------------------------------------------------------------
		[Newtonsoft.Json.JsonIgnore]
		public UIProperty uiProperty { get; set; } = new UIProperty();
		[Newtonsoft.Json.JsonIgnore]
		public ClientTweet originalTweet { get; private set; }
		
		public ClientTweet quoted_status { get; set; }//QT일 경우 해당 트윗 정보
		/// <summary>
		/// UI용 리트윗 여부 플래그
		/// </summary>
		public bool isRetweet { get; set; } = false;
		public bool isMedia { get; private set; } = false;//미디어가 있는지 여부
		public bool isPhoto { get; private set; } = false;//사진인지 여부
		public bool isMovie { get; private set; } = false;//동영상인지 여부
		public bool isReply { get; private set; } = false;
		public bool isQTRetweet { get; private set; } = false;
		public bool isUrl { get; private set; } = false;
		public bool isMention { get; private set; } = false;
		public ClientEntities mediaEntities { get; private set; }//미디어 엔티티
		public ClientEntities lastEntities { get; private set; }
		private string _dateString;
		public string dateString
		{
			get
			{
				if (string.IsNullOrEmpty(_dateString) || string.IsNullOrEmpty(source))
					return string.Empty;
				else if (_dateString == null || source == null)
					return "";
				else
					return $"{_dateString} / via {source}";
			}
			private set { _dateString = value; }
		}
		public string retweetText
		{
			get
			{
				if (user == null) return "";
				return $"Retweet By {user.screen_name} / {user.name}" ?? "";
			}
		}
		public string nameText
		{
			get
			{
				if (user == null) return "";
				return $"{user.screen_name} / {user.name}";
			}
		}

		public Dictionary<string, ClientMedia> dicPhoto { get; private set; } = new Dictionary<string, ClientMedia>();//key: displayUrl
		public ClientMedia tweetMovie { get; private set; } = null;
		public List<ClientURL> listUrl { get; private set; } = new List<ClientURL>();
		public HashSet<string> hashMention { get; private set; } = new HashSet<string>();

		public string full_text { get { return _text; } set { _text = HttpUtility.HtmlDecode(value); } }//API땡기면 full_text가 옴
		public string text { get { return _text; } set { _text = HttpUtility.HtmlDecode(value); } }//스트리밍이면 text로 와서 이렇게 사용
		public User user { get; set; }//트윗 쓴 사람 정보, 리트윗일 경우 리트윗 정보에 원 트윗 user정보 있음
		public ClientTweet retweeted_status { get; set; }
		public ClientEntities entities { get; set; }
		public ClientEntities extended_entities { get; set; }//이미지가 여러장일 경우 사용됨
		public string in_reply_to_status_id_str { get; set; }
		public string quoted_status_id_str { get; set; }//인용 리트윗 트윗 id
		public object created_at { get { return _dateTime; } set { SetDateTime(value); } }
		public long id { get; set; }//트윗 id
		public bool truncated { get; set; }//140자 넘는 경우 알려주는 거
		public ClientExtendedTweet extended_tweet { get; set; }
		public string source { get { return _source; } set { SetSource(value); } }
		public int retweet_count { get; set; }//리트윗 카운트
		public int favorite_count { get; set; }//별 카운트
		private bool _favorited;
		public bool favorited//별박았는지
		{
			get { return _favorited; }
			set { _favorited = value; OnPropertyChanged("favorited"); }
		}
		private bool _retweeted = false;
		public bool retweeted//리트윗 했는지
		{
			get { return _retweeted; }
			set { _retweeted = value; OnPropertyChanged("retweeted"); }
		}

		private bool isExtendTweet = false;
		public DateTime dateTime { get { return _dateTime; } }
		private DateTime _dateTime;
		private string _source;
		private string _text;//트윗이요
		public void Init()//t.co 문자 변환 등 변경이 필요한 값들을 변경해준다
		{
			if (originalTweet != null) return;
			SetOriginalTweet();
			SetBoolean();
			ReplaceText();

			//인용트윗일 경우 인용 트윗 내부 데이터도 init, API로 땡길 경우 차단 하거나 당한 경우에 인용 트윗 안 날아옴
			if (isQTRetweet && quoted_status != null)
				quoted_status.Init();
		}

		public ClientTweet() { }
		public ClientTweet(string stringTweet)
		{
			this.user = new User();
			this.originalTweet = this;
			originalTweet.text = stringTweet;
		}

		private void SetOriginalTweet()
		{
			if (retweeted_status != null)
			{
				//retweeted_status.retweeted = true;
				isRetweet = true;
			}
			if (isRetweet)
			{
				if (retweeted_status == null)
					originalTweet = this;
				else
					originalTweet = retweeted_status;
			}
			else
				originalTweet = this;
		}

		private void SetBoolean()
		{
			if (string.IsNullOrEmpty(originalTweet.in_reply_to_status_id_str) == false)
				isReply = true;
			if (string.IsNullOrEmpty(originalTweet.quoted_status_id_str) == false)
				isQTRetweet = true;
			if (originalTweet.extended_tweet != null)
				isExtendTweet = true;

			if (isExtendTweet)//확장 트윗일 경우 확장 트윗의 일반,확장 엔티티 설정
			{
				if (originalTweet.extended_tweet.extended_entities != null)
					mediaEntities = originalTweet.extended_tweet.extended_entities;
				else
					mediaEntities = originalTweet.entities;
				lastEntities = originalTweet.extended_tweet.entities;
			}
			else
			{
				if (originalTweet.extended_entities != null)
					mediaEntities = originalTweet.extended_entities;
				else
					mediaEntities = originalTweet.entities;
				lastEntities = originalTweet.entities;
			}
			if (mediaEntities?.media != null)
				if (mediaEntities.media.Count > 0)
					isMedia = true;

			for (int i = 0; i < lastEntities.user_mentions.Count; i++)
			{
				if (DataInstence.CheckIsMe(lastEntities.user_mentions[i].id))
					isMention = true;

				hashMention.Add(lastEntities.user_mentions[i].screen_name);
			}

			if (lastEntities.urls != null)
				if (lastEntities.urls.Count > 0)
					isUrl = true;
		}


		private void ReplaceText()
		{
			if (originalTweet.truncated)//140자가 넘는 트윗이나 이미지 2장이상??일 경우 사용
			{
				if (isExtendTweet)//140자가 넘을 경우 
					originalTweet._text = HttpUtility.HtmlDecode(originalTweet.extended_tweet.full_text);//140자 넘는 텍스트로 변경
				ReplaceURL(originalTweet);
			}
			else
				ReplaceURL(originalTweet);
		}

		private void ReplaceURL(ClientTweet tweet)
		{
			if (entities == null) return;

			if (isUrl)//url이 있을 경우 변경
				for (int i = 0; i < lastEntities.urls.Count; i++)
				{
					tweet._text = tweet._text.Replace(lastEntities.urls[i].url, lastEntities.urls[i].display_url);
					listUrl.Add(lastEntities.urls[i]);
				}

			if (isMedia)//미디어가 있을 경우
				for (int i = 0; i < mediaEntities.media.Count; i++)
				{
					tweet._text = tweet._text.Replace(mediaEntities.media[i].url, mediaEntities.media[i].display_url);
					if (dicPhoto.ContainsKey(mediaEntities.media[i].display_url) == false)
					{
						if(mediaEntities.media[i].type=="photo")
						{
							dicPhoto.Add(mediaEntities.media[i].display_url, mediaEntities.media[i]);
							isPhoto = true;
						}
						else
						{
							tweetMovie = mediaEntities.media[i];
							isMovie = true;
						}
					}
				}
		}

		private void SetDateTime(object value)
		{
			_dateTime = DateTime.ParseExact(value.ToString(), "ddd MMM dd HH:mm:ss zzzz yyyy", CultureInfo.InvariantCulture);
			dateString = _dateTime.ToString("F",CultureInfo.CurrentCulture);
		}

		private void SetSource(string value)
		{
			_source = System.Text.RegularExpressions.Regex.Replace(value, "<[^>]*>", string.Empty);
		}
	}


	public class UserSemi
	{
		public UserSemi(string name, string screen_name, long id, string profile_image_url)
		{
			this.name = name;
			this.screen_name = screen_name;
			this.id = id;
			this.profile_image_url = profile_image_url;
			//this.id_str = id_str;
		}
		
		public string name { get; private set; }
		public string screen_name { get; private set; }
		public long id { get; private set; }
		public string profile_image_url { get; set; }
		public void UpdateName(string name)
		{
			this.name = name;
		}
		//public string id_str { get; private set; }
	}
	public class ClientBlockIds
	{
		public long next_cursor { get; set; }
		//public string next_cursor_str { get; set; }
		public long previous_cursor { get; set; }
		//public string previous_cursor_str { get; set; }
		public long[] ids;
	}
	public class User:BaseNoty
	{
		public User() { }
		public static void CopyUser(User to, User from)
		{
			to.profile_image_url = from.profile_image_url;
			to.name = from.name;
			to.id = from.id;
			to.verified = from.verified;
			to.screen_name = from.screen_name;
			to.favourites_count = from.favourites_count;
			to.friends_count = from.friends_count;
		}
		[Newtonsoft.Json.JsonIgnore]
		private string _profile_image_url = string.Empty;
		public string profile_image_url
		{
			get { return _profile_image_url; }
			set { _profile_image_url = value; OnPropertyChanged("profile_image_url"); }
		}
		public string name { get; set; } = string.Empty;
		public string screen_name { get; set; } = string.Empty;
		public long id { get; set; } = 0;
		public bool Protected { get; set; } = false;
		public bool verified { get; set; } = false;
		public int favourites_count { get; set; } = 0;
		public int friends_count { get; set; } = 0;
	}

	//팔로 리스트 땡길 때 사용
	public class ClientUsers
	{
		public long previous_cursor { get; set; }
		public string previous_cursor_str { get; set; }
		public long next_cursor { get; set; }
		public User[] users;
	}

	//dm용 클래스
	public class ClientDirectMessage
	{
		public UIProperty uiProperty { get; set; } = new UIProperty();
		private string _text;
		private DateTime _dateTime;
		public DateTime dateTime { get { return _dateTime; } }

		public void Init()//링크 변환 등
		{
			if (entities == null) return;

			if (entities.urls != null)
			{
				for (int i = 0; i < entities.urls.Count; i++)
					_text = _text.Replace(entities.urls[i].url, entities.urls[i].display_url);
			}

			if(entities.media!=null)
			{
				//for(int i=0;i<entities.media.Count;i++)
				if (entities.media.Count > 0)
					_text = _text.Replace(entities.media[0].url, entities.media[0].display_url);
			}
		}
		public string nameText { get { return string.Format("From {0} / {1} To {2} / {3}", sender.screen_name, sender.name,
									recipient.screen_name, recipient.name); } }
		public string dateString { get { return _dateTime.ToString("yyyy년 MMM월 dd일 dddd HH:mm:ss"); } }
		public object created_at { get { return _dateTime; } set { SetDateTime(value); } }
        public ClientEntities entities { get; set; }
		public ClientSender sender { get; set; }
		public ClientRecipient recipient { get; set; }
		public long id { get; set; }
        public long recipient_id { get; set; }
        public string recipient_screen_name { get; set; }
        public long sender_id { get; set; }
        public string sender_screen_name { get; set; }
		public string text { get { return _text; } set { _text = HttpUtility.HtmlDecode(value); } }
		private void SetDateTime(object value)
		{
			_dateTime = DateTime.ParseExact(value.ToString(), "ddd MMM dd HH:mm:ss zzzz yyyy", CultureInfo.InvariantCulture);
		}
	}

	//dm용 클래스, 받는 사람 정보
	public class ClientRecipient
	{
		private DateTime dateTime;
		public object created_at { get { return dateTime; } set { SetDateTime(value); } }
		//public long id { get; set; }
		public string name { get; set; }
		public string screen_name { get; set; }
		public string profile_image_url { get; set; }

		private void SetDateTime(object value)
		{
			dateTime = DateTime.ParseExact(value.ToString(), "ddd MMM dd HH:mm:ss zzzz yyyy", CultureInfo.InvariantCulture);
		}
	}

	//dm용 클래스, 보내는 사람 정보
	public class ClientSender
	{
		private DateTime dateTime;
		public object created_at { get { return dateTime; } set { SetDateTime(value); } }
		//public long id { get; set; }
		public string name { get; set; }
		public string screen_name { get; set; }
		public string profile_image_url { get; set; }
		private void SetDateTime(object value)
		{
			dateTime = DateTime.ParseExact(value.ToString(), "ddd MMM dd HH:mm:ss zzzz yyyy", CultureInfo.InvariantCulture);
		}
	}

	
	public class ClientExtendedTweet
	{
		public string full_text { get; set; }//140자 넘는 트윗일 경우 사용
		public ClientEntities entities { get; set; }
		public ClientEntities extended_entities { get; set; }
	}

	public class ClientEntities
	{
		public List<ClientURL> urls = new List<ClientURL>();// { get; set; }
		public List<ClientHashtag> hashtags = new List<ClientHashtag>();// { get; set; }
		public List<ClientUserMentions> user_mentions = new List<ClientUserMentions>();
		public List<ClientMedia> media = new List<ClientMedia>();
		//public List<ClientSymbol> symbols = new List<ClientSymbol>();// { get; set; }
	}

	public class VideoInfo
	{
		public long duration_millis { get; set; }//총 재생시간(ms)
		public List<int> aspect_ratio = new List<int>();
		public List<Variant> variants = new List<Variant>();
	}
	
	public class Variant
	{
		public int bitrate { get; set; }
		public string content_type { get; set; }
		public string url { get; set; }
	}


	public class ClientHashtag
	{
		public string text { get; set; }
	}

	//public class ClientSymbol
	//{
	//	public string text { get; set; }
	//}

	public class ClientUserMentions//리트윗 한 글의 원 유저 정보. 답변 보낼 때 사용
	{
		public string screen_name { get; set; }
		public string name { get; set; }
		public long id { get; set; }
	}

	public class ClientURL
	{
		public ClientURL() { }
		public ClientURL(string url, string expandedUrl, string displayUrl)
		{
			this.url = url;
			this.expanded_url = expandedUrl;
			this.display_url = displayUrl;
		}
		public ClientURL(ClientMedia media)
		{
			this.url = media.url;
			this.expanded_url = media.expanded_url;
			this.display_url = media.display_url;
		}
		public string url { get; set; }
		public string expanded_url { get; set; }
		public string display_url { get; set; }
	}

	public class ClientMultimedia//전송 후 받는 id용
	{
		public string media_id_string { get; set; }
		public long media_id { get; set; }

	}

	public class ClientExtendedEntities
	{
		public ClientMedia[] media;
	}

	public class ClientMedia
	{
		public long id { get; set; }
		public string media_url_https { get; set; }                 //":"https://pbs.twimg.com/media/C06Y8onVEAA6Ktk.jpg",
		public string url { get; set; }                                 //":"https://t.co/gULwuVQFC6",
		public string display_url { get; set; }                         //":"pic.twitter.com/gULwuVQFC6",
		public string expanded_url { get; set; }                        //":"https://twitter.com/umasukesankana/status/814756998243680256/photo/1",
		public string type { get; set; }
		public ClientSize sizes = new ClientSize();
		public VideoInfo video_info { get; set; }
	}

	public class ClientSize
	{
		public ClientLarge large = new ClientLarge();
		public ClientTumb thumb = new ClientTumb();
		public ClientMedium medium = new ClientMedium();
		public ClientSmall small = new ClientSmall();

	}

	public class ClientLarge
	{
		public int w { get; set; }
		public int h { get; set; }
		public string resize { get; set; }
	}

	public class ClientTumb
	{
		public int w { get; set; }
		public int h { get; set; }
		public string resize { get; set; }
	}

	public class ClientMedium
	{
		public int w { get; set; }
		public int h { get; set; }
		public string resize { get; set; }
	}

	public class ClientSmall
	{
		public int w { get; set; }
		public int h { get; set; }
		public string resize { get; set; }
	}

	public class ClientFollowingUpdate
	{
		public ClientRelationship relationship { get; set; }
	}

	public class ClientRelationship
	{
		public ClientTraget target { get; set; }
		public ClientSource source { get; set; }
	}

	public class ClientTraget
	{
		public long id { get; set; }
		public string screen_name { get; set; }
	}

	public class ClientSource
	{
		public bool want_retweets { get; set; }
	}


	//-------------------------------------------------------------------------------------------------------
	//------------------------------------------유저스트리밍-----------------------------------------------
	//-------------------------------------------------------------------------------------------------------
	public class ClientStreamDelete
	{
		public StreamDelete delete { get; set; }
	}

	public class StreamDelete
	{
		public Status status { get; set; }
	}

	public class Status
	{
		public long id { get; set; }
	}

	public class StreamDirectMessage
	{
		public ClientDirectMessage direct_message { get; set; }
	}

	public class StreamEvent
	{
		public string Event { get; set; }
		public string created_at { get; set; }
		public StreamSource source { get; set; }
		public User target { get; set; }//유저관련 이벤트일 경우 유저 정보
		public ClientTweet target_object { get; set; }//트윗 관련 이벤트일 경우 트윗 정보
	}

	public class StreamSource//.....???
	{

	}
}
