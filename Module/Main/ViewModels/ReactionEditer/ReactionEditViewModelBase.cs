using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using ReactiveFolder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Main.ViewModels.ReactionEditer
{
	abstract public class ReactionEditViewModelBase : BindableBase
	{
		public FolderReactionModel Reaction { get; private set; }

		public ReactiveProperty<bool> IsValid { get; private set; }



		public ReactionEditViewModelBase(FolderReactionModel reactionModel)
		{
			Reaction = reactionModel;

			IsValid = new ReactiveProperty<bool>();

			// モデルへの問い合わせだけでVMの構築を必要としない操作のため
			// 例外的にbaseコンストラクタ内から子クラスへの抽象メソッドを呼び出す
			CheckValidation();
		}

		public void CheckValidation()
		{
			IsValid.Value = IsValidateModel();
		}

		abstract protected bool IsValidateModel();



	}
}
