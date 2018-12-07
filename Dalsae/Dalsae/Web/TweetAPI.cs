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

namespace Dalsae.API
{
	public enum eResponse
	{
		NONE,
		STREAM,
		TIME_LINE,
		USER_TIMELINE,
		MENTION,
		UPDATE,
		MY_INFO,
		USER_INFO,
		MY_TWEET,
		IMAGE,
		RETWEET,
		UN_RETWEET,
		FAVORITE_LIST,
		FAVORITE_CREATE,
		FAVORITE_DESTROY,
		DELETE_TWEET,
		FOLLOWING_LIST,
		FOLLOWING_IDS,
		FOLLOWER_IDS,
		FOLLOWER_LIST,
		FOLLOWING,
		UNFOLLOWING,
		BLOCK_IDS,
		GET_DM,
		SEND_DM,
		RETWEET_OFF_IDS,
		FOLLOWING_UPDATE,
		SINGLE_TWEET,
		BLOCK_CREAE,
		BLOCK_DESTROY,
	}

	//기초 패킷 클래스
	public class BasePacket
	{
		public bool isMore { get; set; } = false;
		public string url { get; set; }
        public eResponse eresponse { get; set; }
		public string method;
		public Dictionary<string, string> dicParams = new Dictionary<string, string>();

		public string MethodGetUrl()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(url);
			sb.Append("?");
			foreach (string item in dicParams.Keys)
			{
				sb.Append(item);
				sb.Append("=");
				sb.Append(Uri.EscapeDataString(dicParams[item]));
				sb.Append("&");
			}
			sb.Remove(sb.Length - 1, 1);

