using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Dalsae.Data
{

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
