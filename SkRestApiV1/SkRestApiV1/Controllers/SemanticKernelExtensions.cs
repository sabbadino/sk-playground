using Microsoft.SemanticKernel;

namespace SkRestApiV1.Controllers;

public static class SemanticKernelExtensions
{
    public static IKernelBuilder AddAddKeyedKernel(this IServiceCollection services, string kernelName)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register a KernelPluginCollection to be populated with any IKernelPlugins that have been
        // directly registered in DI. It's transient because the Kernel will store the collection
        // directly, and we don't want two Kernel instances to hold on to the same mutable collection.
        services.AddTransient<KernelPluginCollection>();

        // Register the Kernel as transient. It's mutable and expected to be mutated by consumers,
        // such as via adding event handlers, adding plugins, storing state in its Data collection, etc.
        services.AddKeyedTransient<Kernel>(kernelName);

        // Create and return a builder that can be used for adding services and plugins
        // to the IServiceCollection.
        // i cannot call new KernelBuilder(services) because the constructor is internal    
        // so in this case the service collection of semantic kernel and the one of the app will be different 
        // and the plugins registered in the app will not be available in the kernel
        // And I guess that if a plugin needs something in the constructor, and that something is required out
        // of the kernel it must be registered twice

        // just to be able to do kb.GetType(), typeof(KernelBuilder) is not allowed 
        var kb = Kernel.CreateBuilder();

        // this is an hack to call the internal constructor of KernelBuilder    
        var kernelBuilder  = Activator.CreateInstance(kb.GetType(), services) as IKernelBuilder;  
        ArgumentNullException.ThrowIfNull(kernelBuilder);
        return kernelBuilder;


    }

}