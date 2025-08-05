# CreateTicketAzureFunction

## Summary

Azure function to create a ticket in a freshdesk instance
- This application create ticket in a freshdesk instance using the API.

## Prerequisites

## Version 

![dotnet 8](https://img.shields.io/badge/net8.0-blue.svg)

## API permission

MSGraph

n/a

Sharepoint

n/a

## App setting

| Name                    	| Description                                                                   				   |
| -------------------------	| ------------------------------------------------------------------------------------------------ |
| AzureWebJobsStorage		| Connection string for the storage account														   |
| fdDomain					| Freshdesk domain name                                                                            |
| keyVaultUrl				| Address for the key vault                                                                        |
| secretName				| Secret name used to authorize the function app                                                   |

The data you need for the project are:

|Parameter|Type|
|---|---|
|email|string|
|reasonOneVal|string|
|ticketDescription|string|
|reasonTwoVal|string
|pageURL|string
|startDate|string
|endDate|string
|emailTo|string
|isOngoing|string
|attachment|files

## Version history

Version|Date|Comments
-------|----|--------
Current|2025-07-24| .NET 8 upgrade

## Disclaimer

**THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.**