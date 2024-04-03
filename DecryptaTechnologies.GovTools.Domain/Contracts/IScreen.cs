namespace DecryptaTechnologies.GovTools.Domain.Contracts;

/// <summary>
/// An interface denoting a typical UI "Screen" which can be activated and deactivated
/// </summary>
public interface IScreen
{

    Task BeforeActivateAsync();

    Task ActivateAsync();

    Task DeactivateAsync();

}
