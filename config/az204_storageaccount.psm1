[CmdletBinding()]
Param(
    [string]$RGName="ImageResizeWebApp",
    [string]$StorageAccountName = "shsimageresizewebapp",
    [string]$ServicePlanName="ImageResizeServicePlan",
    [string]$WebAppName="ShsImageResizeWebApp"
)

function CreateImageResizeContainers {
    if ($(az group exists --name $RGName) -eq 'true') {
        $confirmation = Read-Host "Resource group $RGName already exists, do you want to re-create it?" 
        if ($confirmation -eq 'y') {
            az group delete --name $RGName
        } else {
            return
        }
    }
    Write-Verbose -Message "Create resource group $RGName"
    az group create --name $rgName --location northeurope

    Write-Verbose -Message "Create storage account"
    az storage account create --name $StorageAccountName --location northeurope --resource-group $rgName --sku Standard_LRS --kind StorageV2 --access-tier hot

    Write-Verbose -Message "Create containers"
    $blobStorageAccountKey=$(az storage account keys list -g $RGName -n $StorageAccountName --query "[0].value" --output tsv)

    az storage container create --name images --account-name $StorageAccountName --account-key $blobStorageAccountKey
    az storage container create --name thumbnails --account-name $StorageAccountName --account-key $blobStorageAccountKey --public-access container
}

function SetThumbNailsContainerAccessMode {
    [CmdletBinding()]
    param(
        [string]$PublicAccess="off"
    )
    Write-Verbose -Message "Set thumbnails public-access to $PublicAccess"
    $blobStorageAccountKey=$(az storage account keys list -g $RGName -n $StorageAccountName --query "[0].value" --output tsv)
    az storage container set-permission --account-name $StorageAccountName --account-key $blobStorageAccountKey --name thumbnails --public-access $PublicAccess
}

function CreateWebApp {
    az appservice plan create --name $ServicePlanName --resource-group $RGName --sku Free
    az webapp create --name $WebAppName --resource-group $RGName --plan $ServicePlanName

}

function SetWebAppConfig {
    $blobStorageAccountKey=$(az storage account keys list -g $RGName -n $StorageAccountName --query "[0].value" --output tsv)
    az webapp config appsettings set --name $WebAppName --resource-group $RGName --settings `
    AzureStorageConfig__AccountName=$StorageAccountName `
    AzureStorageConfig__ImageContainer=images `
    AzureStorageConfig__ThumbnailContainer=thumbnails `
    AzureStorageConfig__AccountKey=$blobStorageAccountKey `
    AzureStorageConfig__BlobStorageUri="https://{0}.blob.core.windows.net/{1}/{2}" `
    AzureStorageConfig__AccountUri="https://{0}.blob.core.windows.net/"
    
}

function DeployWebApp {
    $webAppList = $(az webapp list --resource-group $RGName --query "[].name" --output tsv)
    if ($webAppList.length -eq 0) {
        Write-Verbose -Message "Create web app" + $WebAppName      
        try {  
            CreateWebApp -ErrorAction Stop
        } catch {
            Write-Error -Message "Unable to create web app"
        }
    }
     
}

Export-ModuleMember -Function CreateImageResizeContainers, SetThumbNailsContainerAccessMode, DeployWebApp, SetWebAppConfig
