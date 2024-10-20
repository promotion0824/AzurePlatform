
Param
(
    [string]
    $TenantId = "a80618f8-f5e9-43bf-a98f-107dd8f54aa9",

    [string]
    $AppId = "5d038cb6-e52a-4f13-8fd4-b5f73d810a9b",

    [string]
    $SpObjectId = "37405803-aab8-4791-a452-15ecde4510a9"
)

Connect-AzureAD -TenantId $TenantId
Add-AzureADApplicationOwner -ObjectId $AppId -RefObjectId $SpObjectId
