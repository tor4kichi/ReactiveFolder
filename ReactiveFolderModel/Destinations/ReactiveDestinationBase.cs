using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using ReactiveFolder.Model.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Model.Destinations
{
	// * * * * * * * * * * * * 

		
	// TODO: Initializeで渡されるworkDirが無効な場合に対応する


	// * * * * * * * * * * * * 


	public abstract class ReactiveDestinationBase : ReactiveStreamBase, IStreamContextFinalizer
	{
		static Dictionary<string, Func<ReactiveStreamContext, string>> FormatMap;


		public class RenameGlobalObject
		{
			internal ReactiveStreamContext __Context { get; set; }


			public RenameGlobalObject()
			{

			}

			public string datetime
			{
				get
				{
					var date = DateTime.Now;
					return String.Join("", new[] {
						date.Year,
						date.Month,
						date.Day,
						date.Hour,
						date.Second
					});
				}
			}

			public string date
			{
				get
				{
					var date = DateTime.Now;
					return String.Join("", new[] {
						date.Year,
						date.Month,
						date.Day,
					});
				}
			}

			public string time
			{
				get
				{
					var date = DateTime.Now;
						return String.Join("", new[] {
						date.Hour,
						date.Second
					});
				}
			}

			public string name
			{
				get
				{
					return __Context.Name;
				}
			}



		}
	

		public string OutputName { get; private set; }

		private string _OutputNamePattern;
		public string OutputNamePattern
		{
			get
			{
				return _OutputNamePattern;
			}
			set
			{
				if (SetProperty(ref _OutputNamePattern, value))
				{
					var tolower = _OutputNamePattern;

					_RenameScriptCode = $"return $\"{_OutputNamePattern}\";";
				}
			}
		}


		private string _RenameScriptCode;
		

		public ReactiveDestinationBase(string outputNamePattern = "{name}")
		{
			OutputNamePattern = outputNamePattern;
		}



		abstract public string GetDistinationFolderPath();
		public DirectoryInfo GetDestinationFolder()
		{
			var path = GetDistinationFolderPath();

			// check
			if (path == null)
			{
				return null;
			}

			if (String.IsNullOrEmpty(path))
			{
				return null;
			}

			if (false == Path.IsPathRooted(path))
			{
				return null;
			}

			return new DirectoryInfo(path);
		}

		override public void Initialize(DirectoryInfo workDir)
		{
			var destFolder = GetDestinationFolder();

			if(destFolder == null)
			{
				return;
			}

			if (false == destFolder.Exists)
			{
				// Note: 勝手につくちゃっていいの？
				destFolder.Create();
			}

			base.Initialize(workDir);
		}


		protected override ValidationResult InnerValidate()
		{
			var result = new ValidationResult();

			var destFolder = GetDestinationFolder();
			if (destFolder == null)
			{
				result.AddMessage("出力先のフォルダの指定に問題があります。");
			}

			if (destFolder != null && false == destFolder.Exists)
			{
				result.AddMessage("出力先のフォルダが存在しません。");
			}


			try
			{
				var renamed = TestRename();

				if (renamed == null)
				{
					result.AddMessage("出力名の評価に失敗しました。with no exception.");
				}
			}
			catch(Exception e)
			{
				result.AddMessage("出力名の評価に失敗しました。");
				result.AddMessage(e.Message);
			}
			

			return result;
		}

		public string TestRename()
		{
			var dummyContext = GenerateTempStreamContext();
			
			var task = FormatedString(dummyContext);

			task.Wait();

			if (task.IsCompleted)
			{
				var text = task.Result;
				return text;
			}
			else
			{
				return null;
			}
		}


		override public IObservable<ReactiveStreamContext> Chain(IObservable<ReactiveStreamContext> prev)
		{
			return prev
				.Do(async context => OutputName = await FormatedString(context))
				.Do(context => context.Finalize(this));
		}


		static RenameGlobalObject globalObject = new RenameGlobalObject();

		private async Task<string> FormatedString(ReactiveStreamContext context)
		{
			// scriptingによる名前解決を行う
			var script = CSharpScript.Create<string>(_RenameScriptCode,
				globalsType: typeof(RenameGlobalObject));


			globalObject.__Context = context;

			
			var messages = script.Compile();
			
			var state = await script.RunAsync(globalObject);			

			return state.ReturnValue;
		}
	}
}
