//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
//using Newtonsoft.Json;
//using static Dalsae.FileManager;
//using static Dalsae.DalsaeManager;
//using static Dalsae.TwitterWeb;
//using static Dalsae.DataManager;
//using static Dalsae.TweetManager;
//using System.Text.RegularExpressions;
//using System.Collections.Specialized;
//using System.Windows.Threading;

//namespace Dalsae
//{
//	public class ListBoxManager
//	{
//		public ListBox listBox { get; set; }
//		private bool isAddedTweet = false;
//		private ScrollViewer _scrollViewer = null;
//		private double prevScrollPosition = -1;
//		public ScrollViewer scrollViewer
//		{
//			get
//			{
//				return _scrollViewer;
//			}
//			set
//			{
//				_scrollViewer = value;
//				_scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
//			}
//		}

	
//		public ListBoxManager(ListBox listbox)
//		{
//			this.listBox = listbox;
//			((INotifyCollectionChanged)listBox.ItemsSource).CollectionChanged +=
//				new NotifyCollectionChangedEventHandler(CollectionChanged);
//			listBox.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
//		}

//		/// <summary>
//		/// ListBox에 바인딩 시킨 Source에 변화가 있을 경우 호출 되는 이벤트
//		/// sender.status가 Generated일 때 ListBoxItem이 생성되어 있어 높이를 구할 수 있다
//		/// </summary>
//		private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
//		{
//			ItemContainerGenerator icGene = sender as ItemContainerGenerator;
//			if (icGene.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated && isAddedTweet == false)
//				return;
//			ListBoxItem firstItem = listBox.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;

//			if (_scrollViewer == null || firstItem == null) return;

//			if (_scrollViewer.ContentVerticalOffset != 0.0 && isAddedTweet)
//			{
//				_scrollViewer.ScrollToVerticalOffset(_scrollViewer.ContentVerticalOffset + firstItem.DesiredSize.Height);
//			}
//			isAddedTweet = false;
//		}

//		public void ShowListBox()
//		{
//			listBox.Visibility = Visibility.Visible;
//			Focus();
//		}

//		public void HideListBox()
//		{
//			listBox.Visibility = Visibility.Hidden;
//		}

//		public void Focus()
//		{
//			if (listBox.Items.Count == 0)
//			{
//				listBox.Focus();
//				return;
//			}
//			int index = listBox.SelectedIndex;
//			if (index == -1) index = 0;

//			ListBoxItem selTweet = listBox.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
//			if (selTweet == null)
//			{
//				listBox.Focus();
//			}
//			else
//			{
//				//이전 스크롤을 유지하는 기능
//				Point selPoint = selTweet.TranslatePoint(new Point(0, 0), scrollViewer);
//				if (selPoint.Y < 0 && selPoint.Y + selTweet.ActualHeight < 0 || selPoint.Y > scrollViewer.ActualHeight)
//					prevScrollPosition = scrollViewer.VerticalOffset;
//				selTweet.IsSelected = false;
//				selTweet.Focus();
//				selTweet.IsSelected = true;
//			}
//		}

//		public void SelectFirst()
//		{
//			if (listBox.Items.Count == 0) return;
//			prevScrollPosition = -1;
//			listBox.SelectedItem = listBox.Items[0];
//			ListBoxItem item = listBox.ItemContainerGenerator.ContainerFromItem(listBox.SelectedItem) as ListBoxItem;
//			item.IsSelected = true;
//			item.Focus();
//		}

//		public void SelectEnd()
//		{
//			if (listBox.Items.Count == 0) return;
//			prevScrollPosition = -1;
//			listBox.SelectedItem = listBox.Items[listBox.Items.Count - 1];
//			ListBoxItem item = listBox.ItemContainerGenerator.ContainerFromItem(listBox.SelectedItem) as ListBoxItem;
//			item.IsSelected = true;
//			item.Focus();
//		}

//		private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
//		{
//			if (e.Action == NotifyCollectionChangedAction.Add)
//			{
//				isAddedTweet = true;
//			}
//		}

//		private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
//		{
//			if (prevScrollPosition != -1)
//			{
//				scrollViewer.ScrollToVerticalOffset(prevScrollPosition);
//				prevScrollPosition = -1;
//			}
//		}
//	}
//}
