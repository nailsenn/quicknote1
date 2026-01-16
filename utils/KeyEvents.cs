using LibVLCSharp.Shared;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using static QN_Rewrite.utils.Defins;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;


namespace QN_Rewrite.utils;

internal class KeyEvents
{
	public static void KeyReleased(KeyEventArgs kevent, List<StoredInfo> list, MediaPlayer player)
	{
		int i = list.FindLastIndex(item => item.index == kevent.Key && item.end == 0);
		if (i == -1) return;

		StoredInfo copy = list[i];

		if (copy.start <= copy.end)
		{
			copy.endPos = copy.startPos;
			copy.end = copy.start;
		}
		else
		{
			copy.endPos = player.Position;
			copy.end = player.Time;
		}

		list[i] = copy;
	}


	public static void KeyPressed(
	KeyEventArgs kevent, List<StoredInfo> list, MediaPlayer player, TextBlock textC, Canvas markers)
	{
		if (player.Media?.State == VLCState.Error) return;

		if (kevent.Key == Key.Escape || kevent.Key == Key.RightCtrl)
			player.Stop();

		else if (kevent.Key == Key.LeftCtrl || kevent.Key == Key.Down)
			player.SetRate(Math.Max(1, player.Rate * 0.5f));

		else if (kevent.Key == Key.LeftShift || kevent.Key == Key.Up)
			player.SetRate(Math.Min(64, player.Rate * 2f));

		else if (kevent.Key == Key.Tab || kevent.Key == Key.Left)
			player.Time -= (long)(Math.Log2(player.Rate) * 10000) + 5000;

		else if (kevent.Key == Key.Space || kevent.Key == Key.Right)
			player.Time += (long)(Math.Log2(player.Rate) * 10000) + 5000;

		else if (kevent.Key == Key.System || kevent.Key == Key.Enter)
		{
			if (player.Media?.State == VLCState.Paused)
				player.Play();

			else if (player.Media?.State == VLCState.Playing)
				player.Pause();
		}
		else if ((kevent.Key == Key.X || kevent.Key == Key.RightShift) && list.Count > 0 && !kevent.IsRepeat)
		{
			var last = list.Last();
			markers.Children.Remove(last.rect);
			list.Remove(last);
			textC.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#ff9191")!;
		}
		else if (KeyAssignedInfo.ContainsKey(kevent.Key) && !kevent.IsRepeat)
		{
			int count = list.Count;

			if (count > 0)
			{
				StoredInfo prev = list[count - 1];
				long dif = player.Time - prev.end;
				if (dif <= 6000 && dif >= 0)
				{
					if (prev.rect.Fill == Brushes.LimeGreen)
						prev.rect.Fill = Brushes.Red;
					else
						prev.rect.Fill = Brushes.LimeGreen;

					prev.end = 0;
					prev.endPos = 0;
					prev.index = kevent.Key;
					list[count - 1] = prev;
					return;
				}
			}

			list.Add(NewMarker(kevent.Key, markers, player.Position, player.Time));
			textC.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#b0ffbd")!;
		}
	}
}
