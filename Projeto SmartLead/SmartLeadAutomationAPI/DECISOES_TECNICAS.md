# Decisões Técnicas - SmartLead Automation API

Este documento detalha as decisões técnicas tomadas durante o desenvolvimento da SmartLead Automation API, explicando o raciocínio por trás de cada escolha.

---

## 1. Arquitetura em Camadas

### Decisão
Adotamos uma arquitetura em camadas com separação clara entre:
- **Presentation Layer** (Controllers)
- **Business Layer** (Services)
- **Data Layer** (DbContext + EF Core)
- **Database Layer** (SQLite)

### Justificativa
```
┌─────────────────────────────────────┐
│  Presentation (Controllers)         │  ← Recebe requisições HTTP
│  - Validação de entrada             │  - Retorna respostas HTTP
│  - Mapeamento DTO → Service         │  - Sem lógica de negócio
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│  Business (Services)                │  ← Contém toda a lógica
│  - Regras de classificação          │  - Regras de validação
│  - Cálculo de score                 │  - Orquestração
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│  Data (DbContext)                   │  ← Acesso a dados
│  - Entity Framework Core            │  - Query optimization
│  - Migrations                       │  - Seed data
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│  Database (SQLite)                  │  ← Persistência
│  - Índices otimizados               │  - Soft delete
└─────────────────────────────────────┘
```

**Benefícios:**
- **Testabilidade**: Cada camada pode ser testada com mocks
- **Manutenibilidade**: Mudanças isoladas não afetam outras camadas
- **Escalabilidade**: Facilidade para extrair serviços no futuro

---

## 2. Entity Framework Core com SQLite

### Decisão
Usar EF Core 8.0 com SQLite como banco de dados.

### Justificativa

#### Por que SQLite?
| Aspecto | SQLite | SQL Server | PostgreSQL |
|---------|--------|------------|------------|
| **Setup** | Zero config | Requer instalação | Requer instalação |
| **Portabilidade** | Arquivo único | Servidor dedicado | Servidor dedicado |
| **Custo** | Gratuito | Licença/caros | Gratuito |
| **Performance (leitura)** | Excelente | Excelente | Excelente |
| **Volume de dados** | Até 100k | Ilimitado | Ilimitado |
| **Concorrência** | Boa | Excelente | Excelente |

**SQLite é ideal para:**
- MVPs e protótipos
- Aplicações single-server
- Cargas de leitura intensiva
- Ambientes com recursos limitados

#### Por que EF Core?
```csharp
// Com EF Core (limpo e type-safe)
var leads = await _context.Leads
    .Where(l => l.Priority == PriorityLevel.Alta)
    .OrderByDescending(l => l.Score)
    .ToListAsync();

// Com Dapper/ADO.NET (mais verboso)
var sql = "SELECT * FROM Leads WHERE Priority = @priority ORDER BY Score DESC";
var leads = await connection.QueryAsync<Lead>(sql, new { priority = 3 });
```

**Benefícios do EF Core:**
- Migrations versionadas
- Change tracking automático
- Lazy/Eager loading
- Query optimization

---

## 3. Classificação Automática de Prioridade

### Decisão
Implementar algoritmo de pontuação ponderada (0-100) com 7 critérios.

### Justificativa

#### Por que Pontuação Numérica?
```
┌────────────────────────────────────────────────────────┐
│  Lead Score: 88/100                                    │
│  ████████████████████████████████████████████░░░░░░░   │
│                                                        │
│  Breakdown:                                            │
│  • Valor Estimado:    30/30 pts  ████████████████████  │
│  • Origem (LinkedIn): 18/20 pts  ██████████████████░░  │
│  • Dados Completos:   20/20 pts  ████████████████████  │
│  • Engajamento:       10/20 pts  ██████████░░░░░░░░░░  │
│  • Recência:          10/10 pts  ████████████████████  │
│  • Cargo (Diretor):   10/10 pts  ████████████████████  │
│  • Bônus Indicação:   10/10 pts  ████████████████████  │
└────────────────────────────────────────────────────────┘
```

**Vantagens:**
1. **Transparência**: Usuário entende por que um lead é prioritário
2. **Configurabilidade**: Pesos ajustáveis via `appsettings.json`
3. **Comparabilidade**: Facilidade para comparar leads
4. **Evolução**: Novos critérios podem ser adicionados

#### Critérios Escolhidos
Baseados em frameworks consagrados (HubSpot Lead Scoring, Salesforce Einstein):

