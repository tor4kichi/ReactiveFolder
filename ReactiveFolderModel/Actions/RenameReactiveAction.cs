using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.Actions
{
	public class RenameReactiveAction : ReactiveActionBase
	{
		static Dictionary<string, Func<ReactiveStreamContext, string>> FormatMap;

		static RenameReactiveAction()
		{
			FormatMap = new Dictionary<string, Func<ReactiveStreamContext, string>>();

			#region DateTime

			FormatMap.Add("#{datetime}", (context) =>
			{
				var date = DateTime.Now;
				return String.Join("", new[] {
					date.Year,
					date.Month,
					date.Day,
					date.Hour,
					date.Second
				});
			});

			FormatMap.Add("#{date}", (context) =>
			{
				var date = DateTime.Now;
				return String.Join("", new[] {
					date.Year,
					date.Month,
					date.Day,
				});
			});
			FormatMap.Add("#{time}", (context) =>
			{
				var date = DateTime.Now;
				return String.Join("", new[] {
					date.Hour,
					date.Second
				});
			});

			#endregion

			#region _DateTime
			FormatMap.Add("#{_datetime}", (context) => 
			{
				var date = DateTime.Now;
				return String.Join("_", new[] {
					date.Year,
					date.Month,
					date.Day,
					date.Hour,
					date.Second
				});
			});

			FormatMap.Add("#{_date}", (context) =>
			{
				var date = DateTime.Now;
				return String.Join("_", new[] {
					date.Year,
					date.Month,
					date.Day,
				});
			});
			FormatMap.Add("#{_Time}", (context) =>
			{
				var date = DateTime.Now;
				return String.Join("_", new[] {
					date.Hour,
					date.Second
				});
			});

			#endregion

			#region -DateTime
			FormatMap.Add("#{-datetime}", (context) =>
			{
				var date = DateTime.Now;
				return String.Join("-", new[] {
					date.Year,
					date.Month,
					date.Day,
					date.Hour,
					date.Second
				});
			});

			FormatMap.Add("#{-date}", (context) =>
			{
				var date = DateTime.Now;
				return String.Join("-", new[] {
					date.Year,
					date.Month,
					date.Day,
				});
			});
			FormatMap.Add("#{-time}", (context) =>
			{
				var date = DateTime.Now;
				return String.Join("-", new[] {
					date.Hour,
					date.Second
				});
			});

			#endregion


			FormatMap.Add("#{name}", (context) => 
			{
				return context.Name;
			});

			FormatMap.Add("#{index}", (context) =>
			{
				// TODO: 
				return 0.ToString();
			});
		}






		public string NameWithFormat { get; set; }


		public RenameReactiveAction(string name = "#{name}")
		{
			NameWithFormat = name;
		}


		public override void Reaction(ReactiveStreamContext context)
		{
			// context.Name = String.Format();
			context.Name = FormatedString(context);
		}

		public override ValidationResult Validate()
		{
			var dummyContext = new ReactiveStreamContext(new DirectoryInfo(""), "/filename.ext");
			var formatString = FormatedString(dummyContext);

			var result = new ValidationResult();

			foreach (var invalidChar in Path.GetInvalidFileNameChars())
			{
				if (formatString.Contains(invalidChar))
				{
					result.AddMessage($"Rename string can not contain '{invalidChar}'");
				}
			}

			if (result.HasValidationError)
			{
				// FormatMapのKeysをresult.Messagesに書き出す
				var renamecanUse =  String.Join(",", FormatMap.Keys);
				result.AddMessage($"Rename can use tags:{renamecanUse}");
			}

			return result;
		}

		private string FormatedString(ReactiveStreamContext context)
		{
			// TODO: NameWithFormatに正規表現による置換処理を実行する
			// 元のファイル名をアレンジする機能を提供する
			// なお、C#6で追加された補完文字列では実行時の解釈ができないため
			// 動的な文字列解釈が可能な方法が必要となる。
			// 
			var nameWorking = NameWithFormat;
			foreach (var pair in FormatMap)
			{
				nameWorking = nameWorking.Replace(pair.Key, pair.Value(context));
			}

			return nameWorking;
		}

	}
}
