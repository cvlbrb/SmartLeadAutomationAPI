# Quick Start - SmartLead Automation API

Guia rápido para começar a usar a API em 5 minutos.

---

## ⚡ Início Rápido

### Opção 1: Usando o Script (Linux/Mac)

```bash
# Clone ou extraia o projeto
cd SmartLeadAutomationAPI

# Execute o script
./run.sh
```

### Opção 2: Comandos Manuais

```bash
# 1. Restaurar pacotes
dotnet restore

# 2. Compilar
dotnet build

# 3. Executar
dotnet run
```

### Opção 3: Visual Studio / VS Code

1. Abra a solução `SmartLeadAutomationAPI.sln`
2. Pressione `F5` ou clique em "Run"

---

## 🌐 Acessando a API

Após iniciar, acesse:

| URL | Descrição |
|-----|-----------|
| `https://localhost:7001/swagger` | **Documentação Interativa** |
| `https://localhost:7001/api/leads` | API de Leads |
| `https://localhost:7001/api/stats` | Estatísticas |
| `https://localhost:7001/health` | Health Check |

---

## 🧪 Testes Rápidos

### Criar um Lead (curl)

```bash
curl -X POST "https://localhost:7001/api/leads" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Teste Rápido",
    "email": "teste@exemplo.com",
    "source": "Website",
    "estimatedValue": 10000
  }'
```

### Listar Leads

```bash
curl "https://localhost:7001/api/leads"
```

### Ver Estatísticas

```bash
curl "https://localhost:7001/api/stats"
```

---

## 📁 Estrutura do Projeto

```
SmartLeadAutomationAPI/
├── Controllers/      # Endpoints da API
├── Services/         # Lógica de negócio
├── Models/           # Entidades e Enums
├── DTOs/             # Objetos de transferência
├── Data/             # Contexto do banco
├── appsettings.json  # Configurações
└── Program.cs        # Ponto de entrada
```

---

## 🔧 Comandos Úteis

### Entity Framework

```bash
# Criar migration
dotnet ef migrations add NomeMigration

# Aplicar migrations
dotnet ef database update

# Remover última migration
dotnet ef migrations remove
```

### Build e Testes

```bash
# Build de Release
dotnet build -c Release

# Executar tests (quando implementados)
dotnet test

# Publicar
dotnet publish -c Release -o ./publish
```

---

## 🐛 Troubleshooting

### Porta em uso
```bash
# Linux/Mac
lsof -i :7001
kill -9 <PID>

# Windows
netstat -ano | findstr :7001
taskkill /PID <PID> /F
```

### Certificado HTTPS
```bash
# Linux - Trust certificate
dotnet dev-certs https --trust
```

### Limpar e Reconstruir
```bash
dotnet clean
dotnet restore --force
dotnet build
```

---

## 📚 Próximos Passos

1. Explore a documentação Swagger
2. Teste os endpoints com o arquivo `requests.http`
3. Leia o `README.md` completo
4. Consulte `DECISOES_TECNICAS.md` para detalhes técnicos

---

**Pronto!** A API está rodando e pronta para uso! 🚀
