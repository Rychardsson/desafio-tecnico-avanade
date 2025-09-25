# 🛒 Desafio Técnico Avanade - Sistema de Microserviços E-commerce

Sistema completo de e-commerce desenvolvido com arquitetura de microserviços utilizando .NET 8, RabbitMQ e API Gateway.

## 🏗️ Arquitetura

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   API Gateway   │────│  Auth Service   │────│   RabbitMQ      │
│   (Ocelot)      │    │     (JWT)       │    │  (Messaging)    │
│   Port: 5000    │    │   Port: 5003    │    │  Port: 5672     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                        │                        │
         ├────────────────────────┼────────────────────────┤
         │                        │                        │
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ Estoque Service │    │ Vendas Service  │    │   Shared Lib    │
│   Port: 5001    │────│   Port: 5002    │────│  (Common DTOs)  │
│  (Products)     │    │   (Orders)      │    │   (Services)    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🚀 Tecnologias Utilizadas

- **Framework**: .NET 8
- **Database**: SQL Server LocalDB
- **ORM**: Entity Framework Core 9.0
- **Message Broker**: RabbitMQ 7.1.2
- **API Gateway**: Ocelot 24.0.1
- **Authentication**: JWT Bearer Tokens
- **Password Hashing**: BCrypt.Net-Next 4.0.3
- **Logging**: Serilog 9.0.0
- **Mapping**: AutoMapper 12.0.1
- **Documentation**: Swagger/OpenAPI

## 📦 Microserviços

### 🔐 AuthService (Port: 5003)
- **Função**: Autenticação e autorização de usuários
- **Tecnologias**: JWT, BCrypt, Entity Framework
- **Endpoints**:
  - `POST /api/auth/register` - Registro de usuário
  - `POST /api/auth/login` - Login e geração de token
  - `GET /api/auth/profile` - Perfil do usuário autenticado

### 📦 EstoqueService (Port: 5001)
- **Função**: Gerenciamento de produtos e estoque
- **Tecnologias**: Entity Framework, RabbitMQ Publisher
- **Endpoints**:
  - `GET /api/produtos` - Listar produtos
  - `GET /api/produtos/{id}` - Buscar produto por ID
  - `POST /api/produtos` - Criar produto
  - `PUT /api/produtos/{id}` - Atualizar produto
  - `DELETE /api/produtos/{id}` - Deletar produto
  - `GET /api/produtos/{id}/validar-estoque/{quantidade}` - Validar estoque
  - `POST /api/produtos/{id}/atualizar-estoque` - Atualizar estoque

### 🛒 VendasService (Port: 5002)
- **Função**: Gerenciamento de pedidos e vendas
- **Tecnologias**: Entity Framework, RabbitMQ Publisher, HTTP Client
- **Endpoints**:
  - `GET /api/pedidos` - Listar pedidos
  - `GET /api/pedidos/{id}` - Buscar pedido por ID
  - `POST /api/pedidos` - Criar pedido
  - `PUT /api/pedidos/{id}/status` - Atualizar status do pedido
  - `DELETE /api/pedidos/{id}` - Cancelar pedido

### 🌐 ApiGateway (Port: 5000)
- **Função**: Roteamento, rate limiting e autenticação centralizada
- **Tecnologias**: Ocelot, JWT Validation
- **Recursos**:
  - Rate limiting (100 req/min)
  - Load balancing
  - JWT token validation
  - Request/Response logging

## 🐰 Integração RabbitMQ

### Filas Configuradas:
- `pedido.criado` - Eventos de criação de pedidos
- `pedido.confirmado` - Pedidos confirmados  
- `pedido.status.atualizado` - Mudanças de status
- `estoque.atualizado` - Atualizações de estoque
- `estoque.insuficiente` - Alertas de estoque baixo
- `estoque.validacao.request` - Solicitações de validação
- `estoque.validacao.response` - Respostas de validação

### Messages Classes:
- `PedidoCriadoMessage`
- `PedidoConfirmadoMessage`
- `PedidoStatusAtualizadoMessage`
- `EstoqueAtualizadoMessage`
- `EstoqueInsuficienteMessage`
- `EstoqueValidacaoRequest/Response`

## 🛠️ Como Executar

### Pré-requisitos
- .NET 8 SDK
- SQL Server LocalDB
- RabbitMQ Server (opcional para desenvolvimento)

### 1. Clone o repositório
```bash
git clone https://github.com/Rychardsson/desafio-tecnico-avanade.git
cd desafio-tecnico-avanade
```

