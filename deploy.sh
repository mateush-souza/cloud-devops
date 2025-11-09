#!/bin/bash

set -e
    
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' 

echo -e "${YELLOW}Etapa 1: Configurando variáveis...${NC}"

RESOURCE_GROUP="rg-challenge-fiap-558424"
LOCATION="brazilsouth"
DB_SERVER_NAME="sql-motoconnect-558424"
DB_NAME="motoconnectdb"
APP_SERVICE_PLAN="asp-motoconnect-fiap-558424"
WEB_APP_NAME="webapp-motoconnect-558424"
DB_ADMIN_USER="admin_cloud"
DB_ADMIN_PASSWORD="Senhaforte123!"

if [ -z "$DB_ADMIN_USER" ] || [ -z "$DB_ADMIN_PASSWORD" ]; then
    echo -e "${YELLOW}Erro: DB_ADMIN_USER e DB_ADMIN_PASSWORD devem ser definidos como variáveis de ambiente${NC}"
    echo "Exemplo: export DB_ADMIN_USER='seu_usuario' && export DB_ADMIN_PASSWORD='sua_senha'"
    exit 1
fi

echo -e "${GREEN}✓ Variáveis configuradas${NC}\n"

# --- Registrar Resource Providers ---

echo -e "${YELLOW}Etapa 2: Verificando Resource Providers...${NC}"

wait_for_provider() {
    local namespace=$1
    local name=$2
    
    if ! az provider show --namespace "$namespace" --query "registrationState" -o tsv 2>/dev/null | grep -q "Registered"; then
        echo "Registrando $name (isso pode demorar alguns minutos)..."
        az provider register --namespace "$namespace"
        
        echo "Aguardando registro de $name..."
        local max_attempts=30
        local attempt=0
        while [ $attempt -lt $max_attempts ]; do
            if az provider show --namespace "$namespace" --query "registrationState" -o tsv 2>/dev/null | grep -q "Registered"; then
                echo "$name registrado com sucesso"
                return 0
            fi
            attempt=$((attempt + 1))
            sleep 10
        done
        echo -e "${YELLOW}Aviso: $name ainda não está registrado após 5 minutos, continuando...${NC}"
    else
        echo "$name já está registrado"
    fi
}

wait_for_provider "Microsoft.Sql" "Microsoft.Sql"
wait_for_provider "Microsoft.Web" "Microsoft.Web"
wait_for_provider "Microsoft.Storage" "Microsoft.Storage"

echo -e "${GREEN}✓ Resource Providers verificados${NC}\n"

# --- Criar Infraestrutura ---

echo -e "${YELLOW}Etapa 3: Criando Grupo de Recursos...${NC}"
az group create \
    --name $RESOURCE_GROUP \
    --location $LOCATION \
    --output none
echo -e "${GREEN}✓ Grupo de Recursos criado${NC}\n"

echo -e "${YELLOW}Etapa 4: Criando Servidor SQL...${NC}"
az sql server create \
    --name $DB_SERVER_NAME \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --admin-user $DB_ADMIN_USER \
    --admin-password $DB_ADMIN_PASSWORD \
    --output none
echo -e "${GREEN}✓ Servidor SQL criado${NC}\n"

echo -e "${YELLOW}Etapa 5: Configurando Firewall...${NC}"
az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server $DB_SERVER_NAME \
    --name AllowAzureServices \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0 \
    --output none
az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server $DB_SERVER_NAME \
    --name AllowAll \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 255.255.255.255 \
    --output none
echo -e "${GREEN}✓ Firewall configurado${NC}\n"

echo -e "${YELLOW}Etapa 6: Criando Banco de Dados...${NC}"
az sql db create \
    --resource-group $RESOURCE_GROUP \
    --server $DB_SERVER_NAME \
    --name $DB_NAME \
    --service-objective S0 \
    --backup-storage-redundancy Local \
    --output none
echo -e "${GREEN}✓ Banco de Dados criado${NC}\n"

echo -e "${YELLOW}Etapa 7: Criando App Service Plan...${NC}"
az appservice plan create \
    --name $APP_SERVICE_PLAN \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --sku B1 \
    --is-linux \
    --output none
echo -e "${GREEN}✓ App Service Plan criado${NC}\n"

echo -e "${YELLOW}Etapa 8: Criando Web App...${NC}"
az webapp create \
    --resource-group $RESOURCE_GROUP \
    --plan $APP_SERVICE_PLAN \
    --name $WEB_APP_NAME \
    --runtime "DOTNETCORE:8.0" \
    --output none
echo -e "${GREEN}✓ Web App criado${NC}\n"

echo -e "${YELLOW}Etapa 9: Configurando Connection String...${NC}"
CONNECTION_STRING="Server=tcp:${DB_SERVER_NAME}.database.windows.net,1433;Initial Catalog=${DB_NAME};Persist Security Info=False;User ID=${DB_ADMIN_USER};Password=${DB_ADMIN_PASSWORD};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Configurar via App Settings ao invés de Connection Strings (mais compatível com .NET no Linux)
az webapp config appsettings set \
    --resource-group $RESOURCE_GROUP \
    --name $WEB_APP_NAME \
    --settings \
        "ConnectionStrings__DefaultConnection=$CONNECTION_STRING" \
    --output none

echo -e "${GREEN}✓ Connection String configurada${NC}\n"

echo -e "${YELLOW}Etapa 10: Configurando App Settings...${NC}"
az webapp config appsettings set \
    --resource-group $RESOURCE_GROUP \
    --name $WEB_APP_NAME \
    --settings \
        ASPNETCORE_ENVIRONMENT="Production" \
        WEBSITE_RUN_FROM_PACKAGE="0" \
        SCM_DO_BUILD_DURING_DEPLOYMENT="false" \
    --output none
