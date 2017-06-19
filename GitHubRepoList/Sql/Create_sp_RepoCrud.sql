CREATE FUNCTION [dbo].[UserHasPrivileges](@login VARCHAR(MAX), @password VARCHAR(MAX), @privilege VARCHAR(MAX))
RETURNS BIT
AS
BEGIN
	IF @privilege = 'READ'
	BEGIN
		IF EXISTS (SELECT * FROM Users WHERE [login] = @login AND [password] = @password AND ([is_read] = 'True' OR [is_admin] = 'True'))
		BEGIN
			RETURN CAST(1 AS BIT)
		END
	END
	ELSE IF @privilege = 'WRITE'
	BEGIN
		IF EXISTS (SELECT * FROM Users WHERE [login] = @login AND [password] = @password AND ([is_write] = 'True' OR [is_admin] = 'True'))
		BEGIN
			RETURN CAST(1 AS BIT)
		END
	END
	ELSE IF @privilege = 'ADMIN'
	BEGIN
		IF EXISTS (SELECT * FROM Users WHERE [login] = @login AND [password] = @password AND [is_admin] = 'True')
		BEGIN
			RETURN CAST(1 AS BIT)
		END
	END

	RETURN CAST(0 AS BIT)
END
GO

CREATE PROCEDURE GetRepos
	-- AUTHENTICATION DETAILS
	@login		VARCHAR(MAX),
	@password	VARCHAR(MAX)
	-- END OF AUTHENTICATION DETAILS
AS
BEGIN
	--IF EXISTS (SELECT * FROM Users WHERE [login] = @login AND [password] = @password AND ([is_read] = 'True' OR [is_admin] = 'True'))
	IF (SELECT [dbo].[UserHasPrivileges](@login, @password, 'READ')) = 'True'
	BEGIN
		SELECT * FROM Repoes
	END
	ELSE
	BEGIN
		RAISERROR ('Insufficient privileges!', 11, 1)
	END
END
GO

CREATE PROCEDURE CreateRepo
	@name			NVARCHAR(MAX),
	@full_name		NVARCHAR(MAX),
	@description	NVARCHAR(MAX),
	@url			NVARCHAR(MAX),
	@created_at		NVARCHAR(MAX),
	@private		BIT = 0,
	@fork			BIT = 0,
	@sort_position	INT = 0,
	@html_url		NVARCHAR(MAX) = '',
	@forks_url		NVARCHAR(MAX) = '',
	@updated_at		NVARCHAR(MAX) = '',
	@git_url		NVARCHAR(MAX) = ''
AS
BEGIN
	INSERT INTO Repoes ([name], [full_name], [description], [url], [created_at], [private], [fork], [sort_position], [html_url], [updated_at], [forks_url], [git_url])
	VALUES (@name, @full_name, @description, @url, @created_at, @private, @fork, @sort_position, @html_url, @updated_at, @forks_url, @git_url);
END
GO

CREATE PROCEDURE UpdateRepo
	-- AUTHENTICATION VARS
	@login		VARCHAR(MAX),
	@password	VARCHAR(MAX),
	-- END OF AUTHENTICATION VARS
	@repo_id		VARCHAR(MAX),
	@column_name	VARCHAR(MAX),
	@new_value		VARCHAR(MAX)
AS
BEGIN
	--IF EXISTS (SELECT * FROM Users WHERE [login] = @login AND [password] = @password AND ([is_write] = 'True' OR [is_admin] = 'True'))
	IF (SELECT [dbo].[UserHasPrivileges](@login, @password, 'WRITE')) = 'True'
	BEGIN
		DECLARE @update_query NVARCHAR(MAX)
		SET @update_query = 'UPDATE Repoes SET [' + @column_name + ']=''' + @new_value + ''' WHERE [id]=' + @repo_id + ';'
		EXEC SP_EXECUTESQL @update_query
	END
	ELSE
	BEGIN
		RAISERROR ('Insufficient privileges!', 11, 1)
	END
END
GO

CREATE PROCEDURE DeleteRepo
	-- AUTHENTICATION VARS
	@login		VARCHAR(MAX),
	@password	VARCHAR(MAX),
	-- END OF AUTHENTICATION VARS
	@repo_id		VARCHAR(MAX)
AS
BEGIN
	--IF EXISTS (SELECT * FROM Users WHERE [login] = @login AND [password] = @password AND ([is_write] = 'True' OR [is_admin] = 'True'))
	IF (SELECT [dbo].[UserHasPrivileges](@login, @password, 'WRITE')) = 'True'
	BEGIN
		DELETE FROM Repoes WHERE [id] = @repo_id
	END
	ELSE
	BEGIN
		RAISERROR ('Insufficient privileges!', 11, 1)
	END
END
GO