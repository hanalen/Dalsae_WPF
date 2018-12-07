using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalsae.Data
{
	public class UserInfo : BaseNoty
	{
		//URL변경 작업
		public void Init()
		{
			if (string.IsNullOrEmpty(url)) return;

			if (entities.url != null)
				for (int i = 0; i < entities.url.urls.Length; i++)
					if (entities.url.urls[i].url == url)
						url = entities.url.urls[i].expanded_url;
		}

		private void SetDateTime(object value)
		{
			dateTime = DateTime.ParseExact(value.ToString(), "ddd MMM dd HH:mm:ss zzzz yyyy", CultureInfo.InvariantCulture);
			dateString = $"가입일: {dateTime.ToString("yyyy년 MMM월 dd일 dddd HH:mm:ss")}";
		}
		private DateTime dateTime;
		public Entities entities { get; set; }
		public string dateString { get; set; }
		public object created_at { get { return dateTime; } set { SetDateTime(value); } }
		private string _profile_image_url;
		public string profile_image_url
		{
			get { return _profile_image_url; }
			set
			{
				_profile_image_url = value.ToString().Replace("_normal", "_bigger");
				profile_image_orig = value.ToString().Replace("_normal", "");
			}
		}//인장
		public string profile_image_orig { get; set; }
		public string profile_image_url_https { get; set; }
		public string profile_banner_url { get; set; }//배경 이미지
		public string url { get; set; }//링크
		public string profile_background_color { get; set; }//배경 색
		public string location { get; set; }//위치
		public string description { get; set; }//바이오
		public int statuses_count { get; set; }//트윗수
		public int followers_count { get; set; }
		public int friends_count { get; set; }
		private bool _following;
		public bool following { get { return _following; } set { _following = value; OnPropertyChanged("following"); } }
		public bool block { get; set; } = false;
		public string name { get; set; }// "OAuth Dancer",
		private string _screen_name;
		public string screen_name { get { return _screen_name; } set { _screen_name = $"@{value.ToString()}"; } }
		public long id { get; set; }
		public bool Protected { get; set; }
		public bool verified { get; set; }
		public int favourites_count { get; set; }
	}

	public class Entities
	{
		public URL url { get; set; }
	}
	public class URL
	{
		public ClientURL[] urls { get; set; }
	}
}
