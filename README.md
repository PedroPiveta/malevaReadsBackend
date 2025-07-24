# MalevaReads

## Visão Geral

O MalevaReads é um sistema composto por múltiplos microsserviços para gerenciamento de usuários e livros, utilizando autenticação JWT, bancos de dados PostgreSQL e MongoDB, e um API Gateway para roteamento centralizado.

## Serviços e Portas

| Serviço         | Porta HTTP | Porta HTTPS | Banco de Dados | Porta DB |
|-----------------|------------|-------------|---------------|----------|
| gateway-service | 8081       | 8082        | -             | -        |
| user-service    | 5000       | 5001        | PostgreSQL     | 5432     |
| book-service    | 7000       | 7001        | MongoDB        | 27017    |

- O gateway expõe a API unificada nas portas 8081 (HTTP) e 8082 (HTTPS).
- O user-service expõe suas APIs nas portas 5000 (HTTP) e 5001 (HTTPS).
- O book-service expõe suas APIs nas portas 7000 (HTTP) e 7001 (HTTPS).

## Como Rodar

### Com Docker Compose (Recomendado)

1. Certifique-se de ter o Docker e Docker Compose instalados.
2. No diretório raiz do projeto, execute:

```bash
docker-compose up --build
```

3. Acesse os serviços:
   - Gateway: http://localhost:8081
   - User Service: http://localhost:5000
   - Book Service: http://localhost:7000
   - PostgreSQL: localhost:5432 (usuário: postgres, senha: postgres)
   - MongoDB: localhost:27017

4. Para parar os serviços:

```bash
docker-compose down
```

### Rodando Localmente (Sem Docker)

1. Configure as strings de conexão nos arquivos `appsettings.json` de cada serviço.
2. Inicie os bancos de dados (PostgreSQL e MongoDB) localmente.
3. No diretório de cada serviço, execute:

```bash
dotnet run
```

4. Os serviços estarão disponíveis nas portas configuradas acima.

## Autenticação

- O sistema utiliza JWT para autenticação.
- A chave secreta está definida em `appsettings.json` de cada serviço.
- O token deve ser enviado no header `Authorization: Bearer <token>`.

## Endpoints Principais

- **User Service:** `/api/user` (cadastro, login, perfil)
- **Book Service:** `/api/book` (CRUD de livros)
- **Gateway:** Roteia `/users/*` para o user-service e `/books/*` para o book-service.

## Swagger

Cada serviço expõe documentação Swagger em:
- User Service: http://localhost:5000/swagger
- Book Service: http://localhost:7000/swagger
- Gateway: http://localhost:8081/swagger

## Variáveis de Ambiente Importantes

- `ASPNETCORE_HTTP_PORTS` e `ASPNETCORE_HTTPS_PORTS` para definir as portas de cada serviço.
---

Para dúvidas ou contribuições, abra uma issue ou pull request. 