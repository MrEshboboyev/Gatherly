using System.Reflection;

namespace Gatherly.ArchitectureTests;

public abstract class BaseTest
{
    protected static readonly Assembly DomainAssembly = typeof(Gatherly.Domain.AssemblyReference).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(Gatherly.Application.AssemblyReference).Assembly;
    protected static readonly Assembly PersistenceAssembly = typeof(Persistence.AssemblyReference).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.AssemblyReference).Assembly;
    protected static readonly Assembly PresentationAssembly = typeof(Presentation.AssemblyReference).Assembly;
}
