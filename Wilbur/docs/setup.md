# Auth Setup
Once the bot is deployed you need to setup the bot configuration in azure to point to an azure ad app registration with appropriate scopes defined
Details can be found here https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp%2Caadv2#create-the-azure-ad-identity-provider

The scopes the bot currently uses are
```
openid profile User.Read
```

# Keyvault setup

Add the client secret from the Microsoft App ID the bot is deployed to use into the bots keyvault as 'MicrosoftAppPassword'
The app id is set in the deployment template when creating the bot.


# Role Setup

The Bot uses a custom role to access SQL and Postgres firewall rules in azure, the defnition of this role can be found in this repository at /Scripts/RolesAdnPermission/sql-fw-rule-admin.json. The bot identity in dev is assigned this role for the 'Product' Managment group. The dev bot is assigned the role to a subset of subscription, when running locally the bot runs under your own account for permissions.
