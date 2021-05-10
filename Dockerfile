FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0 as base
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build-env /app/out .
COPY configure.sh ./
RUN bash configure.sh
EXPOSE 80 443
ENTRYPOINT ["dotnet", "space.linuxct.malninstall.Configuration.dll"]
