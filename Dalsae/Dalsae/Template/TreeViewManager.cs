using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using SharpRaven;
using SharpRaven.Data;
using static Dalsae.TweetManager;

namespace Dalsae.Template
{
	public class MyVirtualizingStackPanel : VirtualizingStackPanel
	{
		/// <summary>
		/// Publically expose BringIndexIntoView.
		/// </summary>
		public void BringIntoView(int index)
		{
			this.BringIndexIntoView(index);
		}
	}

	public class TreeViewManager
	{
		public TreeViewManager(TreeView treeView, ObservableCollection<ClientTweet> listTweet)
		{
			this.treeView = treeView;
			this.listTweet = listTweet;
			((INotifyCollectionChanged)treeView.ItemsSource).CollectionChanged +=
				new NotifyCollectionChangedEventHandler(CollectionChanged);
			//treeView.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
		}
		public TreeViewManager(TreeView treeView, ObservableCollection<ClientDirectMessage> listDM)
		{
			this.treeView = treeView;
			this.listDM= listDM;
			((INotifyCollectionChanged)treeView.ItemsSource).CollectionChanged +=
				new NotifyCollectionChangedEventHandler(CollectionChanged);
			//treeView.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
		}

		public void SetTweetGrid(Grid gridTweet, Grid gridRetweet)
		{

		}

		/// <summary>
		/// TweetManager에서 관리 중인 트윗 리스트
		/// 트리뷰 순회에 사용한다
		/// </summary>
		private ObservableCollection<ClientTweet> listTweet = null;
		/// <summary>
		/// TweetManager에서 관리 중인 DM리트스
		/// 트리뷰 순회에 사용한다
		/// </summary>
		private ObservableCollection<ClientDirectMessage> listDM = null;
		public TreeView treeView { get; set; }
		private bool isAddedTweet = false;
		private ScrollViewer _scrollViewer = null;
		private double prevScrollPosition = -1;
		public ScrollViewer scrollViewer
		{
			get
			{
				return _scrollViewer;
			}
			set
			{
				_scrollViewer = value;
				_scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
			}
		}

	

		private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (prevScrollPosition != -1)
			{
				scrollViewer.ScrollToVerticalOffset(prevScrollPosition);
				if (isAddedTweet == false)
					prevScrollPosition = -1;
			}
		}

