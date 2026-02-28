USE [LeveDb];
GO

SET NOCOUNT ON;
GO

DECLARE @SenhaHashPadrao NVARCHAR(MAX);
DECLARE @GestorId INT;
DECLARE @Colaborador1Id INT;
DECLARE @Colaborador2Id INT;

SELECT TOP 1
    @GestorId = Id,
    @SenhaHashPadrao = SenhaHash
FROM dbo.Usuarios
WHERE Email = 'ti@leveinvestimentos.com.br';

IF @GestorId IS NULL
BEGIN
    RAISERROR('Usuário gestor inicial não encontrado. Execute a aplicação ao menos uma vez antes de rodar este script.', 16, 1);
    RETURN;
END;

-- =========================================================
-- USUÁRIOS DE EXEMPLO
-- Todos usarão a mesma senha do gestor inicial: teste123
-- =========================================================

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Email = 'colaborador1@leveinvestimentos.com.br')
BEGIN
    INSERT INTO dbo.Usuarios
    (
        NomeCompleto,
        DataNascimento,
        TelefoneFixo,
        TelefoneCelular,
        Email,
        Endereco,
        FotoNomeArquivo,
        SenhaHash,
        IsGestor
    )
    VALUES
    (
        'Colaborador Um',
        '1998-05-10',
        '1133334444',
        '1199991111',
        'colaborador1@leveinvestimentos.com.br',
        'Rua Exemplo, 100 - Bragança Paulista/SP',
        NULL,
        @SenhaHashPadrao,
        0
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Email = 'colaborador2@leveinvestimentos.com.br')
BEGIN
    INSERT INTO dbo.Usuarios
    (
        NomeCompleto,
        DataNascimento,
        TelefoneFixo,
        TelefoneCelular,
        Email,
        Endereco,
        FotoNomeArquivo,
        SenhaHash,
        IsGestor
    )
    VALUES
    (
        'Colaborador Dois',
        '2000-08-20',
        '1144445555',
        '1199992222',
        'colaborador2@leveinvestimentos.com.br',
        'Avenida Demonstração, 200 - Bragança Paulista/SP',
        NULL,
        @SenhaHashPadrao,
        0
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Email = 'gestor2@leveinvestimentos.com.br')
BEGIN
    INSERT INTO dbo.Usuarios
    (
        NomeCompleto,
        DataNascimento,
        TelefoneFixo,
        TelefoneCelular,
        Email,
        Endereco,
        FotoNomeArquivo,
        SenhaHash,
        IsGestor
    )
    VALUES
    (
        'Gestor Secundário',
        '1989-03-15',
        '1155556666',
        '1199993333',
        'gestor2@leveinvestimentos.com.br',
        'Praça Exemplo, 300 - Bragança Paulista/SP',
        NULL,
        @SenhaHashPadrao,
        1
    );
END;

SELECT @Colaborador1Id = Id
FROM dbo.Usuarios
WHERE Email = 'colaborador1@leveinvestimentos.com.br';

SELECT @Colaborador2Id = Id
FROM dbo.Usuarios
WHERE Email = 'colaborador2@leveinvestimentos.com.br';

-- =========================================================
-- TAREFAS DE EXEMPLO
-- Status: 0 = Pendente | 1 = Concluida
-- =========================================================

IF NOT EXISTS (SELECT 1 FROM dbo.Tarefas WHERE Mensagem = 'Entrar em contato com cliente sobre documentação.')
BEGIN
    INSERT INTO dbo.Tarefas
    (
        Mensagem,
        DataLimite,
        Status,
        DataCriacao,
        DataConclusao,
        ResponsavelId,
        GestorCriadorId
    )
    VALUES
    (
        'Entrar em contato com cliente sobre documentação.',
        DATEADD(DAY, 3, GETDATE()),
        0,
        GETDATE(),
        NULL,
        @Colaborador1Id,
        @GestorId
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Tarefas WHERE Mensagem = 'Atualizar planilha de acompanhamento de tarefas.')
BEGIN
    INSERT INTO dbo.Tarefas
    (
        Mensagem,
        DataLimite,
        Status,
        DataCriacao,
        DataConclusao,
        ResponsavelId,
        GestorCriadorId
    )
    VALUES
    (
        'Atualizar planilha de acompanhamento de tarefas.',
        DATEADD(DAY, 5, GETDATE()),
        0,
        GETDATE(),
        NULL,
        @Colaborador2Id,
        @GestorId
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Tarefas WHERE Mensagem = 'Revisar cadastro de usuários pendentes.')
BEGIN
    INSERT INTO dbo.Tarefas
    (
        Mensagem,
        DataLimite,
        Status,
        DataCriacao,
        DataConclusao,
        ResponsavelId,
        GestorCriadorId
    )
    VALUES
    (
        'Revisar cadastro de usuários pendentes.',
        DATEADD(DAY, -2, GETDATE()),
        1,
        DATEADD(DAY, -5, GETDATE()),
        DATEADD(DAY, -1, GETDATE()),
        @Colaborador1Id,
        @GestorId
    );
END;

PRINT 'População concluída com sucesso.';
GO