#!/bin/bash

# Script para executar a SmartLead Automation API
# Autor: SmartLead Team
# Data: Janeiro 2024

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Função para imprimir mensagens coloridas
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Verifica se o .NET SDK está instalado
if ! command -v dotnet &> /dev/null; then
    print_error ".NET SDK não encontrado!"
    echo "Por favor, instale o .NET 8.0 SDK: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

# Exibe versão do .NET
DOTNET_VERSION=$(dotnet --version)
print_info ".NET SDK Version: $DOTNET_VERSION"

# Verifica se é .NET 8.0 ou superior
if [[ ! "$DOTNET_VERSION" =~ ^8\.[0-9]+ ]]; then
    print_warning "Versão recomendada: .NET 8.0 ou superior"
fi

echo ""
print_info "Iniciando SmartLead Automation API..."
echo ""

# Restaura pacotes NuGet
print_info "Restaurando pacotes NuGet..."
if dotnet restore; then
    print_success "Pacotes restaurados com sucesso!"
else
    print_error "Falha ao restaurar pacotes!"
    exit 1
fi

echo ""

# Verifica se existe banco de dados
DB_PATH="Data/Database/smartlead.db"
if [ -f "$DB_PATH" ]; then
    print_info "Banco de dados encontrado: $DB_PATH"
else
    print_warning "Banco de dados não encontrado. Será criado automaticamente."
fi

echo ""

# Compila o projeto
print_info "Compilando projeto..."
if dotnet build --no-restore --configuration Release; then
    print_success "Projeto compilado com sucesso!"
else
    print_error "Falha na compilação!"
    exit 1
fi

echo ""
print_info "Iniciando aplicação..."
echo ""
print_info "URLs da aplicação:"
echo "  • API Base:       https://localhost:7001/api"
echo "  • Swagger UI:     https://localhost:7001/swagger"
echo "  • Health Check:   https://localhost:7001/health"
echo ""
print_info "Pressione Ctrl+C para parar a aplicação"
echo ""

# Executa a aplicação
dotnet run --no-build --configuration Release

# Código de saída
EXIT_CODE=$?

if [ $EXIT_CODE -eq 0 ]; then
    echo ""
    print_success "Aplicação encerrada com sucesso!"
else
    echo ""
    print_error "Aplicação encerrada com erro (código: $EXIT_CODE)"
fi

exit $EXIT_CODE
