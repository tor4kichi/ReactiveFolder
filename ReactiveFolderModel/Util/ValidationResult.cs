using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveFolder.Models.Util
{
	public class ValidationResult
	{
		public static ValidationResult ValidResult
		{
			get
			{
				return new ValidationResult();
			}
		}



		private List<string> _Messages;


		public ValidationResult(params string[] messages)
		{
			_Messages = messages?.ToList();
		}


		public IEnumerable<string> Messages
		{
			get
			{
				return _Messages;
			}
		}

		public bool IsValid
		{
			get
			{
				if (_Messages == null)
				{
					return true;
				}
				else
				{
					return _Messages.Count == 0;
				}
			}
		}

		public bool HasError
		{
			get
			{
				return !this.IsValid;
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
			foreach (var message in messages)
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
