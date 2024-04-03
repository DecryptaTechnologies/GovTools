namespace DecryptaTechnologies.GovTools.Domain.Contracts;

/// <summary>
/// An interface denoting something which is hosting an <see cref="IScreen"/> and is an <see cref="IScreen"/> itself
/// </summary>
public interface IHostScreen : IScreen
{

    IScreen? CurrentScreen { get; set; }

    Task SwitchScreenAsync<T>() where T : IScreen;

    Task SwitchScreenAsync(IScreen screen);

    Task BeforeActivateChildAsync(IScreen screen);

}
