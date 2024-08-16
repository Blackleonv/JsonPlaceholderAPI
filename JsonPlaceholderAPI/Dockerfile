#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-1809 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0-nanoserver-1809 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["JsonPlaceholderAPI/JsonPlaceholderAPI.csproj", "JsonPlaceholderAPI/"]
RUN dotnet restore "./JsonPlaceholderAPI/./JsonPlaceholderAPI.csproj"
COPY . .
WORKDIR "/src/JsonPlaceholderAPI"
RUN dotnet build "./JsonPlaceholderAPI.csproj" -c %BUILD_CONFIGURATION% -o /app/build



FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./JsonPlaceholderAPI.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JsonPlaceholderAPI.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
       locales \
    && rm -rf /var/lib/apt/lists/*
    
RUN locale-gen en_US.UTF-8  
ENV LANG en_US.UTF-8  
ENV LC_ALL en_US.UTF-8
