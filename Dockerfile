FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN apk update
RUN apk upgrade
RUN apk add curl
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /
COPY ["om-svc-gateway/om-svc-gateway.csproj", "./"]
RUN dotnet restore "om-svc-gateway.csproj"
COPY . .

FROM build AS publish
RUN dotnet publish "om-svc-gateway/om-svc-gateway.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "om-svc-gateway.dll"]