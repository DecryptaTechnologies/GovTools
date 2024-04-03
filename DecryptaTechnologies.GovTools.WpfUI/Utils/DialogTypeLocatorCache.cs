namespace DecryptaTechnologies.GovTools.WpfUI.Utils;

internal class DialogTypeLocatorCache
{

    private readonly Dictionary<Type, Type> cache;

    //
    // Summary:
    //     Gets the number of dialog type/view model type pairs contained in the cache.
    internal int Count => cache.Count;

    //
    // Summary:
    //     Initializes a new instance of the MvvmDialogs.DialogTypeLocators.DialogTypeLocatorCache
    //     class.
    internal DialogTypeLocatorCache()
    {
        cache = new Dictionary<Type, Type>();
    }

    //
    // Summary:
    //     Adds the specified view model type with its corresponding dialog type.
    //
    // Parameters:
    //   viewModelType:
    //     Type of the view model.
    //
    //   dialogType:
    //     Type of the dialog.
    internal void Add(Type viewModelType, Type dialogType)
    {
        if (viewModelType == null)
        {
            throw new ArgumentNullException("viewModelType");
        }

        if (dialogType == null)
        {
            throw new ArgumentNullException("dialogType");
        }

        if (cache.ContainsKey(viewModelType))
        {
            throw new ArgumentException($"View model of type '{viewModelType}' is already added.", "viewModelType");
        }

        cache.Add(viewModelType, dialogType);
    }

    //
    // Summary:
    //     Gets the dialog type for specified view model type.
    //
    // Parameters:
    //   viewModelType:
    //     Type of the view model.
    //
    // Returns:
    //     The dialog type if found; otherwise null.
    internal Type? Get(Type viewModelType)
    {
        if (viewModelType == null)
        {
            throw new ArgumentNullException("viewModelType");
        }

        cache.TryGetValue(viewModelType, out Type? value);
        return value;
    }

    //
    // Summary:
    //     Removes all view model types with its corresponding dialog types.
    internal void Clear()
    {
        cache.Clear();
    }
}
