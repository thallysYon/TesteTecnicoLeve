# Leve Investimentos

Aplicação web desenvolvida em **ASP.NET Core MVC** para gerenciamento de usuários e tarefas, com autenticação, controle de acesso por perfil, upload de foto e notificações por e-mail.

## Tecnologias utilizadas

* **ASP.NET Core MVC**
* **C#**
* **Entity Framework Core**
* **SQL Server LocalDB**
* **UIkit**
* **MailKit**

## Funcionalidades

### Autenticação

* Login com e-mail e senha
* Logout
* Controle de acesso por autenticação
* Controle de acesso por perfil (**gestor** e **colaborador**)
* Tela de acesso negado

### Usuários

* Cadastro de usuários
* Upload e salvamento de foto
* Listagem em cards com paginação
* Visualização de detalhes
* Edição de cadastro
* Acesso restrito para gestor

### Tarefas

* Cadastro de tarefas
* Listagem em cards com paginação
* Visualização de detalhes
* Edição de tarefa
* Conclusão de tarefa pelo responsável
* Atualização de status

### Notificações por e-mail

* Envio de e-mail ao responsável quando uma nova tarefa é atribuída
* Envio de e-mail ao gestor quando a tarefa é concluída

---

## Usuário inicial

Ao iniciar a aplicação, é criado um usuário gestor padrão para acesso inicial:

* **E-mail:** `ti@leveinvestimentos.com.br`
* **Senha:** `teste123`

---

## Como executar o projeto

### Pré-requisitos

* .NET SDK compatível
* SQL Server LocalDB
* Visual Studio 2022 (ou compatível)

### 1. Configurar o banco

No arquivo `appsettings.json`, configure a connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=LeveDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 2. Aplicar as migrations

No Package Manager Console:

```powershell
Update-Database
```

> Caso ainda não existam migrations no seu ambiente, gere antes com `Add-Migration`.

### 3. Configurar e-mail

Adicione a seção abaixo no `appsettings.json`:

```json
"EmailSettings": {
  "Host": "smtp.gmail.com",
  "Port": 465,
  "NomeRemetente": "Leve Investimentos",
  "EmailRemetente": "seuemail@gmail.com",
  "Usuario": "seuemail@gmail.com",
  "Senha": "",
  "UsarSsl": true
}
```

> Para desenvolvimento, a recomendação é manter a senha fora do `appsettings.json` e usar configuração local.

### 4. Executar

Basta iniciar o projeto normalmente pelo Visual Studio (`F5` ou `Ctrl + F5`).

---

## Estrutura resumida

* `Controllers` → ações e fluxo da aplicação
* `Models` → entidades principais
* `ViewModels` → modelos das telas
* `Data` → contexto e inicialização do banco
* `Services` → serviços auxiliares (senha e e-mail)
* `Configurations` → configurações tipadas, como SMTP
* `Views` → páginas Razor
* `wwwroot` → arquivos estáticos e imagens

---

## Observações

* As fotos dos usuários são salvas em `wwwroot/imagens/usuarios`
* No banco é armazenado apenas o nome do arquivo da imagem
* O envio de e-mail foi implementado sem interromper o fluxo principal caso ocorra falha no SMTP

---

## Fluxos principais

### Gestor

* acessa o sistema
* cadastra usuários
* visualiza e edita usuários
* cadastra tarefas
* visualiza e edita tarefas
* recebe notificação por e-mail quando uma tarefa é concluída

### Colaborador

* acessa o sistema
* visualiza suas tarefas
* conclui suas tarefas
* recebe notificação por e-mail ao receber uma nova tarefa


---

## Autor

Desenvolvido por **Thallys Yon** como solução para desafio técnico.