			return sb.ToString();
		}
	}

	//Pin받기위해 띄움
	class PacketGetOAuth : BasePacket
	{
		public PacketGetOAuth()
		{
			this.url = "https://api.twitter.com/oauth/request_token";
			this.method = "POST";
			oauth_callback = "oob";
		}
		public string oauth_callback { get { return dicParams["oauth_callback"]; } set { dicParams["oauth_callback"] = value; } }
	}

	//핀 받고 엑세스 토큰받을 때 사용
	class PacketGetAccessToken : BasePacket
	{
		public PacketGetAccessToken()
		{
			this.url = "https://api.twitter.com/oauth/access_token";
			this.method = "POST";
		}
		public string oauth_verifier { get { return dicParams["oauth_verifier"]; } set { dicParams["oauth_verifier"] = value; } }
	}

	//다중계정 아이디 변경 확인 용
	class PacketLookUp : BasePacket
	{
		public PacketLookUp(long user_id)
		{
			this.url = "https://api.twitter.com/1.1/users/lookup.json";
			this.method = "GET";
		}
		public object user_id { get { return dicParams["user_id"]; } set { dicParams["user_id"] = value.ToString(); } }
	}

	class PacketFavoritesList : BasePacket
	{
		public PacketFavoritesList()
		{
			url = "https://api.twitter.com/1.1/favorites/list.json";
			method = "GET";
			eresponse = eResponse.FAVORITE_LIST;
			count = 40.ToString();
			tweet_mode = "extended";
		}
		public object user_id { get { return dicParams["user_id"]; } set { dicParams["user_id"] = value.ToString(); } }
		public string screen_name { get { return dicParams["screen_name"]; } set { dicParams["screen_name"] = value; } }
		public string count { get { return dicParams["count"]; } set { dicParams["count"] = value; } }
		public object max_id { get { return dicParams["max_id"]; } set { dicParams["max_id"] = value.ToString(); isMore = true; } }
		public string tweet_mode { get { return dicParams["tweet_mode"]; } set { dicParams["tweet_mode"] = value; } }
	}

	//유저 트윗 긁을 떄 사용
	class PacketUserTimeLine : BasePacket
	{
		public PacketUserTimeLine()
		{
			url = "https://api.twitter.com/1.1/statuses/user_timeline.json";
			method = "GET";
			eresponse = eResponse.USER_TIMELINE;
			count = 40.ToString();
			tweet_mode = "extended";
			//exclude_replies = true;
			//trim_user = true;
		}
		//public object user_id { get { return dicParams["user_id"]; } set { dicParams["user_id"] = value.ToString(); } }
		public string screen_name { get { return dicParams["screen_name"]; } set { dicParams["screen_name"] = value; } }
		public object max_id { get { return dicParams["max_id"]; } set { dicParams["max_id"] = value.ToString(); isMore = true; } }
		public string count { get { return dicParams["count"]; } set { dicParams["count"] = value; } }
		public string tweet_mode { get { return dicParams["tweet_mode"]; } set { dicParams["tweet_mode"] = value; } }
		//public object trim_user { get { return dicParams["trim_user"]; } set { dicParams["trim_user"] = value.ToString(); } }
		//public object exclude_replies { get { return dicParams["exclude_replies"]; } set { dicParams["exclude_replies"] = value.ToString(); } }
		//public object contributor_details { get { return dicParams["contributor_details"]; } set { dicParams["contributor_details"] = value.ToString(); } }
		//public object include_rts { get { return dicParams["include_rts"]; } set { dicParams["include_rts"] = value.ToString(); } }
	}

	//자기 정보 긁을 때 사용
	class PacketVerifyCredentials : BasePacket
	{
		public PacketVerifyCredentials()
		{
			url = "https://api.twitter.com/1.1/account/verify_credentials.json";
			method = "GET";
			eresponse = eResponse.MY_INFO;
		}
	}

	public class PacketUserShow : BasePacket
	{
		public PacketUserShow(string screen_name)
		{
			url = "https://api.twitter.com/1.1/users/show.json";
			method = "GET";
			eresponse = eResponse.USER_INFO;
			this.screen_name = screen_name;
		}
		public PacketUserShow(long id)
		{
			url = "https://api.twitter.com/1.1/users/show.json";
			method = "GET";
			eresponse = eResponse.USER_INFO;
			user_id = id.ToString();
		}
		public string screen_name { get { return dicParams["screen_name"]; } set { dicParams["screen_name"] = value.ToString(); } }
		public string user_id { get { return dicParams["user_id"]; } set { dicParams["user_id"] = value.ToString(); } }
	}

	//트윗 업로드에 사용
	public class PacketUpdate : BasePacket
	{
		/// <summary>
		/// 트윗&DM 생성자
		/// </summary>
		/// <param name="isTweet">DM일 경우 false</param>
		public PacketUpdate(bool isTweet)
		{
			this.method = "POST";
			if (isTweet)
			{
				url = "https://api.twitter.com/1.1/statuses/update.json";
				eresponse = eResponse.UPDATE;
			}
			else
			{
				url = "https://api.twitter.com/1.1/direct_messages/new.json";
				eresponse = eResponse.SEND_DM;
			}
		}

		//-----------------DM용-------------------------
		public string text { get { return dicParams["text"]; } set { dicParams["text"] = value.ToString(); } }
		public object screen_name { get { return dicParams["screen_name"]; } set { dicParams["screen_name"] = value.ToString(); } }
		public object user_id { get { return dicParams["user_id"]; } set { dicParams["user_id"] = value.ToString(); } }
		//-------------------------------------------------
		public string status { get { return dicParams["status"]; } set { dicParams["status"] = value.ToString(); } }
		public string in_reply_to_status_id { get { return dicParams["in_reply_to_status_id"]; } set { dicParams["in_reply_to_status_id"] = value.ToString(); } }
		public string media_ids { get { return dicParams["media_ids"]; } set { dicParams["media_ids"] = value.ToString(); } }
		public string possibly_sensitive { set { dicParams["status"] = value.ToString(); } }
		public string lat { set { dicParams["status"] = value.ToString(); } }
		public string Long { set { dicParams["status"] = value.ToString(); } }
		public string place_id { set { dicParams["status"] = value.ToString(); } }
		public string display_coordinates { set { dicParams["status"] = value.ToString(); } }
		public string trim_user { set { dicParams["status"] = value.ToString(); } }
		//public string media_ids { set { dicParams["status"] = value.ToString(); } }
	}

	//홈 타임라인 땡길때
	class PacketHomeTimeLine : BasePacket
	{
		public PacketHomeTimeLine()
		{
			url = "https://api.twitter.com/1.1/statuses/home_timeline.json";
			method = "GET";
			eresponse = eResponse.TIME_LINE;
			count = 40.ToString();
			tweet_mode = "extended";
		}
		public object count { get { return dicParams["count"]; } set { dicParams["count"] = value.ToString(); } }
		public string tweet_mode { get { return dicParams["tweet_mode"]; } set { dicParams["tweet_mode"] = value; } }
		public object max_id { get { return dicParams["max_id"]; } set { dicParams["max_id"] = value.ToString(); isMore = true; } }
		public object since_id { get { return dicParams["since_id"]; } set { dicParams["since_id"] = value.ToString(); } }
	}

	//이미지 올릴 떄
	public class PacketMediaUpload : BasePacket, IDisposable
    {
        public PacketMediaUpload()
        {
            this.url = "https://upload.twitter.com/1.1/media/upload.json";
            this.method = "POST";
        }

        //public string media { get { return dicParams["media"]; } set { dicParams["media"] = value.ToString(); } }
		public string additional_owners { get { return dicParams["additional_owners"]; } set { dicParams["additional_owners"] = value.ToString(); } }

        public string extension { get; set; }

        private readonly MemoryStream _mediaStream = new MemoryStream();
        public Stream mediaStream { get { return _mediaStream; } }

        ~PacketMediaUpload()
        {
            this.Dispose(false);
        }
        public void Dispose()
        {
            this.Dispose(true);
        }
        private bool m_disposed = false;
        protected void Dispose(bool disposing)
        {
            if (this.m_disposed) return;
            this.m_disposed = true;

            this._mediaStream.Dispose();
        }
    }

	//관글 on
	class PacketFavorites_Create : BasePacket
	{
		public PacketFavorites_Create()
		{
			url = "https://api.twitter.com/1.1/favorites/create.json";
			method = "POST";
			eresponse = eResponse.FAVORITE_CREATE;
		}

		public object id { get { return dicParams["id"]; } set { dicParams["id"] = value.ToString(); } }
		public string include_entities { get { return dicParams["include_entities"]; } set { dicParams["include_entities"] = value.ToString(); } }
	}

	//관글off
	class PacketFavorites_Destroy : BasePacket
	{
		public PacketFavorites_Destroy()
		{
			url = "https://api.twitter.com/1.1/favorites/destroy.json";
			method = "POST";
			eresponse = eResponse.FAVORITE_DESTROY;
		}

		public object id { get { return dicParams["id"]; } set { dicParams["id"] = value.ToString(); } }
		public string include_entities { get { return dicParams["include_entities"]; } set { dicParams["include_entities"] = value.ToString(); } }
	}

	//retweet
	class PacketRetweet : BasePacket
	{
		public PacketRetweet(long id)
		{
			url = "https://api.twitter.com/1.1/statuses/retweet/" + id + ".json";
			method = "POST";
			eresponse = eResponse.RETWEET;
			this.id = id;
		}
		public object id { get { return dicParams["id"]; } set { dicParams["id"] = value.ToString(); } }
	}

	//리트윗 취소
	class PacketUnRetweet : BasePacket
	{
		public PacketUnRetweet(long id)
		{
			url = "https://api.twitter.com/1.1/statuses/unretweet/" + id + ".json";
			method = "POST";
			eresponse = eResponse.UN_RETWEET;
			this.id = id;
		}

		public object id { get { return dicParams["id"]; } set { dicParams["id"] = value.ToString(); } }
	}

	class PacketMentionTimeLine : BasePacket
	{
		public PacketMentionTimeLine()
		{
			url = "https://api.twitter.com/1.1/statuses/mentions_timeline.json";
			method = "GET";
			eresponse = eResponse.MENTION;
			count = 40.ToString();
			tweet_mode = "extended";
		}

		public string count { get { return dicParams["count"]; } set { dicParams["count"] = value.ToString(); } }
		public object max_id { get { return dicParams["max_id"]; } set { dicParams["max_id"] = value.ToString(); isMore = true; } }
		public string tweet_mode { get { return dicParams["tweet_mode"]; } set { dicParams["tweet_mode"] = value; } }
		public object since_id { get { return dicParams["since_id"]; } set { dicParams["since_id"] = value.ToString(); } }
		//public string max_id { get { return dicParams["max_id"]; } set { dicParams["max_id"] = value.ToString(); } }
		//public string trim_user { get { return dicParams["trim_user"]; } set { dicParams["trim_user"] = value.ToString(); } }
		//public string include_entities { get { return dicParams["include_entities"]; } set { dicParams["include_entities"] = value.ToString(); } }
	}

	class PacketUserStream : BasePacket
	{
		public PacketUserStream()
		{
			url = "https://userstream.twitter.com/1.1/user.json";
			method = "GET";
			eresponse = eResponse.NONE;
		}
		public string delimited            { get { return dicParams["delimited"];            } set { dicParams["delimited"]            = value.ToString(); } }
		public string stall_warnings       { get { return dicParams["stall_warnings"];       } set { dicParams["stall_warnings"]       = value.ToString(); } }
		public string with                 { get { return dicParams["with"];                 } set { dicParams["with"]                 = value.ToString(); } }
		public string replies              { get { return dicParams["replies"];              } set { dicParams["replies"]              = value.ToString(); } }
		public string track                { get { return dicParams["track"];                } set { dicParams["track"]                = value.ToString(); } }
		public string locations            { get { return dicParams["locations"];            } set { dicParams["locations"]            = value.ToString(); } }
		public string stringify_friend_ids { get { return dicParams["stringify_friend_ids"]; } set { dicParams["stringify_friend_ids"] = value.ToString(); } }
	}

	class PacketTweetDelete : BasePacket
	{
		public PacketTweetDelete(long id)
		{
			url = "https://api.twitter.com/1.1/statuses/destroy/" + id + ".json";
			method = "POST";
			this.id = id.ToString();
			eresponse = eResponse.DELETE_TWEET;
		}
		public object id { get { return dicParams["id"]; } set { dicParams["id"] = value.ToString(); } }
		public string trim_user { get { return dicParams["trim_user"]; } set { dicParams["trim_user"] = value.ToString(); } }
	}

	class PacketFollowingIds : BasePacket//팔로잉 아이디만 가져옴, max 5000
	{
		public PacketFollowingIds(string screenName, long cursor=-1)
		{
			url = "https://api.twitter.com/1.1/friends/ids.json";
			method = "GET";
			eresponse = eResponse.FOLLOWING_IDS;
			count = 5000;
			screen_name = screenName;
			this.cursor = cursor;
		}
		public object screen_name { get { return dicParams["screen_name"]; } set { dicParams["screen_name"] = value.ToString(); } }
		public object count { get { return dicParams["count"]; } set { dicParams["count"] = value.ToString(); } }
		public object cursor { get { return dicParams["cursor"]; } set { dicParams["cursor"] = value.ToString(); } }
	}

	class PacketFollowerIds : BasePacket//팔로워 아이디만 가져옴, max 5000
	{
		public PacketFollowerIds(string screenName, long cursor = -1)
		{
			url = "https://api.twitter.com/1.1/followers/ids.json";
			method = "GET";
			eresponse = eResponse.FOLLOWING_IDS;
			count = 5000;
			screen_name = screenName;
			this.cursor = cursor;
		}
		public object screen_name { get { return dicParams["screen_name"]; } set { dicParams["screen_name"] = value.ToString(); } }
		public object count { get { return dicParams["count"]; } set { dicParams["count"] = value.ToString(); } }
		public object cursor { get { return dicParams["cursor"]; } set { dicParams["cursor"] = value.ToString(); } }
	}

	class PacketBlockCreate : BasePacket
	{
		public PacketBlockCreate(long id)
		{
			url = "https://api.twitter.com/1.1/blocks/create.json";
			method = "POST";
			eresponse = eResponse.BLOCK_CREAE;
			this.user_id = id;
		}
		public object user_id { get { return dicParams["user_id"]; } set { dicParams["user_id"] = value.ToString(); } }
	}

	public class PacketBlockDestroy : BasePacket
	{
		public PacketBlockDestroy(long id)
		{
			url = "https://api.twitter.com/1.1/blocks/destroy.json";
			method = "POST";
			eresponse = eResponse.BLOCK_DESTROY;
			this.user_id = id;
		}
		public object user_id { get { return dicParams["user_id"]; } set { dicParams["user_id"] = value.ToString(); } }
	}

	public class PacketFollow : BasePacket
	{
		public PacketFollow(string screen_name)
		{
			url = "https://api.twitter.com/1.1/friendships/create.json";
			method = "POST";
			eresponse = eResponse.FOLLOWING;
			this.screen_name = screen_name;
		}
		public string screen_name { get { return dicParams["screen_name"]; } set { dicParams["screen_name"] = value.ToString(); } }
		public object user_id { get { return dicParams["user_id"]; } set { dicParams["user_id"] = value.ToString(); } }
	}

	class PacketUnFollow : BasePacket
	{
		public PacketUnFollow(string screen_name)
		{
			url = "https://api.twitter.com/1.1/friendships/destroy.json";
			method = "POST";
			eresponse = eResponse.UNFOLLOWING;
			this.screen_name = screen_name;
		}
		public string screen_name { get { return dicParams["screen_name"]; } set { dicParams["screen_name"] = value.ToString(); } }
		public object user_id { get { return dicParams["user_id"]; } set { dicParams["user_id"] = value.ToString(); } }
	}

	class PacketFollowingList : BasePacket//팔로잉 리스트, max 200
	{
		public PacketFollowingList()
		{
			url = "https://api.twitter.com/1.1/friends/list.json";
			method = "GET";
			eresponse = eResponse.FOLLOWING_LIST;
			count = 40.ToString();
		}
		public object user_id { get { return dicParams["user_id"]; } set { dicParams["user_id"] = value.ToString(); } }
		public string screen_name { get { return dicParams["screen_name"]; } set { dicParams["screen_name"] = value.ToString(); } }
		public object cursor { get { return dicParams["cursor"]; } set { dicParams["cursor"] = value.ToString(); } }
		public object count { get { return dicParams["count"]; } set { dicParams["count"] = value.ToString(); } }
		public string skip_status { get { return dicParams["skip_status"]; } set { dicParams["skip_status"] = value.ToString(); } }
		public string include_user_entities { get { return dicParams["include_user_entities"]; } set { dicParams["include_user_entities"] = value.ToString(); } }
	}

	class PacketFollowerList : BasePacket//팔로잉 리스트, max 200
	{
		public PacketFollowerList()
		{
			url = "https://api.twitter.com/1.1/followers/list.json";
			method = "GET";
			eresponse = eResponse.FOLLOWER_LIST;
			count = 40.ToString();
		}
		public object user_id { get { return dicParams["user_id"]; } set { dicParams["user_id"] = value.ToString(); } }
		public string screen_name { get { return dicParams["screen_name"]; } set { dicParams["screen_name"] = value.ToString(); } }
		public object cursor { get { return dicParams["cursor"]; } set { dicParams["cursor"] = value.ToString(); } }
		public string count { get { return dicParams["count"]; } set { dicParams["count"] = value.ToString(); } }
		public string skip_status { get { return dicParams["skip_status"]; } set { dicParams["skip_status"] = value.ToString(); } }
		public string include_user_entities { get { return dicParams["include_user_entities"]; } set { dicParams["include_user_entities"] = value.ToString(); } }
	}

	class PacketBlockIds : BasePacket//블락리스트, max 5000
	{
		public PacketBlockIds()
		{
			url = "https://api.twitter.com/1.1/blocks/ids.json";
			method = "GET";
			eresponse = eResponse.BLOCK_IDS;

		}
		public string stringify_ids { get { return dicParams["stringify_ids"]; } set { dicParams["stringify_ids"] = value.ToString(); } }
		public object cursor { get { return dicParams["cursor"]; } set { dicParams["cursor"] = value.ToString(); } }
	}

	public class PacketDirectMessageSend : BasePacket
	{
		public PacketDirectMessageSend()
		{
			url = "https://api.twitter.com/1.1/direct_messages/new.json";
			method = "GET";
			eresponse = eResponse.SEND_DM;
		}
		public object screen_name { get { return dicParams["screen_name"]; } set { dicParams["screen_name"] = value.ToString(); } }
		public object user_id { get { return dicParams["user_id"]; } set { dicParams["user_id"] = value.ToString(); } }
	}


	public class PacketGetDM : BasePacket
	{
		public PacketGetDM()
		{
			url = "https://api.twitter.com/1.1/direct_messages.json";
			method = "GET";
			eresponse = eResponse.GET_DM;
			count = 20;
		}
		public object count { get { return dicParams["count"]; } set { dicParams["count"] = value.ToString(); } }
	}

	public class PacketGetRetweetOffIds : BasePacket
	{
		public PacketGetRetweetOffIds()
		{
			url = "https://api.twitter.com/1.1/friendships/no_retweets/ids.json";
			method = "GET";
			eresponse = eResponse.RETWEET_OFF_IDS;
		}
	}

	public class PacketUpdateFollowingData : BasePacket
	{
		public PacketUpdateFollowingData()
		{
			url = "https://api.twitter.com/1.1/friendships/update.json";
			method = "POST";
			eresponse = eResponse.FOLLOWING_UPDATE;
		}

		public object user_id { get { return dicParams["user_id"]; } set { dicParams["user_id"] = value.ToString(); } }
		public object retweets { get { return dicParams["retweets"]; } set { dicParams["retweets"] = value.ToString(); } }
	}

	class PacketImage : BasePacket
	{
		public PacketImage(string url)
		{
			this.url = url;
			method = "GET";
		}
	}

	public class PacketSingleTweet : BasePacket
	{
		public PacketSingleTweet(string id)
		{
			url = "https://api.twitter.com/1.1/statuses/show.json";
			method = "GET";
			eresponse = eResponse.SINGLE_TWEET;
			this.id = id;
			tweet_mode = "extended";
		}
		public object id { get { return dicParams["id"]; } set { dicParams["id"] = value.ToString(); } }
		public string tweet_mode { get { return dicParams["tweet_mode"]; } set { dicParams["tweet_mode"] = value; } }
	}
}