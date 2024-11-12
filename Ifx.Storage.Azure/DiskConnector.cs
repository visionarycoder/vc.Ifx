using Microsoft.Extensions.Logging;

namespace vc.Ifx.Storage.Azure;

public class DiskConnector
{
    private readonly ComputeManagementClient computeManagementClient;
    private readonly ILogger<DiskConnector> logger;

    public DiskConnector(string subscriptionId)
    {
        logger = LoggerFactory.Create(builder => builder.AddDebug()).CreateLogger<DiskConnector>();
        computeManagementClient = new ComputeManagementClient(subscriptionId, new DefaultAzureCredential());
    }

    public DiskConnector(ILogger<DiskConnector> logger, string subscriptionId)
    {
        this.logger = logger;
        computeManagementClient = new ComputeManagementClient(subscriptionId, new DefaultAzureCredential());
    }

    public async Task CreateDiskAsync(string resourceGroupName, string diskName, int sizeInGb, string location)
    {
        var disk = new Disk
        {
            Location = location,
            Sku = new DiskSku { Name = DiskStorageAccountTypes.StandardLRS },
            DiskSizeGB = sizeInGb,
            CreationData = new CreationData(DiskCreateOption.Empty)
        };

        await computeManagementClient.Disks.StartCreateOrUpdate(resourceGroupName, diskName, disk).WaitForCompletionAsync();
        logger.LogInformation($"Disk {diskName} created in resource group {resourceGroupName} with size {sizeInGb} GB.");
    }

    public async Task DeleteDiskAsync(string resourceGroupName, string diskName)
    {
        await computeManagementClient.Disks.StartDelete(resourceGroupName, diskName).WaitForCompletionAsync();
        logger.LogInformation($"Disk {diskName} deleted from resource group {resourceGroupName}.");
    }
}