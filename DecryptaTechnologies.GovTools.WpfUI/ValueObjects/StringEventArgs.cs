namespace DecryptaTechnologies.GovTools.WpfUI.ValueObjects;

public class StringEventArgs : EventArgs
{

    public string Value { get; set; }

    public StringEventArgs(string value)
    {
        Value = value;
    }

}