| Critério | Peso | Racional |
|----------|------|----------|
| Valor Estimado | 0-30 | Leads de alto valor justificam mais atenção |
| Origem | 0-20 | Alguns canais têm maior qualidade |
| Dados Completos | 0-20 | Leads completos demonstram maior interesse |
| Engajamento | 0-20 | Respostas indicam interesse real |
| Recência | 0-10 | Leads recentes têm maior conversão |
| Cargo | 0-10 | Cargos de decisão convertem mais |
| Bônus Indicação | +10 | Referências têm taxa 4x maior de conversão |

---

## 4. Data Transfer Objects (DTOs)

### Decisão
Criar DTOs separados para cada operação (Create, Update, Response).

### Justificativa

#### Por que não expor Entidades diretamente?
```csharp
// ❌ PROBLEMA: Expor entidade diretamente
public class Lead  // Entidade
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string? IpAddress { get; set; }  // Sensível!
    public bool IsActive { get; set; }
}

// POST /api/leads retorna a entidade completa
// Inclui IpAddress e outros campos sensíveis!
```

```csharp
// ✅ SOLUÇÃO: DTO específico para resposta
public class LeadResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Priority { get; set; }
    public int Score { get; set; }
    // IpAddress NÃO está aqui!
}
```

#### DTOs Criados
| DTO | Propósito | Campos |
|-----|-----------|--------|
| `CreateLeadDto` | Criar lead | Campos necessários para criação |
| `UpdateLeadDto` | Atualizar lead | Campos modificáveis |
| `LeadResponseDto` | Resposta da API | Campos seguros para exposição |
| `LeadFilterDto` | Filtros de busca | Parâmetros de query |
| `LeadStatisticsDto` | Estatísticas | Métricas agregadas |

**Benefícios:**
- **Segurança**: Campos sensíveis não expostos
- **Validação**: Data Annotations nos DTOs
- **Documentação**: Swagger gera schemas precisos
- **Flexibilidade**: API evolui independente do modelo

---

## 5. Soft Delete (Exclusão Lógica)

### Decisão
Implementar soft delete ao invés de hard delete.

### Justificativa

#### Implementação
```csharp
public class Lead
{
    public bool IsActive { get; set; } = true;  // Flag de exclusão
    public DateTime? DeletedAt { get; set; }    // Quando foi excluído
    
    public void SoftDelete()
    {
        IsActive = false;
        DeletedAt = DateTime.UtcNow;
    }
    
    public void Restore()
    {
        IsActive = true;
        DeletedAt = null;
    }
}

// Query filter global (não retorna inativos por padrão)
modelBuilder.Entity<Lead>().HasQueryFilter(e => e.IsActive);
```

#### Por que Soft Delete?
| Aspecto | Soft Delete | Hard Delete |
|---------|-------------|-------------|
| **Recuperação** | ✅ Possível | ❌ Impossível |
| **Auditoria** | ✅ Histórico preservado | ❌ Dados perdidos |
| **Relatórios** | ✅ Dados históricos | ❌ Estatísticas quebradas |
| **LGPD** | ✅ Marcação lógica | ⚠️ Pode violar retenção |
| **Storage** | ⚠️ Maior uso | ✅ Menor uso |

**Quando usar Hard Delete:**
- Dados temporários (logs, caches)
- Explicitamente requerido por regulamentação
- Backup confiável e testado

---

## 6. Paginação Server-Side

### Decisão
Implementar paginação no banco de dados com `Skip/Take`.

### Justificativa

#### Por que não paginar na memória?
```csharp
// ❌ PROBLEMA: Paginação na memória
var allLeads = await _context.Leads.ToListAsync();  // Carrega TUDO!
var page = allLeads.Skip(100).Take(20).ToList();     // Depois filtra
// Para 100k leads: ~50MB de RAM, lento
```

```csharp
// ✅ SOLUÇÃO: Paginação no banco
var page = await _context.Leads
    .Skip(100)   // OFFSET 100
    .Take(20)    // LIMIT 20
    .ToListAsync();
// SQL gerado: SELECT ... LIMIT 20 OFFSET 100
// Para 100k leads: ~2KB de RAM, rápido
```

#### Implementação
```csharp
public class PagedResponseDto<T>
{
    public List<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
```

**Benefícios:**
- **Performance**: Menos dados transferidos
- **Escalabilidade**: Funciona com milhões de registros
- **UX**: Carregamento rápido
- **Mobile**: Essencial para conexões lentas

---

## 7. Validação em Múltiplas Camadas

### Decisão
Validar dados em DTOs (Data Annotations) E no serviço.