echo -e "${GREEN}✓ App Settings configurado${NC}\n"

# --- Deploy da Aplicação ---

echo -e "${YELLOW}Etapa 11: Preparando projeto .NET...${NC}"

API_PROJECT=$(find . \( -path "*/src/Api/*.csproj" -o -path "*/Api/*.csproj" \) | head -n 1)
PROJECT_FILE=$(find . \( -maxdepth 2 -name "*.csproj" -o -maxdepth 2 -name "*.sln" \) | head -n 1)

if [ -z "$PROJECT_FILE" ]; then
    echo "Nenhum projeto encontrado. Clonando repositório..."
    
    if [ -d "challenge-moto-connect" ]; then
        echo "Removendo clone anterior..."
        rm -rf challenge-moto-connect
    fi
    
    git clone https://github.com/mateush-souza/challenge-moto-connect.git
    cd challenge-moto-connect
    
    API_PROJECT=$(find . \( -path "*/src/Api/*.csproj" -o -path "*/Api/*.csproj" \) | head -n 1)
    PROJECT_FILE=$(find . \( -maxdepth 2 -name "*.csproj" -o -maxdepth 2 -name "*.sln" \) | head -n 1)
    
    if [ -z "$PROJECT_FILE" ]; then
        echo -e "${YELLOW}Erro: Não foi possível encontrar projeto .NET no repositório clonado.${NC}"
        exit 1
    fi                                                                  
fi

if [ -n "$API_PROJECT" ]; then
    PROJECT_FILE="$API_PROJECT"
    echo "Projeto da API encontrado: $PROJECT_FILE"
elif [[ "$PROJECT_FILE" == *.sln ]]; then
    API_PROJECT=$(find . \( -path "*/src/Api/*.csproj" -o -path "*/Api/*.csproj" \) | head -n 1)
    if [ -n "$API_PROJECT" ]; then
        PROJECT_FILE="$API_PROJECT"
        echo "Projeto da API encontrado: $PROJECT_FILE"
    else
        echo "Solução encontrada: $PROJECT_FILE"
        echo -e "${YELLOW}Aviso: Tentando publicar a solução completa...${NC}"
    fi
else
    echo "Projeto encontrado: $PROJECT_FILE"
fi

echo -e "${YELLOW}Compilando aplicação...${NC}"
dotnet publish "$PROJECT_FILE" -c Release -o ./publish
echo -e "${GREEN}✓ Aplicação compilada${NC}\n"

echo -e "${YELLOW}Etapa 12: Fazendo deploy para Azure...${NC}"
rm -f app.zip
python3 - <<'PYTHON'
import os
import zipfile

with zipfile.ZipFile('app.zip', 'w', zipfile.ZIP_DEFLATED) as archive:
    for root, _, files in os.walk('publish'):
        for name in files:
            path = os.path.join(root, name)
            archive.write(path, os.path.relpath(path, 'publish'))
PYTHON

az webapp deployment source config-zip \
    --resource-group $RESOURCE_GROUP \
    --name $WEB_APP_NAME \
    --src app.zip

rm -f app.zip

echo -e "${GREEN}✓ Deploy concluído${NC}\n"

echo -e "${YELLOW}Etapa 12.1: Aplicando migrations no banco de dados...${NC}"
echo "Instalando dotnet-ef globalmente (se necessário)..."
dotnet tool install --global dotnet-ef 2>/dev/null || dotnet tool update --global dotnet-ef

echo "Aplicando migrations no banco de dados Azure..."
CONNECTION_STRING_MIGRATION="Server=tcp:${DB_SERVER_NAME}.database.windows.net,1433;Initial Catalog=${DB_NAME};Persist Security Info=False;User ID=${DB_ADMIN_USER};Password=${DB_ADMIN_PASSWORD};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

cd "$PROJECT_FILE" && cd ..
dotnet ef database update --connection "$CONNECTION_STRING_MIGRATION" --project "$PROJECT_FILE"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Migrations aplicadas com sucesso${NC}\n"
else
    echo -e "${YELLOW}⚠ Erro ao aplicar migrations. Continuando...${NC}\n"
fi

cd - > /dev/null
rm -rf ./publish

echo -e "${GREEN}✓ Banco de dados configurado${NC}\n"

echo -e "${YELLOW}Etapa 13: Reiniciando Web App...${NC}"
az webapp restart \
    --resource-group $RESOURCE_GROUP \
    --name $WEB_APP_NAME \
    --output none

sleep 5

echo -e "${GREEN}✓ Web App reiniciado${NC}\n"

# --- Finalização ---

WEBAPP_URL="https://$WEB_APP_NAME.azurewebsites.net"

echo -e "${GREEN}======================================================${NC}"
echo -e "${GREEN}DEPLOY CONCLUÍDO COM SUCESSO!${NC}"
echo -e "${GREEN}======================================================${NC}"
echo -e "URL da Aplicação: ${YELLOW}$WEBAPP_URL${NC}"
echo -e "Servidor SQL: ${YELLOW}${DB_SERVER_NAME}.database.windows.net${NC}"
echo -e "Banco de Dados: ${YELLOW}$DB_NAME${NC}"
echo -e "Usuário: ${YELLOW}$DB_ADMIN_USER${NC}"
echo ""
echo "Aguarde 2-3 minutos para a aplicação iniciar completamente."
echo -e "${GREEN}======================================================${NC}"
