-- ============================================================
-- e-Auditoria — Migration: InitialCreate
-- Gerado em: 2025-01-01
--
-- Execute UMA VEZ no banco eauditoria antes de subir a API:
--   psql -U postgres -d eauditoria -f scripts/migration.sql
--
-- Se o banco ainda não existe, crie antes:
--   psql -U postgres -c "CREATE DATABASE eauditoria ENCODING 'UTF8';"
-- ============================================================

BEGIN;

-- -------------------------------------------------------
-- Tabela: empresas
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS empresas (
    id                UUID                     NOT NULL,
    razao_social      CHARACTER VARYING(200)   NOT NULL,
    cnpj              CHARACTER VARYING(14)    NOT NULL,
    regime_tributario INTEGER                  NOT NULL,
    ativo             BOOLEAN                  NOT NULL DEFAULT TRUE,
    criado_em         TIMESTAMP WITH TIME ZONE NOT NULL,
    atualizado_em     TIMESTAMP WITH TIME ZONE,

    CONSTRAINT "PK_empresas" PRIMARY KEY (id)
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_empresas_cnpj
    ON empresas (cnpj);

-- -------------------------------------------------------
-- Tabela: obrigacoes_acessorias
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS obrigacoes_acessorias (
    id              UUID                     NOT NULL,
    empresa_id      UUID                     NOT NULL,
    tipo            INTEGER                  NOT NULL,
    periodicidade   INTEGER                  NOT NULL,
    competencia     INTEGER                  NOT NULL,
    ano_competencia INTEGER                  NOT NULL,
    vencimento      TIMESTAMP WITH TIME ZONE NOT NULL,
    status          INTEGER                  NOT NULL,
    criado_em       TIMESTAMP WITH TIME ZONE NOT NULL,

    CONSTRAINT "PK_obrigacoes_acessorias" PRIMARY KEY (id),
    CONSTRAINT "FK_obrigacoes_acessorias_empresas_empresa_id"
        FOREIGN KEY (empresa_id) REFERENCES empresas (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_obrigacoes_empresa_tipo_competencia
    ON obrigacoes_acessorias (empresa_id, tipo, competencia, ano_competencia);

CREATE INDEX IF NOT EXISTS ix_obrigacoes_empresa_mes_ano
    ON obrigacoes_acessorias (empresa_id, competencia, ano_competencia);

CREATE INDEX IF NOT EXISTS ix_obrigacoes_vencimento_status
    ON obrigacoes_acessorias (vencimento, status);

-- -------------------------------------------------------
-- Tabela: entregas_obrigacoes
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS entregas_obrigacoes (
    id            UUID                     NOT NULL,
    obrigacao_id  UUID                     NOT NULL,
    data_entrega  TIMESTAMP WITH TIME ZONE NOT NULL,
    observacao    CHARACTER VARYING(500),
    criado_em     TIMESTAMP WITH TIME ZONE NOT NULL,

    CONSTRAINT "PK_entregas_obrigacoes" PRIMARY KEY (id),
    CONSTRAINT "FK_entregas_obrigacoes_obrigacoes_acessorias_obrigacao_id"
        FOREIGN KEY (obrigacao_id) REFERENCES obrigacoes_acessorias (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_entregas_obrigacao_id
    ON entregas_obrigacoes (obrigacao_id);

-- -------------------------------------------------------
-- Tabela de controle do EF Core
-- Registra a migration para que o EF não tente reaplicá-la
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId"    CHARACTER VARYING(150) NOT NULL,
    "ProductVersion" CHARACTER VARYING(32)  NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250101000000_InitialCreate', '9.0.1')
ON CONFLICT DO NOTHING;

COMMIT;
