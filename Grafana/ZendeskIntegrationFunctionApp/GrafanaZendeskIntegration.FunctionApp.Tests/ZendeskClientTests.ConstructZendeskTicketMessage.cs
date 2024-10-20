// -----------------------------------------------------------------------
// <copyright file="ZendeskClientTests.ConstructZendeskTicketMessage.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Newtonsoft.Json;

namespace GrafanaZendeskIntegration.FunctionApp.Tests;

using Azure.Data.Tables;
using global::GrafanaZendeskIntegration.FunctionApp.Models;
using global::GrafanaZendeskIntegration.FunctionApp.Services;
using Microsoft.Extensions.Logging;
using Moq;
using static System.Runtime.InteropServices.JavaScript.JSType;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Tests are self documented")]
public partial class ZendeskClientTests
{
    private readonly Mock<ILoggerFactory> loggerFactory = new();
    private readonly Mock<ILogger> log = new();
    private readonly IOptions<ZendeskClientOptions> zendeskClientOptions = Options.Create(new ZendeskClientOptions());
    private readonly IOptions<StorageClientOptions> storageClientOptions = Options.Create(new StorageClientOptions());
    private readonly Mock<IHttpClientFactory> mockHttpClientFactory = new();
    private StorageClient storageClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZendeskClientTests"/> class.
    /// </summary>
    public ZendeskClientTests()
    {
        Mock<TableClient> tableClient = new();
        this.storageClient = new StorageClient(this.storageClientOptions, tableClient.Object, this.loggerFactory.Object);

        this.zendeskClientOptions = Options.Create(new ZendeskClientOptions()
        {
            ZendeskTicketsApiUrl = "https://boguswillowinc.zendesk.com/api/v2/tickets",
            ZendeskUserName = "Command Portal",
            ZendeskUserEmailAddress = "commandportal@willowinc.com",
            ZendeskUserApiToken = "bogus",
            EnvironmentsToGenerateTicketsFor = "dev,uat,ppe,prod,prd",
        });

        this.loggerFactory
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(this.log.Object);
    }

    [TestMethod]
    public void GrafanaAlerts_DatasourceError_Should_Return_Empty_Ticket()
    {
        GrafanaAlert alert = new()
        {
            commonLabels = new Commonlabels { alertname = "DatasourceError" },
        };
        var zendeskClient = new ZendeskClient(this.zendeskClientOptions, [], this.mockHttpClientFactory.Object, this.loggerFactory.Object);
        var shouldProcess = zendeskClient.ShouldProcessAlert(alert);

        Assert.IsFalse(shouldProcess);
    }

    [TestMethod]
    public void GrafanaAlerts_DatasourceNoData_Should_Return_Empty_Ticket()
    {
        GrafanaAlert alert = new()
        {
            commonLabels = new Commonlabels { alertname = "DatasourceNoData" },
        };
        var zendeskClient = new ZendeskClient(this.zendeskClientOptions, new List<GrafanaZendeskFieldMapping>(), this.mockHttpClientFactory.Object, this.loggerFactory.Object);
        var shouldProcess = zendeskClient.ShouldProcessAlert(alert);

        Assert.IsFalse(shouldProcess);
    }

    [TestMethod]
    public void GrafanaAlerts_Should_Not_Process_Alerts_For_Non_Supported_Environments()
    {
        GrafanaAlert alert = new()
        {
            commonLabels = new Commonlabels
            {
                alertname = "firing",
                environment = "nonprod",
            },
            alerts = [
            new Alert
            {
                status = "firing",
            },
        ],
        };
        this.zendeskClientOptions.Value.EnvironmentsToGenerateTicketsFor = "prd";

        var zendeskClient = new ZendeskClient(this.zendeskClientOptions, new List<GrafanaZendeskFieldMapping>(), this.mockHttpClientFactory.Object, this.loggerFactory.Object);
        var shouldProcess = zendeskClient.ShouldProcessAlert(alert);

        Assert.IsFalse(shouldProcess);
    }

    [TestMethod]
    public void GrafanaAlerts_Should_Process_Alerts_For_Supported_Environments()
    {
        GrafanaAlert alert = new()
        {
            commonLabels = new Commonlabels
            {
                alertname = "firing",
                environment = "abcdefg",
            },
            alerts = [
            new Alert
            {
                status = "firing",
            },
        ],
        };
        this.zendeskClientOptions.Value.EnvironmentsToGenerateTicketsFor = "abcdefg";

        var zendeskClient = new ZendeskClient(this.zendeskClientOptions, new List<GrafanaZendeskFieldMapping>(), this.mockHttpClientFactory.Object, this.loggerFactory.Object);
        var shouldProcess = zendeskClient.ShouldProcessAlert(alert);

        Assert.IsTrue(shouldProcess);
    }

