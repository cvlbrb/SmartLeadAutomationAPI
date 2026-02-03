using Microsoft.EntityFrameworkCore;
using SmartLeadAutomation.Models.Entities;

namespace SmartLeadAutomation.Data
{
    /// <summary>
    /// Contexto do Entity Framework Core para o SmartLead Automation API.
    /// Gerencia todas as entidades e configurações do banco de dados SQLite.
    /// </summary>
    public class SmartLeadDbContext : DbContext
    {
        /// <summary>
        /// Construtor com opções de configuração
        /// </summary>
        public SmartLeadDbContext(DbContextOptions<SmartLeadDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// DbSet para a entidade Lead
        /// </summary>
        public DbSet<Lead> Leads { get; set; } = null!;

        /// <summary>
        /// Configurações adicionais do modelo
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da entidade Lead
            modelBuilder.Entity<Lead>(entity =>
            {
                // Nome da tabela
                entity.ToTable("Leads");

                // Chave primária
                entity.HasKey(e => e.Id);

                // Índice único para email (evita duplicatas)
                entity.HasIndex(e => e.Email)
                      .IsUnique()
                      .HasDatabaseName("IX_Leads_Email_Unique");

                // Índice para busca por nome
                entity.HasIndex(e => e.Name)
                      .HasDatabaseName("IX_Leads_Name");

                // Índice para ExternalId (integração com outros sistemas)
                entity.HasIndex(e => e.ExternalId)
                      .IsUnique()
                      .HasDatabaseName("IX_Leads_ExternalId_Unique")
                      .HasFilter("[ExternalId] IS NOT NULL");

                // Índice composto para filtros comuns
                entity.HasIndex(e => new { e.Priority, e.Status, e.IsActive })
                      .HasDatabaseName("IX_Leads_Priority_Status_Active");

                // Índice para origem
                entity.HasIndex(e => e.Source)
                      .HasDatabaseName("IX_Leads_Source");

                // Índice para data de criação (ordenação)
                entity.HasIndex(e => e.CreatedAt)
                      .HasDatabaseName("IX_Leads_CreatedAt");

                // Configuração de propriedades
                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.Email)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.Phone)
                      .HasMaxLength(20);

                entity.Property(e => e.Company)
                      .HasMaxLength(200);

                entity.Property(e => e.JobTitle)
                      .HasMaxLength(100);

                entity.Property(e => e.ExternalId)
                      .HasMaxLength(100);

                entity.Property(e => e.EstimatedValue)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Notes)
                      .HasMaxLength(2000);

                entity.Property(e => e.Tags)
                      .HasMaxLength(500);

                entity.Property(e => e.SourceData)
                      .HasMaxLength(2000);

                entity.Property(e => e.IpAddress)
                      .HasMaxLength(45);

                entity.Property(e => e.UserAgent)
                      .HasMaxLength(500);

                // Filtro global para exclusão lógica (não retorna inativos por padrão)
                entity.HasQueryFilter(e => e.IsActive);

