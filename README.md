# CreateTicketAzureFunction

## Summary

Azure function to create a ticket in a freshdesk instance
- This application create ticket in a freshdesk instance using the API.

## Prerequisites

## Version 

![dotnet 6](https://img.shields.io/badge/net6.0-blue.svg)

## API permission

MSGraph

| API / Permissions name    | Type        | Admin consent | Justification                       |
| ------------------------- | ----------- | ------------- | ----------------------------------- |


Sharepoint

| API / Permissions name    | Type      | Admin consent | Justification                       |
| ------------------------- | --------- | ------------- | ----------------------------------- |

## App setting

| Name                    	| Description                                                                   					         |
| -------------------------	| ------------------------------------------------------------------------------------------------ |
| DOMAIN | Freshdesk domain name|
| API_KEY | Freshdesk API key |

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

## Disclaimer

**THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.**