    /// <summary>
    /// Parse the Grafana Alert message.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [TestMethod]
    public async Task GrafanaAlert_Parse()
    {
        const string testFilename = @"TestFiles/mytest.json";
        string grafanaAlertsMessage = await File.ReadAllTextAsync(testFilename);

        var zendeskClient = new ZendeskClient(this.zendeskClientOptions, new List<GrafanaZendeskFieldMapping>(), this.mockHttpClientFactory.Object, this.loggerFactory.Object);
        var grafanaAlert = GrafanaAlert.CreateInstance(grafanaAlertsMessage);

        Assert.IsNotNull(grafanaAlert);
        var zendeskAlertMessage = zendeskClient.ConstructZendeskTicketMessage(grafanaAlert);

        Assert.IsNotNull(zendeskAlertMessage);
        Assert.IsInstanceOfType<ZendeskTicketMessage>(zendeskAlertMessage);
    }

    /// <summary>
    /// Test to verify that the ConstructZendeskTicketMessage method returns a valid ZendeskTicketMessage object.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [TestMethod]
    public async Task GrafanaAlert2516943293410712447_ReturnsValidZendeskTicket()
    {
        const string testFilename = @"TestFiles/2516943293410712447_GrafanaAlert.json";
        string grafanaAlertsMessage = await File.ReadAllTextAsync(testFilename);

        var grafanaAlert = GrafanaAlert.CreateInstance(grafanaAlertsMessage);
        Assert.IsNotNull(grafanaAlert);
    }

    /// <summary>
    /// Test to verify that the ConstructZendeskTicketMessage method populates the ticket with the correct panel url values.
    /// </summary>
    /// <param name="id">Ticket Id.</param>
    /// <param name="value">Url.</param>
    [TestMethod]
    [DataRow(9249847891343, "panelUrlValue")]
    public void ParsePanelUrl(long id, string value)
    {
        GrafanaAlert grafanaAlert = GenerateTestGrafanaAlert();
        grafanaAlert.alerts[0].panelURL = value;
        var zendeskClient = new ZendeskClient(this.zendeskClientOptions, new List<GrafanaZendeskFieldMapping>(), this.mockHttpClientFactory.Object, this.loggerFactory.Object);
        var zendeskTicketObject = zendeskClient.ConstructZendeskTicketMessage(grafanaAlert);

        Assert.AreEqual(value, zendeskTicketObject.ticket.custom_fields.Find(x => x.id == id)!.value);
    }

    /// <summary>
    /// Test to verify that the ConstructZendeskTicketMessage method populates the ticket with the correct dashboard url values.
    /// </summary>
    /// <param name="id">Ticket Id.</param>
    /// <param name="value">Dashboard URL.</param>
    [TestMethod]
    [DataRow(9249848448911, "dashboardUrlValue")]
    public void ConstructZendeskTicketMessage_Should_Populate_Dashboard_Url_FieldId(long id, string value)
    {
        GrafanaAlert grafanaAlert = GenerateTestGrafanaAlert();
        grafanaAlert.alerts[0].dashboardURL = value;
        var zendeskClient = new ZendeskClient(this.zendeskClientOptions, new List<GrafanaZendeskFieldMapping>(), this.mockHttpClientFactory.Object, this.loggerFactory.Object);
        var zendeskTicketObject = zendeskClient.ConstructZendeskTicketMessage(grafanaAlert);
        Assert.IsNotNull(zendeskTicketObject, "zendeskTicketObject is null");

        var zendeskTicketCustomField = zendeskTicketObject.ticket.custom_fields.Find(x => x.id == id);
        Assert.IsNotNull(zendeskTicketCustomField, "Zendesk custom field is null");
        Assert.AreEqual(value, zendeskTicketCustomField.value);
    }

    /// <summary>
    ///     Test to verify that the ConstructZendeskTicketMessage method populates the ticket with the correct TSG url values.
    /// </summary>
    /// <param name="id">Ticket Id.</param>
    /// <param name="value">Ticket URL</param>
    [TestMethod]
    [DataRow(9249847581199, "tsgUrlValue")]
    public void ConstructZendeskTicketMessage_Should_Populate_TSG_Url_FieldId(long id, string value)
    {
        GrafanaAlert grafanaAlert = GenerateTestGrafanaAlert();
        grafanaAlert.commonLabels.tsg = value;
        var zendeskClient = new ZendeskClient(this.zendeskClientOptions, new List<GrafanaZendeskFieldMapping>(), this.mockHttpClientFactory.Object, this.loggerFactory.Object);
        var zendeskTicketObject = zendeskClient.ConstructZendeskTicketMessage(grafanaAlert);
        Assert.IsNotNull(zendeskTicketObject, "zendeskTicketObject is null");

        var zendeskTicketCustomField = zendeskTicketObject.ticket.custom_fields.Find(x => x.id == id);
        Assert.IsNotNull(zendeskTicketCustomField, "Zendesk custom field is null");
        Assert.AreEqual(value, zendeskTicketCustomField.value);
    }

