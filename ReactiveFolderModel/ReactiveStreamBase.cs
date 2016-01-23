using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ReactiveFolder.Model
{
	public abstract class ReactiveStreamBase
	{
		public virtual void Initialize(DirectoryInfo workDir)
		{
		}

		public abstract ValidationResult Validate();

		public abstract IObservable<ReactiveStreamContext> Chain(IObservable<ReactiveStreamContext> prev);
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
