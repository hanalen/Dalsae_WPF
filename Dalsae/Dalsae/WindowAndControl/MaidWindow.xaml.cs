using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

namespace Dalsae.WindowAndControl
{
	/// <summary>
	/// MaidWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MaidWindow : Window
	{
		public MaidWindow(Window owner)
		{
			this.Owner = owner;
			ShowInTaskbar = false;
			InitializeComponent();

			Assembly assembly = Assembly.GetExecutingAssembly();
			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
			labelVersion.Text = $"버전 {fvi.FileVersion}v";
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				Close();
		}

		private void hyperLink_Click(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			Process.Start(e.Uri.AbsoluteUri);
			e.Handled = true;
		}
	}
}
