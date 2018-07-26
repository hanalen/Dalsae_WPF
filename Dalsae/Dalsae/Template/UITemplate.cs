using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Newtonsoft.Json;
using SharpRaven;
using SharpRaven.Data;
using static Dalsae.DataManager;

namespace Dalsae
{
	public class UIProperty:BaseNoty
	{
		public TweetList childNode { get; private set; } = new TweetList();
		public UIProperty parentTweet { get; set; }
		public delegate void DeleAddSingleTweet(ClientTweet tweet, UIProperty parent, bool isQT);
		private HashSet<long> hashTweet = new HashSet<long>();//대화 트윗 hash
		private bool _isDeleteTweet = false;
		public bool isDeleteTweet
		{
			get { return _isDeleteTweet; }
			set { _isDeleteTweet = value; OnPropertyChanged("isDeleteTweet"); }
		}
		private bool _isHighlight = false;
		public bool isHighlight
		{
			get { return _isHighlight; }
			set { _isHighlight = value; OnPropertyChanged("isHighlight"); }
		}
		private bool _isShowQtTweet = false;

		private bool _isBackOne = true;
		public bool isBackOne
		{
			get { return _isBackOne; }
			set { _isBackOne = value; OnPropertyChanged("isBackOne"); }
		}

		public bool isQtTweet { get; private set; } = false;
		/// <summary>
		/// Delegate용 함수, 인용&대화 트윗을 불러와서 추가 할 때 사용
		/// </summary>
		/// <param name="tweet">인용&대화 트윗</param>
		/// <param name="parent">부모 트윗, 인용일 경우 인용이 오고 아닐 경우 상위 트윗이 옴</param>
		/// <param name="isDeahwa">대화인지 인용인지, true: 대화 / false: 인용</param>
		public void AddSingleTweet(ClientTweet tweet, UIProperty parent, bool isQT)
		{
			//부모 연결, 인용 트윗 설정
			tweet.uiProperty.parentTweet = parent == null ? this : parent;
			tweet.uiProperty.isQtTweet = isQT;
			
			if(parent==null)//TL에 보이는 최상위 트윗에서 호출
				AddTweet(tweet);
			else//하위 트윗에서 호출, 인용 혹은 대화에서 호출 시
				parent.AddTweet(tweet);
		}

		/// <summary>
		/// 하위 트윗 추가(대화, 인용)
		/// </summary>
		/// <param name="tweet">대화 트윗</param>
		private void AddTweet(ClientTweet tweet)
		{
			if (hashTweet.Contains(tweet.id)) return;

			hashTweet.Add(tweet.id);
			childNode.Add(tweet);
		}

		/// <summary>
		/// 트리뷰에서 닫을 경우 하위 트윗을 날린다. 캐싱은 나중에 생각 하자
		/// </summary>
		/// <param name="tweet"></param>
		public void ClearDeahwa()
		{
			childNode.Clear();
			hashTweet.Clear();
		}
	}


