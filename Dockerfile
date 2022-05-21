#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ./DropboxSync.UIL/DropboxSync.UIL.csproj DropboxSync.UIL/
COPY ./DropboxSync.BLL/DropboxSync.BLL.csproj DropboxSync.BLL/
COPY ./DropboxSync.Helpers/DropboxSync.Helpers.csproj DropboxSync.Helpers/
RUN dotnet restore "DropboxSync.UIL/DropboxSync.UIL.csproj"
COPY . .
WORKDIR "/src/DropboxSync.UIL"
RUN dotnet build "DropboxSync.UIL.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DropboxSync.UIL.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DropboxSync.UIL.dll"]