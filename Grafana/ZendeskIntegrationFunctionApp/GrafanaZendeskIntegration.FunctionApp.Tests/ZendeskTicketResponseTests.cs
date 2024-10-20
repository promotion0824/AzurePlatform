using GrafanaZendeskIntegration.FunctionApp.Services;

namespace GrafanaZendeskIntegration.FunctionApp.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ZendeskTicketResponseTests
{
    [TestMethod]
    public async Task CreateInstance_Should_Parse_Valid_Ticket_Json()
    {
        // Arrange
        const string testFilename = "TestFiles/ValidZendeskTicketResponse.json";
        string zendeskTicketResponseJson = await File.ReadAllTextAsync(testFilename);

        // Act
        var zendeskTicketResponse = ZendeskTicketResponse.CreateInstance(zendeskTicketResponseJson);

        // Assert
        Assert.IsNotNull(zendeskTicketResponse);
        Assert.IsInstanceOfType(zendeskTicketResponse, typeof(ZendeskTicket));
        Assert.AreEqual(32629, ((ZendeskTicket)zendeskTicketResponse).ticket.id);
    }

    [TestMethod]
    public async Task CreateInstance_Should_Parse_Valid_DetailedError_Response_Json()
    {
        // Arrange
        const string testFilename = "TestFiles/ErrorZendeskTicketResponse1.json";
        string zendeskTicketResponseJson = await File.ReadAllTextAsync(testFilename);

        // Act
        var zendeskTicketResponse = ZendeskTicketResponse.CreateInstance(zendeskTicketResponseJson);

        // Assert
        Assert.IsNotNull(zendeskTicketResponse);
        Assert.IsInstanceOfType(zendeskTicketResponse, typeof(DetailedError));
        Assert.AreEqual("Forbidden", ((DetailedError)zendeskTicketResponse).error.title);
    }

    [TestMethod]
    public async Task CreateInstance_Should_Parse_Valid_SimpleError_Response_Json()
    {
        // Arrange
        const string testFilename = "TestFiles/ErrorZendeskTicketResponse2.json";
        string zendeskTicketResponseJson = await File.ReadAllTextAsync(testFilename);

        // Act
        var zendeskTicketResponse = ZendeskTicketResponse.CreateInstance(zendeskTicketResponseJson);

        // Assert
        Assert.IsNotNull(zendeskTicketResponse);
        Assert.IsInstanceOfType(zendeskTicketResponse, typeof(SimpleError));
        Assert.AreEqual("RecordNotFound", ((SimpleError)zendeskTicketResponse).error);
    }
}
