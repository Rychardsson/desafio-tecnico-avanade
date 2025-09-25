# Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-09-25

### Adicionado

#### 🏗️ Arquitetura
- Implementação completa de arquitetura de microserviços
- API Gateway com Ocelot para roteamento centralizado
- Biblioteca compartilhada (Shared) para componentes comuns
- Configuração de Solution com múltiplos projetos

#### 🔐 AuthService
- Sistema de autenticação com JWT tokens
- Registro e login de usuários
- Hash de senhas com BCrypt
- Validação de tokens
- Entity Framework com SQL Server LocalDB
- Swagger/OpenAPI documentation

#### 📦 EstoqueService
- CRUD completo de produtos
- Controle de estoque com validação
- Integração RabbitMQ para eventos de estoque
- API RESTful com validação de dados
- Repository pattern e Service layer
- Logging estruturado com Serilog

#### 🛒 VendasService
- CRUD completo de pedidos
- Validação de estoque via HTTP
- Integração RabbitMQ para eventos de pedidos
- Comunicação com EstoqueService
- Gerenciamento de status de pedidos
- AutoMapper para mapeamento de objetos

#### 🌐 ApiGateway
- Roteamento inteligente com Ocelot
- Rate limiting (100 req/min)
- Autenticação JWT centralizada
- Load balancing
- Request/Response logging
- Configuração por ambiente

#### 🐰 RabbitMQ Integration
- Sistema de mensageria assíncrona completo
- Filas configuradas:
  - `pedido.criado` - Eventos de criação de pedidos
  - `pedido.confirmado` - Pedidos confirmados
  - `pedido.status.atualizado` - Mudanças de status
  - `estoque.atualizado` - Atualizações de estoque
  - `estoque.insuficiente` - Alertas de estoque baixo
  - `estoque.validacao.request/response` - Validação assíncrona
- Message classes estruturadas para comunicação entre serviços
- RabbitMQService compartilhado para publicação de mensagens

#### 📚 Shared Library
- Models de domínio (Produto, Pedido, ItemPedido, Usuario)
- DTOs para comunicação entre serviços
- Services compartilhados (JwtService, RabbitMQService)
- Constants para configurações globais
- Helpers utilitários (ApiResponse)
- Messages classes para RabbitMQ

#### 🛠️ Infraestrutura
- Configuração completa do Entity Framework
- Migrations automáticas
- Connection strings por ambiente
- Logging configurado em todos os serviços
- Tratamento de erros padronizado
- Validação de dados com Data Annotations

#### 📖 Documentação
- README.md completo com instruções de instalação
- Documentação da arquitetura
- Exemplos de uso da API
- Guia de contribuição
- CHANGELOG.md para versionamento

### 🔧 Tecnologias Utilizadas
- .NET 8
- Entity Framework Core 9.0
- SQL Server LocalDB
- RabbitMQ Client 7.1.2
- Ocelot 24.0.1
- JWT Bearer 8.0.0
- AutoMapper 12.0.1
- Serilog 9.0.0
- BCrypt.Net-Next 4.0.3
- Swagger/OpenAPI

### 🎯 Funcionalidades
- ✅ Autenticação JWT completa
- ✅ CRUD de produtos com controle de estoque
- ✅ CRUD de pedidos com validação
- ✅ API Gateway com roteamento
- ✅ Comunicação assíncrona via RabbitMQ
- ✅ Logging estruturado
- ✅ Documentação Swagger
- ✅ Rate limiting e proteção
- ✅ Validação de dados
- ✅ Padrão Repository/Service
- ✅ Mapeamento automático
- ✅ Configuração por ambiente

### 🚀 Deploy
- Scripts de execução
- Configuração Docker para RabbitMQ
- Instruções de ambiente de desenvolvimento
- Guia de testes da API

---

## Formato das versões

- **MAJOR**: Mudanças incompatíveis na API
- **MINOR**: Funcionalidades adicionadas de forma compatível
- **PATCH**: Correções de bugs compatíveis

## Tipos de mudanças

- `Adicionado`: novas funcionalidades
- `Alterado`: mudanças em funcionalidades existentes
- `Descontinuado`: funcionalidades que serão removidas
- `Removido`: funcionalidades removidas
- `Corrigido`: correções de bugs
- `Segurança`: vulnerabilidades corrigidas