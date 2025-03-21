# 📦 Sistema de Pedidos - .NET 8 + PostgreSQL + SQS/S3 + Docker

Este projeto é uma API de Pedidos desenvolvida em .NET 8 com suporte a:

- ✅ Recebimento de pedidos via API
- ✅ Enfileiramento de pedidos via Amazon SQS (simulado com LocalStack)
- ✅ Salvamento de grandes pedidos no Amazon S3 (quando excedem 256KB)
- ✅ Processamento assíncrono dos pedidos com um Consumer (HostedService)
- ✅ Armazenamento em banco PostgreSQL
- ✅ Cálculo de imposto com Feature Flag
- ✅ Logs estruturados com Serilog
- ✅ Docker Compose para subir toda a stack local

---

## 🚀 Como rodar o projeto em outra máquina

### ✅ 1. Pré-requisitos instalados

Antes de rodar o projeto, você precisa ter instalado:

- [Docker](https://www.docker.com/products/docker-desktop/)
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (caso queira rodar fora do Docker)
- [Git](https://git-scm.com/)

---

### ✅ 2. Clone o repositório

```bash
git clone https://github.com/jpmmaion498/Pedido.git
cd seu-repo
```

---

### ✅ 3. Subir a aplicação com Docker

Com o Docker rodando, execute:

```bash
docker-compose up --build -d
```

Esse comando irá subir:

- `Pedido.Api` (sua API)
- `PostgreSQL` (porta 5432)
- `LocalStack` (porta 4566) simulando SQS e S3

---

### ✅ 4. Acessar a aplicação

- Acesse a **Swagger UI** da API:

```
http://localhost:8080/swagger
```

---

## 📂 Estrutura do Projeto

```
Pedido/
├── Pedido.Api/                  → API HTTP (.NET 8)
├── Pedido.Application/          → Serviços de domínio (regras de negócio)
├── Pedido.Domain/               → Entidades e interfaces
├── Pedido.Infrastructure/       → Repositórios, Contexto EF, Serviços AWS
├── docker-compose.yml           → Sobe API + PostgreSQL + LocalStack
├── .env                         → Variáveis de ambiente do banco
└── README.md
```

---

## ⚙️ Feature Flags

A regra de cálculo de imposto pode ser controlada no `appsettings.json`:

```json
"Features": {
  "UsarNovaRegraImposto": true
}
```

- `false`: imposto = total * 0.3
- `true` : imposto = total * 0.2 (reforma tributária)

---

## 📤 Envio de pedidos para a fila

Pedidos são enviados via `POST /api/pedidos`.

- Se o tamanho do JSON for **menor que 256KB**, vai direto para o SQS.
- Se for **maior que 256KB**, é salvo no S3 e apenas uma referência é enviada à fila.

---

## 🧾 Exemplo de requisição `POST /api/pedidos`

```json
{
  "pedidoId": "d3f6a013-1c18-4c13-9b6f-0a95c51c0fd9",
  "clienteId": 123,
  "itens": [
    {
      "produtoId": 1001,
      "quantidade": 2,
      "valor": 19.90
    }
  ]
}
```

---

## 🗃️ Consumo da fila

O projeto possui um `BackgroundService` que:

- Lê mensagens da fila SQS
- Caso a mensagem contenha referência a um arquivo no S3, faz o download
- Salva o pedido no banco PostgreSQL

---

## 📑 Logs estruturados com Serilog

Todos os logs da aplicação estão estruturados com Serilog:

- Console
- Arquivo: `Logs/app.log`

---

## ✅ Requisitos do desafio atendidos

- [x] Validação de duplicidade de pedidos
- [x] Cálculo de imposto com 2 regras
- [x] Controle de cálculo via feature flag
- [x] Suporte à alta volumetria (até 200 mil/dia) com fila
- [x] Armazenamento de pedidos grandes no S3
- [x] Exposição via API REST + Swagger
- [x] Docker Compose para execução local

---

## 🧪 Testes

> Testes automatizados ainda não foram implementados.

---

## ✉️ Contato

Se tiver dúvidas ou sugestões, fique à vontade para abrir uma issue ou me chamar!

---


---

## 🎥 Demonstração

![Swagger em funcionamento](./docs/swagger-demo.gif)

> Você pode gravar um GIF com uma ferramenta como [ScreenToGif](https://www.screentogif.com/) ou [LiceCap](https://www.cockos.com/licecap/) e salvar em `docs/swagger-demo.gif` para ser exibido aqui.

