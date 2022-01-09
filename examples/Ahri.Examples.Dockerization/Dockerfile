#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . /src
RUN ls -al /src
RUN dotnet restore "examples/Ahri.Examples.Dockerization/Ahri.Examples.Dockerization.csproj"
WORKDIR "/src/examples/Ahri.Examples.Dockerization"
RUN dotnet build "Ahri.Examples.Dockerization.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ahri.Examples.Dockerization.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5000
ENTRYPOINT ["dotnet", "Ahri.Examples.Dockerization.dll"]