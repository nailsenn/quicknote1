using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using static QN_Rewrite.utils.Defins;


namespace QN_Rewrite.utils;

internal class Format
{
	public static string ToClock(long time)
	{
		TimeSpan ts = TimeSpan.FromMilliseconds(time);
		return string.Format($"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}");
	}


	public static void WriteEdl(string path, List<StoredInfo> list)
	{
		// await media.Parse(MediaParseOptions.ParseLocal);
		// VideoTrack videoData = media.Tracks[0].Data.Video;
		// uint fps = videoData.FrameRateNum / videoData.FrameRateNum;
		File.WriteAllText(path, "TITLE: cs_inputs\nFCM: NON-DROP FRAME\n\n");

		for (int i = 0; i < list.Count; i++)
		{
			StoredInfo info = list[i];
			KeyInfo desc = KeyAssignedInfo[info.index];
			string repeat = String.Concat(Enumerable.Repeat(ToClock(info.start) + ":00 ", 4));
			string frames_dur = ((info.end - info.start) / 1000 * 30).ToString();

			string line1 = $"{(i + 1).ToString("000")} 001 V C {repeat}\n";
			string line2 = $"|C:ResolveColor{desc.color} |M:{desc.name} |D:{frames_dur}\n\n";
			File.AppendAllText(path, line1 + line2);
		}
	}



	public static List<StoredInfo> ExistingEdl(string edl, long length, Canvas canvas)
	{
		Regex tcode = new Regex(@"\b\d{2}:\d{2}:\d{2}:\d{2}\b");
		Regex minfo = new Regex(@"\|M:(.*?) \|D:(\d+)");
		List<StoredInfo> collection = [];
		long prev_tcode = 0;


		foreach (string line in File.ReadLines(edl))
		{
			if (prev_tcode == 0)
			{
				Match t_match = tcode.Match(line);
				if (t_match.Success)
				{
					string[] entries = t_match.Value.Split(":", 4);
					prev_tcode = (
						(Int64.Parse(entries[0]) * 1000 * 60 * 60) +
						(Int64.Parse(entries[1]) * 1000 * 60) +
						(Int64.Parse(entries[2]) * 1000)
					);
					continue;
				}
			}

			else
			{
				Match i_match = minfo.Match(line);
				if (!i_match.Success) continue;

				long duration = (Int64.Parse(i_match.Groups[2].Value) * 1000) / 30;
				long _endPoint = prev_tcode + duration;
				foreach (KeyValuePair<Key, KeyInfo> entry in KeyAssignedInfo)
				{
					if (entry.Value.name == i_match.Groups[1].Value)
					{
						collection.Add(
							NewMarker(entry.Key, canvas,
							(float)prev_tcode / length, prev_tcode,
							(float)_endPoint / length, _endPoint)
						);

						prev_tcode = 0;
					}
					else continue;
				}
			}
		}
		return collection;
	}
}