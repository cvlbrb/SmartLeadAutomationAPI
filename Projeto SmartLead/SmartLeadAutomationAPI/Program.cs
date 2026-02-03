using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using SmartLeadAutomation.Data;
using SmartLeadAutomation.Services;
using System.Reflection;

namespace SmartLeadAutomation
{
    /// <summary>
    /// Classe principal de inicialização da aplicação SmartLead Automation API.
    /// Configura serviços, middleware e pipeline de requisições HTTP.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Ponto de entrada da aplicação
        /// </summary>
        public static void Main(string[] args)
        {
            // Configura o Serilog para logging estruturado
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/smartlead-api-.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Iniciando SmartLead Automation API...");

                var builder = WebApplication.CreateBuilder(args);

                // Configura logging com Serilog
                builder.Host.UseSerilog();

                // Configura serviços da aplicação
                ConfigureServices(builder.Services, builder.Configuration);

                var app = builder.Build();

                // Configura pipeline de middleware
                ConfigureMiddleware(app);

                // Inicializa o banco de dados
                InitializeDatabase(app);

                // Inicia a aplicação
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Falha crítica na inicialização da aplicação");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Configura os serviços da aplicação (DI container)
        /// </summary>
        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            #region Database Configuration

            // Configura Entity Framework Core com SQLite
            services.AddDbContext<SmartLeadDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseSqlite(connectionString);

                // Habilita logging detalhado em modo de desenvolvimento
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            #endregion

            #region Business Services

            // Registra o serviço de leads (Scoped = uma instância por requisição)
            services.AddScoped<ILeadService, LeadService>();

            #endregion

            #region API Configuration

            // Configura controllers com opções de JSON
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Serializa enums como strings (mais legível na API)
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                    // Ignora propriedades nulas na resposta
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                    // Formatação identada para facilitar leitura
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            // Configura API Explorer para descoberta de endpoints
            services.AddEndpointsApiExplorer();

            #endregion

            #region Swagger Configuration

            // Configura Swagger com documentação detalhada
            services.AddSwaggerGen(options =>
            {
                // Informações básicas da API
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SmartLead Automation API",
                    Version = "v1",
                    Description = @"API RESTful para gestão inteligente e automação de leads.

## Funcionalidades Principais

- **CRUD Completo de Leads**: Criação, consulta, atualização e exclusão de leads
- **Classificação Automática**: Priorização inteligente baseada em múltiplos critérios
- **Validação de Dados**: Validação de email e prevenção de duplicatas
- **Estatísticas em Tempo Real**: Dashboards e relatórios de desempenho
- **Filtros Avançados**: Busca por múltiplos critérios com paginação

## Autenticação

Esta versão da API não requer autenticação. Futuras versões incluirão JWT.

## Rate Limiting

Limite de 1000 requisições por minuto por IP.

## Suporte

Para suporte, entre em contato: suporte@smartlead.com",
                    Contact = new OpenApiContact
                    {
                        Name = "SmartLead Team",
                        Email = "suporte@smartlead.com",
                        Url = new Uri("https://smartlead.com/support")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });

                // Inclui comentários XML na documentação do Swagger
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                // Habilita anotações Swagger
                options.EnableAnnotations();

                // Configura esquema de segurança (preparado para JWT futuro)
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            #endregion

            #region CORS Configuration

            // Configura CORS para permitir requisições do frontend
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:3000",    // React/Vue/Angular dev server
                            "http://localhost:4200",    // Angular
                            "http://localhost:8080",    // Vue
                            "https://smartlead.com"     // Produção
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            #endregion

            #region Health Checks

            // Adiciona health checks para monitoramento
            services.AddHealthChecks()
                .AddDbContextCheck<SmartLeadDbContext>("database", tags: new[] { "db", "sqlite" });

            #endregion

            #region Compression

            // Habilita compressão de respostas
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            #endregion
        }

        /// <summary>
        /// Configura o pipeline de middleware da aplicação
        /// </summary>
        private static void ConfigureMiddleware(WebApplication app)
        {
            // Redireciona requisições HTTP para HTTPS
            app.UseHttpsRedirection();

            // Habilita compressão de respostas
            app.UseResponseCompression();

            // Configura CORS
            app.UseCors("AllowFrontend");

            // Configura Swagger em todos os ambientes
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartLead Automation API v1");
                options.RoutePrefix = "swagger"; // Acessível em /swagger
                options.DocumentTitle = "SmartLead API Documentation";
                options.DefaultModelsExpandDepth(2);
                options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
                options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
                options.DisplayRequestDuration();
                options.EnableDeepLinking();
                options.EnableFilter();
            });

            // Redireciona raiz para Swagger
            app.MapGet("/", () => Results.Redirect("/swagger"));

            // Health check endpoint
            app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            exception = e.Value.Exception?.Message,
                            duration = e.Value.Duration.ToString()
                        }),
                        totalDuration = report.TotalDuration.ToString()
                    };
                    await context.Response.WriteAsJsonAsync(response);
                }
            });

            // Endpoint de readiness (para Kubernetes)
            app.MapHealthChecks("/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("db")
            });

            // Endpoint de liveness (para Kubernetes)
            app.MapHealthChecks("/live");

            // Adiciona middleware de autorização (preparado para futura implementação)
            app.UseAuthorization();

            // Mapeia controllers
            app.MapControllers();
        }

        /// <summary>
        /// Inicializa o banco de dados com migrations e seed data
        /// </summary>
        private static void InitializeDatabase(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<SmartLeadDbContext>();
                var logger = services.GetRequiredService<ILogger<Program>>();

                logger.LogInformation("Inicializando banco de dados...");

                // Garante que o diretório do banco de dados existe
                var dbPath = context.Database.GetDbConnection().DataSource;
                if (!string.IsNullOrEmpty(dbPath))
                {
                    var dbDirectory = Path.GetDirectoryName(dbPath);
                    if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
                    {
                        Directory.CreateDirectory(dbDirectory);
                        logger.LogInformation("Diretório do banco de dados criado: {Directory}", dbDirectory);
                    }
                }

                // Aplica migrations pendentes
                context.Database.Migrate();

                logger.LogInformation("Banco de dados inicializado com sucesso");
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Erro ao inicializar banco de dados");
                throw;
            }
        }
    }
}
