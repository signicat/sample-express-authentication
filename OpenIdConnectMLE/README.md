# OpenID Connect Sample for ASP.NET Core

Sample application with login/logout and a profile page displaying user claims.

## Requirements
- [.NET 5.0+ SDK](https://dotnet.microsoft.com/download)

## How to run

1. Add your OpenID client ID and client secret to `appsettings.json`
2. Run the application with `dotnet run`
3. Go to https://localhost:5001 to view the application

## Client configuration

Your OpenID client requires the following configuration for this sample application:

- Flow: Authorization code
- Redirect URI: `https://localhost:5001/callback`
- Post logout redirect URI: `https://localhost:5001/signout-callback`

Clients can be configured in the Signicat Express Dashboard ([test](https://dashboard-test.signicat.io) / [prod](https://dashboard.signicat.io))