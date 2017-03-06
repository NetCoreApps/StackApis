FROM microsoft/dotnet:1.1-sdk-projectjson
COPY src /app
WORKDIR /app
RUN ["dotnet", "restore"]
WORKDIR /app/StackApis
RUN ["dotnet", "build"]
EXPOSE 5000/tcp
ENV ASPNETCORE_URLS https://*:5000
ENTRYPOINT ["dotnet", "run", "--server.urls", "http://*:5000"]