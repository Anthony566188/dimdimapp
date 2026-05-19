# DimDimApp — CP3 DevOps Tools & Cloud Computing

## Equipe

| Nome | RM |
|------|----|
| Anthony De Souza Henriques | RM566188 |
| Gustavo Araújo Da Silva | RM566526 |
| Guilherme Santos Fonseca | RM564232 |

---

## Sobre o Projeto

API RESTful para gerenciamento de contas bancárias.  
Stack: **.NET 8** + **PostgreSQL 16**, totalmente conteinerizada com Docker, rodando em nuvem (Azure).

---

## Arquitetura

```
┌─────────────────────────────┐
│      Rede: dimdim-network   │
│                             │
│  ┌──────────────────────┐   │
│  │  Container: app-RM   │   │  ← .NET 8 API (porta 8080)
│  │  Imagem: dimdimapp   │   │
│  │  User: dimdimuser    │   │
│  └──────────┬───────────┘   │
│             │               │
│  ┌──────────▼───────────┐   │
│  │  Container: db-RM    │   │  ← PostgreSQL 16
│  │  Imagem: postgres:16 │   │
│  │  Volume: dimdim-data │   │
│  └──────────────────────┘   │
└─────────────────────────────┘
```

---

## HOW TO — Passo a Passo Completo

> Execute todos os comandos no terminal da VM do Azure (conectado via SSH)

## Pré-requisitos
- VM do Azure com AlmaLinux 10.1 já criada
- Acesso SSH à VM
- Docker não precisa estar instalado (este README instala)
- Conexão de internet estável

---

### PARTE 1 — Conectar na VM do Azure

1. Abra o **Portal do Azure** → acesse sua VM → clique em **Conectar** → **SSH**
2. Copie o comando de conexão (algo como `ssh usuario@IP-DA-VM`)
3. No Windows, abra o **PowerShell** ou **Terminal** e cole o comando
4. Aceite a chave digitando `yes` quando solicitado
5. Digite sua senha se pedido

---

### PARTE 2 — Instalar o Docker na VM (CentOS/RedHat)

Cole cada bloco de comandos no terminal da VM:

```bash
# Atualiza os pacotes do sistema
sudo yum update -y

# Instala pacotes necessários
sudo yum install -y yum-utils

# Adiciona o repositório oficial do Docker
sudo yum-config-manager --add-repo https://download.docker.com/linux/rhel/docker-ce.repo

# Instala o Docker
sudo yum install -y docker-ce docker-ce-cli containerd.io

# Inicia o serviço do Docker
sudo systemctl start docker

# Deixa o Docker iniciar automaticamente
sudo systemctl enable docker

# Permite usar docker sem sudo
sudo usermod -aG docker $USER

# Aplica o novo grupo (escolha UMA das opções abaixo)
# Opção 1: Reinicie o terminal/SSH (disconnect e reconecte)
# Opção 2: Digite isto para aplicar agora
newgrp docker

# Verifica se o Docker foi instalado corretamente
docker --version
```

---

### PARTE 3 — Clonar o repositório

```bash
# Instala o Git (se não tiver)
sudo yum install -y git

# Clona o repositório (substitua pela URL do seu GitHub)
git clone https://github.com/Anthony566188/dimdimapp.git

# Entra na pasta do projeto
cd dimdimapp
```

---

### PARTE 4 — Criar a rede Docker

```bash
# Cria a rede que conectará os dois containers
docker network create dimdim-network

# Confirma que foi criada
docker network ls
```

---

### PARTE 5 — Criar o volume nomeado para o banco

```bash
# Cria o volume nomeado para persistência dos dados
docker volume create dimdim-data

# Confirma que foi criado
docker volume ls
```

---

### PARTE 6 — Subir o container do Banco de Dados

```bash
docker run -d \
  --name db-RM566526 \
  --network dimdim-network \
  -e POSTGRES_DB=dimdimdb \
  -e POSTGRES_USER=dimdim \
  -e POSTGRES_PASSWORD=dimdim123 \
  -v dimdim-data:/var/lib/postgresql/data \
  -v $(pwd)/db/init.sql:/docker-entrypoint-initdb.d/init.sql \
  -p 5432:5432 \
  postgres:16
```

Aguarde alguns segundos e verifique se está rodando:

```bash
docker ps
```

Você deve ver o container `db-RM566526` com status `Up`.

---

### PARTE 7 — Build da imagem da aplicação