                // Dados iniciais (seed) para testes
                entity.HasData(
                    new Lead
                    {
                        Id = 1,
                        Name = "João Silva",
                        Email = "joao.silva@empresa.com",
                        Phone = "(11) 98765-4321",
                        Company = "Tech Solutions Brasil",
                        JobTitle = "Diretor de TI",
                        Priority = Models.Enums.PriorityLevel.Alta,
                        Score = 85,
                        Status = Models.Enums.LeadStatus.Qualificado,
                        Source = Models.Enums.LeadSource.LinkedIn,
                        EstimatedValue = 50000.00m,
                        ConversionProbability = 70,
                        MarketingConsent = true,
                        HasResponded = true,
                        InteractionCount = 3,
                        CreatedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
                        UpdatedAt = new DateTime(2024, 1, 20, 14, 15, 0, DateTimeKind.Utc),
                        LastContactDate = new DateTime(2024, 1, 20, 14, 15, 0, DateTimeKind.Utc),
                        Notes = "Interessado em soluções de automação. Orçamento aprovado.",
                        Tags = "enterprise,automation,hot",
                        IsActive = true
                    },
                    new Lead
                    {
                        Id = 2,
                        Name = "Maria Santos",
                        Email = "maria.santos@startup.com",
                        Phone = "(21) 97654-3210",
                        Company = "Startup Inovadora",
                        JobTitle = "CEO",
                        Priority = Models.Enums.PriorityLevel.Media,
                        Score = 65,
                        Status = Models.Enums.LeadStatus.EmQualificacao,
                        Source = Models.Enums.LeadSource.Website,
                        EstimatedValue = 15000.00m,
                        ConversionProbability = 45,
                        MarketingConsent = true,
                        HasResponded = false,
                        InteractionCount = 1,
                        CreatedAt = new DateTime(2024, 1, 18, 9, 0, 0, DateTimeKind.Utc),
                        Notes = "Buscando soluções para crescimento rápido.",
                        Tags = "startup,growth",
                        IsActive = true
                    },
                    new Lead
                    {
                        Id = 3,
                        Name = "Pedro Costa",
                        Email = "pedro.costa@consultoria.com",
                        Phone = "(31) 96543-2109",
                        Company = "Consultoria Estratégica",
                        JobTitle = "Gerente de Projetos",
                        Priority = Models.Enums.PriorityLevel.Baixa,
                        Score = 35,
                        Status = Models.Enums.LeadStatus.Novo,
                        Source = Models.Enums.LeadSource.Facebook,
                        EstimatedValue = 3000.00m,
                        ConversionProbability = 20,
                        MarketingConsent = false,
                        HasResponded = false,
                        InteractionCount = 0,
                        CreatedAt = new DateTime(2024, 1, 20, 16, 45, 0, DateTimeKind.Utc),
                        Notes = "Apenas pesquisando opções no mercado.",
                        IsActive = true
                    },
                    new Lead
                    {
                        Id = 4,
                        Name = "Ana Oliveira",
                        Email = "ana.oliveira@grandeempresa.com",
                        Phone = "(11) 95432-1098",
                        Company = "Grande Empresa S.A.",
                        JobTitle = "Diretora Comercial",
                        Priority = Models.Enums.PriorityLevel.Alta,
                        Score = 92,
                        Status = Models.Enums.LeadStatus.EmNegociacao,
                        Source = Models.Enums.LeadSource.Indicacao,
                        EstimatedValue = 120000.00m,
                        ConversionProbability = 85,
                        MarketingConsent = true,
                        HasResponded = true,
                        InteractionCount = 5,
                        CreatedAt = new DateTime(2024, 1, 10, 11, 0, 0, DateTimeKind.Utc),
                        UpdatedAt = new DateTime(2024, 1, 22, 10, 30, 0, DateTimeKind.Utc),
                        LastContactDate = new DateTime(2024, 1, 22, 10, 30, 0, DateTimeKind.Utc),
                        Notes = "Indicação de cliente atual. Negociação avançada.",
                        Tags = "enterprise,referral,vip",
                        IsActive = true
                    },
                    new Lead
                    {
                        Id = 5,
                        Name = "Carlos Mendes",
                        Email = "carlos.mendes@email.com",
                        Phone = "(41) 94321-0987",
                        Company = "Empresa Média Ltda",
                        JobTitle = "Analista de Sistemas",
                        Priority = Models.Enums.PriorityLevel.Media,
                        Score = 55,
                        Status = Models.Enums.LeadStatus.Novo,
                        Source = Models.Enums.LeadSource.GoogleAds,
                        EstimatedValue = 8000.00m,
                        ConversionProbability = 35,
                        MarketingConsent = true,
                        HasResponded = false,
                        InteractionCount = 0,
                        CreatedAt = new DateTime(2024, 1, 21, 8, 15, 0, DateTimeKind.Utc),
                        Notes = "Buscando informações sobre preços.",
                        Tags = "smb,price-sensitive",
                        IsActive = true
                    }
                );
            });
        }

        /// <summary>
        /// Configurações de conexão
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Configuração padrão para desenvolvimento
                optionsBuilder.UseSqlite("Data Source=smartlead.db");
            }

            // Habilitar logging detalhado em modo de desenvolvimento
#if DEBUG
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
#endif
        }

        /// <summary>
        /// Salva as alterações e atualiza automaticamente os timestamps
        /// </summary>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Salva as alterações de forma assíncrona com atualização de timestamps
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Atualiza automaticamente os timestamps de criação e modificação
        /// </summary>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries<Lead>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
