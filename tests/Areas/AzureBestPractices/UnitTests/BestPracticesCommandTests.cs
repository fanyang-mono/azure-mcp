// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Text.Json;
using AzureMcp.Areas.AzureBestPractices.Commands;
using AzureMcp.Models.Command;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AzureMcp.Tests.Areas.AzureBestPractices.UnitTests;

[Trait("Area", "AzureBestPractices")]
public class BestPracticesCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BestPracticesCommand> _logger;
    private readonly CommandContext _context;
    private readonly BestPracticesCommand _command;
    private readonly Parser _parser;

    public BestPracticesCommandTests()
    {
        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();

        _context = new(_serviceProvider);
        _logger = Substitute.For<ILogger<BestPracticesCommand>>();
        _command = new(_logger);
        _parser = new(_command.GetCommand());
    }

    [Fact]
    public async Task ExecuteAsync_GeneralCodeGeneration_ReturnsAzureBestPractices()
    {
        var args = _parser.Parse(["--resource", "general", "--action", "all"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<string[]>(json);

        Assert.NotNull(result);
        Assert.Contains("When querying data plane resources, prefer using azmcp commands over az cli commands.", result[0]);
        Assert.Contains("Managed Identity (Azure-hosted)", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_AzureFunctionsCodeGeneration_ReturnsAzureBestPractices()
    {
        var args = _parser.Parse(["--resource", "azurefunctions", "--action", "code-generation"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<string[]>(json);

        Assert.NotNull(result);
        Assert.Contains("Use the latest programming models (v4 for JavaScript, v2 for Python)", result[0]);
        Assert.Contains("Azure Functions Core Tools for creating Function Apps", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_AzureFunctionsDeployment_ReturnsAzureBestPractices()
    {
        var args = _parser.Parse(["--resource", "azurefunctions", "--action", "deployment"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<string[]>(json);

        Assert.NotNull(result);
        Assert.Contains("flex consumption plan", result[0]);
        Assert.Contains("Always use Linux OS for Python", result[0]);
        Assert.Contains("Function authentication", result[0]);
        Assert.Contains("Application Insights", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidResource_ReturnsBadRequest()
    {
        var args = _parser.Parse(["--resource", "invalid", "--action", "code-generation"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Contains("Invalid resource", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidAction_ReturnsBadRequest()
    {
        var args = _parser.Parse(["--resource", "general", "--action", "invalid"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Contains("Invalid action", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_GeneralDeployment_ReturnsBadRequest()
    {
        var args = _parser.Parse(["--resource", "general", "--action", "deployment"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Contains("The 'general' resource only supports 'all' action", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MissingResource_ReturnsBadRequest()
    {
        var args = _parser.Parse(["--action", "code-generation"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Contains("Both resource and action parameters are required", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MissingAction_ReturnsBadRequest()
    {
        var args = _parser.Parse(["--resource", "general"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Contains("Both resource and action parameters are required", response.Message);
    }
}
