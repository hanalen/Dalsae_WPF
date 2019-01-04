using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.IO;
using static Dalsae.DataManager;
using static Dalsae.FileManager;
using System.Drawing.Text;
using System.Globalization;
using Microsoft.Win32;

namespace Dalsae.WindowAndControl
{
	/// <summary>
	/// OptionWIndow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class OptionWIndow : Window
	{
		private new System.Drawing.Font font = null;
		private string[] arrHighlight;
		private string[] arrMuteWord;
		private string[] arrMuteClient;
		private string[] arrMuteUser;
		private Dictionary<long, string> dicMuteTweet;

		public OptionWIndow()
		{
			InitializeComponent();
			ShowInTaskbar = false;

			SetCheckBox();
			SetComboBox();
			SetFont();
			SetImagePath();
			SetMute();
		}

		private void SetCheckBox()
		{
			Option option = DataInstence.option;

			//트윗 등록
			checkSendTweet.IsChecked = option.isYesnoTweet;
			checkRetweetProtect.IsChecked = option.isRetweetProtectUser;
			checkSendEnter.IsChecked = option.isSendEnter;

			//이미지 뷰어
			checkShowTweet.IsChecked = option.isShowImageTweet;
			checkBottomPreview.IsChecked = option.isShowImageBottom;
			checkLoadOrg.IsChecked = option.isLoadOriginalImage;

			//알림
			checkMute.IsChecked = option.isMuteMention;
			checkNotiMention.IsChecked = option.isNotiRetweet;
			checkNotiTL.IsChecked = option.isShowRetweet;
			checkPlayNoti.IsChecked = option.isPlayNoti;

			//UI
			checkSmallUI.IsChecked = option.isSmallUI;
			checkShowPreview.IsChecked = option.isShowPreview;
			checkShowPropic.IsChecked = option.isShowPropic;
			checkShowBigPropic.IsChecked = option.isBigPropic;

			//폰트
			cbBold.IsChecked = option.isBoldFont;

			//시작 옵션
			checkLoadBlock.IsChecked = option.isLoadBlock;
			checkLoadFollwing.IsChecked = option.isLoadFollwing;

			//스트리밍
			cbAutoRunStream.IsChecked = option.isAutoRunStreaming;
			cbUseStream.IsChecked = option.isUseStreaming;
			tbStreamFilePath.Text = option.streamFilePath;
			tbPort.Text = option.streamPort.ToString();

			//알림 설정 연동 컨트롤 on/off
			if (option.isPlayNoti == false)
			{
				textNoti.Visibility = Visibility.Hidden;
				comboNoti.Visibility = Visibility.Hidden;
				textNoti2.Visibility = Visibility.Hidden;
			}
		}

		private void SetComboBox()
		{
			string[] arrSound = FileInstence.GetSoundList();
			if (arrSound != null)
			{
				for (int i = 0; i < arrSound.Length; i++)
					comboNoti.Items.Add(arrSound[i]);
				comboNoti.SelectedIndex = comboNoti.Items.IndexOf(DataInstence.option.notiSound);
			}

			string[] arrSkin = FileInstence.GetSkinList();
			if (arrSkin != null)
			{
				for (int i = 0; i < arrSkin.Length; i++)
					comboSkin.Items.Add(arrSkin[i]);
				comboSkin.SelectedIndex = comboSkin.Items.IndexOf(DataInstence.option.skinName);
			}
		}

		private void SetFont()
		{
			InstalledFontCollection listFontCollection = new InstalledFontCollection();
			List<FontFamily> listFont = new List<FontFamily>();
			for (int i = 0; i < listFontCollection.Families.Length; i++)
				listFont.Add(new FontFamily(listFontCollection.Families[i].Name));
			comboFont.ItemsSource = listFont;

			List<int> listSize = new List<int>();
			for (int i = 8; i < 50; i++)
				listSize.Add(i);
			comboFontSize.ItemsSource = listSize;
			comboFont.SelectedItem = DataInstence.option.font;
			comboFontSize.SelectedItem = DataInstence.option.fontSize;
			cbBold.IsChecked = DataInstence.option.isBoldFont;
		}


		private void SetImagePath()
		{
			tbImagePath.Text = DataInstence.option.imageFolderPath;
		}

		private void SetMute()
		{
			Option option = DataInstence.option;

			arrHighlight = option.listHighlight.ToArray();
			arrMuteClient = option.listMuteClient.ToArray();
			arrMuteUser = option.listMuteUser.ToArray();
			arrMuteWord = option.listMuteWord.ToArray();
			dicMuteTweet = option.dicMuteTweet;
		}

		private void Save()
		{
			Option option = DataInstence.option;

			int port = 8080;
			if (int.TryParse(tbPort.Text, out port) == false)
			{
				MessageBox.Show(this, "스트리밍 PORT번호에 숫자만 입력 해주세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
				tbPort.Focus();
				return;
			}


			if (comboFont.SelectedItem != null && comboFont.SelectedItem is FontFamily)
				option.font = comboFont.SelectedItem as FontFamily;
			if (comboFontSize.SelectedItem != null)
				option.fontSize = (int)comboFontSize.SelectedItem;
			option.isBoldFont = (bool)cbBold.IsChecked;

			//이미지 경로가 수정 되었을 경우에만 수정
			if (option.imageFolderPath != tbImagePath.Text)
			{
				//경로가 기본 폴더인지 체크하고 기본 폴더면 기본 폴더로 수정, 저장
				if (new DirectoryInfo("Image").FullName == new DirectoryInfo(tbImagePath.Text).FullName)
					option.imageFolderPath = "Image";
				else
					option.imageFolderPath = tbImagePath.Text;
			}
			if(option.streamFilePath!=tbStreamFilePath.Text)
			{
				option.streamFilePath = tbStreamFilePath.Text;
			}
			//트윗 등록
			option.isYesnoTweet = (bool)checkSendTweet.IsChecked;
			option.isRetweetProtectUser=(bool)checkRetweetProtect.IsChecked;
			option.isSendEnter = (bool)checkSendEnter.IsChecked;

			//이미지 뷰어
			option.isShowImageTweet = (bool)checkShowTweet.IsChecked;
			option.isShowImageBottom = (bool)checkBottomPreview.IsChecked;
			option.isLoadOriginalImage = (bool)checkLoadOrg.IsChecked;

			//알림
			option.isMuteMention = (bool)checkMute.IsChecked;
			option.isNotiRetweet = (bool)checkNotiMention.IsChecked;
			option.isShowRetweet = (bool)checkNotiTL.IsChecked;
			option.isPlayNoti = (bool)checkPlayNoti.IsChecked;

			//UI
			option.isSmallUI = (bool)checkSmallUI.IsChecked;
			option.isShowPreview = (bool)checkShowPreview.IsChecked;
			DalsaeManager.DalsaeInstence.ChangeSmallUI(option.isSmallUI);
			option.isShowPropic = (bool)checkShowPropic.IsChecked;
			option.isBigPropic = (bool)checkShowBigPropic.IsChecked;

			//스트리밍
			option.isUseStreaming = (bool)cbUseStream.IsChecked;
			option.isAutoRunStreaming = (bool)cbAutoRunStream.IsChecked;
			option.streamFilePath = tbStreamFilePath.Text;
			option.streamPort = port;

			//시작 옵션
			option.isLoadBlock = (bool)checkLoadBlock.IsChecked;
			option.isLoadFollwing = (bool)checkLoadFollwing.IsChecked;

			if (comboSkin.SelectedItem != null)
			{
				if(option.skinName!=comboSkin.SelectedItem.ToString())
				{
					option.skinName = comboSkin.SelectedItem.ToString();
					FileInstence.UpdateSkin();
				}
			}
			if (comboNoti.SelectedItem != null)
			{
				if(option.notiSound!=comboNoti.SelectedItem.ToString())
				{
					option.notiSound = comboNoti.SelectedItem.ToString();
					DalsaeManager.DalsaeInstence.ChangeSoundNoti(option.notiSound);
				}
			}
			//뮤트, 하이라이트
			option.listHighlight.Clear();
			option.listHighlight.AddRange(arrHighlight);
			option.listMuteClient.Clear();
			option.listMuteClient.AddRange(arrMuteClient);
			option.listMuteUser.Clear();
			option.listMuteUser.AddRange(arrMuteUser);
			option.listMuteWord.Clear();
			option.listMuteWord.AddRange(arrMuteWord);

			option.dicMuteTweet.Clear();
			foreach (KeyValuePair<long, string> item in dicMuteTweet)
				option.dicMuteTweet.Add(item.Key, item.Value);

			FileInstence.UpdateOption(option);
		}

		private void checkPlayNoti_Chacked(object sender, RoutedEventArgs e)
		{
			textNoti.Visibility = Visibility.Visible;
			comboNoti.Visibility = Visibility.Visible;
			textNoti2.Visibility = Visibility.Visible;
		}

		private void checkPlayNoti_Unchecked(object sender, RoutedEventArgs e)
		{
			textNoti.Visibility = Visibility.Hidden;
			comboNoti.Visibility = Visibility.Hidden;
			textNoti2.Visibility = Visibility.Hidden;
		}

		private void buttonCancle_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void buttonOk_Click(object sender, RoutedEventArgs e)
		{
			Save();
			FileInstence.UpdateOption(DataInstence.option);
			Close();
		}

		private void buttonMute_Click(object sender, RoutedEventArgs e)
		{
			MuteWindow win = new WindowAndControl.MuteWindow();
			win.SetMutes(arrHighlight, arrMuteWord, arrMuteClient, arrMuteUser, dicMuteTweet);
			win.Owner = this;
			if(win.ShowDialog().Value)//뮤트 목록 갱신
			{
				arrHighlight = win.GetHighlight().ToArray();
				arrMuteClient = win.GetClient().ToArray();
				arrMuteUser = win.GetUser().ToArray();
				arrMuteWord = win.GetWord().ToArray();
				dicMuteTweet = win.GetTweet();
			}
		}

		private void window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				DialogResult = false;
		}


		private void buttonPath_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
			fbd.SelectedPath = Environment.CurrentDirectory;

			if (fbd.ShowDialog()== System.Windows.Forms.DialogResult.OK)
			{
				tbImagePath.Text = fbd.SelectedPath;
			}
		}

		private void checkShowPropic_Checked(object sender, RoutedEventArgs e)
		{
			checkShowBigPropic.Visibility = Visibility.Visible;
		}

		private void checkShowPropic_Unchecked(object sender, RoutedEventArgs e)
		{
			checkShowBigPropic.Visibility = Visibility.Hidden;
		}

		private void btnStreamPath_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Title = "열기";
			ofd.Filter = "싫애 파일(*.exe)|*.exe;";

			if(ofd.ShowDialog().Value)
				tbStreamFilePath.Text = ofd.FileName;
		}
	}
}