    /// <summary>
    ///    Test to verify that the ConstructZendeskTicketMessage method populates the ticket with the correct standard field values.
    /// </summary>
    [TestMethod]
    [DataRow("group_id", "commonLabels.Owner", "Cloud Ops", "7350842174095")]
    public void UpdateZendeskTicketStandardFieldFromGrafanaAlertCommonLabels_Should_Replace_Values_Correctly(string zendeskTicketFieldId, string grafanaFieldName, string owner, string expected)
    {
        var fieldMappings = new List<GrafanaZendeskFieldMapping>
        {
            new GrafanaZendeskFieldMapping
            {
                DefaultValue = false,
                GrafanaFieldName = grafanaFieldName,
                GrafanaFieldValue = owner,
                ZendeskTicketFieldId = zendeskTicketFieldId,
                ZendeskTicketFieldTag = expected,
            },
        };

        ZendeskClient zendeskClient = new(this.zendeskClientOptions, fieldMappings, this.mockHttpClientFactory.Object, this.loggerFactory.Object);
        var zendeskTicketRequester = new ZendeskTicketRequester
        {
            name = "user",
            email = "user@domain.com",
        };
        var zendeskTicketMessageBody = new ZendeskTicketMessageBody
        {
            status = "new",
            subject = "subject",
            requester = zendeskTicketRequester,
        };

        zendeskClient.UpdateZendeskTicketStandardFieldFromGrafanaAlertCommonLabels(
            owner,
            grafanaFieldName,
            zendeskTicketFieldId,
            zendeskTicketMessageBody);

        Assert.AreEqual(expected, zendeskTicketMessageBody.group_id.ToString());
    }

    /// <summary>
    ///    Test to verify that the UpdateZendeskTicketCustomFieldFromGrafanaAlertCommonLabels method populates the ticket with the correct custom field values.
    /// </summary>
    [TestMethod]
    [DataRow("7231972707215", "commonLabels.Severity", "2", "2_-_medium")]
    [DataRow("8001260747407", "commonLabels.Environment", "dev", "dev")]
    public void UpdateZendeskTicketCustomFieldFromGrafanaAlertCommonLabels_Should_Replace_Values_Correctly(string zendeskTicketFieldId, string grafanaFieldName, string severity, string expected)
    {
        var fieldMappings = new List<GrafanaZendeskFieldMapping>
        {
            new GrafanaZendeskFieldMapping
            {
                DefaultValue = false,
                GrafanaFieldName = grafanaFieldName,
                GrafanaFieldValue = severity,
                ZendeskTicketFieldId = zendeskTicketFieldId,
                ZendeskTicketFieldTag = expected,
            },
        };

        ZendeskClient zendeskClient = new(this.zendeskClientOptions, fieldMappings, this.mockHttpClientFactory.Object, this.loggerFactory.Object);
        var zendeskTicketRequester = new ZendeskTicketRequester
        {
            name = "user",
            email = "user@domain.com",
        };
        var zendeskTicketMessageBody = new ZendeskTicketMessageBody
        {
            status = "new",
            subject = "subject",
            requester = zendeskTicketRequester,
            custom_fields = [],
        };
        zendeskClient.UpdateZendeskTicketCustomFieldFromGrafanaAlertCommonLabels(
            severity,
            grafanaFieldName,
            zendeskTicketFieldId,
            zendeskTicketMessageBody);

        var customField =
            zendeskTicketMessageBody.custom_fields.Find(x => x.id == long.Parse(zendeskTicketFieldId));

        Assert.IsNotNull(customField);
        Assert.AreEqual(expected, customField.value);
    }

