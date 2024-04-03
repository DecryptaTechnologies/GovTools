using System.ComponentModel;

namespace DecryptaTechnologies.GovTools.Domain.Contracts;

public interface ICanReactToSelectionAsync
{

    Task ReactToSelectionAsync(INotifyPropertyChanged owner);

}
