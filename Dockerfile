FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Node.js para o build do Tailwind CSS
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

WORKDIR /src

# Instalar dependências npm antes do resto (cache layer)
COPY WebServicos/WebServicos/package*.json WebServicos/WebServicos/
RUN cd WebServicos/WebServicos && npm ci

COPY WebServicos/WebServicos/WebServicos.csproj WebServicos/WebServicos/
RUN dotnet restore WebServicos/WebServicos/WebServicos.csproj

COPY . .
WORKDIR /src/WebServicos/WebServicos
RUN dotnet publish -c Release -o /app/publish --nologo

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000
ENTRYPOINT ["dotnet", "WebServicos.dll"]
