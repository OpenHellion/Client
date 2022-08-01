using System;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.Network;

namespace ZeroGravity.Objects
{
	public class LogItem : Item
	{
		public int LogID;

		private string text;

		public GameObject Canvas;

		public Text MainLogText;

		public Text HeadingText;

		public override void ChangeEquip(EquipType type, Player pl)
		{
			Canvas.SetActive(type == EquipType.Hands);
		}

		public override void ProcesStatsData(DynamicObjectStats dos)
		{
			base.ProcesStatsData(dos);
			LogItemStats logItemStats = dos as LogItemStats;
			LogID = logItemStats.LogID;
			LogItemTypes logID = (LogItemTypes)LogID;
			string path = "UI/TextsForLogs/" + logID;
			MainLogText.text = (Resources.Load(path) as TextAsset).text.Replace("<br>", Environment.NewLine);
			Text headingText = HeadingText;
			LogItemTypes logID2 = (LogItemTypes)LogID;
			headingText.text = logID2.ToString().Replace('_', ' ');
			ToolTip = HeadingText.text;
			Name = HeadingText.text;
		}

		public override DynamicObjectAuxData GetAuxData()
		{
			LogItemData baseAuxData = GetBaseAuxData<LogItemData>();
			baseAuxData.logID = LogID;
			return baseAuxData;
		}
	}
}
