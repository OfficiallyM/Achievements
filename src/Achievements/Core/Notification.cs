using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Achievements.Core
{
	internal class Notification
	{
		public AchievementData Achievement { get; set; }
		public int? OldProgress { get; set; }
		public int? NewProgress { get; set; }
		public float? StartDisplayTime { get; set; }
	}
}
