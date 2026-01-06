using FluentAssertions;
using NetArchTest.Rules;

namespace BankChu.CoreBanking.Domain.Tests.Architecture;

public class DomainArchitectureTests
{
    [Fact]
    public void Domain_Should_Not_Depend_On_Other_Layers()
    {
        var domainAssembly = typeof(DependencyMarker).Assembly;

        var result = Types
            .InAssembly(domainAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "BankChu.CoreBanking.Application",
                "BankChu.CoreBanking.Infrastructure",
                "BankChu.CoreBanking.IoC",
                "BankChu.CoreBanking.Api")
            .GetResult();

        result.IsSuccessful
           .Should()
           .BeTrue(
               result.IsSuccessful
                   ? null
                   : $"Architecture violations found: {string.Join(", ", result.FailingTypeNames)}"
           );
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Frameworks()
    {
        var domainAssembly = typeof(DependencyMarker).Assembly;

        var result = Types
            .InAssembly(domainAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "System.Data",
                "Dapper")
            .GetResult();

        result.IsSuccessful
            .Should()
            .BeTrue(
                result.IsSuccessful
                   ? null
                   : $"Framework dependency violations found: {string.Join(", ", result.FailingTypeNames)}"
            );
    }
}
