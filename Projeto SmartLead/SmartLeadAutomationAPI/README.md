# SmartLead Automation API

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=c-sharp)
![SQLite](https://img.shields.io/badge/SQLite-3-003B57?logo=sqlite)
![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?logo=swagger)
![License](https://img.shields.io/badge/License-MIT-yellow.svg)

**API RESTful para gestão inteligente e automação de leads com classificação automática de prioridade**

[Documentação](#documentação) • [Instalação](#instalação) • [Uso](#uso) • [API Endpoints](#api-endpoints) • [Arquitetura](#arquitetura)

</div>

---

## 📋 Sumário

- [Visão Geral](#visão-geral)
- [Funcionalidades](#funcionalidades)
- [Tecnologias](#tecnologias)
- [Instalação](#instalação)
- [Configuração](#configuração)
- [Uso](#uso)
- [API Endpoints](#api-endpoints)
- [Exemplos de Requisições](#exemplos-de-requisições)
- [Arquitetura](#arquitetura)
- [Decisões Técnicas](#decisões-técnicas)
- [Contribuição](#contribuição)
- [Licença](#licença)

---

## 🎯 Visão Geral

A **SmartLead Automation API** é uma solução completa para gestão de leads que automatiza o processo de classificação e priorização de potenciais clientes. A API utiliza algoritmos inteligentes para calcular a pontuação de cada lead com base em múltiplos critérios, permitindo que equipes de vendas foquem nos leads com maior potencial de conversão.

### Principais Diferenciais

- ✅ **Classificação Automática**: Priorização inteligente baseada em 7 critérios
- ✅ **Prevenção de Duplicatas**: Validação de email único com índices otimizados
- ✅ **Exclusão Lógica**: Soft delete para preservar histórico
- ✅ **Estatísticas em Tempo Real**: Dashboards com métricas de conversão
- ✅ **Documentação Interativa**: Swagger UI com exemplos completos

---

## ✨ Funcionalidades

### CRUD Completo de Leads
- Criar, ler, atualizar e excluir leads
- Validação completa de dados de entrada
- Paginação e filtros avançados
- Busca por nome, email ou empresa

### Classificação Automática de Prioridade
O sistema calcula uma pontuação (0-100) baseada em:

| Critério | Peso | Descrição |
|----------|------|-----------|
| Valor Estimado | 0-30 pts | Leads com maior valor recebem mais pontos |
| Origem | 0-20 pts | Indicações e LinkedIn têm maior valor |
| Dados Completos | 0-20 pts | Empresa, cargo, telefone e notas |
| Engajamento | 0-20 pts | Respostas, interações e consentimento |
| Recência | 0-10 pts | Leads mais recentes são priorizados |
| Cargo/Posição | 0-10 pts | Cargos de decisão (Diretor, CEO) |
| Bônus Indicação | +10 pts | Bônus especial para leads indicados |

**Classificação de Prioridade:**
- 🔴 **Alta**: Score ≥ 80
- 🟡 **Média**: Score ≥ 50
- 🟢 **Baixa**: Score < 50

### Validação e Integridade
- Validação de formato de email
- Prevenção de emails duplicados
- Índices únicos no banco de dados
- Tratamento de concorrência

### Estatísticas e Relatórios
- Total de leads por período
- Distribuição por prioridade
- Distribuição por origem
- Taxa de conversão
- Valor estimado total
- Pontuação média

---

## - Tecnologias

### Stack Principal
- **.NET 8.0** - Framework moderno e de alta performance
- **ASP.NET Core Web API** - Para construção de APIs RESTful
- **Entity Framework Core 8.0** - ORM para acesso a dados
- **SQLite** - Banco de dados leve e portátil

### Bibliotecas e Ferramentas
- **Swashbuckle.AspNetCore 6.5** - Documentação Swagger/OpenAPI
- **FluentValidation** - Validação de dados robusta
- **Serilog** - Logging estruturado
- **AutoMapper** - Mapeamento entre entidades e DTOs

### Padrões e Práticas
- **Arquitetura em Camadas** - Controllers, Services, Models, DTOs
- **Repository Pattern** - Via Entity Framework
- **Dependency Injection** - Injeção de dependências nativa
- **DTO Pattern** - Data Transfer Objects para comunicação
- **Soft Delete** - Exclusão lógica para auditoria

---

## - Instalação

### Pré-requisitos
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) ou superior
- [Git](https://git-scm.com/) (opcional)

### Passo a Passo

1. **Clone o repositório** (ou extraia os arquivos):
```bash
git clone https://github.com/seu-usuario/smartlead-automation-api.git
cd smartlead-automation-api
```

2. **Restaure as dependências**:
```bash
dotnet restore
```

3. **Execute as migrations** (criação do banco de dados):
```bash
dotnet ef database update
```

> - **Nota**: Se o comando `dotnet ef` não estiver disponível, instale com:
> ```bash
> dotnet tool install --global dotnet-ef
> ```

4. **Execute a aplicação**:
```bash
dotnet run
```

5. **Acesse a documentação**:
```
https://localhost:7001/swagger
```

---

## - Configuração

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Data/Database/smartlead.db"
  },
  "LeadClassificationRules": {
    "HighPriorityScoreThreshold": 80,
    "MediumPriorityScoreThreshold": 50,
    "HighValueThreshold": 10000,
    "MediumValueThreshold": 5000,
    "DaysToConsiderRecent": 7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `ASPNETCORE_ENVIRONMENT` | Ambiente (Development/Production) | Production |
| `ConnectionStrings__DefaultConnection` | String de conexão com SQLite | `Data Source=smartlead.db` |

---

## - Uso

### Acesso à Documentação

Após iniciar a aplicação, acesse:

- **Swagger UI**: `https://localhost:7001/swagger`
- **Health Check**: `https://localhost:7001/health`
- **API Base**: `https://localhost:7001/api`

### Estrutura de Pastas

```
SmartLeadAutomationAPI/
├── Controllers/          # Controllers da API
│   ├── LeadsController.cs
│   └── StatsController.cs
├── Data/                 # Contexto do EF Core
│   ├── Database/         # Arquivos SQLite
│   └── SmartLeadDbContext.cs
├── Models/               # Entidades e Enums
│   ├── Entities/         # Entidades do domínio
│   │   └── Lead.cs
│   └── Enums/            # Enumerações
│       ├── LeadSource.cs
│       ├── LeadStatus.cs
│       └── PriorityLevel.cs
├── DTOs/                 # Data Transfer Objects
│   ├── CreateLeadDto.cs
│   ├── UpdateLeadDto.cs
│   ├── LeadResponseDto.cs
│   ├── LeadFilterDto.cs
│   ├── LeadStatisticsDto.cs
│   ├── PagedResponseDto.cs
│   └── ApiResponseDto.cs
├── Services/             # Lógica de negócio
│   ├── ILeadService.cs
│   └── LeadService.cs
├── appsettings.json      # Configurações
├── Program.cs            # Ponto de entrada
└── README.md             # Este arquivo
```

---

## - API Endpoints

### Leads

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/api/leads` | Listar todos os leads (com filtros) |
| `GET` | `/api/leads/{id}` | Obter lead por ID |
| `GET` | `/api/leads/by-email/{email}` | Buscar lead por email |
| `GET` | `/api/leads/check-email/{email}` | Verificar se email existe |
| `POST` | `/api/leads` | Criar novo lead |
| `PUT` | `/api/leads/{id}` | Atualizar lead |
| `DELETE` | `/api/leads/{id}` | Remover lead (soft delete) |
| `POST` | `/api/leads/{id}/restore` | Restaurar lead removido |
| `PATCH` | `/api/leads/{id}/status` | Atualizar status |
| `POST` | `/api/leads/{id}/mark-responded` | Marcar como respondido |
| `POST` | `/api/leads/{id}/recalculate-priority` | Recalcular prioridade |

### Estatísticas

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/api/stats` | Estatísticas gerais |
| `GET` | `/api/stats/by-priority` | Distribuição por prioridade |
| `GET` | `/api/stats/by-source` | Distribuição por origem |
| `GET` | `/api/stats/dashboard` | Resumo para dashboard |
| `GET` | `/api/stats/conversion-metrics` | Métricas de conversão |

### Health Checks

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/health` | Status geral da aplicação |
| `GET` | `/ready` | Readiness probe (Kubernetes) |
| `GET` | `/live` | Liveness probe (Kubernetes) |

---

## - Exemplos de Requisições

### Criar um Lead

**Request:**
```bash
curl -X POST "https://localhost:7001/api/leads" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "João Silva",
    "email": "joao.silva@empresa.com",
    "phone": "(11) 98765-4321",
    "company": "Tech Solutions Brasil",
    "jobTitle": "Diretor de TI",
    "source": "LinkedIn",
    "estimatedValue": 50000,
    "marketingConsent": true,
    "notes": "Interessado em soluções de automação"
  }'
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Lead criado com sucesso",
  "data": {
    "id": 6,
    "name": "João Silva",
    "email": "joao.silva@empresa.com",
    "priority": "Alta",
    "score": 88,
    "status": "Novo",
    "source": "LinkedIn",
    "estimatedValue": "R$ 50.000,00",
    "priorityColor": "#dc3545"
  },
  "statusCode": 201,
  "timestamp": "2024-01-22T14:30:00Z"
}
```

### Listar Leads com Filtros

**Request:**
```bash
curl -X GET "https://localhost:7001/api/leads?priority=Alta&source=LinkedIn&pageSize=10" \
  -H "Accept: application/json"
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "name": "João Silva",
        "email": "joao.silva@empresa.com",
        "priority": "Alta",
        "score": 85,
        "status": "Qualificado"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 2,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

### Obter Estatísticas

**Request:**
```bash
curl -X GET "https://localhost:7001/api/stats" \
  -H "Accept: application/json"
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "totalLeads": 5,
    "activeLeads": 5,
    "newLeadsThisWeek": 3,
    "priorityDistribution": {
      "highPriority": 2,
      "mediumPriority": 2,
      "lowPriority": 1,
      "highPriorityPercentage": 40.0
    },
    "conversionRate": 20.0,
    "totalEstimatedValue": 188000.00,
    "averageScore": 66.4
  }
}
```

### Atualizar Status

**Request:**
```bash
curl -X PATCH "https://localhost:7001/api/leads/1/status?newStatus=EmNegociacao" \
  -H "Accept: application/json"
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Status atualizado para 'EmNegociacao'",
  "data": {
    "id": 1,
    "status": "EmNegociacao",
    "updatedAt": "2024-01-22T15:00:00Z"
  }
}
```

---

## - Arquitetura

### Diagrama de Camadas

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │   Swagger   │  │ Controllers │  │  Health Checks      │ │
│  │     UI      │  │  (REST API) │  │  (/health, /live)   │ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     Business Layer                           │
│  ┌───────────────────────────────────────────────────────┐  │
│  │              LeadService (ILeadService)                │  │
│  │  • Classificação automática                           │  │
│  │  • Regras de negócio                                  │  │
│  │  • Validações                                         │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      Data Layer                              │
│  ┌─────────────────────────┐  ┌───────────────────────────┐ │
│  │   SmartLeadDbContext    │  │    Entity Framework       │ │
│  │   (DbContext)           │  │    Core (SQLite)          │ │
│  └─────────────────────────┘  └───────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   Database Layer                             │
│              SQLite (smartlead.db)                           │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  • Tabela Leads (com índices otimizados)              │  │
│  │  • Soft delete (IsActive + DeletedAt)                 │  │
│  │  • Seed data para testes                              │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Fluxo de Classificação

```
┌──────────────┐
│  Criar Lead  │
└──────┬───────┘
       │
       ▼
┌──────────────────┐
│ Validar Email    │
│ (formato + duplicado)
└──────┬───────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│         Calcular Pontuação              │
│  ┌─────────┐ ┌─────────┐ ┌───────────┐ │
│  │  Valor  │ │ Origem  │ │   Dados   │ │
│  │Estimado │ │         │ │ Completos │ │
│  └────┬────┘ └────┬────┘ └─────┬─────┘ │
│       │           │            │       │
│       └───────────┴────────────┘       │
│                   │                    │
│                   ▼                    │
│  ┌─────────┐ ┌─────────┐ ┌───────────┐ │
│  │Engajamen│ │Recência │ │   Cargo   │ │
│  │   to    │ │         │ │           │ │
│  └─────────┘ └─────────┘ └───────────┘ │
└─────────────────────────────────────────┘
       │
       ▼
┌──────────────────┐
│ Score Total      │
│ (máx: 100 pts)   │
└──────┬───────────┘
       │
       ▼
┌──────────────────┐
│ Definir Prioridade│
│ • Alta (≥80)     │
│ • Média (≥50)    │
│ • Baixa (<50)    │
└──────┬───────────┘
       │
       ▼
┌──────────────────┐
│  Salvar no BD    │
└──────────────────┘
```

---

## - Decisões Técnicas

### 1. Por que SQLite?

**Vantagens para este projeto:**
- **Portabilidade**: Arquivo único, fácil de versionar e migrar
- **Zero configuração**: Não requer instalação de servidor
- **Performance**: Excelente para cargas de leitura intensiva
- **Custo**: Gratuito e open-source

**Quando migrar:**
- Volume > 100k leads
- Múltiplos servidores de aplicação
- Necessidade de replicação

### 2. Arquitetura em Camadas

**Separação de responsabilidades:**
- **Controllers**: Apenas recebem requisições e retornam respostas
- **Services**: Contêm toda a lógica de negócio e regras de classificação
- **Models**: Entidades puras sem lógica de negócio
- **DTOs**: Contratos de API desacoplados das entidades

**Benefícios:**
- Testabilidade: Cada camada pode ser testada isoladamente
- Manutenibilidade: Mudanças em uma camada não afetam as outras
- Escalabilidade: Facilidade para extrair serviços no futuro

### 3. Classificação Automática

**Por que pontuação ponderada?**
- Flexibilidade: Pesos podem ser ajustados sem mudar código
- Transparência: Score explicável para usuários
- Configurabilidade: Limites de prioridade via appsettings.json

**Critérios escolhidos:**
Baseados em frameworks de lead scoring (HubSpot, Salesforce):
- **Demográficos**: Cargo, empresa (fit com ICP)
- **Comportamentais**: Engajamento, recência
- **Contextuais**: Origem, valor estimado

### 4. Soft Delete vs Hard Delete

**Decisão**: Soft delete (exclusão lógica)

**Motivos:**
- Preservar histórico para auditoria
- Possibilidade de recuperação
- Manter integridade de relatórios históricos
- Conformidade com LGPD (dados não são perdidos, apenas marcados)

**Implementação:**
- Campo `IsActive` (bool)
- Campo `DeletedAt` (DateTime?)
- Query filter global no EF Core
- Endpoint de restore

### 5. DTOs vs Entidades Diretas

**Decisão**: Usar DTOs para todas as operações de API

**Benefícios:**
- Segurança: Campos sensíveis não expostos
- Flexibilidade: API pode evoluir independente do modelo
- Validação: Data Annotations nos DTOs, não nas entidades
- Documentação: Swagger gera schema mais preciso

### 6. Paginação com Filtros

**Implementação:**
- Padrão repository com `IQueryable`
- Filtros aplicados antes da materialização
- Paginação no banco (SQL LIMIT/OFFSET)
- Metadados de paginação na resposta

**Vantagens:**
- Performance: Menos dados transferidos
- UX: Carregamento rápido
- Escalabilidade: Funciona com milhões de registros

### 7. Logging com Serilog

**Por que Serilog ao invés de logging built-in?**
- Logging estruturado (JSON)
- Sinks múltiplos (console, arquivo, serviços externos)
- Enrichment (informações de contexto)
- Configuração fluente

---

## - Contribuição

Contribuições são bem-vindas! Siga os passos:

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/nova-feature`)
3. Commit suas mudanças (`git commit -m 'Adiciona nova feature'`)
4. Push para a branch (`git push origin feature/nova-feature`)
5. Abra um Pull Request

### Diretrizes de Código
- Siga as convenções de nomenclatura C#
- Adicione testes para novas funcionalidades
- Mantenha a cobertura de código acima de 80%
- Documente APIs com comentários XML

---

## 📄 Licença

Este projeto está licenciado sob a licença MIT - veja o arquivo [LICENSE](LICENSE) para detalhes.

```
MIT License

Copyright (c) 2024 SmartLead Team

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
```

---

## - Suporte

- **Email**: gabrielccavaloti@gmail.com


---

<div align="center">



[⬆ Voltar ao topo](#smartlead-automation-api)

</div>
