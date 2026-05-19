# ============================
# STAGE 1 - Build da aplicação
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish

# ============================
# STAGE 2 - Imagem final
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Diretório de trabalho da aplicação
WORKDIR /dimdimapp

# Cria usuário não-root chamado dimdimuser
RUN adduser --disabled-password --gecos "" dimdimuser

# Copia os arquivos publicados do stage de build
COPY --from=build /app/publish .

# Define dono dos arquivos para o usuário criado
RUN chown -R dimdimuser:dimdimuser /dimdimapp

# Variável de ambiente com a string de conexão
ENV DATABASE_URL="Host=db;Port=5432;Database=dimdimdb;Username=dimdim;Password=dimdim123"

# Porta que a aplicação vai escutar
ENV ASPNETCORE_URLS="http://+:8080"

# Troca para o usuário não-root
USER dimdimuser

# Porta exposta
EXPOSE 8080

# Comando de inicialização
ENTRYPOINT ["dotnet", "DimDimApp.dll"]
