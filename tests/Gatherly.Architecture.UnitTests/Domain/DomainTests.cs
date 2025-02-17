using FluentAssertions;
using Gatherly.Domain.Primitives;
using NetArchTest.Rules;
using System.Reflection;

namespace Gatherly.Architecture.UnitTests.Domain;

public class DomainTests
{
    private static readonly Assembly DomainAssembly = typeof(Entity).Assembly;

    [Fact]
    public void DomainEvents_Should_BeSealed()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEvent))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
