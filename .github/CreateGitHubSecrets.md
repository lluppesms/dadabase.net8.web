# Set up GitHub Secrets

The GitHub workflows in this project require several secrets set at the repository level.

---

## Azure Resource Creation Credentials

You need to set up the Azure Credentials secret in the GitHub Secrets at the Repository level before you do anything else.

See [https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions) for more info.

To create these secrets, customize and run this command::

``` bash
gh auth login

gh secret set AZURE_CLIENT_ID -b <GUID>
gh secret set AZURE_CLIENT_SECRET -b <GUID>
gh secret set AZURE_TENANT_ID -b <GUID>
gh secret set AZURE_SUBSCRIPTION_ID -b <yourAzureSubscriptionId>
```

---

## Bicep Configuration Values

These variables and secrets are used by the Bicep templates to configure the resource names that are deployed.  Make sure the App_Name variable is unique to your deploy. It will be used as the basis for the website name and for all the other Azure resources, which must be globally unique.
To create these additional secrets and variables, customize and run this command:

Secret Values:

``` bash
gh auth login

gh variable set APP_NAME -b "xxx-dadabase"
gh variable set RESOURCEGROUP_LOCATION -b "eastus"
gh variable set RESOURCEGROUP_PREFIX -b "rg_dadabase-webg" 
gh variable set API_KEY -b "somesecretstring"

gh secret set LOGIN_CLIENTID -b "<yourADClientId>"
gh secret set LOGIN_DOMAIN -b "<yourdomain>.onmicrosoft.com"
gh secret set LOGIN_INSTANCEENDPOINT -b "https://login.microsoftonline.com/"
gh secret set LOGIN_TENANTID -b "<yourTenantId>"

gh variable set RESOURCEGROUP_LOCATION -b "eastus"

gh variable set APP_PROJECT_FOLDER_NAME -b "src\Dadabase\DadABase.Web"
gh variable set APP_PROJECT_NAME -b "DadABase.Web"
gh variable set APP_TEST_FOLDER_NAME -b "src\Dadabase\DadABase.Tests"
gh variable set APP_TEST_PROJECT_NAME -b "DadABase.Tests"
```

---

## References

[Deploying ARM Templates with GitHub Actions](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions)

[GitHub Secrets CLI](https://cli.github.com/manual/gh_secret_set)
