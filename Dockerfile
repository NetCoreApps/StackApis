FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
COPY src /app
WORKDIR /app

RUN dotnet restore
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app
COPY --from=build /app/StackApis/out .
ENV ASPNETCORE_URLS http://*:5000
ENTRYPOINT ["dotnet", "StackApis.dll"]