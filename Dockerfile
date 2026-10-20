FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /App

# Copy everything
COPY Source Source

# Restore as distinct layers
RUN dotnet publish Source/Server/RTServer.csproj -p:LangVersion=preview -c Release -r linux-x64 -p:PublishSingleFile=true -p:SelfContained=true -p:PublishReadyToRun=true -p:WarningLevel=0

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /App

COPY --from=build-env /App/Source/Server/bin/Release/net10.0/linux-x64/publish/RTServer /App/Server/RTServer

WORKDIR /Data

VOLUME /Data

EXPOSE 25555/tcp

ENTRYPOINT ["/App/Server/RTServer"]