### 2. Restaurar dependências
```bash
dotnet restore
```

### 3. Compilar a solução
```bash
dotnet build
```

### 4. Executar RabbitMQ (Docker)
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### 5. Executar os serviços

**Terminal 1 - AuthService:**
```bash
cd src/Services/AuthService
dotnet run
```

**Terminal 2 - EstoqueService:**
```bash
cd src/Services/EstoqueService/EstoqueService
dotnet run
```

**Terminal 3 - VendasService:**
```bash
cd src/Services/VendasService/VendasService
dotnet run
```

**Terminal 4 - ApiGateway:**
```bash
cd src/Gateway/ApiGateway
dotnet run
```

### 6. Acessar os serviços

- **API Gateway**: https://localhost:5000
- **Auth Service**: https://localhost:5003/swagger
- **Estoque Service**: https://localhost:5001/swagger
- **Vendas Service**: https://localhost:5002/swagger
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

## 📁 Estrutura do Projeto

```
desafio-tecnico-avanade/
├── src/
│   ├── Shared/
│   │   └── Shared/                 # Biblioteca compartilhada
│   │       ├── Models/             # Entidades do domínio
│   │       ├── DTOs/               # Data Transfer Objects
│   │       ├── Services/           # Serviços compartilhados
│   │       ├── Messages/           # Classes de mensagens RabbitMQ
│   │       ├── Constants/          # Constantes da aplicação
│   │       └── Helpers/            # Classes utilitárias
│   ├── Services/
│   │   ├── AuthService/            # Serviço de autenticação
│   │   ├── EstoqueService/         # Serviço de estoque
│   │   └── VendasService/          # Serviço de vendas
│   └── Gateway/
│       └── ApiGateway/             # Gateway de API
├── README.md
└── desafio-tecnico-avanade.sln
```

## 🔧 Configuração

### appsettings.json

Cada serviço possui sua configuração específica:

**AuthService:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AuthServiceDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "sua-chave-secreta-super-segura-256-bits",
    "Issuer": "AuthService",
    "Audience": "MicroservicesApp",
    "ExpirationInMinutes": 60
  }
}
```

**EstoqueService/VendasService:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EstoqueServiceDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "RabbitMQ": {
    "ConnectionString": "amqp://guest:guest@localhost:5672/"
  }
}
```

## 🧪 Testando a API

### 1. Registrar usuário
```bash
curl -X POST https://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "João Silva",
    "email": "joao@email.com",
    "senha": "123456"
  }'
```

### 2. Fazer login
```bash
curl -X POST https://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "joao@email.com",
    "senha": "123456"
  }'
```

### 3. Criar produto (com token)
```bash
curl -X POST https://localhost:5000/api/estoque/produtos \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -d '{
    "nome": "Produto Teste",
    "descricao": "Descrição do produto",
    "preco": 99.99,
    "quantidadeEstoque": 100
  }'
```

### 4. Criar pedido
```bash
curl -X POST https://localhost:5000/api/vendas/pedidos \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -d '{
    "itens": [
      {
        "produtoId": 1,
        "quantidade": 2,
        "precoUnitario": 99.99
      }
    ]
  }'
```

## 🎯 Funcionalidades Implementadas

- ✅ **Autenticação JWT completa**
- ✅ **CRUD de produtos com controle de estoque**
- ✅ **CRUD de pedidos com validação de estoque**
- ✅ **API Gateway com roteamento inteligente**
- ✅ **Comunicação assíncrona via RabbitMQ**
- ✅ **Logging estruturado com Serilog**
- ✅ **Documentação Swagger em todos os serviços**
- ✅ **Rate limiting e proteção de endpoints**
- ✅ **Validação de dados e tratamento de erros**
- ✅ **Padrão Repository e Service Layer**
- ✅ **Mapeamento automático com AutoMapper**
- ✅ **Configuração por ambiente**

## 🔄 Fluxo de Negócio

1. **Usuário se registra** no AuthService
2. **Usuário faz login** e recebe JWT token
3. **Admin cadastra produtos** no EstoqueService
4. **Usuário cria pedido** no VendasService
5. **Sistema valida estoque** via HTTP
6. **Sistema publica eventos** via RabbitMQ
7. **Estoque é atualizado** automaticamente
8. **Pedido é confirmado** ou rejeitado

## 🤝 Contribuindo

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📝 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 👨‍💻 Autor

**Rychardsson** - [GitHub](https://github.com/Rychardsson)

---

⭐ Se este projeto te ajudou, não esqueça de dar uma estrela!