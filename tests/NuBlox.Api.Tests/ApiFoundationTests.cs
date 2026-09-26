using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NuBlox.Api.Tests;

[TestClass]
public sealed class ApiFoundationTests
{
    [TestMethod]
    public async Task LivenessAndReadinessAreHealthy()
    {
        await using WebApplicationFactory<Program> factory = new();
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage live = await client.GetAsync("/health/live");
        HttpResponseMessage ready = await client.GetAsync("/health/ready");

        Assert.AreEqual(HttpStatusCode.OK, live.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, ready.StatusCode);
    }

    [TestMethod]
    public async Task VersionedMetadataEndpointExposesCanonicalProductIdentity()
    {
        await using WebApplicationFactory<Program> factory = new();
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/v1/meta");
        response.EnsureSuccessStatusCode();

        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement root = document.RootElement;

        Assert.AreEqual("NBEOS", root.GetProperty("productCode").GetString());
        Assert.AreEqual("v1", root.GetProperty("apiVersion").GetString());
    }

    [TestMethod]
    public async Task OpenApiContractIsPublished()
    {
        await using WebApplicationFactory<Program> factory = new();
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/openapi/v1.json");
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(json, "\"openapi\"");
        StringAssert.Contains(json, "/api/v1/meta");
    }

    [TestMethod]
    public async Task UnknownRouteUsesProblemDetails()
    {
        await using WebApplicationFactory<Program> factory = new();
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/this-route-does-not-exist");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
