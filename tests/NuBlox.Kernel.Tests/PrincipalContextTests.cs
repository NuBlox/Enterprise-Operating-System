using System.Security.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuBlox.Kernel.Identity;

namespace NuBlox.Kernel.Tests;

[TestClass]
public sealed class PrincipalContextTests
{
    [TestMethod]
    public void AuthenticatedNormalizedClaimsResolveToPrincipalContext()
    {
        Guid principalId = Guid.NewGuid();
        ClaimsPrincipal claimsPrincipal = CreateClaimsPrincipal(
            principalId,
            PrincipalKind.Human,
            "https://identity.example.test",
            "external-subject-123");

        bool resolved = PrincipalContextResolver.TryResolve(claimsPrincipal, out PrincipalContext? principal);

        Assert.IsTrue(resolved);
        Assert.IsNotNull(principal);
        Assert.AreEqual(principalId, principal.PrincipalId.Value);
        Assert.AreEqual(PrincipalKind.Human, principal.Kind);
        Assert.AreEqual("https://identity.example.test", principal.Issuer);
        Assert.AreEqual("external-subject-123", principal.Subject);
    }

    [TestMethod]
    public void UnauthenticatedClaimsCannotCreatePrincipalContext()
    {
        ClaimsIdentity identity = new(new[]
        {
            new Claim(NuBloxClaimTypes.PrincipalId, Guid.NewGuid().ToString("D")),
            new Claim(NuBloxClaimTypes.PrincipalKind, PrincipalKind.Human.ToString()),
            new Claim(NuBloxClaimTypes.Issuer, "https://identity.example.test"),
            new Claim(NuBloxClaimTypes.Subject, "external-subject-123")
        });

        bool resolved = PrincipalContextResolver.TryResolve(new ClaimsPrincipal(identity), out PrincipalContext? principal);

        Assert.IsFalse(resolved);
        Assert.IsNull(principal);
    }

    [TestMethod]
    public void TenantContextUsesOnlyAlreadyVerifiedTenantIdentity()
    {
        ClaimsPrincipal claimsPrincipal = CreateClaimsPrincipal(
            Guid.NewGuid(),
            PrincipalKind.Service,
            "https://workload.example.test",
            "service-processor");

        Assert.IsTrue(PrincipalContextResolver.TryResolve(claimsPrincipal, out PrincipalContext? principal));
        Assert.IsNotNull(principal);

        TenantId verifiedTenantId = new(Guid.NewGuid());
        RequestContext context = PrincipalContextResolver.CreateForVerifiedTenant(principal, verifiedTenantId);

        Assert.AreEqual(PrincipalKind.Service, context.Principal.Kind);
        Assert.AreEqual(verifiedTenantId, context.TenantId);
    }

    [TestMethod]
    public void MissingNormalizedPrincipalIdIsRejected()
    {
        ClaimsIdentity identity = new(new[]
        {
            new Claim(NuBloxClaimTypes.PrincipalKind, PrincipalKind.Human.ToString()),
            new Claim(NuBloxClaimTypes.Issuer, "https://identity.example.test"),
            new Claim(NuBloxClaimTypes.Subject, "external-subject-123")
        }, authenticationType: "test");

        bool resolved = PrincipalContextResolver.TryResolve(new ClaimsPrincipal(identity), out PrincipalContext? principal);

        Assert.IsFalse(resolved);
        Assert.IsNull(principal);
    }

    private static ClaimsPrincipal CreateClaimsPrincipal(
        Guid principalId,
        PrincipalKind principalKind,
        string issuer,
        string subject)
    {
        ClaimsIdentity identity = new(new[]
        {
            new Claim(NuBloxClaimTypes.PrincipalId, principalId.ToString("D")),
            new Claim(NuBloxClaimTypes.PrincipalKind, principalKind.ToString()),
            new Claim(NuBloxClaimTypes.Issuer, issuer),
            new Claim(NuBloxClaimTypes.Subject, subject)
        }, authenticationType: "test");

        return new ClaimsPrincipal(identity);
    }
}
