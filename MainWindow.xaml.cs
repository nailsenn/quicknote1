using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using static QN_Rewrite.utils.Defins;
using static QN_Rewrite.utils.Format;
using static QN_Rewrite.utils.KeyEvents;

using LibVLCSharp.Shared;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using LibVLCSharp.WPF;
namespace QN_Rewrite;


public partial class MainWindow : Window
{
	private delegate void PlayerArgs(Uri uri, long jump);
	private static event PlayerArgs? CreateNewPlayer;
	private MediaPlayer? player;

	private DispatcherTimer? timeui;
	private double DeltaWidth = 0;

	internal static List<StoredInfo> MarkersList = new();


	public MainWindow()
	{
		var libvlc = new LibVLC();
		InitializeComponent();

		CreateNewPlayer += async (Uri uri, long jump) =>
		{
			string ext = Path.GetExtension(uri.AbsolutePath);
			string edl = uri.AbsolutePath.Replace("%20", " ").Replace(ext, ".edl");
			var media = new Media(libvlc, uri);
			await media.Parse(MediaParseOptions.ParseLocal);


			if (File.Exists(edl))
				MarkersList = ExistingEdl(edl, media.Duration, Markers);

			if (jump == 0 && MarkersList.Count > 0)
				jump = MarkersList[MarkersList.Count - 1].end;

			player = new MediaPlayer(media);
			player.Volume = 0;
			player.SetRate(8);
			player.Play();
			player.Time = jump;
			VideoView.MediaPlayer = player;

			timeui = new DispatcherTimer();
			timeui.Interval = TimeSpan.FromMilliseconds(100);
			timeui.Tick += UpdateDisplays;
			timeui.Start();

			KeyDown += Pressed;
			KeyUp += Released;


			player.Stopped += (s, e) =>
			{
				timeui.Tick -= UpdateDisplays;
				KeyDown -= Pressed;
				KeyUp -= Released;
				WriteEdl(edl, MarkersList);

				Dispatcher.BeginInvoke(() =>
				{
					player.Dispose();
					libvlc.Dispose();
					Environment.Exit(0);
				});
			};
		};
	}


	private void Button_Click(object sender, RoutedEventArgs e)
	{
		string? path = (InputStack.Children[0] as TextBox)?.Text.Replace("\"", "");
		string? timecode = (InputStack.Children[1] as TextBox)?.Text;

		if (path != null && Path.Exists(path))
		{
			long converted = 0;
			short exp = 2;

			if (timecode != null && timecode.Length == 8)
			{
				string[] split = timecode.Split(":");
				foreach (string v in split)
				{
					if (int.TryParse(v, out int n))
					{
						converted += n * (int)Math.Pow(60, exp);
						exp -= 1;
					}
					else
					{
						converted = 0;
						break;
					}
				}
			}

			VideoView.Visibility = Visibility.Visible;
			InputStack.Visibility = Visibility.Hidden;
			CreateNewPlayer?.Invoke(new Uri(path), converted * 1000);
		}
	}


	private void UpdateDisplays(object? sender, EventArgs args)
	{
		if (player != null && player.IsPlaying && player.Time < player.Media?.Duration)
		{
			TextA.Text = $"x{player.Rate.ToString()}";
			TextB.Text = ToClock(player.Time);
			TextC.Text = MarkersList.Count.ToString();
			ProgressBar.Value = player.Position * 100;


			if (DeltaWidth == this.Width) return;
			DeltaWidth = this.Width;

			int count = Markers.Children.Count;
			if (count < 1) return;

			foreach (StoredInfo v in MarkersList)
			{
				if (v.end > 0)
				{
					Canvas.SetLeft(v.rect, this.Width * v.startPos);
					v.rect.Width = Markers.ActualWidth * (v.endPos - v.startPos);
					v.rect.Height = ProgressBar.ActualHeight;
				}
				else
					v.rect.Width = 0;
			}
		}
	}


	internal void Released(object? sender, KeyEventArgs e)
	{
		if (player == null) return;
		KeyReleased(e, MarkersList, player);
		DeltaWidth = 0;
	}

	internal void Pressed(object? sender, KeyEventArgs e)
	{
		if (player == null) return;
		KeyPressed(e, MarkersList, player, TextC, Markers);
	}
}
