#create resource group
$rgName = "ImageResizeWebApp"
if ($(az group exists --name $rgName) -eq 'true') {
    $confirmation = Read-Host "Resource group $rgName already exists, do you want to re-create it?" 
    if ($confirmation -eq 'y') {
        az group delete --name $rgName
    } else {
        return
    }
}
    
az group create --name $rgName --location northeurope

#Create storage account
$blobStorageAccount = "shsimageresizewebapp"
az storage account create --name $blobStorageAccount --location northeurope --resource-group $rgName --sku Standard_LRS --kind StorageV2 --access-tier hot

#Create containers
$blobStorageAccountKey=$(az storage account keys list -g $rgName -n $blobStorageAccount --query "[0].value" --output tsv)

az storage container create --name images --account-name $blobStorageAccount --account-key $blobStorageAccountKey
az storage container create --name thumbnails --account-name $blobStorageAccount --account-key $blobStorageAccountKey --public-access container

