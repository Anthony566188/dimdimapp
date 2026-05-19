-- Script de inicialização do banco DimDim
-- Este arquivo é executado automaticamente quando o container PostgreSQL sobe pela primeira vez

CREATE TABLE IF NOT EXISTS "Contas" (
    "Id"        SERIAL PRIMARY KEY,
    "Titular"   VARCHAR(100) NOT NULL,
    "Saldo"     NUMERIC(15,2) NOT NULL DEFAULT 0,
    "CriadoEm" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Dados iniciais para teste
INSERT INTO "Contas" ("Titular", "Saldo") VALUES
    ('João Silva', 1500.00),
    ('Maria Souza', 3200.50);
