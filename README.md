# 🌾 AgroSolutions - Plataforma de Agricultura de Precisão (IoT)

**FIAP - Pós-Graduação em Arquitetura de Sistemas .NET com Azure**
**Hackathon Final (Fase 5) - Grupo 59**

## 👥 Integrantes
* **Elias Oliveira Prates** (RM364079) - Discord: Elias Prates
* **William Henrique Cirino** (RM361204) - Discord: WilliamHC-RM361204
* **Thiago Martins Reboredo** (RM364884) - Discord: Thiago Reboredo

---

## 📖 Sobre o Projeto
A AgroSolutions é uma cooperativa agrícola em transição para a **Agricultura 4.0**. Este projeto é o MVP de uma plataforma baseada em IoT e análise de dados, projetada para processar informações de sensores de campo (umidade, temperatura, precipitação) e gerar alertas inteligentes para os produtores rurais, otimizando o uso de recursos e aumentando a sustentabilidade.

## 🏗️ Decisões Arquiteturais e Padrões (Fase 5)

Nossa solução foi desenhada do zero adotando **Clean Architecture** e princípios **SOLID**:
* **Microsserviços Desacoplados (SRP):** Separamos responsabilidades críticas. A Ingestão de dados (`IngestionService`) é focada apenas em alta disponibilidade para receber milhares de requisições, enquanto a regra de negócio de análise (`AlertService`) roda isolada em um Worker.
* **Inversão de Dependência (DIP):** Uso extensivo de interfaces (`IAuthService`, `IPropertyService`) garantindo alta testabilidade (25 testes unitários implementados).
* **Mensageria Resiliente:** Utilizamos **RabbitMQ via MassTransit** para comunicação assíncrona entre a ingestão de dados e o processamento de alertas, garantindo tolerância a falhas.

## 🔒 Conformidade com a LGPD (Privacidade de Dados)
Como lidamos com dados sensíveis de Produtores Rurais, implementamos rigorosos controles de privacidade:
* **Direito ao Esquecimento:** Criamos o endpoint `DELETE /api/user/me` que permite ao produtor remover completamente sua conta, propriedades e dados atrelados.
* **Política de Privacidade:** Consulte nosso [PrivacyPolicy.md](./PrivacyPolicy.md) detalhando as bases legais e finalidades da coleta.
* **Segurança By Design:** Senhas são hasheadas via BCrypt, e o tráfego é protegido por JWT. Chaves sensíveis da infraestrutura residem estritamente em **Kubernetes Secrets**.

---

## 🚀 Tecnologias e Infraestrutura (K8s)

Atendendo aos requisitos obrigatórios e corrigindo as lições aprendidas em fases anteriores, **toda a infraestrutura como código (IaC) está versionada neste repositório** na pasta `/k8s`.

* **Backend:** .NET 8 (Minimal APIs, Worker Services), Entity Framework Core, PostgreSQL.
* **Mensageria:** RabbitMQ + MassTransit.
* **Orquestração:** Kubernetes (Manifestos completos: Deployments, Services, ConfigMaps e Secrets).
* **Escalabilidade (HPA):** O `IngestionService` possui Horizontal Pod Autoscaler (`hpa.yaml`) configurado para suportar picos de sensores IoT.
* **Observabilidade:** Stack **Prometheus + Grafana** (na pasta `/k8s/monitoring`), incluindo um Dashboard em JSON (`dashboard.json`) para monitoramento de umidade e alertas de seca.
* **CI/CD:** Pipeline automatizada via GitHub Actions (`ci-cd.yml`) com execução obrigatória de testes unitários.

---

## ⚙️ Como Executar o Projeto Localmente

### Opção 1: Via Docker Compose (Desenvolvimento Rápido)
1. Na raiz do projeto, execute:
   ```bash
   docker-compose up --build -d