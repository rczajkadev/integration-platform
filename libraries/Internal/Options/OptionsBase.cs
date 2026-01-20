using Microsoft.Extensions.Configuration;

namespace Integrations.Options;

public abstract class OptionsBase(IConfiguration configuration)
{
    protected IConfiguration Configuration { get; } = configuration;
}
