# ----------------------------------------------------------------------
# FASE 1: BUILD - Cria a DLL de produção (PingerService)
# ----------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
# NOTA: Usando '10.0-preview' para ser consistente com o seu API Dockerfile

# Define o diretório de trabalho dentro do container
WORKDIR /src

# Copia o arquivo de projeto e restaura as dependências
# Assumindo que este Dockerfile está no mesmo diretório que PingerService.csproj
COPY ["PingerService.csproj", "."]
RUN dotnet restore

# Copia todo o restante do código
COPY . .

# Publica a aplicação em modo Release
# O binário final será /app/publish/PingerService.dll
RUN dotnet publish -c Release -o /app/publish


# ----------------------------------------------------------------------
# FASE 2: RUNTIME - Executa a aplicação
# ----------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final

WORKDIR /app

# Copia os arquivos publicados da fase de build
COPY --from=build /app/publish .

# Define a porta (Render fará o mapeamento)
ENV ASPNETCORE_URLS=http://+:8080

# Inicia a aplicação
ENTRYPOINT ["dotnet", "PingerService.dll"]