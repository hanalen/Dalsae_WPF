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
using static Dalsae.FileManager;
using static Dalsae.DalsaeManager;
using static Dalsae.TwitterWeb;
using static Dalsae.DataManager;

namespace Dalsae.WindowAndControl
{
	/// <summary>
	/// PinControl.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class PinWindow : Window
	{
		public PinWindow()
		{
			InitializeComponent();
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			int pin;
			if (int.TryParse(textBox.Text, out pin) && textBox.Text.Length == 7)
			{
				DalsaeInstence.InputPin(textBox.Text);
				Close();
			}
			else
			{
				MessageBox.Show(this, "웹 페이지에서 발급 받은.\r7자리 숫자를 제대로 입력 해주세요", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void pinWIndow_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				button_Click(null, null);
			}
			else if(e.Key== Key.Escape)
			{
				Close();
			}
		}

		private void Window_Activated(object sender, EventArgs e)
		{
			textBox.Focus();
		}
	}
}