    [TestMethod]
    public void GenerateCommonLabelsTable_Should_Add_EntityName_If_It_Exists()
    {
        GrafanaAlert grafanaAlert = new()
        {
            commonLabels = new Commonlabels
            {
                alertname = "myAlertName",
                customercode = "myCustomerCode",
                environment = "myEnvironment",
                EntityName = "myEntityName",
            },
        };

        var commonLabelsTable = ZendeskClient.GenerateCommonLabelsTable(grafanaAlert, "Title");

        // Assert alertName is in the table
        Assert.IsTrue(commonLabelsTable.Contains("alertname"));
        Assert.IsTrue(commonLabelsTable.Contains("myAlertName"));

        // Assert alertName is in the table
        Assert.IsTrue(commonLabelsTable.Contains("customercode"));
        Assert.IsTrue(commonLabelsTable.Contains("myCustomerCode"));

        // Assert alertName is in the table
        Assert.IsTrue(commonLabelsTable.Contains("environment"));
        Assert.IsTrue(commonLabelsTable.Contains("myEnvironment"));

        Assert.IsTrue(commonLabelsTable.Contains("EntityName"));
        Assert.IsTrue(commonLabelsTable.Contains("myEntityName"));
    }

    [TestMethod]
    [DataRow(360029547271, "prd:eus2:11:brk-in1", "brookfield")]
    [DataRow(360029547271, "prd:weu:02:bnp-nuv", "nuveen")]
    public void ConstructZenDeskTicket_Should_Update_Owner_Based_On_FullCustomerInstance(long id, string fullCustomerInstance, string result)
    {
        GrafanaAlert grafanaAlert = new()
        {
            commonLabels = new Commonlabels
            {
                fullcustomerinstancename = fullCustomerInstance,
            },
        };

        List<GrafanaZendeskFieldMapping> fieldMappings =
        [
            new GrafanaZendeskFieldMapping { DefaultValue = false, GrafanaFieldName = "commonLabels.customer-code", GrafanaFieldValue = "brk", ZendeskTicketFieldId = "360029547271", ZendeskTicketFieldTag = "brookfield" },
            new GrafanaZendeskFieldMapping { DefaultValue = false, GrafanaFieldName = "commonLabels.customer-code", GrafanaFieldValue = "bnp", ZendeskTicketFieldId = "360029547271", ZendeskTicketFieldTag = "nuveen" },
        ];

        var zendeskClient = new ZendeskClient(this.zendeskClientOptions, fieldMappings, this.mockHttpClientFactory.Object, this.loggerFactory.Object);
        var zendeskTicketObject = zendeskClient.ConstructZendeskTicketMessage(grafanaAlert);
        Assert.IsTrue(zendeskTicketObject.ticket.custom_fields.First(x => x.id == id).value == result);
    }

    [TestMethod]
    [DataRow("building1, building2", "building1")]
    [DataRow("building1,building2", "building1")]
    [DataRow(",", "")]
    [DataRow("", "")]
    public void ConstructZenDeskTicket_Should_Update_Building_Custom_Field(string buildings, string expectedResult)
    {
        const long id = 4413864107161;
        GrafanaAlert grafanaAlert = new()
        {
            commonLabels = new Commonlabels
            {
                buildings = buildings,
            },
        };

        var zendeskClient = new ZendeskClient(this.zendeskClientOptions, new (), this.mockHttpClientFactory.Object, this.loggerFactory.Object);
        var zendeskTicketObject = zendeskClient.ConstructZendeskTicketMessage(grafanaAlert);

        // If the expected result is empty, then the custom field should not exist
        if (string.IsNullOrEmpty(expectedResult))
        {
            Assert.IsTrue(zendeskTicketObject.ticket.custom_fields.TrueForAll(x => x.id != id));
        }
        else
        {
            Assert.IsTrue(zendeskTicketObject.ticket.custom_fields.First(x => x.id == id).value == expectedResult);
        }
    }

    [TestMethod]
    public async Task GetMergedTicketParent_Should_Return_Parent_Ticket_IdAsync()
    {
        const string testFilename = "TestFiles/MergedAudit.json";
        string audit = await File.ReadAllTextAsync(testFilename);

        var zendeskClient = new ZendeskClient(this.zendeskClientOptions, new List<GrafanaZendeskFieldMapping>(), this.mockHttpClientFactory.Object, this.loggerFactory.Object);
        var auditObject = JsonConvert.DeserializeObject<ZendeskTicketAudits>(audit);
        var ticketNumber = zendeskClient.GetMergedTicketParent(auditObject);

        Assert.AreEqual("33500", ticketNumber);
    }

private static GrafanaAlert GenerateTestGrafanaAlert()
    {
        GrafanaAlert grafanaAlert = new();
        List<Alert> alerts =
        [
            new()
            {
                status = "firing",
            },
        ];
        grafanaAlert.alerts = alerts.ToArray();
        grafanaAlert.commonLabels = new Commonlabels
        {
            alertname = "name",
            destination = "destination",
            grafana_folder = "folder",
            app = "app",
            customercode = "customercode",
            environment = "environment",
            owner = "owner",
            region = "region",
            severity = "2",
            stamp = "stamp",
        };
        grafanaAlert.status = "Default";
        return grafanaAlert;
    }
}