### Justificativa

#### Camadas de Validação
```
┌─────────────────────────────────────────┐
│  1. Cliente (JavaScript/React/Vue)      │  ← UX imediata
│     - Required fields                   │
│     - Formatos básicos                  │
└─────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────┐
│  2. Controller (ModelState)             │  ← Data Annotations
│     - [Required], [EmailAddress]        │
│     - [Range], [MaxLength]              │
└─────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────┐
│  3. Serviço (Regras de Negócio)         │  ← Lógica complexa
│     - Email duplicado                   │
│     - Validações de domínio             │
└─────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────┐
│  4. Banco de Dados (Constraints)        │  ← Última linha de defesa
│     - UNIQUE indexes                    │
│     - NOT NULL                          │
└─────────────────────────────────────────┘
```

**Por que múltiplas camadas?**
- **Defense in depth**: Falha em uma camada não compromete o sistema
- **UX**: Feedback imediato ao usuário
- **Segurança**: Cliente não é confiável
- **Integridade**: Banco garante consistência

---

## 8. Swagger/OpenAPI

### Decisão
Usar Swashbuckle.AspNetCore para documentação automática.

### Justificativa

#### Benefícios
```yaml
# Documentação gerada automaticamente:
paths:
  /api/leads:
    post:
      summary: "Criar lead"
      description: "Cria um novo lead com classificação automática"
      requestBody:
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/CreateLeadDto'
      responses:
        201:
          description: "Lead criado"
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/LeadResponseDto'
```

