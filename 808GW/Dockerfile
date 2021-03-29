#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["808GW/808GW.csproj", "808GW/"]
COPY ["JTServer/JTServer.csproj", "JTServer/"]
RUN dotnet restore "808GW/808GW.csproj"
COPY . .
WORKDIR "/src/808GW"
RUN dotnet build "808GW.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "808GW.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
VOLUME /MyData
#更改时区
RUN /bin/sh -c 'ln -sf /usr/share/zoneinfo/Asia/Shanghai /etc/localtime'
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "808GW.dll"]