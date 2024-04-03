using DecryptaTechnologies.GovTools.Domain.Contracts;

namespace DecryptaTechnologies.GovTools.WpfUI.ValueObjects;

public class NavigationMessage
{

    public IScreen Screen { get; }

    public NavigationMessage(IScreen screen)
    {
        Screen = screen;
    }

}
