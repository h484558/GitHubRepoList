CREATE PROCEDURE AuthUser
	-- AUTHENTICATION DETAILS
	@login		VARCHAR(MAX),
	@password	VARCHAR(MAX)
	-- END OF AUTHENTICATION DETAILS
AS
BEGIN
	SELECT * FROM Users WHERE [login] = @login AND [password] = @password
END
GO

CREATE PROCEDURE GetUsers
	-- AUTHENTICATION DETAILS
	@login		VARCHAR(MAX),
	@password	VARCHAR(MAX)
	-- END OF AUTHENTICATION DETAILS
AS
BEGIN
	IF (SELECT [dbo].[UserHasPrivileges](@login, @password, 'ADMIN')) = 'True'
	BEGIN
		SELECT * FROM Users
	END
	ELSE
	BEGIN
		RAISERROR ('Insufficient privileges!', 11, 1)
	END
END
GO

CREATE PROCEDURE UpdateUser
	-- AUTHENTICATION VARS
	@login		VARCHAR(MAX),
	@password	VARCHAR(MAX),
	-- END OF AUTHENTICATION VARS
	@user_id		VARCHAR(MAX),
	@column_name	VARCHAR(MAX),
	@new_value		VARCHAR(MAX)
AS
BEGIN
	IF (SELECT [dbo].[UserHasPrivileges](@login, @password, 'ADMIN')) = 'True'
	BEGIN
		DECLARE @update_query NVARCHAR(MAX)
		SET @update_query = 'UPDATE Users SET [' + @column_name + ']=''' + @new_value + ''' WHERE [id]=' + @user_id + ';'
		EXEC SP_EXECUTESQL @update_query
	END
	ELSE
	BEGIN
		RAISERROR ('Insufficient privileges!', 11, 1)
	END
END
GO