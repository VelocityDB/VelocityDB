using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using VelocityDb.Indexing;

namespace VelocityDbSchema.IndexedTimeSeries {

	[UniqueConstraint]
	[OnePerDatabase]
	[Index("TargetDate,Symbol,Study,Analysis, DaysBack , CheckWindow ")]
	public class IndexedTimeArray : VelocityClass<IndexedTimeArray> {
		DateTime TargetDate;
		[Index]
		[IndexStringByHashCode]
		string Symbol;
		[Index]
		[IndexStringByHashCode]
		string Study;
		[Index]
		[IndexStringByHashCode]
		string Analysis;
		int DaysBack;
		int CheckWindow;
		double[] TikArray;

		public IndexedTimeArray(DateTime targetDate, string symbol, string study, string analysis, int daysback, int checkwindow, double[] tikArray)
			: base() {
			this.TargetDate = targetDate;
			this.Symbol = symbol;
			this.Study = study;
			this.Analysis = analysis;
			this.DaysBack = daysback;
			this.CheckWindow = checkwindow;
			this.TikArray = tikArray;
		}

		public IndexedTimeArray() : base() { }


		public static IEnumerable<IndexedTimeArray> FindBySymbol(string symbol, string study) {
      return from a in VelocityDBStatic.Session.OfType<IndexedTimeArray>() where a.Symbol == symbol select a;
		}
		public static bool GetVector_Test01(DateTime targetDate, string symbol, string study, string analysis, int daysback, int checkwindow, out double[] vector) {
			vector = new double[] { 0 };
      VelocityDBStatic.Session.BeginRead();
      UInt32 dbNum = VelocityDBStatic.Session.DatabaseNumberOf(typeof(IndexedTimeArray));
      Database db = VelocityDBStatic.Session.OpenDatabase(dbNum);
      var q = from v in VelocityDBStatic.Session.Index<IndexedTimeArray>(db)
					where v.TargetDate == targetDate
					&& v.Symbol == symbol
					&& v.Study == study
					&& v.Analysis == analysis
					&& v.DaysBack == daysback
					&& v.CheckWindow == checkwindow
					select v.TikArray;

			if (q.Count() == 0) {
				return false;
			}
			else {
				vector = (double[])q.ToList()[0];
				return true;
			}
		}

	}
}
