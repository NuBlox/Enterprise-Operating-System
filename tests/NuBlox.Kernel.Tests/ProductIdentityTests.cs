using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NuBlox.Kernel.Tests;

[TestClass]
public sealed class ProductIdentityTests
{
    [TestMethod]
    public void ProductionKernelUsesExpectedAssemblyIdentity()
    {
        string? assemblyName = typeof(ProductIdentity).Assembly.GetName().Name;

        Assert.AreEqual("NuBlox.Kernel", assemblyName);
    }

    [TestMethod]
    public void ProductionKernelDoesNotReferenceSpikeAssemblies()
    {
        AssemblyName[] references = typeof(ProductIdentity).Assembly.GetReferencedAssemblies();

        Assert.IsFalse(
            references.Any(reference =>
                reference.Name?.Contains("FoundationSpike", StringComparison.OrdinalIgnoreCase) == true),
            "Production kernel must not depend on disposable architecture-spike assemblies.");
    }
}