	public class BaseNoty : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(string info)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}
	}

	public class PropicConverter:IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			object ret = null;
			string param = parameter?.ToString() ?? "";
			if (targetType.Name == "ImageSource")
			{
				if (value is ClientTweet)
				{
					ClientTweet tweet = value as ClientTweet;
					if (tweet.originalTweet == null)
					{
						App.SendException("Propic Converter Original Tweet NULL!","");
						return null;
					}
					if (tweet.originalTweet.user == null)
					{
						App.SendException("Propic Converter Original Tweet -> user NULL!","");
						return null;
					}
					if (param == "big")
						ret = tweet.originalTweet?.user?.profile_image_url.Replace("_normal", "_bigger");
					else
						ret = tweet.originalTweet?.user?.profile_image_url;
				}
				else if(value is string)
				{
					string str = value.ToString();
					if (param == "big")
						ret = str.Replace("_normal", "_bigger");
					else
						ret = str;
				}
			}
			else if (targetType.Name == "Visibility")
			{
				if (param == "big")
					ret = DataInstence.option.isBigPropic ? Visibility.Visible : Visibility.Collapsed;
				else
					ret = DataInstence.option.isBigPropic == false ? Visibility.Visible : Visibility.Collapsed;
			}
			return ret;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	public class PreviewConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			object ret = null;
			if (targetType.Name == "ImageSource")
			{
				ClientTweet tweet = value as ClientTweet;
				if (tweet == null) return null;
				if (tweet.isPhoto)
				{
					string param = parameter?.ToString() ?? "";
					if (param == "one" && tweet.mediaEntities.media.Count > 0)
						ret = $"{tweet.mediaEntities.media[0].media_url_https}:thumb";
					else if (param == "two" && tweet.mediaEntities.media.Count > 1)
						ret = $"{tweet.mediaEntities.media[1].media_url_https}:thumb";
					else if (param == "three" && tweet.mediaEntities.media.Count > 2)
						ret = $"{tweet.mediaEntities.media[2].media_url_https}:thumb";
					else if (param == "four" && tweet.mediaEntities.media.Count > 3)
						ret = $"{tweet.mediaEntities.media[3].media_url_https}:thumb";
					else
						ret = null;
				}
				else if(tweet.isMovie)//동영상 썸네일
				{
					string param = parameter?.ToString() ?? "";
					if (param == "one" && tweet.extended_entities?.media.Count > 0)
						ret = $"{tweet.extended_entities.media[0].media_url_https}:thumb";
					else
						ret = null;
				}
			}
			else if (targetType.Name == "Visibility")
			{
				ret = DataInstence.option.isShowPreview ? Visibility.Visible : Visibility.Collapsed;
			}
			return ret;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	public class ContentConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			object ret = null;
			ret = (bool)value ? "언팔로우 하기": "팔로우 하기";
			return ret;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	//그리드에 BackGround랑 Brush가 겹쳐서 분리. 나중에 정리 해야할듯
	public class FontColorConverter:IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//ClientTweet tweet = value as ClientTweet;
			//if (tweet == null) return null;
			object ret = null;
			if (targetType.Name == "Brush")
				ret = (bool)value ? DataInstence.skin.mention : DataInstence.skin.tweet;
			return ret;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>
	/// 배경 색 컨버터
	/// </summary>
	public class BackgroundConvert : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			object ret = null;

			UIProperty uiProperty = null;
			if (value is ClientTweet)
				uiProperty = ((ClientTweet)value).uiProperty;
			else if (value is ClientDirectMessage)
				uiProperty = ((ClientDirectMessage)value).uiProperty;

			if (uiProperty == null)
				return null;

			if (uiProperty.isBackOne)
				ret = DataInstence.skin.blockOne;
			else
				ret = DataInstence.skin.blockTwo;
			return ret;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	public class BoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//ClientTweet tweet = value as ClientTweet;
			//if (tweet == null) return null;
			object ret = null;
			if (targetType.Name == "FontWeight")
				ret = DataInstence.option.isBoldFont ? FontWeights.Bold : FontWeights.Normal;
			else if (targetType.Name == "TextDecorationCollection")
				ret = (bool)value ? "Strikethrough" : "";
			else if (targetType.Name == "Visibility")
			{
				if(value is bool)
					ret = (bool)value ? Visibility.Visible : Visibility.Collapsed;
				else if(value is string)
				{
					string str = value.ToString();
					if (string.IsNullOrEmpty(str))
						ret = Visibility.Collapsed;
					else
						ret = Visibility.Visible;
				}
			}
			else if (targetType.Name == "Brush")
				ret = (bool)value ? DataInstence.skin.blockOne : DataInstence.skin.blockTwo;
			else if (targetType.Name == "Foreground")
				ret = (bool)value ? DataInstence.skin.mention : DataInstence.skin.tweet;
			return ret;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	public class SumConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Type t = values[0].GetType();
			double totalWidth = 0;
			double.TryParse(values[0].ToString(), out totalWidth);
			double parentCount = 0;
			double.TryParse(values[1].ToString(), out parentCount);//자식까지 넓이 제대로 구하려면 부모 수를 -1뺴서
			parentCount--;                                  //최상위 item에서도 20픽셀 안 빠지게 계산 해야함

			double ret = totalWidth - parentCount * 20.0;
			if (ret < 0)
				ret = 10;
			return ret;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	// count the number of TreeViewItems before reaching ScrollContentPresenter
	public class ParentCountConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			int parentCount = 1;
			DependencyObject o = VisualTreeHelper.GetParent(value as DependencyObject);
			while (o != null && o.GetType().FullName != "System.Windows.Controls.ScrollContentPresenter")
			{
				if (o.GetType().FullName == "System.Windows.Controls.TreeViewItem")
					parentCount += 1;
				o = VisualTreeHelper.GetParent(o);
			}
			return parentCount;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class DeahwaConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//ClientTweet tweet = value as ClientTweet;
			//if (tweet == null) return null;
			object ret = null;
			if (targetType.Name == "Brush")
				ret = (bool)value ? DataInstence.skin.mentionOne : DataInstence.skin.mentionTwo;
			else if (targetType.Name == "Visibility")
				ret = (bool)value ? Visibility.Visible : Visibility.Collapsed;
			return ret;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	public class DeahwaTemplate : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;

			if (element != null && item != null && item is ClientTweet)
			{
				ClientTweet tweet = item as ClientTweet;
				if (tweet != null)
					return element.FindResource("dhTweet") as DataTemplate;
			}

			return null;
		}
	}

	public class TweetTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;

			if (element != null && item != null && item is ClientTweet)
			{
				ClientTweet tweet = item as ClientTweet;
				if (tweet.originalTweet == null)
					return element.FindResource("moreButton") as DataTemplate;
				else if (tweet.isRetweet)
					return element.FindResource("retweetControl") as DataTemplate;
				else
					return element.FindResource("tweetControl") as DataTemplate;

			}

			return null;
		}
	}

	public class DataTemplateParameters : DependencyObject
	{
		public static double GetValueToCompare(DependencyObject obj)
		{
			return (double)obj.GetValue(ValueToCompareProperty);
		}

		public static void SetValueToCompare(DependencyObject obj, double value)
		{
			obj.SetValue(ValueToCompareProperty, value);
		}

		public static readonly DependencyProperty ValueToCompareProperty =
			DependencyProperty.RegisterAttached("ValueToCompare", typeof(double),
												  typeof(DataTemplateParameters));

	}

	public class WidthNoti:BaseNoty
	{
		private double _width;
		public double Width
		{
			get { return _width; }
			set { _width = value; OnPropertyChanged("Width"); }
		}
	}

	public class BoolFlagNoti : BaseNoty
	{
		private bool _isOn=false;
		public bool isOn
		{
			get { return _isOn; }
			set { _isOn = value; OnPropertyChanged("isOn"); }
		}
	}

	public class MessageQueue:BaseNoty
	{
		//private const int conShowTime = 2;
		//private DateTime time;
		private Queue<string> queue = new Queue<string>();
		private bool _isShowMessage;
		public bool isShowMessage { get { return _isShowMessage; } set { _isShowMessage = value; OnPropertyChanged("isShowMessage"); } }
		private string _message = string.Empty;
		public string message { get { return _message; } set { _message = value; OnPropertyChanged("message"); } }
		public void AddMessage(string message)
		{
			queue.Enqueue(message);
		}
		//메인윈도우에서 시간 체크할때마다 호출
		//return: true: 메시지가 남아있고 출력했음, false: 메시지큐가 없음
		public bool NextMessage()
		{
			if (queue.Count == 0)
			{
				isShowMessage = false;
				message = string.Empty;
				return false;
			}
			else
			{
				message = queue.Dequeue();
				isShowMessage = true;
				return true;
			}
		}
		//public void CheckMessageByDispatcherTimer(object sender, EventArgs e)
		//{
		//	if (isShowMessage == false) return;

		//	TimeSpan timeSpan = DateTime.Now - time;
		//	if (timeSpan.TotalSeconds < conShowTime) return;//메시지를 보여주는 시간이 안 됐으면 종료
		//	if (queue.Count != 0)
		//	{
		//		time = DateTime.Now;
		//		message = queue.Dequeue();
		//	}
		//	else
		//	{
		//		isShowMessage = false;
		//		message = string.Empty;
		//	}
		//}
	}

}
