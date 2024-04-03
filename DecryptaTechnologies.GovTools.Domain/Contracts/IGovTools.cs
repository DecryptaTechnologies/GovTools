using System.Diagnostics;
using System.Windows;

namespace DecryptaTechnologies.GovTools.Domain.Contracts;

public interface IGovTools
{

    string GetHashoutDirectory();


    string GetWordlistDirectory();

    void ShowLegalMessageIfNotDebugging();

}
