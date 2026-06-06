using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace EAuditoria.API.Dependencies;

public static class SwaggerDependencies
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title   = "e-Auditoria API",
                Version = "v1",
                Description = """
                    ## Painel de Obrigações Acessórias

                    API RESTful para gestão tributária de escritórios contábeis.
                    Permite cadastrar empresas, gerar calendários de obrigações fiscais,
                    registrar entregas e monitorar alertas de vencimento.

                    ### Regimes Tributários suportados
                    | Valor | Descrição |
                    |-------|-----------|
                    | `SimplesNacional` | ME/EPP com faturamento até R$ 4,8 mi |
                    | `LucroPresumido` | Faturamento até R$ 78 mi |
                    | `LucroReal` | Acima de R$ 78 mi ou setores específicos |
                    | `ImunidadeIsencao` | Entidades sem fins lucrativos |

                    ### Status das Obrigações
                    | Valor | Critério |
                    |-------|----------|
                    | `Pendente` | Vencimento futuro, não entregue |
                    | `Atrasada` | Vencimento passado, não entregue |
                    | `Entregue` | Entrega registrada com data de conclusão |
                    | `NaoAplicavel` | Não se aplica ao regime tributário da empresa |
                    """,
                Contact = new OpenApiContact
                {
                    Name  = "e-Auditoria",
                    Email = "dev@eauditoria.com.br",
                },
                License = new OpenApiLicense { Name = "Privado — uso interno" }
            });

            c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Name         = "Authorization",
                Type         = SecuritySchemeType.Http,
                Scheme       = "bearer",
                BearerFormat = "JWT",
                In           = ParameterLocation.Header,
                Description  = "Informe o token JWT obtido em **POST /api/auth/login**.\n\nExemplo: `Bearer eyJhbGci...`",
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = JwtBearerDefaults.AuthenticationScheme,
                        },
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

            c.OrderActionsBy(api =>
                $"{api.ActionDescriptor.RouteValues["controller"]}_{api.RelativePath}_{api.HttpMethod}");

            c.UseInlineDefinitionsForEnums();
        });

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "e-Auditoria API v1");
            c.RoutePrefix             = "swagger";
            c.DocumentTitle           = "e-Auditoria API";
            c.DefaultModelsExpandDepth(-1);
            c.DefaultModelExpandDepth(2);
            c.DisplayRequestDuration();
            c.EnableDeepLinking(); 
            c.EnableFilter(); 
        });

        return app;
    }
}