		private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				if (_scrollViewer.ContentVerticalOffset != 0.0)///스크롤 고정용 기능
				{
					isAddedTweet = true;
					prevScrollPosition = _scrollViewer.ContentVerticalOffset;
					int index = 0;
					if (e.NewItems[0] is ClientTweet)
					{
						ClientTweet tweet = e.NewItems[0] as ClientTweet;
						index = listTweet.IndexOf(tweet);
					}
					else
					{
						ClientDirectMessage dm = e.NewItems[0] as ClientDirectMessage;
						index = listDM.IndexOf(dm);
					}
					TreeViewItem addedItem = GetTreeViewItem(treeView, e.NewItems[0], index);
					if (addedItem == null) return;
					addedItem.Loaded += AddTreeViewItem_LoadedScroll;
				}
				//addedItem.Loaded += SetBackgroundTweet;
				//prevScrollPosition = _scrollViewer.ContentVerticalOffset + firstItem.DesiredSize.Height;
				//_scrollViewer.ScrollToVerticalOffset(prevScrollPosition);
			}
		}

		private void AddTreeViewItem_LoadedScroll(object sender, RoutedEventArgs e)
		{
			TreeViewItem item = sender as TreeViewItem;
			item.Loaded -= AddTreeViewItem_LoadedScroll;
			prevScrollPosition += item.DesiredSize.Height;
			_scrollViewer.ScrollToVerticalOffset(prevScrollPosition);
			isAddedTweet = false;
		}



		/// <summary>
		/// ListBox에 바인딩 시킨 Source에 변화가 있을 경우 호출 되는 이벤트
		/// sender.status가 Generated일 때 ListBoxItem이 생성되어 있어 높이를 구할 수 있다
		/// </summary>
		//private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
		//{
		//	ItemContainerGenerator icGene = sender as ItemContainerGenerator;
		//	if (icGene.Status != GeneratorStatus.ContainersGenerated && isAddedTweet == false)
		//		return;
		//	if (listTweet == null) return;
		//	if (listTweet.Count <= 20) return;
		//	var v = icGene.GenerateBatches();
			
		//	//TreeViewItem firstItem = treeView.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
		//	TreeViewItem firstItem = GetTreeViewItem(treeView, listTweet[0], 0);
		//	//TreeViewItem item = GetTreeViewItem(treeView, )
		//	if (_scrollViewer == null || firstItem == null) return;
		//	firstItem.Loaded += TreeViewItem_Loaded;
		//}

		/// <summary>
		/// 트리뷰아이템이 실제 로드 되고 최종으로 불리는 이벤트
		/// 트리뷰아이템 추가 시 스크롤 이동과 배경 설정을 한다
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//private void TreeViewItem_Loaded(object sender, RoutedEventArgs e)
		//{
		//	TreeViewItem treeViewItem = sender as TreeViewItem;
		//	if (treeViewItem == null) return;
		//	LoadEndAfterScroll(treeViewItem);
		//	LoadEndAfterBackground(treeViewItem);

		//	isAddedTweet = false;
		//}

		/// <summary>
		/// 트리뷰아이템 로드 후 스크롤 고정 관련 작업
		/// </summary>
		/// <param name="item">로드 된 트리뷰아이템</param>
		//private void LoadEndAfterScroll(TreeViewItem item)
		//{
		//	item.Loaded -= TreeViewItem_Loaded;
		//	if (_scrollViewer.ContentVerticalOffset != 0.0 && isAddedTweet)
		//	{
		//		_scrollViewer.ScrollToVerticalOffset(_scrollViewer.ContentVerticalOffset + item.DesiredSize.Height);
		//	}
		//}

		/// <summary>
		/// 트리뷰 아이템 로드 후 배경 설정
		/// </summary>
		/// <param name="item">로드 된 트리뷰 아이템</param>
		//private void LoadEndAfterBackground(TreeViewItem item)
		//{
		//	if (item.DataContext is ClientTweet)
		//		SetBackgroundTweet(item);
		//	else
		//		SetBackgroundDM(item);
		//}

		/// <summary>
		/// 트리뷰 아이템이 트윗일 경우 배경 설정
		/// </summary>
		/// <param name="item">배경 설정 할 트리뷰 아이템</param>
		private void SetBackgroundTweet(object sender, RoutedEventArgs e)
		{
			TreeViewItem item = sender as TreeViewItem;
			item.Loaded -= SetBackgroundTweet;

			UIProperty uiProperty = null;
			if (item.DataContext is ClientTweet)
				uiProperty = ((ClientTweet)item.DataContext).uiProperty;
			else if(item.DataContext is ClientDirectMessage)
				uiProperty = ((ClientDirectMessage)item.DataContext).uiProperty;
			if (uiProperty == null) return;
			if (uiProperty.isBackOne)
			{
				Binding myBinding = new Binding();
				myBinding.Source = DataManager.DataInstence.skin;
				myBinding.Path = new PropertyPath("blockOne");
				myBinding.Mode = BindingMode.OneWay;
				myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
				BindingOperations.SetBinding(item, Control.BackgroundProperty, myBinding);
			}
			else
			{
				Binding myBinding = new Binding();
				myBinding.Source = DataManager.DataInstence.skin;
				myBinding.Path = new PropertyPath("blockTwo");
				myBinding.Mode = BindingMode.OneWay;
				myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
				BindingOperations.SetBinding(item, Control.BackgroundProperty, myBinding);
			}

		}

		/// <summary>
		/// 트리뷰 아이템이 DM일 경우 배경설정
		/// </summary>
		/// <param name="item">배경을 설정 할 트리뷰 아이템</param>
		//private void SetBackgroundDM(TreeViewItem item)
		//{
		//	if (isBackgroundOne)
		//	{
		//		Binding myBinding = new Binding();
		//		myBinding.Source = DataManager.DataInstence.skin.blockOne;
		//		myBinding.Path = new PropertyPath("DataInstence.skin.blockOne");
		//		myBinding.Mode = BindingMode.OneWay;
		//		myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
		//		BindingOperations.SetBinding(item, Control.BackgroundProperty, myBinding);
		//	}
		//	else
		//	{
		//		Binding myBinding = new Binding();
		//		myBinding.Source = DataManager.DataInstence.skin.blockTwo;
		//		myBinding.Path = new PropertyPath("DataInstence.skin.blockTwo");
		//		myBinding.Mode = BindingMode.OneWay;
		//		myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
		//		BindingOperations.SetBinding(item, Control.BackgroundProperty, myBinding);
		//	}

		//	isBackgroundOne = !isBackgroundOne;//배경색을 변경해야 해서 플래그 엎음
		//}

		/// <summary>
		/// Virtualizing된 TreeViewItem을 찾는다.
		/// </summary>
		/// <param name="container">아이템 컨테이너(treeview 혹은 treeviewitem)</param>
		/// <param name="item">찾을 datacontext(여기선 clienttweet)</param>
		/// <param name="startIndex">시작 할 TreeView인덱스(해당 index 하위에 아이템이 있다)</param>
		/// <returns></returns>
		private TreeViewItem GetTreeViewItem(ItemsControl container, object item, int startIndex)
		{
			if (container != null)
			{
				if (container.DataContext == item)
				{
					return container as TreeViewItem;
				}

				// Expand the current container
				if (container is TreeViewItem && !((TreeViewItem)container).IsExpanded)
				{
					container.SetValue(TreeViewItem.IsExpandedProperty, true);
				}

				// Try to generate the ItemsPresenter and the ItemsPanel.
				// by calling ApplyTemplate.  Note that in the 
				// virtualizing case even if the item is marked 
				// expanded we still need to do this step in order to 
				// regenerate the visuals because they may have been virtualized away.

				container.ApplyTemplate();
				ItemsPresenter itemsPresenter =
					(ItemsPresenter)container.Template.FindName("ItemsHost", container);
				if (itemsPresenter != null)
				{
					itemsPresenter.ApplyTemplate();
				}
				else
				{
					// The Tree template has not named the ItemsPresenter, 
					// so walk the descendents and find the child.
					itemsPresenter = FindVisualChild<ItemsPresenter>(container);
					if (itemsPresenter == null)
					{
						container.UpdateLayout();

						itemsPresenter = FindVisualChild<ItemsPresenter>(container);
					}
				}

				Panel itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);


				// Ensure that the generator for this panel has been created.
				UIElementCollection children = itemsHostPanel.Children;

				MyVirtualizingStackPanel virtualizingPanel =
					itemsHostPanel as MyVirtualizingStackPanel;

				for (int i = startIndex, count = container.Items.Count; i < count; i++)
				{
					startIndex = 0;
					TreeViewItem subContainer;
					if (virtualizingPanel != null)
					{
						if (virtualizingPanel.IsLoaded == false) return null;
						if (virtualizingPanel.IsInitialized == false) return null;
						// Bring the item into view so 
						// that the container will be generated.
						virtualizingPanel.BringIntoView(i);

						subContainer =
							(TreeViewItem)container.ItemContainerGenerator.ContainerFromIndex(i);
					}
					else
					{
						subContainer =
							(TreeViewItem)container.ItemContainerGenerator.ContainerFromIndex(i);

						// Bring the item into view to maintain the 
						// same behavior as with a virtualizing panel.
						subContainer.BringIntoView();
					}

					if (subContainer != null)
					{
						// Search the next level for the object.
						TreeViewItem resultContainer = GetTreeViewItem(subContainer, item, startIndex);
						if (resultContainer != null)
						{
							return resultContainer;
						}
						else
						{
							// The object is not under this TreeViewItem
							// so collapse it.
							//subContainer.IsExpanded = true;
						}
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Search for an element of a certain type in the visual tree.
		/// </summary>
		/// <typeparam name="T">The type of element to find.</typeparam>
		/// <param name="visual">The parent element.</param>
		/// <returns></returns>
		private T FindVisualChild<T>(Visual visual) where T : Visual
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
			{
				Visual child = (Visual)VisualTreeHelper.GetChild(visual, i);
				if (child != null)
				{
					T correctlyTyped = child as T;
					if (correctlyTyped != null)
					{
						return correctlyTyped;
					}

					T descendent = FindVisualChild<T>(child);
					if (descendent != null)
					{
						return descendent;
					}
				}
			}

			return null;
		}

		//------------------------------------------------------------------------------------------------------
		//----------------------------------------DM화살표--------------------------------------------------
		//------------------------------------------------------------------------------------------------------


		//------------------------------------------------------------------------------------------------------
		//------------------------------------화살표 아래 키--------------------------------------------------
		//------------------------------------------------------------------------------------------------------
		public bool ArrowDown()
		{
			if (listTweet != null)
			{
				FindNextTweet();
				return true;
			}
			else
			{
				ClientDirectMessage dm = treeView.SelectedItem as ClientDirectMessage;
				if (dm == null&&listDM?.Count>0)
				{
					GetTreeViewItem(treeView, listDM[0], 0)?.Focus();
				}
				else
				{
					int index = listDM.IndexOf(dm) + 1;
					if (index >= listDM.Count) return false;
					GetTreeViewItem(treeView, listDM[index], index)?.Focus();
				}
				return true;
			}
		}

		private void FindNextTweet()
		{
			ClientTweet nextTweet = GetNextTweet_Recursion(null, listTweet);
			FocusTreeViewItem(nextTweet);
		}

		private ClientTweet GetNextTweet_Recursion(ClientTweet parent, ObservableCollection<ClientTweet> listTweet)
		{
			for (int i = 0; i < listTweet.Count; i++)
			{
				ClientTweet nowItem = listTweet[i];
				if (nowItem == treeView.SelectedItem)
				{
					if (nowItem.uiProperty.childNode.Count > 0)
						return nowItem.uiProperty.childNode[0];
					if (i + 1 < listTweet.Count)
					{
						return listTweet[i + 1];
					}
					else
					{
						bFind = true;
						return parent;
					}
				}

				ClientTweet item = GetNextTweet_Recursion(nowItem, nowItem.uiProperty.childNode);
				if (bFind)
				{
					if (i + 1 < listTweet.Count)
					{
						bFind = false;
						return listTweet[i + 1];
					}
					else
					{
						return parent;
					}
				}
				if (item != null)
					return item;
			}
			return null;
		}

		//------------------------------------------------------------------------------------------------------
		//--------------------------------------화살표 위 키--------------------------------------------------
		//------------------------------------------------------------------------------------------------------
		public bool ArrowUp()
		{
			if (listTweet != null)
				return FindPrevTweet();
			else
			{
				ClientDirectMessage dm = treeView.SelectedItem as ClientDirectMessage;
				if (dm != null)
				{
					int index = listDM.IndexOf(dm) - 1;
					if (index < 0) return false;
					GetTreeViewItem(treeView, listDM[index], index)?.Focus();
				}
				return true;
			}
		}

		private bool FindPrevTweet()
		{
			ClientTweet prevTweet = GetPrevItem_Recursion(null, listTweet);
			if (prevTweet == null) return false;
			ClientTweet findTweet = null;
			if (bFind)
				findTweet = prevTweet;
			else
				findTweet = GetPrevChildItem(prevTweet, prevTweet.uiProperty.childNode);
			bFind = false;
			FocusTreeViewItem(findTweet);
			return true;
		}

		/// <summary>
		/// 특정 트윗의 treeviewitem을 찾아 선택 합니다.
		/// </summary>
		/// <param name="tweet">선택 할 트윗</param>
		/// <param name="index">index를 지정 해서 선택 할 경우</param>
		/// <returns></returns>
		private bool FocusTreeViewItem(ClientTweet tweet, int index = -1)
		{
			if (tweet == null) return false;
			if (index == -1)//index를 지정 해서 focus요청 할 경우가 아닌 경우
				index = FindParentIndex(tweet, listTweet);
			if (index == -1) return false;
			TreeViewItem item = null;
			//for (int i = 0; i < 50; i++)
			//{
				item = GetTreeViewItem(treeView, tweet, index);
			//	if (item == null) break;
			//	if (item.IsVisible) break;
			//}
			SelectTreeViewItem(item);
			return true;
		}

		private void SelectTreeViewItem(TreeViewItem item)
		{
			if (item == null) return;

			item.IsSelected = true;
			item.Focus();
		}

		/// <summary>
		/// 이동하려는 트윗의 부모 index를 찾는다
		/// 화살표 네비게이션이나 포커스 할 때 사용
		/// </summary>
		/// <param name="tweet">선택 하려는 트윗</param>
		/// <param name="listTweet">트리뷰 부모 리스트</param>
		/// <returns></returns>
		private int FindParentIndex(ClientTweet tweet, ObservableCollection<ClientTweet> listTweet)
		{
			for (int i = 0; i < listTweet.Count; i++)
			{
				foreach (ClientTweet item in listTweet[i].uiProperty.childNode)
				{
					//지금 이 부분에서 부모 index를 못 구함
					int index = FindParent(tweet, listTweet);
					if (index != -1)
						return index;
				}
				if (listTweet[i] == tweet)
					return i;
			}
			return -1;
		}

		/// <summary>
		/// 재귀로 선택 트윗 찾는 함수, index구하는 함수에서 호출한다
		/// </summary>
		/// <param name="tweet">찾으려는 선택 트윗</param>
		/// <param name="listTweet">순회 할 트윗 리스트(1단 위 부모 리스트)</param>
		/// <returns></returns>
		private int FindParent(ClientTweet tweet, ObservableCollection<ClientTweet> listTweet)
		{
			for (int i = 0; i < listTweet.Count; i++)
			{
				if (listTweet[i].uiProperty == tweet.uiProperty.parentTweet)
				{
					return i;
				}
				foreach (ClientTweet item in listTweet[i].uiProperty.childNode)
				{
					if (FindParent(tweet, listTweet[i].uiProperty.childNode) != -1)
						return i;
				}
			}
			return -1;
		}
		/// <summary>
		/// 트리뷰 상단 순회를 위한 플래그
		/// </summary>
		private bool bFind = false;
		private ClientTweet GetPrevItem_Recursion(ClientTweet parent, ObservableCollection<ClientTweet> items)
		{
			for (int i = items.Count - 1; i > -1; i--)
			{
				ClientTweet prevParent = items[i];

				if (prevParent.Equals(treeView.SelectedItem))//선택 한 아이템일 경우 상위 노드를 리턴해야 함
				{
					if (i - 1 < 0)
					{
						bFind = true;
						return parent;
					}
					else
					{
						ClientTweet prevItem = items[i - 1];
						return prevItem;
					}
				}
				ClientTweet ret = GetPrevItem_Recursion(prevParent, prevParent.uiProperty.childNode);
				if (ret != null)
					return ret;
			}
			return null;
		}

		private ClientTweet GetPrevChildItem(ClientTweet parent, ObservableCollection<ClientTweet> items)
		{
			if (items.Count == 0)
				return parent;
			ClientTweet lastItem = items[items.Count - 1];
			ClientTweet ret = GetPrevChildItem(lastItem, lastItem.uiProperty.childNode);
			if (ret != null)
				return ret;
			else
				return null;
		}

		public void Focus()
		{
			if (treeView.Items?.Count == 0)
				treeView.Focus();
			else if (treeView.SelectedItem == null)
			{
				if (listTweet != null)
					FocusTreeViewItem(listTweet[0], 0);
				else if (listDM != null)
				{
					TreeViewItem item = GetTreeViewItem(treeView, listDM[0], 0);
					if (item != null)
						item.Focus();
				}
			}
			else if(listTweet!=null)//트윗을 선택 하고 이전 스크롤 유지하는 기능
			{
				ClientTweet tweet = treeView.SelectedItem as ClientTweet;
				if (tweet == null)
				{
					//App.SendException("TreeViewManager Focus Tweet NULL!");
					treeView.Focus();
					return;
				}
				int index = FindParentIndex(tweet, listTweet);
				if (index == -1) return;
				TreeViewItem treeViewItem = GetTreeViewItem(treeView, tweet, index);
				if (treeViewItem == null)
				{
					//App.SendException("TreeViewManager Focus TreeViewItem NULL!");
					treeView.Focus();
					return;
				}
				Point selPoint = treeViewItem.TranslatePoint(new Point(0, 0), scrollViewer);
				if (selPoint.Y < 0 && selPoint.Y + treeViewItem.ActualHeight < 0 || selPoint.Y > scrollViewer.ActualHeight)
					prevScrollPosition = scrollViewer.VerticalOffset;
				SelectTreeViewItem(treeViewItem);
			}
			else
			{
				ClientDirectMessage dm = treeView.SelectedItem as ClientDirectMessage;
				int index = listDM.IndexOf(dm);
				GetTreeViewItem(treeView, dm, index)?.Focus();
			}
		}

		public void SelectHome()
		{
			if (listTweet != null)
			{
				if (listTweet.Count == 0) return;
				FocusTreeViewItem(listTweet[0], 0);
			}
			else if (listDM != null)
			{
				if (listDM.Count == 0) return;
				TreeViewItem item = GetTreeViewItem(treeView, listDM[0], 0);
				if (item != null)
					item.Focus();
			}
		}

		public void SelectEnd()
		{
			if (listTweet != null)
			{
				if (listTweet.Count == 0) return;
				FocusTreeViewItem(listTweet[listTweet.Count - 1], listTweet.Count - 1);
			}
			else if (listDM != null)
			{
				if (listDM.Count == 0) return;
				TreeViewItem item = GetTreeViewItem(treeView, listDM[listDM.Count - 1], listDM.Count - 1);
				if (item != null)
					item.Focus();
			}
		}

		public void FindPrevUserTweet(long id)
		{
			if (listTweet == null) return;
			int startIndex = FindParentIndex(treeView.SelectedItem as ClientTweet, listTweet);
			int index = TweetInstence.FindPrevUserTweet(listTweet, startIndex, id);
			if (index == -1) return;
			FocusTreeViewItem(listTweet[index], index);
		}

		public void FindNextUserTweet(long id)
		{
			if (listTweet == null) return;
			int startIndex = FindParentIndex(treeView.SelectedItem as ClientTweet, listTweet);
			int index = TweetInstence.FindNextUserTweet(listTweet, startIndex, id);
			if (index == -1) return;
			FocusTreeViewItem(listTweet[index], index);
		}

		public TreeViewItem GetSelectedTreeViewItem()
		{
			if (listTweet != null)
			{
				int index = FindParentIndex(treeView.SelectedItem as ClientTweet, listTweet);
				if (index == -1) return null;
				return GetTreeViewItem(treeView, treeView.SelectedItem as ClientTweet, index);
			}
			else
			{
				ClientDirectMessage dm = treeView.SelectedItem as ClientDirectMessage;
				if (dm != null)
				{
					int index = listDM.IndexOf(dm);
					if (index < 0) return null;
					return GetTreeViewItem(treeView, listDM[index], index);
				}
				else
					return null;
			}
		}

		public void ShowTreeView()
		{
			Panel.SetZIndex(treeView, 3);
			//treeView.Visibility = System.Windows.Visibility.Visible;
		}

		public void HideTreeView()
		{
			Panel.SetZIndex(treeView, 0);
			//treeView.Visibility = System.Windows.Visibility.Hidden;
		}
	}
}
