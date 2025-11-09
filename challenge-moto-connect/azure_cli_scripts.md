# Scripts de Provisionamento Azure CLI (Opção App Service)

Este arquivo contém a estrutura dos comandos Azure CLI necessários para provisionar os recursos na Azure, conforme exigido pela 4ª entrega.

**NOTA:** Estes comandos são apenas a estrutura. Para a execução real, é necessário estar autenticado no Azure CLI (`az login`) e substituir os placeholders (`<...>` e `[...]`) pelos valores reais do seu projeto.

## 1. Variáveis de Ambiente (Substitua os valores)

```bash
RESOURCE_GROUP="rg-motoconnect-challenge"
LOCATION="eastus" # Escolha a região mais próxima
APP_SERVICE_PLAN="plan-motoconnect-challenge"
WEB_APP_NAME="app-motoconnect-challenge-api"
SQL_SERVER_NAME="sqlserver-motoconnect-challenge"
SQL_DB_NAME="motoconnectdb"
SQL_ADMIN_USER="motosqladmin"
SQL_ADMIN_PASSWORD="<SuaSenhaForteAqui>"
```

## 2. Criação do Grupo de Recursos

```bash
az group create --name $RESOURCE_GROUP --location $LOCATION
```

## 3. Criação do Servidor SQL e Banco de Dados

```bash
# Criação do Servidor SQL
az sql server create \
    --name $SQL_SERVER_NAME \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --admin-user $SQL_ADMIN_USER \
    --admin-password $SQL_ADMIN_PASSWORD

# Configuração da regra de firewall para permitir acesso de serviços Azure
az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER_NAME \
    --name AllowAzureServices \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0

# Criação do Banco de Dados
az sql db create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER_NAME \
    --name $SQL_DB_NAME \
    --service-objective S0
```

## 4. Criação do Plano do Serviço de Aplicativo (App Service Plan)

```bash
az appservice plan create \
    --name $APP_SERVICE_PLAN \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --is-linux \
    --sku B1 # B1 é um SKU básico, altere conforme necessidade
```

## 5. Criação do Serviço de Aplicativo (Web App)

```bash
az webapp create \
    --resource-group $RESOURCE_GROUP \
    --plan $APP_SERVICE_PLAN \
    --name $WEB_APP_NAME \
    --runtime "DOTNET|8.0"
```

## 6. Configuração da String de Conexão no App Service

A string de conexão deve ser configurada como uma *Application Setting* no App Service para que a aplicação .NET a utilize.

```bash
# Construção da string de conexão (substitua os valores)
CONNECTION_STRING="Server=tcp:$SQL_SERVER_NAME.database.windows.net,1433;Initial Catalog=$SQL_DB_NAME;Persist Security Info=False;User ID=$SQL_ADMIN_USER;Password=$SQL_ADMIN_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Configuração no App Service
az webapp config connection-string set \
    --resource-group $RESOURCE_GROUP \
    --name $WEB_APP_NAME \
    --settings DefaultConnection=$CONNECTION_STRING \
    --connection-string-type SQLAzure
```

## 7. Deploy da Aplicação (Exemplo com ZIP Deploy)

```bash
# O comando de deploy real dependerá do seu pipeline de CI/CD.
# Exemplo de deploy manual via ZIP:
# az webapp deployment source config-zip --resource-group $RESOURCE_GROUP --name $WEB_APP_NAME --src <caminho_para_seu_zip_de_publicacao>
```
