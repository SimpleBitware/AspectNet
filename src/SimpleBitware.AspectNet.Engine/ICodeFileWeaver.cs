namespace SimpleBitware.AspectNet.Engine;

public interface ICodeFileWeaver
{
    public string? Run(string fileExtension, string fileContent);
}
