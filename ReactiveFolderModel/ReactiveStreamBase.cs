using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Prism.Mvvm;
using static System.Environment;

namespace ReactiveFolder.Model
{
	public abstract class ReactiveStreamBase : BindableBase
	{
		virtual public void Initialize(DirectoryInfo workDir)
		{
		}


		abstract public ValidationResult Validate();

		abstract public IObservable<ReactiveStreamContext> Chain(IObservable<ReactiveStreamContext> prev);




		protected ReactiveStreamContext GenerateTempStreamContext()
		{
			var tempFolder = Path.GetTempPath();
			return new ReactiveStreamContext(new DirectoryInfo(tempFolder), "filename.png");
		}

	}





	public class ValidationResult
	{
		public static ValidationResult Valid
		{
			get
			{
				return new ValidationResult();
			}
		}



		private List<string> _Messages;


		public ValidationResult(params string[] messages)
		{
			_Messages = messages.ToList();
		}


		public IEnumerable<string> Messages
		{
			get
			{
				return _Messages;
			}
		}

		public bool HasValidationError
		{
			get
			{
				return _Messages?.Count > 0;
			}
		}

		public void AddMessage(string message)
		{
			if (_Messages == null)
			{
				_Messages = new List<string>();
			}

			_Messages.Add(message);
		}

		public void AddMessages(params string[] messages)
		{
			foreach(var message in messages)
			{
				AddMessage(message);
			}
		}

		public void AddMessages(IEnumerable<string> messages)
		{
			foreach (var message in messages)
			{
				AddMessage(message);
			}
		}
	}
}