```bash
# Entra na pasta da aplicação
cd app

# Faz o build da imagem personalizada
docker build -t dimdimapp:latest .

# Volta para a raiz do projeto
cd ..

# Confirma que a imagem foi criada
docker images
```

---

### PARTE 8 — Subir o container da Aplicação

```bash
docker run -d \
  --name app-RM566526 \
  --network dimdim-network \
  -e DATABASE_URL="Host=db-RM566526;Port=5432;Database=dimdimdb;Username=dimdim;Password=dimdim123" \
  -p 8080:8080 \
  dimdimapp:latest
```

Verifique se está rodando:

```bash
docker ps
```

Você deve ver **dois containers** rodando: `app-RM566526` e `db-RM566526`.

---

### PARTE 9 — Liberar a porta 8080 no Azure

1. Acesse o **Portal do Azure** → sua VM → **Rede** → **Configurações de rede**
2. Clique em **Adicionar regra de porta de entrada**
3. Preencha:
   - **Intervalo de portas de destino**: `8080`
   - **Protocolo**: `TCP`
   - **Nome**: `allow-8080`
4. Clique em **Adicionar**

---

### PARTE 10 — Testar a API

#### Abrir o Swagger (interface visual da API)
Acesse no navegador: `http://68.155.146.238:8080/swagger`

#### Testar via terminal (curl):

**CREATE — Criar uma conta:**
```bash
curl -X POST http://68.155.146.238:8080/contas \
  -H "Content-Type: application/json" \
  -d '{"titular": "Ana Lima", "saldo": 2500.00}'
```

**READ ALL — Listar todas as contas:**
```bash
curl http://68.155.146.238:8080/contas
```

**READ ONE — Buscar conta por ID:**
```bash
curl http://68.155.146.238:8080/contas/1
```

**UPDATE — Atualizar uma conta:**
```bash
curl -X PUT http://68.155.146.238:8080/contas/1 \
  -H "Content-Type: application/json" \
  -d '{"titular": "Ana Lima Atualizada", "saldo": 9999.99}'
```

**DELETE — Excluir uma conta:**
```bash
curl -X DELETE http://68.155.146.238:8080/contas/3
```

---

### PARTE 11 — Evidenciar operações no Banco (SELECT direto)

```bash
# Acessa o container do banco
docker container exec -it db-RM566526 psql -U dimdim -d dimdimdb

# Dentro do psql, execute os SELECTs:
SELECT * FROM "Contas";

# Sair do psql
\q
```

---

### PARTE 12 — Evidenciar estrutura e usuário dos containers

**Container da Aplicação:**
```bash
docker container exec -it app-RM566526 sh -c "whoami && pwd && ls -la"
```

**Container do Banco:**
```bash
docker container exec -it db-RM566526 sh -c "whoami && pwd && ls -la"
```

---

## Requisitos Técnicos Atendidos

| Requisito | Como foi atendido |
|-----------|------------------|
| 2 containers | `app-RM` (.NET) + `db-RM` (PostgreSQL) |
| Volume nomeado | `dimdim-data` mapeado no container do banco |
| CRUD completo | Endpoints POST, GET, PUT, DELETE na tabela `Contas` |
| Mesma rede Docker | Ambos na rede `dimdim-network` |
| Usuário não-root | `dimdimuser` definido no Dockerfile |
| Diretório de trabalho | `WORKDIR /dimdimapp` no Dockerfile |
| Variável de ambiente | `DATABASE_URL` e `ASPNETCORE_URLS` |
| Dockerfile + imagem personalizada | `dimdimapp:latest` gerada via `docker build` |
| Container do banco com imagem pública | `postgres:16` sem Dockerfile |
| Execução em background | Flag `-d` em ambos os `docker run` |
| RM no nome dos containers | `app-RM566526` e `db-RM566526` |
| Execução em nuvem | VM no Microsoft Azure |
| How to no GitHub | Este README |

---

## Estrutura do Repositório

---

## Troubleshooting

**Erro ao fazer docker build: "dotnet restore failed"**
- Deixa rodar por alguns minutos — a primeira vez é lenta
- Se persistir, verifique se a VM tem pelo menos 2GB de RAM e 10GB de disco

**Container app não conecta ao banco: "Host not found"**
- Certifique-se de que ambos estão na mesma rede: `docker network inspect dimdim-network`
- Verifique se o nome do container do banco está correto no `DATABASE_URL`

**Porta 8080 não abre no navegador**
- Aguarde 1-2 minutos após subir o container (ele precisa inicializar)
- Verifique se a porta 8080 foi liberada no Azure
- Verifique se o container está realmente rodando: `docker ps`
