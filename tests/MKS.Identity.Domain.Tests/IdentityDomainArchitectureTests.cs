using System.Reflection;
using MKS.Identity.Domain;
using MKS.Shared.Domain.Apps.Entities;
using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.MultiTenancy.Entities;
using MKS.Shared.Domain.Shared.Attributes;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.Tests;

public sealed class IdentityDomainArchitectureTests
{
    [Fact]
    public void EveryIdentityEntityDeclaresAScope()
    {
        var entities = typeof(MKS.Identity.Domain.Users.Entities.UserEntity).Assembly
            .GetTypes()
            .Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                typeof(BaseEntity).IsAssignableFrom(type));

        Assert.NotEmpty(entities);
        Assert.All(entities, entity =>
        {
            Assert.EndsWith("Entity", entity.Name);
            Assert.NotNull(entity.GetCustomAttribute<EntityScopeAttribute>());
        });
    }

    [Fact]
    public void ScopeAttributesMatchRequiredEntityMarkers()
    {
        var entities = typeof(MKS.Identity.Domain.Users.Entities.UserEntity).Assembly
            .GetTypes()
            .Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                typeof(BaseEntity).IsAssignableFrom(type));

        foreach (var entity in entities)
        {
            var scope = entity.GetCustomAttribute<EntityScopeAttribute>()?.Scope;
            if (typeof(IHasTenantEntity).IsAssignableFrom(entity))
            {
                Assert.Equal(CEntityScope.Tenant, scope);
            }

            if (typeof(IHasAppEntity).IsAssignableFrom(entity))
            {
                Assert.Equal(CEntityScope.App, scope);
            }
        }
    }
}
