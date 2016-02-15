using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Prism.Mvvm;
using static System.Environment;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace ReactiveFolder.Models
{
	[DataContract]
	public abstract class ReactiveStreamBase : BindableBase, Util.IValidatable
	{

		/// <summary>
		/// このストリームを利用する親リアクションモデル
		/// ストリーム上のプロパティ変更を伝えるために利用します。
		/// </summary>
		public FolderReactionModel ParentReactionModel { get; private set; }



		private Util.ValidationResult _ValidateResult;
		public Util.ValidationResult ValidateResult
		{
			get
			{
				return _ValidateResult;
			}
			private set
			{
				base.SetProperty(ref _ValidateResult, value);
			}
		}

		private bool _IsUpdated;

		private bool _IsValid;
		public bool IsValid
		{
			get
			{
				return _IsValid;
			}
			private set
			{
				// Note: SetPropertyを上書きしてValidateチェックをある程度自動化しているが、
				// 上書きしたSetProperty内でIsValidをfalseに設定する都合上、
				// IsValid自体の設定は上書き前の素のSetPropertyを利用する。
				base.SetProperty(ref _IsValid, value);
			}
		}

		





		public ReactiveStreamBase()
		{
			
		}




		virtual public void Initialize(DirectoryInfo workDir)
		{
		}




		abstract protected Util.ValidationResult InnerValidate();

		public bool Validate()
		{
			ValidateResult = InnerValidate();

			_IsUpdated = false;

			IsValid = _ValidateResult.IsValid;

			return IsValid;
		}




		internal void SetParentReactionModel(FolderReactionModel reaction)
		{
			ParentReactionModel = reaction;

			Initialize(ParentReactionModel.WorkFolder);
		}

		internal void ClearParentReactionModel()
		{
			ParentReactionModel = null;

			// TODO: ReactiveStreamBaseからリアクションモデルに依存するパラメータを消去する
//			Initialize(null);
		}

		
		
		protected override bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
		{
			if (base.SetProperty<T>(ref storage, value, propertyName))
			{
				ValidatePropertyChanged();
				return true;
			}
			else
			{
				return false;
			}
		}


		protected void ValidatePropertyChanged()
		{
			IsValid = false;
			_IsUpdated = true;

			// リアクションモデルへ変更を伝える
			ParentReactionModel?.RaisePropertyChangedOnReactiveStream(this);
		}




		

		protected ReactiveStreamContext GenerateTempStreamContext()
		{
			var tempFolder = Path.GetTempPath();
			return new ReactiveStreamContext(new DirectoryInfo(tempFolder), "filename.png");
		}

	}


	abstract public class ReactiveStraightStreamBase : ReactiveStreamBase
	{
		abstract public void Execute(ReactiveStreamContext context);
	}


	abstract public class ReactiveBranchingStreamBase : ReactiveStreamBase
	{
		abstract public IEnumerable<ReactiveStreamContext> GenerateBranch(ReactiveStreamContext context);
	}




}
