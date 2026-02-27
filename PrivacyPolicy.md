# Política de Privacidade — AgroSolutions

**Última atualização:** Fevereiro de 2026

## 1. Introdução

A AgroSolutions está comprometida com a proteção dos dados pessoais dos produtores rurais que utilizam nossa plataforma, em conformidade com a **Lei Geral de Proteção de Dados Pessoais (LGPD — Lei nº 13.709/2018)**.

Este documento descreve quais dados coletamos, como os utilizamos, com quem os compartilhamos e quais são seus direitos como titular dos dados.

---

## 2. Dados Pessoais Coletados

| Dado | Finalidade | Base Legal (LGPD) |
|---|---|---|
| Nome completo | Identificação do produtor rural | Execução de contrato (Art. 7º, V) |
| Endereço de e-mail | Autenticação e comunicação | Execução de contrato (Art. 7º, V) |
| Senha (hash) | Segurança de acesso | Execução de contrato (Art. 7º, V) |
| Dados de propriedades rurais | Gestão de fazendas e talhões | Execução de contrato (Art. 7º, V) |
| Dados de sensores IoT (umidade, temperatura, precipitação) | Alertas de seca e análise agrícola | Legítimo interesse (Art. 7º, IX) |

---

## 3. Como os Dados São Utilizados

- **Autenticação:** E-mail e senha (armazenada como hash BCrypt) são usados para autenticar o acesso à plataforma.
- **Gestão de Propriedades:** Nome, localização e área das propriedades e talhões são usados para organizar e exibir as informações do produtor.
- **Monitoramento IoT:** Dados de umidade do solo, temperatura e precipitação são processados para gerar alertas de seca quando a umidade do solo cai abaixo de 30%.
- **Segurança:** Todos os endpoints são protegidos por autenticação JWT com validade de 24 horas.

---

## 4. Retenção de Dados

- **Dados de conta:** Mantidos enquanto a conta estiver ativa.
- **Dados de propriedades e talhões:** Mantidos enquanto a conta estiver ativa.
- **Dados de sensores:** Processados em tempo real via fila de mensagens (RabbitMQ) e não são armazenados permanentemente.

---

## 5. Compartilhamento de Dados

Os dados pessoais **não são compartilhados** com terceiros para fins comerciais. O processamento ocorre internamente entre os microserviços da plataforma AgroSolutions (IdentityService, PropertyService, IngestionService, AlertService), todos dentro do mesmo ambiente seguro (cluster Kubernetes).

---

## 6. Segurança

- Senhas são armazenadas usando **BCrypt** (hash unidirecional).
- Comunicação entre o cliente e a API é protegida por **JWT (JSON Web Token)**.
- Dados sensíveis em Kubernetes são armazenados em **Secrets** (não em ConfigMaps).
- Credenciais de banco de dados e chaves JWT são gerenciadas via variáveis de ambiente seguras.

---

## 7. Direitos do Titular dos Dados (LGPD — Art. 18)

Como titular dos dados, o produtor rural possui os seguintes direitos:

### 7.1 Direito de Acesso
Você pode solicitar informações sobre quais dados pessoais a AgroSolutions armazena sobre você.

### 7.2 Direito de Correção
Você pode atualizar seus dados diretamente pela plataforma.

### 7.3 Direito ao Esquecimento (Right to be Forgotten)
Você pode solicitar a exclusão completa de sua conta e de todos os dados relacionados através do endpoint:

```http
DELETE /api/user/me
Authorization: Bearer <seu-token-jwt>
```

Este endpoint:
1. Exclui permanentemente sua conta do **IdentityService**.
2. Para exclusão completa dos dados de propriedades e talhões, utilize também:

```http
DELETE /api/properties/owner/{seu-id}
Authorization: Bearer <seu-token-jwt>
```

> **Nota:** Após a exclusão, a operação é irreversível. Todos os dados associados à sua conta serão permanentemente removidos.

### 7.4 Direito de Portabilidade
Você pode solicitar uma exportação dos seus dados entrando em contato com nossa equipe.

### 7.5 Direito de Oposição
Você pode se opor ao processamento de dados para fins de análise, mantendo apenas os dados necessários para o funcionamento básico da plataforma.

---

## 8. Contato — Encarregado de Dados (DPO)

Para exercer seus direitos ou esclarecer dúvidas sobre o tratamento de dados pessoais:

- **E-mail:** privacidade@agrosolutions.com.br
- **Endereço:** AgroSolutions Tecnologia Agrícola Ltda., Brasil

---

## 9. Alterações nesta Política

Esta política pode ser atualizada periodicamente. Em caso de alterações significativas, os usuários serão notificados por e-mail com pelo menos 30 dias de antecedência.

---

*Esta política está em conformidade com a Lei Geral de Proteção de Dados Pessoais (LGPD — Lei nº 13.709/2018) e com o Regulamento Geral sobre a Proteção de Dados (GDPR) da União Europeia, conforme aplicável.*
