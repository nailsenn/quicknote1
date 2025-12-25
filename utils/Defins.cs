using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


namespace QN_Rewrite.utils;

internal class Defins
{
	public static readonly Dictionary<Key, KeyInfo> KeyAssignedInfo = new()
		{
			{ Key.Z, new KeyInfo
			{ name = "na", color = "Cocoa", colorb = Brushes.LightGray }
			},
			{ Key.A, new KeyInfo
			{ name = "dt", color = "Blue", colorb = Brushes.DeepSkyBlue }
			},
			{ Key.S, new KeyInfo
			{ name = "tf", color = "Red", colorb = Brushes.Red}
			},
			{ Key.D, new KeyInfo
			{ name = "as", color = "Yellow", colorb = Brushes.Yellow}
			},
			{ Key.Q, new KeyInfo
			{ name = "rd", color = "Green", colorb = Brushes.ForestGreen }
			},
			{ Key.W, new KeyInfo
			{ name = "ps", color = "Rose", colorb = Brushes.Pink }
			},
			{ Key.E, new KeyInfo
			{ name = "st", color = "Sky", colorb = Brushes.LightBlue }
			},
			{ Key.End, new KeyInfo
			{ name = "action", color = "Blue", colorb = Brushes.Gold }
			}
		};


	public static StoredInfo NewMarker(Key key, Canvas canvas,
		float _startPos, long _start, float _endPos = 0, long _end = 0)
	{
		var ui = new Rectangle
		{
			Opacity = .4,
			Fill = KeyAssignedInfo[key].colorb,
			VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
			HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
		};

		canvas.Children.Add(ui);

		return new StoredInfo
		{
			rect = ui,
			index = key,
			startPos = _startPos,
			endPos = _endPos,
			start = _start,
			end = _end,
		};
	}


	public struct StoredInfo
	{
		public Rectangle rect;
		public Key index;
		public double startPos;
		public double endPos;
		public long start;
		public long end;
	}


	public struct KeyInfo
	{
		public string name;
		public string color;
		public SolidColorBrush colorb;
	}
}