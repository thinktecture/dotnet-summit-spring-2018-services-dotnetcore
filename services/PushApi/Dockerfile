# Build Image
FROM microsoft/aspnetcore-build:2.1.300-preview1 AS build-env
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/aspnetcore:2.1.0-preview1
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "PushApi.dll"]
