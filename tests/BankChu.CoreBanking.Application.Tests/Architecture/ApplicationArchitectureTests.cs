using FluentAssertions;
using NetArchTest.Rules;

namespace BankChu.CoreBanking.Application.Tests.Architecture;

public class ApplicationArchitectureTests
{
    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Api()
    {
        // Arrange
        var applicationAssembly = typeof(DependencyMarker).Assembly;

        // Act
        var result = Types
            .InAssembly(applicationAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "BankChu.CoreBanking.Infrastructure",
                "BankChu.CoreBanking.Api",
                "BankChu.CoreBanking.IoC")
            .GetResult();

        // Assert
        result.IsSuccessful
            .Should()
            .BeTrue(
                result.IsSuccessful
                    ? null
                    : $"Architecture violations found: {string.Join(", ", result.FailingTypeNames)}"
            );
    }
}