**Funcionalidades:**
- 📖 Documentação interativa (Swagger UI)
- 🧪 Testes direto na interface
- 💻 Geração de clientes (TypeScript, C#, Python)
- 🔒 Documentação de autenticação
- 📝 Exemplos de requisições/respostas

---

## 9. Logging Estruturado com Serilog

### Decisão
Usar Serilog ao invés do logging built-in do ASP.NET Core.

### Justificativa

#### Comparação
```csharp
// Built-in (simples)
_logger.LogInformation("Lead criado: {Id}", leadId);
// Output: "Lead criado: 123"

// Serilog (estruturado)
_logger.Information("Lead criado: {@Lead}", lead);
// Output JSON:
// {
//   "Timestamp": "2024-01-22T10:30:00Z",
//   "Level": "Information",
//   "Message": "Lead criado",
//   "Properties": {
//     "Lead": { "Id": 123, "Name": "João", "Score": 85 }
//   }
// }
```

**Benefícios:**
- **Busca**: Fácil filtrar logs no Elasticsearch/Splunk
- **Contexto**: Todas as propriedades disponíveis
- **Sinks**: Console, arquivo, cloud (AWS CloudWatch, Azure)
- **Enrichment**: Informações de request ID, usuário, etc.

---

## 10. Injeção de Dependência Nativa

### Decisão
Usar DI container built-in do ASP.NET Core.

### Justificativa

#### Por que não usar outro container?
| Container | Performance | Funcionalidades | Curva de Aprendizado |
|-----------|-------------|-----------------|---------------------|
| **Built-in** | ⭐⭐⭐ Excelente | Básicas | Baixa |
| Autofac | ⭐⭐ Boa | Avançadas | Média |
| Unity | ⭐⭐ Boa | Avançadas | Média |
| Ninject | ⭐ Regular | Avançadas | Média |

**Built-in é suficiente para:**
- 99% dos cenários comuns
- Scoped/Transient/Singleton lifetimes
- Constructor injection
- Service location (quando necessário)

```csharp
// Registro
builder.Services.AddScoped<ILeadService, LeadService>();

// Uso (automático)
public class LeadsController : ControllerBase
{
    private readonly ILeadService _leadService;
    
    public LeadsController(ILeadService leadService)  // Injetado automaticamente
    {
        _leadService = leadService;
    }
}
```

---

## 11. Configuração via appsettings.json

### Decisão
Parametrizar regras de negócio no arquivo de configuração.

### Justificativa

#### Exemplo
```json
{
  "LeadClassificationRules": {
    "HighPriorityScoreThreshold": 80,
    "MediumPriorityScoreThreshold": 50,
    "HighValueThreshold": 10000,
    "MediumValueThreshold": 5000,
    "DaysToConsiderRecent": 7
  }
}
```

```csharp
// Uso no código
var highThreshold = _configuration
    .GetValue<int>("LeadClassificationRules:HighPriorityScoreThreshold", 80);
```

**Benefícios:**
- **Flexibilidade**: Ajustar sem recompilar
- **A/B Testing**: Diferentes configurações por ambiente
- **Manutenção**: Regras de negócio centralizadas
- **Deploy**: Configurações específicas por ambiente

---

## 12. Índices de Banco de Dados

### Decisão
Criar índices otimizados para consultas frequentes.

### Justificativa

#### Índices Criados
```csharp
// Índice único para email (evita duplicatas + busca rápida)
entity.HasIndex(e => e.Email).IsUnique();

// Índice para ExternalId (integração com CRMs)
entity.HasIndex(e => e.ExternalId).IsUnique();

// Índice composto para filtros comuns
entity.HasIndex(e => new { e.Priority, e.Status, e.IsActive });

// Índice para ordenação por data
entity.HasIndex(e => e.CreatedAt);

// Índice para agrupamento por origem
entity.HasIndex(e => e.Source);
```

**Impacto na Performance:**
| Consulta | Sem Índice | Com Índice | Melhoria |
|----------|-----------|-----------|----------|
| Busca por email | O(n) - Full scan | O(log n) | 1000x |
| Filtro por prioridade | O(n) | O(log n) | 100x |
| Ordenação por data | O(n log n) | O(n) | 10x |

---

## 13. Seed Data para Desenvolvimento

### Decisão
Incluir dados iniciais no DbContext para facilitar desenvolvimento.

### Justificativa

#### Benefícios
- **Onboarding**: Novos devs têm dados para testar imediatamente
- **Demonstrações**: Clientes veem a aplicação com dados realistas
- **Testes**: Dados consistentes para testes automatizados
- **Documentação**: Screenshots com dados significativos

```csharp
entity.HasData(
    new Lead { Id = 1, Name = "João Silva", Priority = PriorityLevel.Alta, ... },
    new Lead { Id = 2, Name = "Maria Santos", Priority = PriorityLevel.Media, ... },
    // ...
);
```

---

## 14. Async/Await em Todo o Pipeline

### Decisão
Usar operações assíncronas de ponta a ponta.

### Justificativa

#### Por que Async?
```
Síncrono (bloqueante):
┌─────────────────────────────────────────────────────────────┐
│ Thread 1: [====DB Query====][Process][Response]             │
│ Thread 2: [====DB Query====][Process][Response]             │
│ Thread 3: [====DB Query====][Process][Response]             │
│                                                             │
│ 100 threads para 100 requests (escalabilidade limitada)     │
└─────────────────────────────────────────────────────────────┘

Assíncrono (não-bloqueante):
┌─────────────────────────────────────────────────────────────┐
│ Thread 1: [DB Query][Process][Response]                     │
│ Thread 2: [DB Query][Process][Response]                     │
│ Thread 3: [DB Query][Process][Response]                     │
│ ...                                                         │
│ 10 threads para 1000 requests (alta escalabilidade)         │
└─────────────────────────────────────────────────────────────┘
```

**Regras:**
- ✅ Métodos assíncronos terminam com "Async"
- ✅ Usar `async Task` ao invés de `void`
- ✅ Propagar `await` em toda a cadeia
- ❌ Nunca usar `.Result` ou `.Wait()`

---

## 15. Tratamento de Erros Global

### Decisão
Implementar tratamento de erros consistente na API.

### Justificativa

#### Estrutura de Resposta
```json
{
  "success": false,
  "message": "Erro de validação",
  "errors": ["Email já cadastrado", "Valor inválido"],
  "statusCode": 400,
  "timestamp": "2024-01-22T10:30:00Z",
  "requestId": "abc123"
}
```

**Benefícios:**
- **Consistência**: Clientes sabem o que esperar
- **Debugging**: Request ID para rastreamento
- **UX**: Mensagens amigáveis para usuários
- **Logs**: Informações suficientes para investigação

---

## Conclusão

Estas decisões técnicas foram tomadas considerando:

1. **Manutenibilidade**: Código fácil de entender e modificar
2. **Escalabilidade**: Arquitetura que suporta crescimento
3. **Performance**: Otimizações onde importam
4. **Segurança**: Múltiplas camadas de proteção
5. **Developer Experience**: Facilidade para desenvolver e testar

O projeto está preparado para evoluir com:
- Autenticação JWT
- Cache distribuído (Redis)
- Filas de processamento (RabbitMQ/Azure Service Bus)
- Testes automatizados (xUnit, Moq)
- CI/CD (GitHub Actions, Azure DevOps)

---

**Documento mantido pela SmartLead Team**
Última atualização: Janeiro 2024
