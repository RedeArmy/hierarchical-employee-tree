USE [master]
GO

CREATE DATABASE [EmployeeTreeDB]
 CONTAINMENT = NONE
GO
       
USE [EmployeeTreeDB]
GO
       
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [EmployeeTreeDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

-- =====================================
-- Create tables
-- =====================================

-- Table for Positions
CREATE TABLE position (
      position_id INT PRIMARY KEY IDENTITY(1,1),
      position_name NVARCHAR(50) UNIQUE NOT NULL
);

INSERT INTO position (position_name)
VALUES
    ('GERENTE'),
    ('SUB GERENTE'),
    ('SUPERVISOR');


-- Table for Employees with hierarchical relation
CREATE TABLE employee (
      employee_id INT PRIMARY KEY IDENTITY(1,1),
      employee_first_name NVARCHAR(50) NOT NULL,
      employee_last_name NVARCHAR(50) NOT NULL,
      position_id INT NOT NULL,
      manager_employee_id INT NULL,
      create_time DATETIME2 DEFAULT GETDATE(),
      CONSTRAINT fk_employee_position FOREIGN KEY (position_id) REFERENCES position(position_id),
      CONSTRAINT fk_employee_manager FOREIGN KEY (manager_employee_id) REFERENCES employee(employee_id)
);

-- Table for Users for authentication and authorization
CREATE TABLE [user] (
    [user_id] INT PRIMARY KEY IDENTITY(1,1),
    username NVARCHAR(50) UNIQUE NOT NULL,
    [password] NVARCHAR(255) NOT NULL,
    [rol] NVARCHAR(20) NOT NULL DEFAULT 'EMPLOYEE', -- e.g., 'Employee', 'Admin'
    employee_id INT NULL, -- Nullable; linked if user is an employee
    is_active BIT NOT NULL DEFAULT 1,
    create_time DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT fk_user_employee FOREIGN KEY (employee_id) REFERENCES employee(employee_id)
    );

INSERT INTO [user] (username, [password], [rol])
VALUES
    ('system', '$2a$12$R2YXyxiVOVTem1tCYfqRMO9I2aZczd0fJqlgGCSCIoqhKyiXBWli2', 'ADMIN'),
    ('admin', '$2a$12$/BYDuBsN7sJY5S9mncyLquwF7W7fWbPYRuQ.PGsXPCZrSgua1orCu', 'ADMIN');

-- Table for Actions
CREATE TABLE [action] (
                          action_id INT PRIMARY KEY IDENTITY(1,1),
    action_name VARCHAR(25) UNIQUE NOT NULL
    );

INSERT INTO [action] (action_name)
VALUES
    ('INSERT'),
    ('CREATE'),
    ('UPDATE'),
    ('DELETE');

-- Audit table to log changes in the system
CREATE TABLE [audit] (
                         audit_id INT PRIMARY KEY IDENTITY(1,1),
    [user_id] INT NOT NULL,
    table_name NVARCHAR(50) NOT NULL,
    record_id  INT NOT NULL,
    action_id INT NOT NULL,
    comment NVARCHAR(255) NULL,
    action_date DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT fk_audit_user FOREIGN KEY ([user_id]) REFERENCES [user]([user_id]),
    CONSTRAINT fk_audit_action FOREIGN KEY (action_id) REFERENCES [action](action_id)
    );


---------------------------------------------------------------------------------------------------------------------------------------------------------------
-- =====================================
-- Create SP 
-- =====================================

-- INSERT SP
CREATE PROCEDURE sp_insert_employee
    @first_name NVARCHAR(50),
    @last_name NVARCHAR(50),
    @position_id INT,
    @manager_id INT = NULL,
    @created_by_user_id INT,
    @create_user BIT = 0, -- Nuevo parámetro opcional
    @username NVARCHAR(50) = NULL,
    @password NVARCHAR(75) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Insertar empleado
INSERT INTO employee (employee_first_name, employee_last_name, position_id, manager_employee_id)
VALUES (@first_name, @last_name, @position_id, @manager_id);

DECLARE @employee_id INT = SCOPE_IDENTITY();
    DECLARE @action_id INT = (SELECT action_id FROM [action] WHERE action_name = 'CREATE');
    DECLARE @usuario NVARCHAR(50) = (SELECT username FROM [user] WHERE [user_id] = @created_by_user_id);
    DECLARE @comment NVARCHAR(200) = 'El ' + @usuario + ' realizó la inserción del nuevo empleado con ID: ' + CAST(@employee_id AS NVARCHAR(25));

    -- Audit
INSERT INTO audit ([user_id], table_name, record_id, action_id, comment)
VALUES (@created_by_user_id, 'employee', @employee_id, @action_id, @comment);

-- Crear usuario si se solicita
IF @create_user = 1
BEGIN
        DECLARE @default_role NVARCHAR(20) = 'EMPLOYEE';

INSERT INTO [user] (username, [password], [rol], employee_id)
VALUES (@username, @password, @default_role, @employee_id);

DECLARE @user_id INT = SCOPE_IDENTITY();
        DECLARE @comment_user NVARCHAR(200) = 'El ' + @usuario + ' realizó la creación del nuevo usuario con ID: ' + CAST(@user_id AS NVARCHAR(25));

         -- Audit
INSERT INTO audit ([user_id], table_name, record_id, action_id, comment)
VALUES (@created_by_user_id, 'user', @user_id, @action_id, @comment_user);
END



    -- Devolver datos del empleado insertado
    IF @create_user = 1
BEGIN
SELECT
    e.employee_id AS EmployeeId,
    e.employee_first_name AS FirstName,
    e.employee_last_name AS LastName,
    e.position_id AS PositionId,
    e.manager_employee_id AS ManagerEmployeeId,
    e.create_time AS CreateTime,
    u.username AS Username,
    CASE WHEN u.username IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS CreateUser
FROM employee e
         LEFT JOIN [user] u ON e.employee_id = u.employee_id
WHERE e.employee_id = @employee_id;
END
ELSE
BEGIN
SELECT
    employee_id AS EmployeeId,
    employee_first_name AS FirstName,
    employee_last_name AS LastName,
    position_id AS PositionId,
    manager_employee_id AS ManagerEmployeeId,
    create_time AS CreateTime
FROM employee
WHERE employee_id = @employee_id;
END

END;

---------------------------------------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[sp_insert_position]
    @position_name NVARCHAR(50),
    @created_by_user_id INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Insertar nueva posición
INSERT INTO position (position_name)
VALUES (@position_name);

DECLARE @position_id INT = SCOPE_IDENTITY();
    DECLARE @usuario NVARCHAR(50) = (SELECT username FROM [user] WHERE [user_id] = @created_by_user_id);
    DECLARE @action_id INT = (SELECT action_id FROM [action] WHERE action_name = 'CREATE');
    DECLARE @comment NVARCHAR(200) = 'El ' + @usuario + ' realizó la creación de un nuevo puesto con ID: ' + CAST(@position_id AS NVARCHAR(25));

    -- Audit
INSERT INTO audit ([user_id], table_name, record_id, action_id, comment)
VALUES (@created_by_user_id, 'position', @position_id, @action_id, @comment);

-- Retornar la posición insertada
SELECT
    position_id AS PositionId,
    position_name AS PositionName
FROM position
WHERE position_id = @position_id;
END;

---------------------------------------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE sp_insert_user
    @username NVARCHAR(50),
    @password NVARCHAR(255),
    @role NVARCHAR(20) = 'EMPLOYEE',
    @employee_id INT = NULL,
    @created_by_user_id INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Insertar usuario
INSERT INTO [user] (
    username,
    [password],
[rol],
    employee_id
)
VALUES (
    @username,
    @password,
    @role,
    @employee_id
    );

DECLARE @user_id INT = SCOPE_IDENTITY();
    DECLARE @usuario NVARCHAR(50) = (SELECT username FROM [user] WHERE [user_id] = @created_by_user_id);
    DECLARE @action_id INT = (SELECT action_id FROM [action] WHERE action_name = 'CREATE');
    DECLARE @comment NVARCHAR(200) = 'El usuario: ' + @usuario + ' realizó la creación del nuevo usuario con ID: ' + CAST(@user_id AS NVARCHAR(25));

    -- Audit
INSERT INTO audit ([user_id], table_name, record_id, action_id, comment)
VALUES (@created_by_user_id, 'user', @user_id, @action_id, @comment);

-- Retornar el usuario insertado
SELECT
    [user_id] AS UserId,
    username AS Username,
    [password] AS PasswordHash,
    [rol] AS Role,
    employee_id AS EmployeeId,
    is_active AS IsActive,
    create_time AS CreateTime
FROM [user]
WHERE [user_id] = @user_id;
END;

---------------------------------------------------------------------------------------------------------------------------------------------------------------
-- UPDATE SP


CREATE PROCEDURE sp_update_employee
    @employee_id INT,
    @new_position_id INT = NULL,
    @new_manager_id INT = NULL,
    @updated_by_user_id INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @current_position_id INT = (SELECT position_id FROM employee WHERE employee_id = @employee_id);
    DECLARE @final_position_id INT = ISNULL(@new_position_id, @current_position_id);

    -- Si el nuevo o actual position_id es 1 (sin jefe), manager debe ser NULL
UPDATE employee
SET
    position_id = @final_position_id,
    manager_employee_id = CASE
                              WHEN @final_position_id = 1 THEN NULL
                              WHEN @new_manager_id IS NULL THEN manager_employee_id
                              ELSE @new_manager_id
        END
WHERE employee_id = @employee_id;


-- Auditoría
DECLARE @usuario NVARCHAR(50) = (SELECT username FROM [user] WHERE [user_id] = @updated_by_user_id);
    DECLARE @action_id INT = (SELECT action_id FROM [action] WHERE action_name = 'UPDATE');
    DECLARE @comment NVARCHAR(200);

    IF @new_position_id IS NOT NULL AND @new_manager_id IS NOT NULL
BEGIN
        SET @comment = 'El ' + @usuario + ' actualizó el puesto y el jefe del empleado con ID: ' + CAST(@employee_id AS NVARCHAR);
END
ELSE IF @new_position_id IS NOT NULL
BEGIN
        SET @comment = 'El ' + @usuario + ' actualizó el puesto del empleado con ID: ' + CAST(@employee_id AS NVARCHAR);
END
ELSE IF @new_manager_id IS NOT NULL
BEGIN
        SET @comment = 'El ' + @usuario + ' actualizó el jefe del empleado con ID: ' + CAST(@employee_id AS NVARCHAR);
END

INSERT INTO audit ([user_id], table_name, record_id, action_id, comment)
VALUES (@updated_by_user_id, 'employee', @employee_id, @action_id, @comment);

-- Retornar el registro actualizado
SELECT
    employee_id AS EmployeeId,
    employee_first_name AS FirstName,
    employee_last_name AS LastName,
    position_id AS PositionId,
    manager_employee_id AS ManagerEmployeeId,
    create_time AS CreateTime
FROM employee
WHERE employee_id = @employee_id;
END;

---------------------------------------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE sp_update_user
    @user_id INT,
    @new_password NVARCHAR(255) = NULL,
    @new_rol NVARCHAR(20) = NULL,
    @new_is_active BIT = NULL,
    @updated_by_user_id INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Actualizar campos de forma condicional
UPDATE [user]
SET
    [password] = ISNULL(@new_password, [password]),
    [rol] = ISNULL(@new_rol, [rol]),
    is_active = ISNULL(@new_is_active, is_active)
WHERE user_id = @user_id;

-- Construcción del comentario dinámico para auditoría
DECLARE @usuario NVARCHAR(50) = (SELECT username FROM [user] WHERE [user_id] = @updated_by_user_id);
    DECLARE @action_id INT = (SELECT action_id FROM [action] WHERE action_name = 'UPDATE');
    DECLARE @comment NVARCHAR(200) = 'El usuario: ' + @usuario + ' actualizó el campo ';

    IF @new_password IS NOT NULL SET @comment += 'password, ';
    IF @new_rol IS NOT NULL SET @comment += 'rol, ';
    IF @new_is_active IS NOT NULL SET @comment += 'is_active, ';

    -- Quitar última coma y espacio si es necesario
    IF RIGHT(@comment, 2) = ', ' SET @comment = LEFT(@comment, LEN(@comment) - 2);

    SET @comment += ' del usuario con ID: ' + CAST(@user_id AS NVARCHAR);

    -- Insertar en auditoría
INSERT INTO audit ([user_id], table_name, record_id, action_id, comment)
VALUES (@updated_by_user_id, 'user', @user_id, @action_id, @comment);

-- Devolver el usuario actualizado
SELECT
    [user_id] AS UserId,
    username AS Username,
    [rol] AS Role,
    employee_id AS EmployeeId,
    is_active AS IsActive,
    create_time AS CreateTime
FROM [user]
WHERE [user_id] = @user_id;
END;

---------------------------------------------------------------------------------------------------------------------------------------------------------------
-- DELETE SP

CREATE PROCEDURE sp_delete_employee
    @employee_id INT,
    @deleted_by_user_id INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Obtener puesto del empleado
    DECLARE @position_id INT = (
        SELECT position_id FROM employee WHERE employee_id = @employee_id
    );

    -- Buscar el nuevo manager: mismo puesto, menor cantidad de subordinados, distinto del que se eliminará
    DECLARE @new_manager_id INT = (
        SELECT TOP 1 e.employee_id
        FROM employee e
        LEFT JOIN (
            SELECT manager_employee_id, COUNT(*) AS SubordinateCount
            FROM employee
            WHERE manager_employee_id IS NOT NULL
            GROUP BY manager_employee_id
        ) ec ON e.employee_id = ec.manager_employee_id
        WHERE e.position_id = @position_id AND e.employee_id <> @employee_id
        ORDER BY ISNULL(ec.SubordinateCount, 0) ASC, e.employee_id ASC
    );

    -- Reasignar subordinados al nuevo manager si se encontró uno
    IF @new_manager_id IS NOT NULL
BEGIN
UPDATE employee
SET manager_employee_id = @new_manager_id
WHERE manager_employee_id = @employee_id;
END
ELSE
BEGIN
        -- Si no hay manager alternativo, quitar la asignación de jefe

SELECT TOP 1 @new_manager_id = e.employee_id
FROM employee e
WHERE e.position_id = @position_id AND e.employee_id <> @employee_id
ORDER BY e.employee_id ASC;

UPDATE employee
SET manager_employee_id = @new_manager_id
WHERE manager_employee_id = @employee_id;
END

    -- Eliminar usuario si existe
    DECLARE @user_id INT = (SELECT user_id FROM [user] WHERE employee_id = @employee_id);
    DECLARE @usuario NVARCHAR(50) = (SELECT username FROM [user] WHERE [user_id] = @deleted_by_user_id);
    IF @user_id IS NOT NULL
BEGIN
DELETE FROM [user] WHERE user_id = @user_id;

-- Auditar eliminación de usuario
DECLARE @user_action_id INT = (SELECT action_id FROM [action] WHERE action_name = 'DELETE');
        DECLARE @user_comment NVARCHAR(200) = 'El usuario ' + ISNULL(@usuario, 'sistema') + ' elimino el user con ID: ' + CAST(@user_id AS NVARCHAR);

INSERT INTO audit ([user_id], table_name, record_id, action_id, comment)
VALUES (@deleted_by_user_id, 'user', @user_id, @user_action_id, @user_comment);
END

    -- Eliminar empleado
DELETE FROM employee WHERE employee_id = @employee_id;

-- Auditar eliminación de empleado
DECLARE @emp_action_id INT = (SELECT action_id FROM [action] WHERE action_name = 'DELETE');
    DECLARE @emp_comment NVARCHAR(200) = 'El usuario ' + ISNULL(@usuario, 'sistema') + ' elimino al empleado con ID: ' + CAST(@employee_id AS NVARCHAR) + ' y reasigno a sus empleados al empleado: ' + ISNULL(CAST(@new_manager_id AS NVARCHAR), 'NULL');

INSERT INTO audit ([user_id], table_name, record_id, action_id, comment)
VALUES (@deleted_by_user_id, 'employee', @employee_id, @emp_action_id, @emp_comment);
END;

---------------------------------------------------------------------------------------------------------------------------------------------------------------
-- Read SP - employee_hierarchy Tree

CREATE PROCEDURE sp_get_employee_hierarchy
    AS
BEGIN
    SET NOCOUNT ON;

    ;WITH EmployeeCTE AS (
    -- Nivel raíz (empleados sin jefe)
    SELECT
        e.employee_id,
        e.employee_first_name,
        e.employee_last_name,
        e.position_id,
        e.manager_employee_id,
        e.create_time,
        CAST(e.employee_first_name + ' ' + e.employee_last_name AS NVARCHAR(MAX)) AS hierarchy_path,
        0 AS level
    FROM employee e
    WHERE e.manager_employee_id IS NULL

    UNION ALL

    -- Subordinados recursivos
    SELECT
        e.employee_id,
        e.employee_first_name,
        e.employee_last_name,
        e.position_id,
        e.manager_employee_id,
        e.create_time,
        CAST(cte.hierarchy_path + ' > ' + e.employee_first_name + ' ' + e.employee_last_name AS NVARCHAR(MAX)) AS hierarchy_path,
        cte.level + 1
    FROM employee e
             INNER JOIN EmployeeCTE cte ON e.manager_employee_id = cte.employee_id
)

     SELECT
         cte.employee_id AS EmployeeId,
         cte.employee_first_name AS FirstName,
         cte.employee_last_name AS LastName,
         p.position_name AS PositionName,
         cte.manager_employee_id AS ManagerId,
         cte.level AS HierarchyLevel
     FROM EmployeeCTE cte
              LEFT JOIN position p ON cte.position_id = p.position_id
     ORDER BY cte.hierarchy_path;
END;