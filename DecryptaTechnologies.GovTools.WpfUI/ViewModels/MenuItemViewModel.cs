using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using rskibbe.I18n.Contracts;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class MenuItemViewModel : ObservableObject
{

    protected ITranslator _translator;

    [ObservableProperty]
    private string? _translationKey;

    public string Text
    {
        get
        {
            if (string.IsNullOrEmpty(TranslationKey))
                return "";
            return _translator.Translate(TranslationKey).Value;
        }
    }

    public MenuItemViewModel(
        ITranslator translator,
        string translationKey
    )
    {
        _translator = translator;
        _translationKey = translationKey;
    }

    [RelayCommand(CanExecute = nameof(CanClick))]
    public virtual Task Click()
    {
        return Task.CompletedTask;
    }

    public virtual bool CanClick()
    {
        return true;
    }

}

public partial class NavigationItemViewModel : MenuItemViewModel
{

    Func<NavigationItemViewModel, Task> _getExecuteFn;

    Func<bool>? _canExecute;

    [ObservableProperty]
    bool _isActive;

    public NavigationItemViewModel(
        Func<NavigationItemViewModel, Task> getExecuteFn,        
        ITranslator translator,
        string translationKey,
        Func<bool>? canExecute = null
    ) : base(translator, translationKey)
    {
        _getExecuteFn = getExecuteFn;
        _canExecute = canExecute;
    }

    public override Task Click()
    {
        return _getExecuteFn(this);
    }

    public override bool CanClick()
    {
        return _canExecute == null || _canExecute.Invoke();
    }

}

public partial class FunctionItemViewModel : MenuItemViewModel
{

    Func<Task> _getExecuteFn;

    Func<bool>? _canExecute;

    public FunctionItemViewModel(
        Func<Task> getExecuteFn,
        ITranslator translator,
        string translationKey,
        Func<bool>? canExecute = null
    ) : base(translator, translationKey)
    {
        _getExecuteFn = getExecuteFn;
        _canExecute = canExecute;
    }

    public override Task Click()
    {
        return _getExecuteFn();
    }

    public override bool CanClick()
    {
        return _canExecute == null || _canExecute.Invoke();
    }

}
