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
		#region json이그노어
		[Newtonsoft.Json.JsonIgnore]
		private bool _isBigPropic = false;
		[Newtonsoft.Json.JsonIgnore]
		private bool _isShowPropic = true;
		[Newtonsoft.Json.JsonIgnore]
		private bool _isShowPreview = true;
		[Newtonsoft.Json.JsonIgnore]
		private FontFamily _font = new FontFamily("맑은 고딕");
		[Newtonsoft.Json.JsonIgnore]
		private bool _isBoldFont = false;
		[Newtonsoft.Json.JsonIgnore]
		private int _fontSize = 14;
		#endregion

		public Option() { }
		public void CheckNullOption()
		{
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

		#region 트윗 전송
		/// <summary>
		/// 트윗 등록 시 물어보고 올릴지 여부를 설정 합니다.
		/// </summary>
		public bool isYesnoTweet { get; set; } = false;
		/// <summary>
		/// 잠금 계정의 트윗을 리트윗 할지 여부를 설정 합니다.
		/// </summary>
		public bool isRetweetProtectUser { get; set; } = true;
		/// <summary>
		/// 트윗 등록 키를 엔터 혹은 컨트롤+엔터로 설정 합니다. 기본 컨트롤 엔터
		/// </summary>
		public bool isSendEnter { get; set; } = false;
		#endregion

		#region UI 및 이미지 뷰어 설정
		/// <summary>
		/// 인장 표시 여부를 설정 합니다.
		/// </summary>
		public bool isShowPropic { get { return _isShowPropic; } set { _isShowPropic = value; OnPropertyChanged("isShowPropic"); } }
		/// <summary>
		/// 인장을 크게 표시할지 여부를 설정 합니다.
		/// </summary>
		public bool isBigPropic { get { return _isBigPropic; } set { _isBigPropic = value; OnPropertyChanged("isBigPropic"); } }
		/// <summary>
		/// 이미지 뷰어에서 항상 이미지 원본을 불러올지 여부를 설정 합니다.
		/// </summary>
		public bool isLoadOriginalImage { get; set; } = false;
		/// <summary>
		/// 상단 UI를 작게 표시 할지 여부를 설정 합니다.
		/// </summary>
		public bool isSmallUI { get; set; } = false;
				/// <summary>
		/// 이미지 미리보기를 TL에 표시 여부를 설정합니다.
		/// </summary>
		public bool isShowPreview { get { return _isShowPreview; } set { _isShowPreview = value; OnPropertyChanged("isShowPreview"); } }//이미지 미리보기
		/// <summary>
		/// 이미지 뷰어에 원본 트윗 표시 여부를 설정 합니다.
		/// </summary>
		public bool isShowImageTweet { get; set; } = true;
		/// <summary>
		/// 이미지 뷰어 하단 바 표시 여부를 설정 합니다.
		/// </summary>
		public bool isShowImageBottom { get; set; } = true;//이미지 뷰어 하단 바
		#endregion

		#region 유저스트리밍 호흡기 설정
		/// <summary>
		/// 유저스트리밍 호흡기를 사용 할지 여부를 설정 합니다.
		/// </summary>
		public bool isUseStreaming { get; set; } = false;
		/// <summary>
		/// 프로그램 구동 시 유저스트리밍 호흡기를 킬지 여부를 설정 합니다.
		/// </summary>
		public bool isAutoRunStreaming { get; set; } = false;
		/// <summary>
		/// 스트리밍 호흡기 파일 경로입니다.
		/// </summary>
		public string streamFilePath { get; set; } = string.Empty;
		/// <summary>
		/// 스트리밍 호흡기 PORT번호입니다.
		/// </summary>
		public int streamPort { get; set; } = 8080;
		#endregion

		#region 트윗 표시 설정
		/// <summary>
		/// 내 트윗이 리트윗 되었을 때 홈에 표시 여부를 설정 합니다.
		/// </summary>
		public bool isShowRetweet { get; set; } = true;
		/// <summary>
		/// 내 트윗이 리트윗 되었을 때 멘션함에 표시 여부를 설정 합니다.
		/// </summary>
		public bool isNotiRetweet { get; set; } = true;
		/// <summary>
		/// 뮤트를 멘션함에도 적용 할지 여부를 설정 합니다.
		/// </summary>
		public bool isMuteMention { get; set; } = true;
		#endregion

		#region 프로그램 초기 구동 설정
		/// <summary>
		/// 프로그램 구동 시 팔로잉 목록을 가져올지 여부를 설정 합니다.
		/// </summary>
		public bool isLoadFollwing { get; set; } = true;
		/// <summary>
		/// 프로그램 구동 시 차단 목록을 가져올지 여부를 설정 합니다.
		/// </summary>
		public bool isLoadBlock { get; set; } = true;

		#endregion

		#region 폰트 설정
		/// <summary>
		/// 트윗 표시 폰트입니다.
		/// </summary>
		public FontFamily font { get { return _font; } set { _font = value; OnPropertyChanged("font"); } }
		/// <summary>
		/// 트윗을 굵게 표시 할 경우 사용 합니다.
		/// </summary>
		public bool isBoldFont { get { return _isBoldFont; } set { _isBoldFont = value; OnPropertyChanged("FontWeight"); } }
		/// <summary>
		/// 폰트 사이즈입니다.
		/// </summary>
		public int fontSize { get { return _fontSize; } set { _fontSize = value; OnPropertyChanged("fontSize"); } }
		#endregion

		#region 스킨 및 알림
		/// <summary>
		/// 스킨 이름입니다.
		/// </summary>
		public string skinName { get; set; } = "pink";
		/// <summary>
		/// 알림음을 재생할지 여부를 설정 합니다.
		/// </summary>
		public bool isPlayNoti { get; set; } = false;
		/// <summary>
		/// 알림음 파일의 경로입니다.
		/// </summary>
		public string notiSound { get; set; }

		#endregion

		#region 뮤트 및 하이라이트

		/// <summary>
		/// 단어 하이라이트 목록입니다.
		/// </summary>
		public List<string> listHighlight = new List<string>();
		/// <summary>
		/// 단어 뮤트 목록입니다.
		/// </summary>
		public List<string> listMuteWord = new List<string>();
		/// <summary>
		/// 클라이언트 뮤트 목록입니다.
		/// </summary>
		public List<string> listMuteClient = new List<string>();
		/// <summary>
		/// 사용자 뮤트 목록 입니다.
		/// </summary>
		public List<string> listMuteUser = new List<string>();
		/// <summary>
		/// 트윗 뮤트 목록입니다.
		/// </summary>
		public Dictionary<long, string> dicMuteTweet = new Dictionary<long, string>();//트윗 뮤트

		#endregion

		/// <summary>
		/// 프로그램의 오류를 자동으로 보낼지 여부를 설정 합니다.
		/// </summary>
		public bool isSendError { get; set; } = false;

		/// <summary>
		/// 이미지 저장 폴더 경로입니다.
		/// </summary>
		public string imageFolderPath { get; set; } = "Image";



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
		}

		public void AddMuteClient(string client)
		{
			listMuteClient.Add(client);
		}

		public void AddMuteTweet(ClientTweet tweet)
		{
			if (dicMuteTweet.ContainsKey(tweet.originalTweet.id) == false)
				dicMuteTweet.Add(tweet.originalTweet.id, tweet.originalTweet.text);
		}
	}

}
