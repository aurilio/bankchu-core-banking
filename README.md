﻿﻿# BankChu.CoreBanking

API de **Core Banking** para gerenciamento de **contas**, **transferências** e **extrato bancário**, do Banco Chu S.A.

## O projeto foi construído com foco em:
- Clean Architecture
- Boas práticas de engenharia
- Segurança (JWT, scopes, policies)
- Performance e resiliência (cache, idempotência)

---

## Tecnologias
- ASP.NET Core 6
- C#
- Clean Architecture
- Entity Framework Core
- SQL Server
- Redis
- Docker / Docker Compose
- JWT Bearer Authentication
- Swagger (OpenAPI)
- Polly (resiliência HTTP)
- Refit (clientes HTTP tipados)

---

## Arquitetura

O projeto segue **Clean Architecture**, com separação clara de responsabilidades:
```
src/
├── BankChu.CoreBanking.Api → API (endpoints, auth, swagger)
├── BankChu.CoreBanking.Application → Casos de uso e regras de aplicação
├── BankChu.CoreBanking.Domain → Entidades e regras de domínio
├── BankChu.CoreBanking.Infrastructure→ Banco, cache, APIs externas
└── BankChu.CoreBanking.IoC → Injeção de dependência
```
---

## 🐳 Como executar o projeto via Docker (pre-built image no Docker Hub)

### Steps

**Clonar o repositorio:**

```bash
git clone https://github.com/aurilio/sales-api.git
```
**Acessar o diretório**
```
cd bankchu-core-banking/docker
```

**Executar o comando**
```bash
docker compose up -d --build
```

A API estará disponível em:
```
http://localhost:8080
http://localhost:8080/swagger
```

## Autenticação e Autorização
A API utiliza JWT Bearer Authentication com controle de acesso por scopes.

Como se autenticar

Gere um token JWT:
```
curl -X POST http://localhost:8080/api/v1/auth/token \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123!"
  }'
```
Resposta de exemplo:
```
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

O token retornado deve ser enviado em todas as requisições protegidas

Criar conta
```
curl -X POST http://localhost:8080/api/v1/accounts \
  -H "Authorization: Bearer <TOKEN_AQUI>" \
  -H "Content-Type: application/json" \
  -d '{
    "document": "12345678901",
    "name": "Empresa XPTO",
    "initialBalance": 1000
  }'
```

## Realizar transferência

O endpoint de transferência (`POST /api/v1/transfers`) exige o header
`Idempotency-Key`.

Esse mecanismo garante que requisições repetidas (por retry de rede,
timeout ou falha do cliente) **não gerem transferências duplicadas**,
um requisito essencial em sistemas financeiros.

### Como funciona

- Requisições com o **mesmo Idempotency-Key** retornam sempre o mesmo resultado
- Requisições com **chaves diferentes** criam novas transferências
- A chave deve ser **única por tentativa de operação**

### Exemplo

```http
Idempotency-Key: 8d7c4f5a-9a23-4a8b-9d22-acde12345678
```
Realizar transferência
```
curl -X POST http://localhost:8080/api/v1/transfers \
  -H "Authorization: Bearer <TOKEN_AQUI>" \
  -H "Idempotency-Key: 8d7c4f5a-9a23-4a8b-9d22-acde12345678" \
  -H "Content-Type: application/json" \
  -d '{
    "fromAccountId": "GUID_ORIGEM",
    "toAccountId": "GUID_DESTINO",
    "amount": 150
  }'

```
Consultar extrato
```
curl -X GET "http://localhost:8080/api/v1/accounts/GUID_AQUI/statements?from=2025-01-01&to=2025-01-31" \
  -H "Authorization: Bearer <TOKEN_AQUI>"
```

Consultar extrato com ETag
```
curl -X GET "http://localhost:8080/api/v1/accounts/GUID_AQUI/statements?from=2025-01-01&to=2025-01-31" \
  -H "Authorization: Bearer <TOKEN_AQUI>" \
  -H "If-None-Match: \"ETAG_AQUI\"" \
  -i
```
