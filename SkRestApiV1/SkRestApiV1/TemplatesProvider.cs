namespace SkRestApiV1;

public interface ITemplatesProvider
{
    Task<string> GetSystemMessage(string name);
}
public class TemplatesProvider : ITemplatesProvider, ISingletonScope
{
    public async Task <string> GetSystemMessage(string name)
    {
        var systemMessage = await File.ReadAllTextAsync($"Templates/systemMessage-{name}.txt");
        return systemMessage;   
    }
}