# Pull .NET Core 3.1 Runtime
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

# Set Working Directory
WORKDIR .

# Copy contents into image
COPY . .

# Set Entry point
ENTRYPOINT ["dotnet", "LendingPlatform.Web.Client.dll"]
