FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app
COPY *.sln ./
COPY space.linuxct.malninstall.Configuration.Api/*.csproj space.linuxct.malninstall.Configuration.Api/
COPY space.linuxct.malninstall.Configuration.Common/*.csproj space.linuxct.malninstall.Configuration.Common/
COPY space.linuxct.malninstall.Configuration.Core.Application/*.csproj space.linuxct.malninstall.Configuration.Core.Application/
COPY space.linuxct.malninstall.Configuration.Core.Application.Contracts/*.csproj space.linuxct.malninstall.Configuration.Core.Application.Contracts/
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:5.0 as base
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build-env /app/out .
COPY configure.sh ./
RUN bash configure.sh
EXPOSE 80 443
ENTRYPOINT ["dotnet", "space.linuxct.malninstall.Configuration.Api.dll"]